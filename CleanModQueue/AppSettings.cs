// AppSettings.cs
// ------------------------------------------------------------------
//
// Part of CleanModQueue .  A class to hold Application settings. These
// are saved as an XML file to the "Roaming" directory.
//
// last saved: <2012-July-31 06:29:45>
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
using System.Windows.Forms;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System.Linq;
using System.Text;

namespace Dino.Tools.Reddit.CleanModQueue
{
    public enum OrderableAction
    {
        AllowUsers,
        DenyUsers,
        AllowDomains,
        DenyDomains,
        AllowUpvotes,
        AllowTitleRegex,
        DenyTitleRegex,
        DenyAuthorMaturity,
        AllowAuthorMaturity
    }

    public class Rules
    {
        public bool DenyAll    { get; set;}
        public bool AllowAll   { get; set;}
    }

    [XmlType("Maturity")]
    public class Maturity
    {
        public int Age   { get; set; }
        public int CommentKarma   { get; set; }
        public int LinkKarma   { get; set; }
        public Maturity() { }
        public Maturity(int v)
        {
            Age = CommentKarma = LinkKarma = v;
        }
    }

    [XmlType("QueueSettings")]
    public partial class QueueSettings
    {
        public QueueSettings()
        {
            Ordering =" AllowUsers, DenyUsers, AllowDomains, DenyDomains, AllowUpvotes, AllowTitleRegex, DenyTitleRegex, DenyAuthorMaturity, AllowAuthorMaturity ";
            var x = OrderedActions.ToString(); // reference it to compute
            UpvoteThreshold = 300;
            SpamDomains = new List<String>
                {"bit.ly", "bitly.com", "tinyurl.com" };
            DeadUsers = new List<String>();
            BlessedUsers = new List<String>();
            BlessedDomains = new List<String>();
            BlessedTitleRegexi = new List<String>();
            SpamTitleRegexi = new List<String>();
            SpamAuthMaturity = new Maturity(0);
            BlessedAuthMaturity = new Maturity(-1);
            UserRules = new Rules();
            DomainRules = new Rules();
            TitleRules = new Rules();
        }

        public string GetTabName()
        {
            if (this.Name==null) return "modqueue";
            return String.Format("{0} ({1})",
                                 this.Name,
                                 this.WatchNewQueue ? "new" : "mod");
        }

        public bool IsaModQueue()
        {
            if (this.Name==null) return true;
            return !this.WatchNewQueue;
        }

        public int UpvoteThreshold         { get;set; }

        /// <summary>
        ///   If true, tells the tool to watch the new queue. If false
        ///   watch the mod queue.
        /// </summary>
        public bool WatchNewQueue          { get;set; }

        /// <summary>
        ///   Whether to poll and clean this queue.  The user can
        ///   enable cleaning on a queue-by-queue basis.
        ///   The user can turn off cleaning temporarily, without
        ///   wiping the rules.
        /// </summary>
        public bool Enabled                { get;set; }

        [XmlAttribute]
        public String Name                 { get;set; }
        public List<string> DeadUsers      { get;set; }
        public List<string> SpamDomains    { get;set; }
        public List<string> SpamTitleRegexi { get;set; }
        public List<string> BlessedUsers   { get;set; }
        public List<string> BlessedDomains { get;set; }
        public List<string> BlessedTitleRegexi { get;set; }

        public Maturity SpamAuthMaturity   { get; set; }
        public Maturity BlessedAuthMaturity   { get; set; }

        public Rules UserRules   { get; set;}
        public Rules DomainRules { get; set;}
        public Rules TitleRules   { get; set;}

        private String _Ordering;
        // this is the xml-serializable property, a string representation.
        // It's a comma separated string.
        public String Ordering
        {
            get
            {
                return _Ordering;
            }
            set
            {
                _Ordering = value;
                _orderedActions = null;
                // referencing OrderedActions causes it to be computed.
                var x = OrderedActions.ToString();
            }
        }

