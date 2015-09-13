// AuditLog.cs
// ------------------------------------------------------------------
//
// Part of CleanModQueue .
// A class providing a simple audit log for actions taken by the tool.
// The tool saves the log as an XML file in the "Roaming" directory.
//
// last saved: <2012-May-10 09:44:25>
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
using System.Text;

namespace Dino.Tools.Reddit.CleanModQueue
{
    public class AuditLog
    {
        public static string StoragePath;
        static AuditLog()
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

            StoragePath = Path.Combine(appDir, "AuditLog.xml" );
        }

        private AuditLog() { }

        public AuditRecord Add(string action, string itemId, Dino.Reddit.Data.OneItemData post)
        {
            var r = new AuditRecord(action, itemId, post);
            this.Records.Add(r);
            return r;
        }

        public string LastSaved   { get; set;}
        private List<AuditRecord> _records;

        [XmlArrayItem("record")]
        public List<AuditRecord> Records
        {
            get
            {
                if (_records==null)
                    _records = new List<AuditRecord>();
                return _records;
            }

            set
            {
                _records = value;
            }
        }

        protected internal void Save()
        {
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            var ser = new XmlSerializer(this.GetType());
            this.LastSaved = System.DateTime.Now.ToString("yyyy MMM dd HH:mm:ss");
            using (var fs = new FileStream(AuditLog.StoragePath, FileMode.Create))
            {
                using (var tw = new StreamWriter(fs, new UTF8Encoding()))
                {
                    ser.Serialize(tw, this, ns);
                }
            }
        }

        protected internal static AuditLog Load()
        {
            AuditLog log = null;
            if (File.Exists(AuditLog.StoragePath))
            {
                var ser = new XmlSerializer(typeof(AuditLog));
                using (var fs = new FileStream(AuditLog.StoragePath, FileMode.Open))
                {
                    log = (AuditLog) ser.Deserialize(fs);
                }
            }
            else
            {
                log = new AuditLog();
            }
            return log;
        }
    }

    public class AuditRecord
    {
        private AuditRecord() { }

        public AuditRecord(string action, string reason, Dino.Reddit.Data.OneItemData post)
        {
            this.Action = action;
            this.Reason = reason;
            this.ItemId = post.name;
            this.Reddit = post.subreddit;
            this.PostId = post.link_id;
            this.Author = post.author;
            this.Title = post.title;
            this.Url = post.url;
            this.Time = System.DateTime.Now.ToString("yyyy MMM dd HH:mm:ss");
        }

        [XmlAttribute("time")]
        public string Time  { get; set;}

        [XmlAttribute("id")]
        public string ItemId   { get; set;}

        [XmlAttribute("postid")]
        public string PostId   { get; set;}

        [XmlAttribute("subreddit")]
        public string Reddit   { get; set;}

        [XmlAttribute("author")]
        public string Author  { get; set;}

        [XmlElement("action")]
        public string Action  { get; set;}

        [XmlElement("reason")]
        public string Reason  { get; set;}

        [XmlElement("title")]
        public string Title  { get; set;}

        [XmlElement("url")]
        public string Url  { get; set;}
    }
}
