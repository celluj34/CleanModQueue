// Reddit.Client.cs
// ------------------------------------------------------------------
//
// Part of CleanModQueue .
//
// Defines a class containing methods to facilitate interactions and
// communications to reddit, including Login, get mod queue, get user
// status, and so on.
//
// For some limited documentation of the reddit API see
// http://www.reddit.com/dev/api/ and
// https://github.com/reddit/reddit/wiki/API . (old)
//
// last saved: <2012-July-31 06:33:42>
// ------------------------------------------------------------------
//
// Copyright (c) 2012 Dino Chiesa
// All rights reserved.
//
// This code is Licensed under the New BSD license.
// See the License.txt file that accompanies this module.
//
// ------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net; // CookieCollection, CookieContainer
using System.Linq;

namespace Dino.Reddit
{
    internal static class Util
    {
        /// <summary>
        ///   Memoize a function.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     This is used as a general way to cache results of
        ///     network communications to reddit.
        ///   </para>
        /// </remarks>
        public static Func<A, R> Memoize<A, R>(Func<A, R> f)
        {
            var mem = new Dictionary<A, R>();
            // A function that wraps the originally-passed function.
            // This wrapper checks the cache before invoking the
            // wrapped function.
            Func<A, R> wrapper = (a) =>
                {
                    R value;
                    if (mem.TryGetValue(a, out value))
                        return value;
                    value = f(a);
                    mem.Add(a, value);
                    return value;
                };
            return wrapper;
        }
    }


    /// <summary>
    ///   Exposes methods for connecting to Reddit and doing various things.
    /// </summary>
    public class Client
    {
        private static readonly String redditLoginAddr = "https://ssl.reddit.com/api/login";
        private static readonly String redditBaseAddr = "http://www.reddit.com";
        System.DateTime lastRequest = new System.DateTime(0);
        Dictionary<String, bool> checkedUserCache;
        TimeSpan requiredDelta;
        TimeSpan fifteenDays;
        string userHash;
        string modHash;
        Func<String, Reddit.Data.Account> mGetAccount;
        Func<String, Reddit.Data.Listing> mGetUserRecentPosts;
        Func<String, Reddit.Data.UserList> mGetModsForSub;
        List<String> reportedUsers;
        HttpClient client;
        HttpClientHandler handler;

        /// <summary>
        ///   Constructor for the Reddit Client. Uses the specified
        ///   request delay (in milliseconds).
        /// </summary>
        /// <remarks>
        ///   <para>
        ///   For example a delay of 800
        ///   specifies that this class will delay so that it makes no more
        ///   than 1 request for every 800 milliseconds. Reddit says that
        ///   clients should make no more than 1 request every 2 seconds, but
        ///   allows for short bursts as long as there are fewer than 30
        ///   requests per minute, over the long term.
        ///   </para>
        /// </remarks>
        ///
        public Client(int requestDelayInMs)
        {
            requiredDelta = new System.TimeSpan(0, 0, 0, 0, requestDelayInMs);
            fifteenDays = new System.TimeSpan(15, 0, 0, 0);
            mGetModsForSub = Util.Memoize<String,Data.UserList>(GetModsForSub);
            mGetAccount = Util.Memoize<String,Data.Account>(uncachedGetAccount);
            mGetUserRecentPosts = Util.Memoize<String,Data.Listing>(GetUserRecentPosts);
            reportedUsers = new List<String>();
            checkedUserCache = new Dictionary<String, bool>();
            // avoid the "too many delimiter characters"
            System.Net.Http.Formatting.MediaTypeFormatter.SkipStreamLimitChecks = true;
            handler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = Dino.Http.CookieManager.GetCookieContainerForUrl(redditBaseAddr)
            };

            client = new HttpClient(handler);
            client.DefaultRequestHeaders.UserAgent.Clear();
            client.DefaultRequestHeaders.Add("User-Agent", "AyeMateyCleanModQueue");

            // client.DefaultRequestHeaders.UserAgent.Add
            //     ( new ProductInfoHeaderValue("CleanModQueue by /u/AyeMatey", "1.0") );
        }

