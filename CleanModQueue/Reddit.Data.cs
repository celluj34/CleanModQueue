// Reddit.Data.cs
// ------------------------------------------------------------------
//
// Part of CleanModQueue .
//
// This module defines a set of classes to hold responses from Reddit's
// API.  These responses include lists of posts, metadata about users or
// reddits, and so on.  They are returned in JSON format, and the
// CleanModQueue tool uses HttpClient (really the HttpContent
// extensions) to de-serialize that JSON into instances of the classes
// defined in this module.
//
// last saved: <2012-June-01 10:05:43>
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
// using System.Linq;
// using System.Text;

namespace Dino.Reddit.Data
{
    public class AccountData
    {
        public string has_mail;
        public String name;
        public double created ; //: 1327371189.0,
        public DateTime createdTime
        {
            get
            {
                long win32FileTime = 10000000*(long)created + 116444736000000000;
                return DateTime.FromFileTime(win32FileTime);
            }
            set
            {
            }
        }

        public double created_utc; //: 1327345989.0,
        public DateTime createdUtcTime
        {
            get
            {
                long win32FileTime = 10000000*(long)created_utc + 116444736000000000;
                return DateTime.FromFileTimeUtc(win32FileTime);
            }
            set
            {
            }
        }

        public int link_karma;
        public int comment_karma;
        public bool is_gold;
        public bool is_mod;
        public string id;
        public string has_mod_mail;
    }


    public class MediaEmbed
    {
        public String content { get; set;}
        public int height     { get; set;}
        public int width      { get; set;}
        public bool scrolling { get; set;}
    }

    public class ObjectEmbed
    {
        public String provider_url  { get; set;}
        public String description   { get; set;}
        public String title         { get; set;}
        public String url           { get; set;}
        public String type          { get; set;}
        public String author_name   { get; set;}
        public String author_url    { get; set;}
        public int height           { get; set;}
        public int width            { get; set;}
        public String html          { get; set;}
        public int thumbnail_width  { get; set;}
        public int thumbnail_height { get; set;}
        public String version       { get; set;}
        public String provider_name { get; set;}
        public String thumbnail_url { get; set;}
    }

    public class Media
    {
        public ObjectEmbed oembed { get; set;}
        public String type        { get; set;}
    }


    /// <summary>
    ///    Holds response from reddit about one post, or one comment, or
    ///    one subreddit.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///      While it would be nice to to have distinct types for post,
    ///     comment, reddit, and so on, because of impedance mismatch
    ///     issues between reddit's data protocol and the
    ///     HttpContent.ReadAsAsync<T> method, there is just one class.
    /// </para>
    /// <para>
    ///     The problem is that ReadasAsync<T> can distinguish between
    ///     different types in a JSON packet by examining the property
    ///     name of the wrapper object.  But in all cases the prop name
    ///     is "data", which means there is no chance to distinguish.
    ///     Therefore the Reddit.Client code always gets a OneItemData,
    ///     regardless of what is contained within that metadata.
    ///   </para>
    /// </remarks>
    public class OneItemData
    {
        public String domain                 { get;set; }  //: "disinfo.com",
        public MediaEmbed media_embed;  //:  {},
        public String levenshtein;  //: null,
        public String subreddit              { get;set; } //: "news",
        public String selftext_html; //: null,
        public String selftext               { get;set; } //: "",
        public String likes                  { get;set; } //: null,
        public bool saved                    { get;set; } //: false,
        public String id                     { get;set; } //: "ot7v9",
        public bool clicked                  { get;set; } //: false,
        public String title                  { get;set; } //: "Supreme Court Rules Congress May Re-Copyright Public Domain Works",
        public Media media                   { get;set; } //: null,
        public int score                     { get;set; } //: 1,
        public bool over_18                  { get;set; } //: false,
        public bool hidden                   { get;set; } //: false,
        public String thumbnail              { get;set; } //: "",
        public String subreddit_id           { get;set; } //: "t5_2qh3l",
        public String author_flair_css_class { get;set; } //: null,
        public int downs                     { get;set; } //: 0,
        public bool is_self                  { get;set; } //: false,
        public String permalink              { get;set; } //: "/r/news/comments/ot7v9/supreme_court_rules_congress_may_recopyright/",
        public String name                   { get;set; } //: "t3_ot7v9",
        public String url                    { get;set; } //: "http://www.disinfo.com/2012/01/supreme-court-rules-congress-may-re-copyright-public-domain-works/",
        public String author_flair_text;  //: null,
        public String author                 { get;set; } //: "[deleted]",
        public double created; //: 1327371189.0,
        public DateTime createdTime
        {
            get
            {
                long win32FileTime = 10000000*(long)created + 116444736000000000;
                return DateTime.FromFileTime(win32FileTime);
            }
            set
            {
            }
        }