        private List<OrderableAction> _orderedActions;
        [XmlIgnore]
        public List<OrderableAction> OrderedActions
        {
            get
            {
                if (_orderedActions == null)
                {
                    // set the value, if unset. happens one time only.
                    _orderedActions = new List<OrderableAction>
                        (Array.ConvertAll
                         ( Ordering.Split(new Char[]{ ',',' ' },
                                          StringSplitOptions.RemoveEmptyEntries),
                           StringToOrderableAction));
                }
                NormalizeActions();
                return _orderedActions;
            }

            set
            {
                // set values all at once
                _orderedActions = new List<OrderableAction>();
                _orderedActions.AddRange(value);
                NormalizeActions();
            }
        }

        private static OrderableAction StringToOrderableAction(String s)
        {
            return (OrderableAction) Enum.Parse(typeof(OrderableAction), s, true);
        }

        // private static bool ContainsAllCandidatesOnce(List<OrderableAction> list1)
        // {
        //     return  list1.Intersect(candidates).Distinct().Any();
        //     return new List<OrderableAction>
        //         ((OrderableAction[])Enum.GetValues(typeof(OrderableAction)))
        //         .All(c => list1.Count(v => v == c) == 1);
        // }

        public static IEnumerable<OrderableAction> MissingCandidates(List<OrderableAction> list1)
        {
            var candidates = (OrderableAction[])(Enum.GetValues(typeof(OrderableAction)));
            return candidates.Except(list1);

            // return new List<OrderableAction>
            //     ((OrderableAction[])Enum.GetValues(typeof(OrderableAction)))
            //     .Where(c => list1.Count(v => v == c) != 1);
        }

        private static List<OrderableAction> AddMissing(List<OrderableAction> a)
        {
            // make sure all items are in the list.
            var missing = MissingCandidates(a);
            if (missing.Count() != 0) a.AddRange(missing);
            return a;
        }

        // public static List<String> AllActionsAsStrings()
        // {
        //     return new List<OrderableAction>
        //         ((OrderableAction[])Enum.GetValues(typeof(OrderableAction)))
        //         .Select(x => x.ToString())
        //         .ToList();
        // }

        private List<OrderableAction> NormalizeActions()
        {
            _orderedActions = _orderedActions.Distinct().ToList();
            AddMissing(_orderedActions);
            // set the string property that specifies ordering
            _Ordering = String.Join(",", _orderedActions.Select(s=> s.ToString()));
            return _orderedActions;
        }

        protected void SetDefaultOrdering()
        {
            if (this.UserRules == null)
            {
                this.UserRules = new Rules
                {
                    AllowAll = true,
                    DenyAll = false
                };
            }
            if (this.DomainRules == null)
            {
                this.DomainRules = new Rules
                {
                    AllowAll = true,
                    DenyAll = false
                };
            }
            if (this.TitleRules == null)
            {
                this.TitleRules = new Rules
                {
                    AllowAll = true,
                    DenyAll = false
                };
            }
            if (Ordering == null)
                Ordering =" AllowUsers, DenyUsers, AllowDomains, DenyDomains, AllowUpvotes, AllowTitleRegex, DenyTitleRegex, DenyAuthorMaturity, AllowAuthorMaturity ";

            var x = OrderedActions.ToString();
        }
    }


    public partial class AppSettings : QueueSettings
    {
        public static string StoragePath;
        public static Encoding utf8;

        static AppSettings()
        {
            string vendorName = "Dino Chiesa";
            string appName = "CleanModQueue";
            string vendorDir =  Path.Combine
                ( Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                  vendorName);
            if (!Directory.Exists(vendorDir))
                Directory.CreateDirectory(vendorDir);

            string appDir = Path.Combine(vendorDir, appName);
            if (!Directory.Exists(appDir))
                Directory.CreateDirectory(appDir);

            StoragePath = Path.Combine(appDir, "Config.xml" );

            // no BOM
            utf8 = new System.Text.UTF8Encoding(false);
        }

        private AppSettings()
        {
            // some defaults - set these prop values here; they may be
            // overwritten when they are present in the
            // to-be-deserialized stream.
            CheckInterval = 6 * 60 * 1000;
            UpvoteThreshold = 300;
            Enabled = true;
            SpamIsAsSpamDoes = true;
            OtherQueues = new List<QueueSettings>();
            AppSettings t = this;
            // default to IE
            List<string> possibles = new List<String> {
                "%ProgramFiles%\\Internet Explorer\\iexplore.exe",
                "%ProgramFiles(x86)%\\Internet Explorer\\iexplore.exe",
                "%windir%\\System32\\iexplore.exe"
            };

            possibles.ForEach(x => {
                if (t.PreferredBrowserExe==null)
                {
                    var s = Environment.ExpandEnvironmentVariables(x);
                    if (File.Exists(s))
                        t.PreferredBrowserExe = s;
                }
                });
        }