        /// <summary>
        ///   Default constructor for the Reddit Client. Uses a request delay of
        ///   2 seconds.
        /// </summary>
        public Client() : this(2001) { }

        /// <summary>
        ///   Enforce the rate throttle.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     For internal use only. Reddit says client applications
        ///     should send at most one request every 2 seconds.
        ///   </para>
        /// </remarks>
        private void EnforceDelay()
        {
            int miniRestPeriod = 125;
            var delta = System.DateTime.Now - lastRequest;
            while (delta < requiredDelta)
            {
                System.Threading.Thread.Sleep(miniRestPeriod);
                System.Windows.Forms.Application.DoEvents();
                delta = System.DateTime.Now - lastRequest;
            }
        }


        public List<String> ExternalDeadList   { get; set; }


        /// <summary>
        ///   Returns the full link to the post or the comment.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     OneItemData is a class that holds metadata about one post, or
        ///     one comment.
        ///   </para>
        ///   <para>
        ///     This method returns the link for either the post or the
        ///     comment referenced in the object.  This works whether
        ///     post refers to a comment or a post.
        ///   </para>
        /// </remarks>
        /// <seealso cref='GetItemUrl'/>
        /// <seealso cref='GetCommentUrl'/>
        public static string GetItemUrl(Reddit.Data.OneItemData post)
        {
            return (String.IsNullOrEmpty(post.permalink))
                ?  Dino.Reddit.Client.GetCommentUrl(post)
                : Dino.Reddit.Client.GetPermalink(post);
        }

        public static string GetItemUrl(string id, string postid, string subreddit)
        {
            if (subreddit==null) return null;
            if (id==null) return null;
            //if (postid==null) return null;

            if (id.StartsWith("t1_") && postid!=null)
                return string.Format("{0}/r/{1}/comments/{2}/x/{3}",
                                     redditBaseAddr, subreddit,
                                     Lop3(postid),
                                     Lop3(id));
            if (id.StartsWith("t3_"))
                return string.Format("{0}/r/{1}/comments/{2}",
                                     redditBaseAddr, subreddit, Lop3(id));
            return null;
        }

        public static string GetAuthorUrl(Reddit.Data.OneItemData post)
        {
            return GetAuthorUrl(post.author);
        }

        public static string GetAuthorUrl(String authorName)
        {
            return (String.IsNullOrEmpty(authorName))
                ? null
                : redditBaseAddr + "/user/" + authorName;
        }

        private static string Lop3(string s)
        {
            return s.Substring(3,s.Length-3);
        }

        /// <summary>
        ///   Get the URL link for a particular comment on a post.
        /// </summary>
        /// <seealso cref='GetItemUrl'/>
        /// <seealso cref='GetPermalink'/>
        public static string GetCommentUrl(Reddit.Data.OneItemData post)
        {
            // eg,  /r/news/comments/t8298/Man_bites_dog/c4keg94
            //
            // The middle segment can be anything, but must be present.
            // The first id is for the link/post, the second is for the
            // comment.
            return string.Format("{0}/r/{1}/comments/{2}/x/{3}",
                                 redditBaseAddr, post.subreddit,
                                 Lop3(post.link_id),
                                 Lop3(post.name));
        }

        /// <summary>
        ///   Get the full permalink for a post.
        /// </summary>
        /// <seealso cref='GetItemUrl'/>
        /// <seealso cref='GetCommentUrl'/>
        public static string GetPermalink(Reddit.Data.OneItemData post)
        {
            return System.Net.WebUtility.HtmlDecode(redditBaseAddr + post.permalink);
        }

        /// <summary>
        ///   Replace newlines and carriage-returns from titles with
        ///   a vertical bar.  This cleans the title for display.
        /// </summary>
        public static string CleanTitle(Reddit.Data.OneItemData post)
        {
            var title = post.title;
            if (title == null) return "--no title--";
            string t = title.Replace('\r', '|').Replace('\n', '|');
            return t;
        }

        private static string MsgForPost(Reddit.Data.OneItemData post)
        {
            return (post == null || post.title == null)
                    ? "..comment from " + post.author + ".."
                    : CleanTitle(post);
        }