        public double created_utc; //: 1327345989.0,
        public DateTime createdUtcTime
        {
            get
            {
                long win32FileTime = 10000000*(long)created_utc + 116444736000000000;
                return DateTime.FromFileTimeUtc(win32FileTime);
            }
            set
            {
            }
        }

        public int num_comments { get;set; }
        public int num_reports { get;set; }
        public int ups { get;set; } //: 1

        // Props for comments only. I cannot figure out how to
        // de-serialize comments differently from links/posts.
        // These remain null for actual posts.
        public String body    { get;set; }
        public String link_id { get;set; }

        // Props for reddits only. I cannot figure out how to
        // de-serialize each object type differently.
        // These remain null for comments or posts.
        public String display_name       { get;set; }
        public bool over18               { get;set; }
        public int subscribers           { get;set; }
        public String header_img         { get;set; }
        public String header_title       { get;set; }
        public String header_size        { get;set; }
        public String description        { get; set;}
        public String public_description { get; set;}

        // not part of the JSON schema - set by the app
        public bool markSpam    { get;set; }
        public bool markRemove  { get;set; }
        public bool markApprove { get;set; }
        public bool handled     { get;set; }
    }

    public class RedditResponse
    {
        public String kind { get;set; }

        // t1 = comment
        // t2 = account
        // t3 = post
        // t5 = reddit
    }

    public class ListingData
    {
        public string modhash { get;set; }
        public List<OneItem> children { get;set; }
        public string after { get;set; }
        public string before { get;set; }
    }

    public class Listing : RedditResponse
    {
        public ListingData data { get;set; }
        public String error { get;set; }
    }

    public class User
    {
        public string name   { get; set;}
        public string id   { get; set;}
    }

    public class UserListData
    {
        public List<User> children { get;set; }
    }

    public class UserList : RedditResponse
    {
        public UserListData data { get;set; }
        public String error { get;set; }
    }

    public class OneItem : RedditResponse
    {
        public OneItemData data { get;set; }
    }

    public class Account : RedditResponse
    {
        public AccountData data { get;set; }
        public String error { get;set; }
    }

    public class LoginResponseData
    {
        public string modhash   { get; set;}
        public string cookie   { get; set;}
    }

    public class LoginResponseInner
    {
        public String[][] errors; // array of one array of N strings
        public LoginResponseData data;
    }

    public class LoginResponse
    {
        public LoginResponseInner json;
    }

    // example listing:
    // {
    //     "kind": "Listing",
    //     "data": {
    //         "modhash": "uh70wvi0qt9cbf3d427af52c8c2527e1addc0b1f473d34fe95",
    //         "children": [{
    //             "kind": "t3",
    //             "data": {
    //                 "domain": "disinfo.com",
    //                 "media_embed": {},
    //                 "levenshtein": null,
    //                 "subreddit": "news",
    //                 "selftext_html": null,
    //                 "selftext": "",
    //                 "likes": null,
    //                 "saved": false,
    //                 "id": "ot7v9",
    //                 "clicked": false,
    //                 "title": "Supreme Court Rules Congress May \u2018Re-Copyright\u2019 Public Domain Works",
    //                 "media": null,
    //                 "score": 1,
    //                 "over_18": false,
    //                 "hidden": false,
    //                 "thumbnail": "",
    //                 "subreddit_id": "t5_2qh3l",
    //                 "author_flair_css_class": null,
    //                 "downs": 0,
    //                 "is_self": false,
    //                 "permalink": "/r/news/comments/ot7v9/supreme_court_rules_congress_may_recopyright/",
    //                 "name": "t3_ot7v9",
    //                 "created": 1327371189.0,
    //                 "url": "http://www.disinfo.com/2012/01/supreme-court-rules-congress-may-re-copyright-public-domain-works/",
    //                 "author_flair_text": null,
    //                 "author": "[deleted]",
    //                 "created_utc": 1327345989.0,
    //                 "num_comments": 1,
    //                 "ups": 1
    //             }
    //         }],
    //         "after": null,
    //         "before": null
    //     }
    // }

}