        protected internal AppSettings(string user,
                                       string password,
                                       CleanModQueue.Form1 form) : this()
        {
            RedditUser = user;
            RedditPwd = password;
            Parent = form;
        }

        [XmlIgnore] public bool WatchNewQueueSpecified;
        [XmlIgnore] public bool NameSpecified;
        [XmlIgnore] internal CleanModQueue.Form1 Parent  { get; set;}
        [XmlIgnore] private int[] geomp;

        [XmlArray]
        public List<QueueSettings> OtherQueues { get; set; }

        // [XmlElement("OtherQueues")]
        // public SerializableDictionary<String,QueueSettings> Queues
        // {
        //     get
        //     {
        //         return new SerializableDictionary<String,QueueSettings>(OtherQueues);
        //     }
        //     set
        //     {
        //         OtherQueues = value.ToDictionary();
        //     }
        // }

        /// <summary>
        ///   Whether to proactively report an author as a spammer, if
        ///   the user of this tool marks one of his posts explicitly as
        ///   spam.
        /// </summary>
        public bool SpamIsAsSpamDoes      { get;set; }

        /// <summary>
        ///   Whether to save the reddit password in plaintext in the
        ///   settings file.
        /// </summary>
        public bool SavePassword          { get;set; }

        /// <summary>
        ///   The reddit user name.
        /// </summary>
        public String RedditUser          { get;set; }

        public String RedditPwd           { get;set; }
        public String PreferredBrowserExe { get;set; }
        public int TitleWidth             { get;set; }
        public int CheckInterval          { get;set; }
        public int NumRemoved             { get;set; }
        public int Runs                   { get;set; }
        public string LastRun             { get;set; }

        public String Geometry
        {
            get
            {
                // get the size of the form
                int w = 0, h = 0, left = 0, top = 0, state = -1;
                if (Parent != null)
                {
                if (Parent.Bounds.Width < Parent.MinimumSize.Width ||
                    Parent.Bounds.Height < Parent.MinimumSize.Height)
                {
                    // RestoreBounds is the size of the window prior to
                    // last minimize action.  But the form may have been
                    // resized since then!
                    w = Parent.RestoreBounds.Width;
                    h = Parent.RestoreBounds.Height;
                    left = Parent.RestoreBounds.Location.X;
                    top = Parent.RestoreBounds.Location.Y;
                }
                else
                {
                    w = Parent.Bounds.Width;
                    h = Parent.Bounds.Height;
                    left = Parent.Location.X;
                    top = Parent.Location.Y;
                }
                }
                return String.Format("{0},{1},{2},{3},{4}",
                                     left, top, w, h, state);
            }

            set
            {
                String s = value;
                if (!String.IsNullOrEmpty(s))
                {
                    geomp = Array.ConvertAll<string, int>
                        (s.Split(','),
                         new Converter<string, int>((t) => { return Int32.Parse(t); }));
                    SetGeom();
                }
            }
        }

        private void CleanConflicts()
        {
            CleanConflictsFor(this);
            foreach (var item in this.OtherQueues)
            {
                CleanConflictsFor(item);
            }
        }

        private static void CleanConflictsFor(QueueSettings s)
        {
            var x1 = s.DeadUsers.Intersect(s.BlessedUsers);
            if (x1.Count()>0)
            {
                foreach (var x in x1)
                    s.DeadUsers.Remove(x);
            }
            // keep only unique items
            s.DeadUsers = s.DeadUsers.Distinct().ToList();
            s.BlessedUsers = s.BlessedUsers.Distinct().ToList();

            var x2 = s.SpamDomains.Intersect(s.BlessedDomains);
            if (x2.Count()>0)
            {
                foreach (var x in x2)
                    s.SpamDomains.Remove(x);
            }
            // keep only unique items
            s.SpamDomains = s.SpamDomains.Distinct().ToList();
            s.BlessedDomains = s.BlessedDomains.Distinct().ToList();
        }