        /// <summary>
        ///   This event fires before and after network communications
        ///   to reddit.  This is intended to aid in user applications
        ///   that want to update their UI with status information.
        /// </summary>
        public event EventHandler<ProgressArgs> Progress;

        private void FireProgress(Stage stage, Activity activity, string msg=null)
        {
            // Copy to a temporary variable to be thread-safe.
            EventHandler<ProgressArgs> h = Progress;
            if (h != null)
            {
                h(this, new ProgressArgs(stage, activity, msg));
            }
        }

        private string DoPostAction(string addr, string postData)
        {
            var msg = new StringContent(postData,
                                        System.Text.Encoding.UTF8,
                                        "application/x-www-form-urlencoded");

            //handler.AllowAutoRedirect = followRedirects;
            var task = client.PostAsync(addr, msg);
            task.Wait();

            var submitResult = task.Result.Content.ReadAsStringAsync().Result;
            lastRequest = System.DateTime.Now;
            return submitResult;
        }

        /// <summary>
        ///   Remove a post.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     Like clicking the "remove" button on the modqueue page.
        ///   </para>
        /// </remarks>
        public string RemovePost(Data.OneItemData post, bool notSpam = false)
        {
            var m = MsgForPost(post);
            FireProgress(Stage.Before, Activity.RemoveItem, m);

            EnforceDelay();
            string addr = redditBaseAddr + "/api/remove.json";
            string postData = String.Format("id={0}&uh={1}&r={2}",
                                            post.name,
                                            modHash,
                                            post.subreddit);
            if (notSpam)
                postData += "&spam=false";

            var r = DoPostAction(addr, postData);
            FireProgress(Stage.After, Activity.RemoveItem, m);
            post.handled = true;
            return r;
        }


        /// <summary>
        ///   Approve a post.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     Like clicking the "approve" button on the modqueue page.
        ///   </para>
        /// </remarks>
        public string ApprovePost(Data.OneItemData post)
        {
            var m = MsgForPost(post);
            FireProgress(Stage.Before, Activity.ApproveItem, m);

            // this.lblStatus.Text = "approving " + post.title;
            // this.Update();

            EnforceDelay();
            string addr = redditBaseAddr + "/api/approve.json";
            string postData = String.Format("id={0}&uh={1}&r={2}",
                                            post.name,
                                            modHash,
                                            post.subreddit);
            var r = DoPostAction(addr, postData);
            FireProgress(Stage.After, Activity.ApproveItem, m);
            post.handled = true;
            return r;
        }


        private T GetRedditThing<T>(string addr)
        {
            // Cookies are sent implicitly for this request,
            // via the HttpHandler.
            EnforceDelay();
            var task = client.GetAsync(addr);
            T result;
            try
            {
                task.Wait();
                HttpResponseMessage resp = task.Result;
                // Wed, 02 May 2012  10:50
                //
                // The problem with using ReadAsAsync<T> with reddit responses
                // is that the JSON is a different schema, but uses the same
                // prop name.  This is sort of bogus, but ... there it
                // is. Ideally there would be a way to do data-dependent
                // de-serialization: look at the value of one property to
                // determine how to de-serialize the content for another
                // property.  I haven't figure that out yet. p
                var t2 = resp.Content.ReadAsAsync<T>();
                t2.Wait();
                result = t2.Result;
                lastRequest = System.DateTime.Now;
            }
            catch
            {
                result = default(T);
            }
            return result;
        }


