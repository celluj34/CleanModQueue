// AuditLogForm.cs
// ------------------------------------------------------------------
//
// Part of CleanModQueue .
// A Windows form to display the Audit log of the tool.
//
// last saved: <2012-May-12 13:17:16>
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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Dino.Tools.Reddit.CleanModQueue
{
    public partial class AuditLogForm : Form
    {
        private AuditLog Log;
        private String BrowserExe;
        private DataGridView.HitTestInfo hitTestInfo;

        public AuditLogForm(AuditLog log, string browserExe)
        {
            InitializeComponent();
            Log = log;
            BrowserExe = browserExe.Trim();
            this.dataGridView1.DataSource = log.Records;
            NoteSize();
        }


        private void Form_Load(object sender, EventArgs e)
        {
            // Add a context menu with a few items
            contextMenuStrip1.Items.Add("View User", null, new EventHandler(ViewUser));
            contextMenuStrip1.Items.Add("View Comments", null, new EventHandler(ViewComments));
            contextMenuStrip1.Items.Add("View Item", null, new EventHandler(ViewItem));

            SetRowHeaders();
        }


        void dataGridView1_MouseUp(object sender, MouseEventArgs e)
        {
            // Load context menu on right mouse click
            if (e.Button == MouseButtons.Right)
                hitTestInfo = dataGridView1.HitTest(e.X, e.Y);
        }

        private void SetRowHeaders()
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
                row.HeaderCell.Value = (row.Index+1).ToString();
            dataGridView1.AutoResizeRowHeadersWidth
                (DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders);
        }

        private void ViewUser(object sender, EventArgs e)
        {
            ViewUrl(2);
        }

        private void ViewItem(object sender, EventArgs e)
        {
            ViewUrl(1);
        }

        private void ViewComments(object sender, EventArgs e)
        {
            ViewUrl(3);
        }

        private string DesiredUrl(AuditRecord rec, int flavor)
        {
            if ((flavor == 1) && !String.IsNullOrEmpty(rec.Url))
            {
                return rec.Url;
            }

            if (flavor == 2)
                return Dino.Reddit.Client.GetAuthorUrl(rec.Author);

            if (flavor == 3)
                return Dino.Reddit.Client.GetItemUrl(rec.ItemId, rec.PostId, rec.Reddit);

            return null;
        }

        private void ViewUrl(int flavor)
        {
            // launch a browser if possible
            if (String.IsNullOrEmpty(BrowserExe)) return;
            if (!System.IO.File.Exists(BrowserExe)) return;
            if (hitTestInfo == null) return;
            if (hitTestInfo.Type != DataGridViewHitTestType.Cell) return;
            if (hitTestInfo.RowIndex < 0) return;

            var ds = (List<AuditRecord>)dataGridView1.DataSource;
            string url = DesiredUrl(ds[hitTestInfo.RowIndex], flavor);
            if (url!=null)
                System.Diagnostics.Process.Start(BrowserExe, url);
        }

        private void NoteSize()
        {
            if (System.IO.File.Exists(AuditLog.StoragePath))
            {
                var fi = new System.IO.FileInfo(AuditLog.StoragePath);
                if (fi.Length == 0)
                    this.lblSize.Text = "The audit log is empty.";
                else
                    this.lblSize.Text = "The audit log is about " +
                    fi.Length/1024 + " kb";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Log.Records.Clear();
            Log.Save();
            this.dataGridView1.DataSource = null;
            this.dataGridView1.DataSource = Log.Records;
            NoteSize();
        }

    }
}