        private void SetGeom()
        {
            if (Parent==null) return;
            if (geomp != null && geomp.Length == 5)
                Parent.Bounds = ConstrainToScreen(new System.Drawing.Rectangle(geomp[0], geomp[1], geomp[2], geomp[3]));

            if (TitleWidth>0)
            {
                Parent.TitleWidth = TitleWidth;
            }
        }

        private System.Drawing.Rectangle ConstrainToScreen(System.Drawing.Rectangle bounds)
        {
            Screen screen = Screen.FromRectangle(bounds);
            System.Drawing.Rectangle workingArea = screen.WorkingArea;
            int width = Math.Min(bounds.Width, workingArea.Width);
            int height = Math.Min(bounds.Height, workingArea.Height);
            // mmm....minimax
            int left = Math.Min(workingArea.Right - width, Math.Max(bounds.Left, workingArea.Left));
            int top = Math.Min(workingArea.Bottom - height, Math.Max(bounds.Top, workingArea.Top));
            return new System.Drawing.Rectangle(left, top, width, height);
        }

        public AppSettings Copy()
        {
            var s = this.AsXmlString();
            var clone = AppSettings.FromXmlString(s);
            return clone;
        }

        protected internal void SaveToStream(Stream stream)
        {
            var t = this.GetType();

            CleanConflicts();

            SpamDomains.Sort();
            DeadUsers.Sort();
            BlessedUsers.Sort();
            BlessedDomains.Sort();

            XmlSerializer ser = null;
            if (!this.SavePassword)
            {
                var o = new XmlAttributeOverrides();
                var attrs = new XmlAttributes();
                attrs.XmlIgnore = true;
                o.Add(t, "RedditPwd", attrs);
                ser = new XmlSerializer(t, o);
            }
            else
                ser = new XmlSerializer(this.GetType());

            using (var tw = new StreamWriter(stream, utf8))
            {
                var ns = new XmlSerializerNamespaces();
                ns.Add("", "");
                ser.Serialize(tw, this, ns);
            }
        }

        protected internal void Save()
        {
            this.Runs++;
            this.LastRun = System.DateTime.Now.ToString("yyyy MMM dd HH:mm:ss");
            using (var fs = new FileStream(AppSettings.StoragePath, FileMode.Create))
            {
                SaveToStream(fs);
            }
        }


        private String AsXmlString()
        {
            var ms = new MemoryStream();
            SaveToStream(ms);
            var s = utf8.GetString(ms.ToArray());
            return s;
        }

        private static AppSettings FromXmlString(String s)
        {
            return LoadFromStream(new MemoryStream(utf8.GetBytes(s)));
        }


        private static Stream GetSettingsStream()
        {
            if (File.Exists(AppSettings.StoragePath))
                return new FileStream(AppSettings.StoragePath, FileMode.Open);

            return
                new MemoryStream(utf8.GetBytes(AppSettings.defaultSettings));
        }

        private static AppSettings LoadFromStream(Stream stream)
        {
            return Load(stream, null);
        }

        protected internal static AppSettings Load(CleanModQueue.Form1 form)
        {
            using (var s = GetSettingsStream())
            {
                return Load(s, form);
            }
        }

        private static AppSettings Load(Stream stream, CleanModQueue.Form1 form)
        {
            var ser = new XmlSerializer(typeof(AppSettings));
            AppSettings settings = null;

            try
            {
                settings = (AppSettings) ser.Deserialize(stream);
                if (form != null)
                {
                    settings.Parent = form;
                    settings.SetGeom();
                }
                settings.CleanConflicts();
                settings.SetDefaultOrdering();
            }
            catch (Exception ex1)
            {
                MessageBox.Show("Cannot read settings. " + ex1.Message,
                                "CleanModQueue error");
                settings = null;
            }
            return settings;
        }