        /// <summary>
        ///   Report a user as a spammer, maybe.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     This method submits a user to /r/reportthespammers, maybe.
        ///   </para>
        ///   <para>
        ///     The method first queries the user. If the user is
        ///     already shadow-banned, this method does nothing further,
        ///     and returns.
        ///   </para>
        ///   <para>
        ///     If the user has an age of over 15 days, and has posted more than 3 items,
        //      and has a link karma below 20, then the account gets
        //      reported as a spammer.
        ///   </para>
        /// </remarks>
        public void MaybeReportUserAsSpammer(string username)
        {
            if (GetDeadUsers().Contains(username)) return;

            Data.Listing list = mGetUserRecentPosts(username);
            if (list==null || list.error != null) return;

            Data.Account acct = mGetAccount(username);
            if (acct==null || acct.data == null) return;

            // report as spammer if....
            // .... account is older than 15 days
            // .... has link karma less than 20
            // .... has posted more than 3 items
            // .... has not been reported already
            var age = DateTime.Now - acct.data.createdTime;
            if (age > fifteenDays &&
                //!acct.data.is_mod &&
                acct.data.link_karma < 20 &&
                list.data.children.Count(x => x.kind.Equals("t3")) > 3 &&
                !reportedUsers.Contains(username) &&
                (!checkedUserCache.ContainsKey(username) ||
                 !checkedUserCache[username] ))
            {
                ReportUserAsSpammer(username);
                reportedUsers.Add(username);
                if (ExternalDeadList != null)
                {
                    if (!ExternalDeadList.Contains(username))
                        ExternalDeadList.Add(username);
                }
                checkedUserCache[username] = true; // dead
            }
        }


        private string ReportUserAsSpammer(string username)
        {
            FireProgress(Stage.Before, Activity.ReportSpammer, username);

            if (String.IsNullOrEmpty(username)) return null;

            username = username.Trim();

            // format of the ID: either FrostyNJ
            // or http://www.reddit.com/user/FrostyNJ
            string id = username.Replace("http://www.reddit.com/user/", "");
            string addr = redditBaseAddr + "/api/submit";

            // To delete a user:
            // uh=f0f0f0f0&kind=link&url=yourlink.com&sr=funny&title=omg-look-at-this&id%23newlink&r=funny&renderstyle=html
            // uh = user modhash
            // kind = "link" or "self"
            // sr = the subreddit (reportthespammers)
            // title = can be anything
            // url = link tp post
            // r = the subreddit, again

            string url = string.Format("{0}/user/{1}", redditBaseAddr, id);
            string postData = string.Format("uh={0}&kind=link&url={1}&sr={2}&title={3}&r={2}",
                                            userHash, url, "reportthespammers", id);

            var r = DoPostAction(addr, postData);
            FireProgress(Stage.After, Activity.ReportSpammer, username);

            return r;
        }


        /// <summary>
        ///   Get the contents of the aggregate Mod Queue for the logged-in user.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     This method does not get the mod queue for a particular
        ///     reddit, but for a particular user, specifically the logged-in
        ///     user. The aggregate mod queue is different for each moderator,
        ///     depending on the various reddits he moderates.
        ///   </para>
        /// </remarks>
        /// <seealso cref='GetModQueue'/>
        public Reddit.Data.Listing GetUserModQueue()
        {
            FireProgress(Stage.Before, Activity.GetModQueue, null);
            string addr = redditBaseAddr + "/r/mod/about/modqueue.json";
            var r = GetRedditThing<Reddit.Data.Listing>(addr);
            FireProgress(Stage.After, Activity.GetModQueue, "");
            if (r != null && r.data!=null)
                modHash = r.data.modhash;
            return r;
        }


        /// <summary>
        ///   Get the contents of the Mod Queue for a particular reddit.
        /// </summary>
        /// <seealso cref='GetUserModQueue'/>
        /// <seealso cref='GetNewQueue'/>
        public Reddit.Data.Listing GetModQueue(string subreddit)
        {
            // eg, http://www.reddit.com/r/news/about/modqueue/
            var r = GetQueue(Activity.GetModQueue,"{0}/r/{1}/about/modqueue.json",
                             subreddit);
            if (r != null && r.data!=null)
                modHash = r.data.modhash;
            return r;
        }

        /// <summary>
        ///   Get the contents of the New Queue for a particular reddit.
        /// </summary>
        /// <seealso cref='GetModQueue'/>
        public Reddit.Data.Listing GetNewQueue(string subreddit)
        {
            // eg, http://www.reddit.com/r/redditdev/new.json
            var r = GetQueue(Activity.GetNewQueue, "{0}/r/{1}/new.json", subreddit);
            return r;
        }

        private Reddit.Data.Listing GetQueue(Activity activity,
                                             string format,
                                             string subreddit)
        {
            FireProgress(Stage.Before, activity, subreddit);
            string addr = String.Format(format, redditBaseAddr, subreddit);
            var r = GetRedditThing<Reddit.Data.Listing>(addr);
            FireProgress(Stage.After, activity, subreddit);
            return r;
        }



