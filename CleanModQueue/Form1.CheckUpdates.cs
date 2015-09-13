using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.ServiceModel.Syndication;

namespace Dino.Tools.Reddit.CleanModQueue
{
    partial class Form1
    {
        private class UpdateCheck
        {
            public bool UpdateAvailable   { get; set; }
            public string Message   { get; set; }
        }

        private static UpdateCheck CheckForUpdates(string feedUrl, Version currentVersion)
        {
            var r = new UpdateCheck();
            r.Message = "";
            // NOTE: Requires a reference to System.ServiceModel.dll
            var formatter = new Atom10FeedFormatter();
            try
            {
                // Read the feed
                using (var reader = System.Xml.XmlReader.Create(feedUrl))
                {
                    formatter.ReadFrom(reader);

                    var latest = (from i in formatter.Feed.Items
                                  where i.Categories.Any(c => IsStable(c.Name))
                                  orderby i.LastUpdatedTime descending
                                  select i).FirstOrDefault();

                    if (latest != null)
                    {
                        var u = latest.Links.Single().Uri.AbsoluteUri;
                        r.Message += String.Format("The latest release is: {0}\n", u);

                        var update = (from i in formatter.Feed.Items
                                      where ExtractVersion(i.Title.Text) > currentVersion &&
                                      i.Categories.Any(c => IsStable(c.Name))
                                      orderby i.LastUpdatedTime descending
                                      select i).FirstOrDefault();

                        if (update != null)
                        {
                            // TODO: Notify user of available download
                            var downloadUrl = update.Links.Single().Uri.AbsoluteUri;
                            r.Message += String.Format("There is an available update: {0}",
                                                       update.Title.Text);
                            r.UpdateAvailable = true;
                        }
                        else
                        {
                            r.Message += "There is no later version.";
                        }
                    }
                    else
                    {
                        r.Message += "Could not find a later version.";
                    }
                }
            }
            catch (System.Exception exc1)
            {
                r.Message += "Cannot check for updates. " + exc1.ToString();
            }
            return r;
        }

        enum ReleaseStatus
        {
            Stable = 1,
            Beta = 2,
            Alpha = 4
        }

        private static Version ExtractVersion(string vstring)
        {
            string pattern = "[A-Za-z ]+ v(?<ver>[\\d\\.]+)";
            var match = Regex.Match(vstring, pattern);
            if (match.Success)
            {
                // numerical ip addr representation
                var version = match.Groups["ver"].ToString();
                return new Version(version);
            }
            return new Version("0.0.0.1");
        }

        private static bool IsStable(string status)
        {
            var value = (ReleaseStatus)Enum.Parse(typeof(ReleaseStatus), status, true) ;
            return value == ReleaseStatus.Stable;
        }
    }
}