        private static string defaultSettings = @"
<AppSettings>
  <RedditUser />
  <RedditPwd />
  <SavePassword>true</SavePassword>
  <SpamIsAsSpamDoes>true</SpamIsAsSpamDoes>
  <UpvoteThreshold>300</UpvoteThreshold>
  <TitleWidth>440</TitleWidth>
  <CheckInterval>14</CheckInterval>
  <DeadUsers>
    <string>2daystuff</string>
    <string>AccuWeather</string>
    <string>adrinjohnston</string>
    <string>ajaysinghinyng</string>
    <string>AlbertFishEats</string>
    <string>alicilcail</string>
    <string>amir9ice</string>
    <string>anangryfellow</string>
    <string>annajanek</string>
    <string>annhanks5</string>
    <string>aormsbeeob</string>
    <string>ashinamita</string>
    <string>aymanbinmoshi</string>
    <string>basilvallb</string>
    <string>beerbouquet</string>
    <string>BobbyDelray</string>
    <string>bschubbehb</string>
    <string>cameronprovan</string>
    <string>canscaaccn</string>
    <string>caraheja</string>
    <string>caseygabrielsc</string>
    <string>chicagopinion</string>
    <string>clareoc2891</string>
    <string>ContentMaster</string>
    <string>cool_mind131</string>
    <string>coolfred8</string>
    <string>cpeakskpck</string>
    <string>d3adm3nwalk</string>
    <string>darr46oera</string>
    <string>dcollins2</string>
    <string>deadlinenewsscotland</string>
    <string>Deepizzaguy</string>
    <string>denissemarie</string>
    <string>dunp96y7</string>
    <string>elizabethnews</string>
    <string>emelzamlml</string>
    <string>evenmore</string>
    <string>ganezthak</string>
    <string>Gaviero</string>
    <string>gdarrell</string>
    <string>gloriousday</string>
    <string>GrethelMorgan</string>
    <string>harakersar</string>
    <string>healthathome</string>
    <string>heyscience</string>
    <string>iambruni</string>
    <string>JDStockman</string>
    <string>jeannecutiepie</string>
    <string>kasham83</string>
    <string>keved</string>
    <string>khushi1</string>
    <string>krishnanvenky</string>
    <string>lajmy0sy7</string>
    <string>LCNews1</string>
    <string>leosmithuk1</string>
    <string>lisadms</string>
    <string>lmpindia1</string>
    <string>Lozaratron</string>
    <string>MAFBUILDERS</string>
    <string>mahfuznasrin</string>
    <string>majarvaaam</string>
    <string>malikacarlan743</string>
    <string>masparrsrr</string>
    <string>mecush2ues</string>
    <string>merrgrmerr</string>
    <string>michaelfisherx3</string>
    <string>michaelkarlon1</string>
    <string>mobla</string>
    <string>Monobarrell</string>
    <string>mzprecha</string>
    <string>ne8de</string>
    <string>NewsTsar</string>
    <string>nfrankie812mez67l</string>
    <string>Niamkeblucy</string>
    <string>nick5bee38</string>
    <string>nioui04w</string>
    <string>odjhl</string>
    <string>patcarnx1</string>
    <string>pdct042</string>
    <string>pghtytfan</string>
    <string>proptigerindia</string>
    <string>RachelEatock</string>
    <string>rachelgurulong</string>
    <string>ravixpert</string>
    <string>rpeoni</string>
    <string>sahabatperawat</string>
    <string>SamyJones</string>
    <string>Shamusdon</string>
    <string>shawndacaa</string>
    <string>silveira</string>
    <string>sreee</string>
    <string>sweaterjon</string>
    <string>terrywo5</string>
    <string>TimmWatanabe</string>
    <string>ttru556klm57k</string>
    <string>tvgnus</string>
    <string>victorsmith</string>
    <string>wastefulquack24</string>
    <string>wbannan</string>
    <string>WinklerPost</string>
    <string>wjohnette</string>
    <string>zacharlawfirm</string>
  </DeadUsers>
  <SpamDomains>
    <string>bit.ly</string>
    <string>bitly.com</string>
    <string>bridgetondentist.net</string>
    <string>change.org</string>
    <string>dentistdupo.com</string>
    <string>dentistlakeworth.net</string>
    <string>i4u.com</string>
    <string>j.mp</string>
    <string>mrdrain.com</string>
    <string>t.co</string>
    <string>wp.me</string>
  </SpamDomains>
  <UserRules>
    <DenyAll>false</DenyAll>
    <AllowAll>false</AllowAll>
  </UserRules>
  <DomainRules>
    <DenyAll>false</DenyAll>
    <AllowAll>false</AllowAll>
  </DomainRules>
  <Ordering> AllowUsers, DenyUsers, AllowDomains, DenyDomains </Ordering>
  <Geometry>92,142,1220,419,0</Geometry>
</AppSettings>
";
    }

}