        /// <summary>
        ///   Gets the list of dead users.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     This is the number of users determined to be "dead" or
        ///     shadow-banned. The module determines this by going to the
        ///     user's page (/user/AyeMatey) and if a 404 results, then
        ///     it marks the user as dead.
        ///   </para>
        ///   <para>
        ///     This is exposed to allow client applications to store
        ///     the list of presumed dead users in a persistent
        ///     store. This allows an app to cache the list.  See <see
        ///     cref='MarkUsersDead'/> for a companion method, which
        ///     would allow an application to apply the contents of a
        ///     cache to an instance of Reddit.Client.
        ///   </para>
        /// </remarks>
        public System.Collections.ObjectModel.ReadOnlyCollection<String> GetDeadUsers()
        {
            return checkedUserCache.Keys
                .Where(x => checkedUserCache[x]) // true = dead
                //.Select(x=>x)
                .ToList()
                .AsReadOnly();
        }

        /// <summary>
        ///   Tells the Reddit.Client instance which users to treat as "Dead"
        ///   or known spammers.
        /// </summary>
        public void MarkUsersDead(IEnumerable<string> deadToMe)
        {
            foreach (var x in deadToMe)
            {
                if (!checkedUserCache.ContainsKey(x))
                    checkedUserCache.Add(x, true); // dead
            }
        }

        /// <summary>
        ///   Tells the Reddit.Client to drop a set of users from its list of
        ///   known spammers.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     An app may wish to re-habilitate a user, or remove it from
        ///     the list of known spammers. It can call this method to do so.
        ///   </para>
        ///   <para>
        ///     Reddit.Client may yet determine the user is dead by
        ///     visiting his /user/Foo page, and getting a 404. If that
        ///     happens, Reddit.Client places that user back on the
        ///     "known spammers" list.
        ///   </para>
        /// </remarks>
        public void MarkUsersUndead(IEnumerable<string> undead)
        {
            foreach (var x in undead)
            {
                if (checkedUserCache.ContainsKey(x))
                    checkedUserCache.Remove(x);
            }
        }

        /// <summary>
        ///   Explicitly test to see if a user is shadow-banned,
        ///   which means the user is a known spammer.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     This is done by HTTP GET on /user/Foo . If that link returns a
        ///     404, then the user is shadow-banned.
        ///   </para>
        /// </remarks>
        public bool IsUserDead(string username)
        {
            if (!checkedUserCache.ContainsKey(username))
            {
                Data.Listing list = mGetUserRecentPosts(username);
                checkedUserCache[username] = (list != null && list.error != null);
            }
            return checkedUserCache[username];
        }

        /// <summary>
        ///   Is the given user a mod for the given subreddit?
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     This may perform an HTTP GET to inquire the
        ///     list of mods for the subreddit. That result is cached
        ///     for future use.
        ///   </para>
        /// </remarks>
        public bool IsUserMod(string username, string subreddit)
        {
            Data.UserList mods = mGetModsForSub(subreddit);
            if (mods == null || mods.data == null || mods.data.children == null)
                return false;
            return mods.data.children.Any(x => username.Equals(x.name));
        }

        /// <summary>
        ///   Gets the list of recent posts for a user.
        /// </summary>
        public Data.Listing GetUserRecentPosts(string username)
        {
            // To determine if the  user is "shadow-banned", retrieve
            // his recent posts, with this:
            //    http://www.reddit.com/user/Foo.json
            //
            // If you get an error 404, then he is shadow-banned.
            //
            // Could also get
            // http://www.reddit.com/user/Foo/submitted.json
            //
            // The former includes comments submitted; the latter
            // gets only posts.
            //
            FireProgress(Stage.Before, Activity.GetPostHistory, username);
            string addr = String.Format("{0}/user/{1}.json",
                                        redditBaseAddr, username);
            var r = GetRedditThing<Data.Listing>(addr);
            FireProgress(Stage.After, Activity.GetPostHistory, username);
            return r;
        }

        /// <summary>
        ///   Gets the list of mods for a subreddit.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     This method does not use a cache. Calling it will always
        ///     result in an HTTP GET.
        ///   </para>
        /// </remarks>
        public Data.UserList GetModsForSub(string subreddit)
        {
            FireProgress(Stage.Before, Activity.GetMods, subreddit);
            string addr = String.Format("{0}/r/{1}/about/moderators.json",
                                        redditBaseAddr, subreddit);
            var r = GetRedditThing<Data.UserList>(addr);
            FireProgress(Stage.After, Activity.GetMods, subreddit);
            return r;
        }


        public Data.Listing GetSubsIMod()
        {
            // http://www.reddit.com/dev/api#GET_reddits_mine_moderator
            FireProgress(Stage.Before, Activity.GetSubs);
            string addr = String.Format("{0}/reddits/mine/moderator.json",
                                        redditBaseAddr);
            var r = GetRedditThing<Data.Listing>(addr);
            FireProgress(Stage.After, Activity.GetSubs);
            return r;
        }

        /// <summary>
        ///   Gets information about a reddit user or account - including
        ///   account creation date, link karma, comment karma, and etc.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     Calling this method will result in an HTTP GET to retrieve the
        ///     information.
        ///   </para>
        /// </remarks>
        private Data.Account uncachedGetAccount(string username)
        {
            // To get information about the user:
            //    http://www.reddit.com/user/Foo/about.json
            FireProgress(Stage.Before, Activity.AboutUser, username);
            string addr = String.Format("{0}/user/{1}/about.json",
                                        redditBaseAddr, username);
            var r = GetRedditThing<Data.Account>(addr);
            FireProgress(Stage.After, Activity.AboutUser, username);
            return r;
        }

        public Data.Account GetAccount(string username)
        {
            Data.Account acct = mGetAccount(username);
            return acct;
        }

        /// <summary>
        ///   Is the instance logged in?
        /// </summary>
        /// <seealso cref='Login'/>
        public bool LoggedIn
        {
            get
            {
                return (userHash!=null);
            }
        }

        /// <summary>
        ///   Logout from reddit.
        /// </summary>
        /// <seealso cref='Login'/>
        public string Logout()
        {
            FireProgress(Stage.Before, Activity.Logout, null);
            string addr = redditBaseAddr + "/logout";
            string postData = string.Format("uh={0}", userHash);
            var r = DoPostAction(addr, postData);
            userHash = null;
            FireProgress(Stage.After, Activity.Logout, null);
            return r;
        }

        /// <summary>
        ///   Authenticate to reddit.
        /// </summary>
        /// <seealso cref='LoggedIn'/>
        public bool Login(string user, string pwd)
        {
            FireProgress(Stage.Before, Activity.Login, null);
            string postData = string.Format("api_type=json&user={0}&passwd={1}",
                                            user, pwd);

            var msg = new StringContent(postData,
                                        System.Text.Encoding.UTF8,
                                        "application/x-www-form-urlencoded");

            Dino.Reddit.Data.LoginResponse result = null;
            try
            {
                var task = client.PostAsync(redditLoginAddr, msg);
                task.Wait();
                var t2 = task.Result.Content.ReadAsAsync<Dino.Reddit.Data.LoginResponse>();
                t2.Wait();
                result = t2.Result;
                if (result != null && result.json != null && result.json.errors.Length == 0)
                    userHash = result.json.data.modhash;
                else
                    userHash = null;
            }
            catch
            {
                userHash = null;
            }

            FireProgress(Stage.After, Activity.Login, userHash);
            return (userHash != null);
        }

        public enum Activity
        {
            Login,
            Logout,
            RemoveItem,
            ApproveItem,
            AboutUser,
            GetPostHistory,
            GetMods,
            GetSubs,
            GetModQueue,
            GetNewQueue,
            ReportSpammer
        }

        public enum Stage
        {
            Before,
            After
        }

        public class ProgressArgs : EventArgs
        {
            public ProgressArgs(Stage stage, Activity activity, string message)
            {
                @Stage = stage;
                @Activity = activity;
                Message = message;
            }

            public string Message     { get; set;}
            public Activity @Activity { get; set;}
            public Stage @Stage       { get; set;}
        }
    }
}
