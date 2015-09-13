// Form1.cs
// ------------------------------------------------------------------
//
// Part of CleanModQueue .
// This is the main form for the tool, which is a Windows Forms app.
//
// last saved: <2012-July-31 21:41:07>
// ------------------------------------------------------------------
//
// Copyright (c) 2012 Dino Chiesa
// All rights reserved.
//
// This code is Licensed under the New BSD license.
// See the License.txt file that accompanies this module.
//
// ------------------------------------------------------------------

//#define DEVELOPMT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using Dino.Reddit;

namespace Dino.Tools.Reddit.CleanModQueue
{
    public partial class Form1 : Form
    {
        int ticks;
        int nRemoved;
        int timerCyclesRemaining;
        AppSettings settings;
        AuditLog log;
        Dino.Reddit.Client reddit;
        string baseFormTitle;
        // MenuItem viewcommentsItem;
        Timer timer1;
        bool checkedForUpdates = false;
        DataGridView.HitTestInfo hitTestInfo;
        System.Windows.Forms.TabPage templatePage;

        public Form1()
        {
            InitializeComponent();

            this.lblStatus.Text = "ready.";

            // In the VS designer, I have a tabPage on the control
            // containing a datagridview with the appropriate columns,
            // column ordering, header text and so on.  That DGV is used
            // as a template for any DGVs that are dynamically added at
            // runtime.
            templatePage = this.tabControl1.Controls[0] as TabPage;
            this.tabControl1.Controls.Clear();

            baseFormTitle = "CleanModQueue v" +
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            this.Text = baseFormTitle;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Add a few items to the (right click) context menu.
            // Probably makes more sense to do this in the designer.
            contextMenuStrip1.Items.Add("Mark Domain as Spam", null, new EventHandler(BlockDomain));
            contextMenuStrip1.Items.Add("un-Mark Domain as Spam", null, new EventHandler(UnBlockDomain));
            contextMenuStrip1.Items.Add("Bless Domain", null, new EventHandler(BlessDomain));
            contextMenuStrip1.Items.Add("un-Bless Domain", null, new EventHandler(UnBlessDomain));
            contextMenuStrip1.Items.Add(new System.Windows.Forms.ToolStripSeparator());
            contextMenuStrip1.Items.Add("Mark User as Spammer", null, new EventHandler(BlockUser));
            contextMenuStrip1.Items.Add("un-Mark User as Spammer", null, new EventHandler(UnBlockUser));
            contextMenuStrip1.Items.Add("Bless User", null, new EventHandler(BlessUser));
            contextMenuStrip1.Items.Add("un-Bless User", null, new EventHandler(UnBlessUser));
            contextMenuStrip1.Items.Add("View User", null, new EventHandler(ViewUser));
            contextMenuStrip1.Items.Add(new System.Windows.Forms.ToolStripSeparator());
            contextMenuStrip1.Items.Add("View Comments", null, new EventHandler(ViewComments));
            contextMenuStrip1.Items.Add("View Link", null, new EventHandler(ViewLink));

            log = AuditLog.Load();
            settings = AppSettings.Load(this);

            reddit = new Dino.Reddit.Client(1001);
            reddit.Progress += reddit_Progress;

            DisableButtons();
            // kick off
            ResetTimer();
        }

        private void MaybeLoginAndGo()
        {
            if (String.IsNullOrEmpty(settings.RedditUser) ||
                String.IsNullOrEmpty(settings.RedditPwd))
            {
                MessageBox.Show("You must provide credentials to login to reddit.",
                                "Need Credentials.");
            }
            else if (reddit.Login(settings.RedditUser, settings.RedditPwd))
            {
#if DEVELOPMT
                // When in development, login, but then don't retrieve
                // queue contents, and don't immediately begin
                // processing the queue.
                ticks++;
                timerCyclesRemaining = 350;
#endif
                if (ticks != 0)
                    EnableButtons();
                timer1.Start();
            }
            else
            {
                MessageBox.Show("Could not login to reddit.", "Login Failed.");
                this.btnSettings.Enabled = true;
                // TODO: Retry
            }
        }

        private void ResetTimer()
        {
            if (timer1 != null) timer1.Stop();
            timer1 = new Timer();
            timer1.Tick += new EventHandler(TimerTick);
            timer1.Interval = 1000;
            timerCyclesRemaining = 1;
            timer1.Start();
        }

        private void DisableButtons()
        {
            this.btnCheck.Enabled = false;
            this.btnProcess.Enabled = false;
            this.btnSettings.Enabled = false;
            this.button3.Enabled = false;
            this.button4.Enabled = false;
            this.tabControl1.Enabled = false;
        }

        private void EnableButtons()
        {
            this.btnCheck.Enabled = true;
            this.btnProcess.Enabled = true;
            this.btnSettings.Enabled = true;
            this.button3.Enabled = true;
            this.button4.Enabled = true;
            this.tabControl1.Enabled = true;
        }

        private ToolStripItem GetCtxMenuItem(string text)
        {
            var selection =
            from ToolStripItem item in this.contextMenuStrip1.Items
                where item.Text == text
                select item;

            if (selection.Count() > 0)
                return selection.First();

            return null;
        }

        void cms_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // The ContextMenuStrip opens.
            // Dynamically enable or disable some of the items in the context menu.

            var cms = sender as ContextMenuStrip;
            var dgv = cms.SourceControl as DataGridView;
            if (dgv == null) return;
            var position = Cursor.Position;
            System.Drawing.Point p = dgv.PointToClient(position);
            hitTestInfo = dgv.HitTest(p.X, p.Y);
            var row = GetRightClickRow();
            if (row < 0) return ;

            var ds = (List<Dino.Reddit.Data.OneItemData>) dgv.DataSource;
            Dino.Reddit.Data.OneItemData post = ds[row];
            TabPage page = dgv.Parent as TabPage;
            QueueSettings qs = page.Tag as QueueSettings;
            var lowDomain = !String.IsNullOrEmpty(post.domain)
                ? post.domain.ToLower() : null;
            var isDomainSpam = (lowDomain != null)
                ? qs.SpamDomains.Any(x => lowDomain.Equals(x)) : false;
            var isDomainBlessed = (lowDomain != null)
                ? qs.BlessedDomains.Any(x => lowDomain.Equals(x)) : false;

            var isUserBlessed = !String.IsNullOrEmpty(post.author)
                ? qs.BlessedUsers.Any(x => post.author.Equals(x)) : false;
            var isUserSpammer = !String.IsNullOrEmpty(post.author)
                ? qs.DeadUsers.Any(x => post.author.Equals(x)) : false;

            var conditionallyEnableItem = new Action<string, bool>( (text, f) => {
                    var item = GetCtxMenuItem(text);
                    if (item != null)
                        item.Enabled = f;
                });

            // Allow "View Comments" even if the post is not a comment;
            // In some cases the mod wants to view and comment directly.
            //
            // conditionallyEnableItem("View Comments",
            //                         !String.IsNullOrEmpty(post.name) &&
            //                         post.name.StartsWith("t1_"));
            conditionallyEnableItem("View Link",
                                    !String.IsNullOrEmpty(post.name) &&
                                    post.name.StartsWith("t3_"));
            conditionallyEnableItem("Mark Domain as Spam",
                                    !String.IsNullOrEmpty(post.domain) &&
                                    !isDomainSpam);
            conditionallyEnableItem("un-Mark Domain as Spam",
                                    !String.IsNullOrEmpty(post.domain) &&
                                    isDomainSpam);
            conditionallyEnableItem("Bless Domain",
                                    !String.IsNullOrEmpty(post.domain) &&
                                    !isDomainBlessed);
            conditionallyEnableItem("un-Bless Domain",
                                    !String.IsNullOrEmpty(post.domain) &&
                                    isDomainBlessed);
            conditionallyEnableItem("Bless User",
                                    !String.IsNullOrEmpty(post.author) &&
                                    !isUserBlessed);
            conditionallyEnableItem("un-Bless User",
                                    !String.IsNullOrEmpty(post.author) &&
                                    isUserBlessed);
            conditionallyEnableItem("Mark User as Spammer",
                                    !String.IsNullOrEmpty(post.author) &&
                                    !isUserSpammer);
            conditionallyEnableItem("un-Mark User as Spammer",
                                    !String.IsNullOrEmpty(post.author) &&
                                    isUserSpammer);
        }


        private void TimerTick(Object obj, EventArgs e)
        {
            // The timer ticks once per second, and this event handles the
            // tick.
            //
            // During steady operation, the event handler either updates a
            // label showing the time remaining until re-scan, or it actually
            // performs the re-scan.
            //
            // In the beginning, this event will do this:
            //
            // First, once, check for updates.
            //
            // After that, before the user is logged in, this event will
            // login, then begin monitoring (MaybeLoginAndGo). There's a
            // special case for the situation before the user has configured a
            // password and userid - in that case the tool cannot login, so it
            // pops the settings form and asks the user for the required info.

            if (!checkedForUpdates)
            {
                timer1.Stop();
                string feedUrl = String.Format("https://{0}.svn.codeplex.com/svn/{0}/versions.xml", "CleanModQueue");
                // Get the version specified in AssemblyInfo.cs
                var currentVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                var r = Form1.CheckForUpdates(feedUrl, currentVersion);
                if (r.UpdateAvailable)
                {
                    MessageBox.Show(r.Message, "There is an update available for CleanModQueue");
                }
                checkedForUpdates = true;
                timer1.Start();
            }

            if (!reddit.LoggedIn)
            {
                timer1.Stop();
                if (settings == null)
                {
                    // there was a problem reading the file.
                    settings = new AppSettings(null,null,this);
                }
                if (String.IsNullOrEmpty(settings.RedditUser) ||
                    String.IsNullOrEmpty(settings.RedditPwd))
                {
                    // pop settings box
                    btnSettings_Click(null, null);
                }

                MaybeLoginAndGo();
                return;
            }

            timerCyclesRemaining--;
            if (timerCyclesRemaining > 0)
            {
                this.Cursor = Cursors.Default;
                this.lblStatus.Text = "Re-scan in " +
                    new TimeSpan(0, 0, timerCyclesRemaining).ToString();
            }
            else
            {
                this.timer1.Stop();
                this.Cursor = Cursors.WaitCursor;
                DisableButtons();
                this.Update();
                ticks++;
                timerCyclesRemaining = settings.CheckInterval * 60; // reset
                try
                {
                    ProcessAllQueues();
                }
                finally
                {
                    this.Cursor = Cursors.Default;
                    EnableButtons();
                    this.Update();
                    timer1.Start();
                }
            }
        }


        private static string Tidy(String s)
        {
            string t = s.Replace('\r', '|').Replace('\n', '|');
            return t;
        }

        private static string TidyBody(String s)
        {
            if (s == null) return "Comment: ??";
            int len = s.Length;
            var v = "Comment: " +
                    ((len < 80)
                     ? Tidy(s)
                     : (Tidy(s.Substring(0, 78)) + "..."));
            return v;
        }

        private System.Windows.Forms.TabPage NewTabPage(string name)
        {
            // Create a new tab page, with a new DataGridView in it.
            // There is one per reddit queue that the user monitors.
            var c = this.tabControl1.Controls.Count;
            var page = new System.Windows.Forms.TabPage
            {
                Location = new System.Drawing.Point(4, 22),
                Name = name,
                Padding = new System.Windows.Forms.Padding(3),
                Size = new System.Drawing.Size(554, 202),
                TabIndex = c+1,
                Text = name,
                UseVisualStyleBackColor = true
            };

            var dgv0 = templatePage.Controls[0] as DataGridView;
            var dgv = new System.Windows.Forms.DataGridView
            {
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoGenerateColumns = false,
                ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                ContextMenuStrip = this.contextMenuStrip1,
                // DataSource = this.bindingSource1,
                Dock = System.Windows.Forms.DockStyle.Fill,
                Location = new System.Drawing.Point(3, 3),
                Name = "dgv" + name,
                RowHeadersWidth = 24,
                SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect,
                Size = new System.Drawing.Size(548, 196),
                TabIndex = 1
            };
            foreach(DataGridViewColumn col in dgv0.Columns)
            {
                var newcol = (DataGridViewColumn) col.Clone();
                dgv.Columns.Add(newcol);
            }

            dgv.CellFormatting += dataGridView_CellFormatting;
            dgv.CurrentCellDirtyStateChanged += dataGridView_CurrentCellDirtyStateChanged;
            dgv.CellValueChanged += dataGridView_CellValueChanged;
            dgv.MouseUp += dataGridView_MouseUp;

            page.Controls.Add(dgv);
            this.tabControl1.Controls.Add(page);
            return page;
        }

        static IEnumerable<QueueSettings> GetAllQueues(AppSettings appSettings)
        {
            // This method just returns an IEnumerable (a list, basically) of
            // the configured queues. Using the settings form, the user
            // configures the list of queues to monitor, with settings for
            // each one.
            //
            // The implementation of this method reflects the evolution of the
            // tool. The tool originally monitored only one queue: the
            // aggregeate "modqueue".  I later added the ability to monitor
            // other queues, and this required a change in the xml schema for
            // the settings data. This is why this method creates a new list
            // and then does a LINQ query on it - because the settings schema
            // requires that.
            var list = new List<QueueSettings>();
            list.Add(appSettings as QueueSettings);
            foreach(var item in appSettings.OtherQueues)
                list.Add(item);

            return list.Where(x => x.Enabled);
        }

        TabPage GetTabPageByName(string tabName)
        {
            // Get the TabPage with the given name, or create and name a new
            // one, if not present. At runtime, there will be one TabPage per
            // queue being monitored.
            var t = from p in this.tabControl1.Controls.OfType<TabPage>()
                where p.Text == tabName
                select p;

            var c = t.Count();
            System.Windows.Forms.TabPage page = null;
            if (c == 0)
            {
                page = NewTabPage(tabName);
            }
            else if (c == 1)
            {
                page = (System.Windows.Forms.TabPage) t.First();
            }

            return page;
        }

        private void RetagSettings(AppSettings copy)
        {
            var list = GetAllQueues(copy);
            foreach(var qs in list)
            {
                var tabName = qs.GetTabName();
                var page = GetTabPageByName(tabName);
                if (page != null)
                    page.Tag = qs;
            }
        }

        private void ProcessAllQueues()
        {
            var list = GetAllQueues(this.settings);
            foreach(var qs in list)
            {
                var tabName = qs.GetTabName();

                // There is a separate list of "dead users" or "dead authors"
                // for each queue.  This isn't optimal - for example if the
                // user monitors the mod queue and the new queue of the same
                // reddit, there will be two distinct lists of dead
                // users. But, it's probably the right thing to not have a
                // single list of dead authors for all reddits.  A particular
                // author might be welcome on one reddit but not on another.
                reddit.ExternalDeadList = qs.DeadUsers;
                reddit.MarkUsersDead(qs.DeadUsers);

                var page = GetTabPageByName(tabName);
                if (page != null)
                {
                    page.Tag = qs;
                    if (ticks == 1)
                    {
                        // first time only
                        RefreshList(page);
                        ProcessListEx(page, true);
                    }
                    else
                    {
                        // Process the list of posts in this tab, twice. In
                        // the first pass, handle any checkboxes, and any
                        // posts that are determined to be spam / blessed
                        // according to the rules. ProcessListEx then
                        // refreshes the list after completion. The second
                        // pass removes any new spam posts, and approves any
                        // blessed posts, according to the rules configured
                        // for that queue. The result on the tab shows posts
                        // that aren't handled by the configured rules.  If
                        // displaying the mod queue, these posts require
                        // direct attention.
                        ProcessListEx(page, false);
                        ProcessListEx(page, true);
                    }
                }
            }
        }


        private Dino.Reddit.Data.OneItemData PostAtCurrentRow(DataGridView dgv, DataGridViewCellFormattingEventArgs e)
        {
            var ds = (List<Dino.Reddit.Data.OneItemData>) dgv.DataSource;
            Dino.Reddit.Data.OneItemData post = ds[e.RowIndex];
            return post;
        }

        private DataGridViewCellStyle _SpamStyle;
        private DataGridViewCellStyle SpamStyle
        {
            get
            {
                if (_SpamStyle == null)
                {
                    _SpamStyle = new DataGridViewCellStyle
                    {
                        BackColor = System.Drawing.Color.FromArgb(240,105,159),
                        Font = dataGridView1.Font,
                        Alignment = DataGridViewContentAlignment.MiddleLeft
                    };
                }
                return _SpamStyle;
            }
        }

        private DataGridViewCellStyle _BlessedStyle;
        private DataGridViewCellStyle BlessedStyle
        {
            get
            {
                if (_BlessedStyle == null)
                {
                    _BlessedStyle = new DataGridViewCellStyle
                    {
                        BackColor = System.Drawing.Color.FromArgb(77,240,109),
                        Font = dataGridView1.Font,
                        Alignment = DataGridViewContentAlignment.MiddleLeft
                    };
                }
                return _BlessedStyle;
            }
        }


        private void dataGridView_CellFormatting(object sender,
                                                 DataGridViewCellFormattingEventArgs e)
        {
            // Format the contents and style of cells in the datagridview.
            // This marks posts in magenta if the rules say they are spam,
            // or green if the rules say they are blessed. Also some
            // other display tweaks.
            var dgv = sender as DataGridView;
            TabPage page = dgv.Parent as TabPage;
            QueueSettings qs = page.Tag as QueueSettings;

            if (e.ColumnIndex == 0)
            {
                e.FormattingApplied = true;
                e.Value = string.Format("{0}", e.RowIndex + 1);
                return;
            }

            if (e.ColumnIndex == 4) // domain
            {
                if (e.Value == null) return;
                var domain = e.Value as String;
                if (String.IsNullOrEmpty(domain)) return;
                string d = domain.ToLower();
                if (qs.SpamDomains.Any(x => (d.Equals(x) || d.EndsWith("." + x))))
                    e.CellStyle = SpamStyle;

                else if (qs.BlessedDomains.Any(x => (d.Equals(x) || d.EndsWith("." + x))))
                    e.CellStyle = BlessedStyle;

                return;
            }

            if (e.ColumnIndex == 7) // title
            {
                var post = PostAtCurrentRow(dgv,e);
                if (post.name.StartsWith("t1_"))
                {
                    // the item is a comment...
                    e.Value = "--Comment--";
                    e.FormattingApplied = true;
                }
                else
                {
                    var title = post.title;
                    if (TitleMatchAnyRegex(qs.SpamTitleRegexi, title))
                        e.CellStyle = SpamStyle;

                    else if (TitleMatchAnyRegex(qs.BlessedTitleRegexi, title))
                        e.CellStyle = BlessedStyle;
                }
                return;
            }

            if (e.ColumnIndex == 8) // author
            {
                if (e.Value == null) return;
                var author = e.Value as String;
                if (String.IsNullOrEmpty(author)) return;
                if (qs.DeadUsers.Contains(author))
                    e.CellStyle = SpamStyle;
                else if (qs.BlessedUsers.Contains(author))
                    e.CellStyle = BlessedStyle;
                return;
            }

            if (e.ColumnIndex == 16) // selftext
            {
                var post = PostAtCurrentRow(dgv,e);
                if (post.name.StartsWith("t1_"))
                {
                    // it is a comment...
                    e.Value = TidyBody(post.body);
                    e.FormattingApplied = true;
                }
                return;
            }
        }

        // The DataGridView.CellValueChanged event occurs when the
        // user-specified value is committed, which typically occurs
        // when focus leaves the cell. In the case of check box cells,
        // however, you will typically want to handle the change
        // immediately. To commit the change when the cell is clicked,
        // you must handle the DataGridView.CurrentCellDirtyStateChanged
        // event. In the handler, if the current cell is a check box
        // cell, call the DataGridView.CommitEdit method and pass in the
        // Commit value.

        void dataGridView_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            var dgv = sender as DataGridView;

            // Manually raise the CellValueChanged event
            // by calling the CommitEdit method.
            if (dgv.IsCurrentCellDirty)
            {
                dgv.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        public void dataGridView_CellValueChanged(object sender,
                                                  DataGridViewCellEventArgs e)
        {
            var dgv = sender as DataGridView;
            // If a check box cell is clicked, this event handler sets the value
            // of a few other checkboxes in the same row as the clicked cell.
            if (e.RowIndex < 0) return; // sometimes negative?
            int ix = e.ColumnIndex;
            if (ix >= 1 && ix <= 3)
            {
                var row = dgv.Rows[e.RowIndex];

                DataGridViewCheckBoxCell checkCell =
                    (DataGridViewCheckBoxCell)row.Cells[ix];

                bool isChecked = (Boolean)checkCell.Value;
                if (isChecked)
                {
                    for (int i = 1; i <= 3; i++)
                    {
                        if (i != ix)
                        {
                            ((DataGridViewCheckBoxCell)row.Cells[i]).Value = false;
                        }
                    }
                }
                dgv.Invalidate();
            }
        }

        void dataGridView_MouseUp(object sender, MouseEventArgs e)
        {
            var dgv = sender as DataGridView;
            // Load context menu on right mouse click
            if (e.Button == MouseButtons.Right)
            {
                hitTestInfo = dgv.HitTest(e.X, e.Y);
            }
        }

        private void UpdateStatus(string msg)
        {
            this.lblStatus.Text = msg;
            this.Update();
        }

        private void reddit_Progress(object sender, Client.ProgressArgs e)
        {
            // The reddit client has a built-in throttle, in order to comply
            // with the requirement by reddit that client apps should not send
            // more than 30 requests in a minute. Therefore, a GUI application
            // may exhibit a delay when performing a multi-step action like
            // "Maybe report user as spammer".  That action may require 3
            // distinct HTTP messages to reddit, and they cannot be batched as
            // far as I know.  The Reddit.Client class exposes a progress
            // event to allow it to tell callers what is happening, on a
            // coarse-grained basis.  This is the handler for that progress
            // event.
            switch (e.Activity)
            {
                case Client.Activity.Login:
                    if (e.Stage == Client.Stage.Before)
                        UpdateStatus("Login...");
                    else if (e.Stage == Client.Stage.After)
                    {
                        if (e.Message != null)
                            UpdateStatus("Logged in.");
                        else
                            UpdateStatus("Login failed.");
                    }
                    break;
                case Client.Activity.Logout:
                    break;
                case Client.Activity.RemoveItem:
                    if (e.Stage == Client.Stage.Before)
                        UpdateStatus("Removing " + e.Message);
                    else
                    {
                        this.Text = baseFormTitle + " | " + (++nRemoved);
                        this.Update();
                    }
                    break;
                case Client.Activity.ApproveItem:
                    if (e.Stage == Client.Stage.Before)
                        UpdateStatus("Approving " + e.Message);
                    break;
                case Client.Activity.AboutUser:
                    if (e.Stage == Client.Stage.Before)
                        UpdateStatus("checking user: " + e.Message);
                    break;
                case Client.Activity.GetPostHistory:
                    if (e.Stage == Client.Stage.Before)
                        UpdateStatus("checking user: " + e.Message);
                    break;
                case Client.Activity.GetMods:
                    break;
                case Client.Activity.GetModQueue:
                    if (e.Stage == Client.Stage.Before)
                        UpdateStatus("Get mod queue...");
                    break;
                case Client.Activity.GetNewQueue:
                    if (e.Stage == Client.Stage.Before)
                        UpdateStatus("Get new queue...");
                    break;
                case Client.Activity.ReportSpammer:
                    if (e.Stage == Client.Stage.Before)
                        UpdateStatus("Report as spammer: " + e.Message);
                    break;
            }
        }

        private void LookingAt(int c, Dino.Reddit.Data.OneItemData post, DataGridView dgv)
        {
            UpdateStatus("looking at: " + Client.CleanTitle(post));
            dgv.CurrentCell = dgv.Rows[c].Cells[0];
        }

        private bool PostFromSpamDomain(Dino.Reddit.Data.OneItemData post, QueueSettings qs)
        {
            string domain = post.domain;
            if (domain == null) return false;
            string d = domain.ToLower();
            return qs.SpamDomains.Any(x => (d.Equals(x) || d.EndsWith("." + x)));
        }

        private bool TitleMatchAnyRegex(List<String> regexi, String title)
        {
            var r = regexi.Any(regex => {
                    // TODO: cache this in a dict within QueueSettings
                    var re1 = new Regex(regex);
                    var m = re1.Match(title);
                    return m.Success;
                });
            return r;
        }

        private bool PostHasSpamTitle(Dino.Reddit.Data.OneItemData post, QueueSettings qs)
        {
            string title = post.title;
            if (String.IsNullOrEmpty(title)) return false;
            return TitleMatchAnyRegex(qs.SpamTitleRegexi, title);
        }

        private bool PostHasBlessedTitle(Dino.Reddit.Data.OneItemData post, QueueSettings qs)
        {
            string title = post.title;
            if (String.IsNullOrEmpty(title)) return false;
            return TitleMatchAnyRegex(qs.BlessedTitleRegexi, title);
        }

        private bool PostFromBlessedDomain(Dino.Reddit.Data.OneItemData post, QueueSettings qs)
        {
            string domain = post.domain;
            if (domain == null) return false;
            string d = domain.ToLower();
            return qs.BlessedDomains.Any(x => (d.Equals(x) || d.EndsWith("." + x)));
        }

        public bool IsUserBlessed(string username, QueueSettings qs)
        {
            if (username == null) return false;
            return qs.BlessedUsers.Any(x => username.Equals(x));
        }

        public bool IsUserBlocked(string username, QueueSettings qs)
        {
            if (username == null) return false;
            return qs.DeadUsers.Any(x => username.Equals(x));
        }

        public bool IsUserImmature(string username, QueueSettings qs)
        {
            if (username == null) return false;

            // optimize this case
            if (qs.SpamAuthMaturity.Age == 0 &&
                qs.SpamAuthMaturity.LinkKarma == 0 &&
                qs.SpamAuthMaturity.CommentKarma == 0)
                return false;

            var acct = reddit.GetAccount(username);
            if (acct == null) return false;

            if (qs.SpamAuthMaturity.Age > 0) {
                var ageInDays = (DateTime.UtcNow - acct.data.createdUtcTime).Days;
                if (ageInDays < qs.SpamAuthMaturity.Age) return true;
            }

            if (qs.SpamAuthMaturity.LinkKarma > 0  &&
                acct.data.link_karma < qs.SpamAuthMaturity.LinkKarma) return true;

            if (qs.SpamAuthMaturity.CommentKarma > 0 &&
                acct.data.comment_karma < qs.SpamAuthMaturity.CommentKarma) return true;

            return false;
        }

        public bool IsUserMature(string username, QueueSettings qs)
        {
            if (username == null) return false;
            // optimize this case
            if (qs.BlessedAuthMaturity.Age == -1 &&
                qs.BlessedAuthMaturity.LinkKarma == -1 &&
                qs.BlessedAuthMaturity.CommentKarma == -1)
                return false;

            var acct = reddit.GetAccount(username);
            if (acct == null) return false;

            if (qs.BlessedAuthMaturity.Age > -1) {
                var ageInDays = (DateTime.UtcNow - acct.data.createdUtcTime).Days;
                if (ageInDays > qs.BlessedAuthMaturity.Age) return true;
            }

            if (qs.BlessedAuthMaturity.LinkKarma > -1 &&
                acct.data.link_karma > qs.BlessedAuthMaturity.LinkKarma) return true;

            if (qs.BlessedAuthMaturity.CommentKarma > -1 &&
                acct.data.comment_karma > qs.BlessedAuthMaturity.CommentKarma) return true;

            return false;
        }


        private void ProcessStage1(int c, Dino.Reddit.Data.OneItemData post, DataGridView dgv)
        {
            string action = null, reason = null;

            LookingAt(c, post, dgv);

            TabPage page = dgv.Parent as TabPage;
            QueueSettings qs = page.Tag as QueueSettings;

            try
            {
                // TODO: allow operator to specify boilerplate messages to
                // be posted when posts are removed by rule.
                if (post.markRemove)
                {
                    action = "remove";
                    reason = "checked";
                    reddit.RemovePost(post, notSpam: true);
                }
                else if (post.markSpam)
                {
                    action = "spam";
                    reason = "checked";
                    reddit.RemovePost(post, notSpam: false);
                    if (settings.SpamIsAsSpamDoes)
                        reddit.MaybeReportUserAsSpammer(post.author);
                }
                else if (post.markApprove)
                {
                    action = "approve";
                    reason = "checked";
                    reddit.ApprovePost(post);
                }

                else
                {
                    // Go through the rules progression, in the order
                    // specified by the user. When adding support for a new
                    // type of rule, you would need to insert some logic
                    // here.
                    foreach (var rule in qs.OrderedActions)
                    {
                        if (!post.handled)
                        {
                            switch (rule)
                            {
                                case OrderableAction.AllowUsers:
                                    if (qs.UserRules.AllowAll)
                                    {
                                        if (qs.IsaModQueue())
                                        {
                                            // Explicitly approve only if mod queue.
                                            // For new queue, no action necessary.
                                            action = "approve";
                                            reason = "* user";
                                            reddit.ApprovePost(post);
                                        }
                                    }
                                    else if (IsUserBlessed(post.author,qs))
                                    {
                                        if (qs.IsaModQueue())
                                        {
                                            action = "approve";
                                            reason = "blessed user";
                                            reddit.ApprovePost(post);
                                        }
                                    }
                                    break;
                                case OrderableAction.DenyUsers:
                                    if (qs.UserRules.DenyAll)
                                    {
                                        action = "remove";
                                        reason = "* user";
                                        reddit.RemovePost(post, true);
                                    }
                                    else if (IsUserBlocked(post.author, qs))
                                    {
                                        action = "remove";
                                        reason = "bad user";
                                        reddit.RemovePost(post, true);
                                    }
                                    break;
                                case OrderableAction.AllowDomains:
                                    if (!qs.IsaModQueue()) break;

                                    if (qs.DomainRules.AllowAll)
                                    {
                                        action = "approve";
                                        reason = "* domain";
                                        reddit.ApprovePost(post);
                                    }
                                    else if (PostFromBlessedDomain(post, qs))
                                    {
                                        action = "approve";
                                        reason = "blessed domain";
                                        reddit.ApprovePost(post);
                                    }
                                    break;
                                case OrderableAction.DenyDomains:
                                    if (qs.DomainRules.DenyAll)
                                    {
                                        action = "remove";
                                        reason = "* domain";
                                        reddit.RemovePost(post, true);
                                    }
                                    else if (PostFromSpamDomain(post, qs))
                                    {
                                        action = "spam";
                                        reason = "bad domain";
                                        reddit.RemovePost(post, false);
                                        reddit.MaybeReportUserAsSpammer(post.author);
                                    }
                                    break;
                                case OrderableAction.AllowUpvotes:
                                    if (qs.IsaModQueue() &&
                                        (qs.UpvoteThreshold > 0 && post.ups > qs.UpvoteThreshold))
                                    {
                                        action = "approve";
                                        reason = "upvotes";
                                        reddit.ApprovePost(post);
                                    }
                                    break;
                                case OrderableAction.DenyTitleRegex:
                                    if (PostHasSpamTitle(post,qs))
                                    {
                                        action = "spam";
                                        reason = "bad title";
                                        reddit.RemovePost(post, false);
                                        reddit.MaybeReportUserAsSpammer(post.author);
                                    }
                                    break;
                                case OrderableAction.AllowTitleRegex:
                                    if (qs.IsaModQueue() &&
                                        PostHasBlessedTitle(post,qs))
                                    {
                                        action = "approve";
                                        reason = "blessed title";
                                        reddit.ApprovePost(post);
                                    }
                                    break;
                                case OrderableAction.DenyAuthorMaturity:
                                    if (IsUserImmature(post.author,qs))
                                    {
                                        action = "remove";
                                        reason = "immature user";
                                        reddit.RemovePost(post);
                                    }
                                    break;
                                case OrderableAction.AllowAuthorMaturity:
                                    if (qs.IsaModQueue() &&
                                        IsUserMature(post.author,qs))
                                    {
                                        action = "approve";
                                        reason = "mature user";
                                        reddit.ApprovePost(post);
                                    }
                                    break;
                            }
                        }
                    }
                }

            }
            catch (System.Exception /* exc1 */)
            {
                // Gulp!
                //
                // Sometimes there are exceptions in the http layer.  This
                // catch just silently swallows them. We'll try again for this
                // post, next time around.
            }

            if (action != null)
                log.Add(action, reason, post);
        }

        private void ProcessStage2(int c,
                                   Dino.Reddit.Data.OneItemData post,
                                   DataGridView dgv)
        {
            LookingAt(c,post,dgv);

            if (reddit.IsUserDead(post.author))
            {
                reddit.RemovePost(post, true);
                log.Add("remove", "dead user", post);
            }
            else if (reddit.IsUserMod(post.author, post.subreddit))
            {
                TabPage page = dgv.Parent as TabPage;
                QueueSettings qs = page.Tag as QueueSettings;
                if (qs.IsaModQueue())
                {
                    reddit.ApprovePost(post);
                    log.Add("approve", "mod user", post);
                }
            }
        }

        private int GetRightClickRow()
        {
            // What row has the user clicked on?
            // hitTestInfo is set in MouseUp, which has fired previously,
            // and independently.
            if (hitTestInfo == null) return -1;
            if (hitTestInfo.Type != DataGridViewHitTestType.Cell) return -1;
            return hitTestInfo.RowIndex;
        }


        private void AddRemoveThingInList(List<String> list,
                                          Func<Dino.Reddit.Data.OneItemData,string> cb,
                                          bool wantAdd)
        {
            var row = GetRightClickRow();
            if (row < 0) return;

            var page = this.tabControl1.SelectedTab;
            var dgv  = page.Controls[0] as DataGridView;

            var ds = (List<Dino.Reddit.Data.OneItemData>) dgv.DataSource;
            Dino.Reddit.Data.OneItemData post = ds[row];
            string item = cb(post);
            if (item != null)
            {
                if (wantAdd)
                {
                    if (!list.Contains(item))
                    {
                        list.Add(item);
                        settings.Save();
                        dgv.Invalidate(); // redisplay
                    }
                }
                else
                {
                    // remove
                    if (list.Contains(item))
                    {
                        list.Remove(item);
                        settings.Save();
                        dgv.Invalidate(); // redisplay
                    }
                }
            }
        }

        private void AddDomainToList(List<String> list)
        {
            AddRemoveThingInList(list, post => post.domain, true);
        }

        private void RemoveDomainFromList(List<String> list)
        {
            AddRemoveThingInList(list, post => post.domain, false);
        }

        private void AddUserToList(List<String> list)
        {
            AddRemoveThingInList(list, post => post.author, true);
        }

        private void RemoveUserFromList(List<String> list)
        {
            AddRemoveThingInList(list, post => post.author, false);
        }

        private void BlockDomain(object sender, EventArgs e)
        {
            var page = this.tabControl1.SelectedTab;
            QueueSettings qs = page.Tag as QueueSettings;
            AddDomainToList( qs.SpamDomains );
        }

        private void UnBlockDomain(object sender, EventArgs e)
        {
            var page = this.tabControl1.SelectedTab;
            QueueSettings qs = page.Tag as QueueSettings;
            RemoveDomainFromList(qs.SpamDomains);
        }

        private void BlessDomain(object sender, EventArgs e)
        {
            var page = this.tabControl1.SelectedTab;
            QueueSettings qs = page.Tag as QueueSettings;
            AddDomainToList(qs.BlessedDomains);
        }

        private void UnBlessDomain(object sender, EventArgs e)
        {
            var page = this.tabControl1.SelectedTab;
            QueueSettings qs = page.Tag as QueueSettings;
            RemoveDomainFromList(qs.BlessedDomains);
        }

        private void BlockUser(object sender, EventArgs e)
        {
            var page = this.tabControl1.SelectedTab;
            QueueSettings qs = page.Tag as QueueSettings;
            AddUserToList(qs.DeadUsers);
        }

        private void BlessUser(object sender, EventArgs e)
        {
            var page = this.tabControl1.SelectedTab;
            QueueSettings qs = page.Tag as QueueSettings;
            AddUserToList(qs.BlessedUsers);
        }

        private void UnBlockUser(object sender, EventArgs e)
        {
            var page = this.tabControl1.SelectedTab;
            QueueSettings qs = page.Tag as QueueSettings;
            RemoveUserFromList(qs.DeadUsers);
        }

        private void UnBlessUser(object sender, EventArgs e)
        {
            var page = this.tabControl1.SelectedTab;
            QueueSettings qs = page.Tag as QueueSettings;
            RemoveUserFromList(qs.BlessedUsers);
        }

        private void ViewUser(object sender, EventArgs e)
        {
            ViewUrl(2);
        }

        private void ViewLink(object sender, EventArgs e)
        {
            ViewUrl(1);
        }

        private void ViewComments(object sender, EventArgs e)
        {
            ViewUrl(0);
        }

        private string DesiredUrl(Dino.Reddit.Data.OneItemData post, int flavor)
        {
            if (flavor == 0) // comments
                return Client.GetItemUrl(post);

            if (flavor == 2 && post.author!=null) // author
                return Client.GetAuthorUrl(post);

            if (post.url == null) // link
                return Client.GetItemUrl(post);

            return System.Net.WebUtility.HtmlDecode(post.url);
        }

        private void ViewUrl(int flavor)
        {
            var page = this.tabControl1.SelectedTab;
            var dgv = page.Controls[0] as DataGridView;

            // launch a browser if possible
            var exe = settings.PreferredBrowserExe.Trim();
            if (String.IsNullOrEmpty(exe)) return;
            if (!System.IO.File.Exists(exe)) return;
            var row = GetRightClickRow();
            if (row < 0) return;
            var ds = (List<Dino.Reddit.Data.OneItemData>) dgv.DataSource;
            string url = DesiredUrl(ds[row], flavor);
            if (url!=null)
                System.Diagnostics.Process.Start(exe, url);
        }


        private void DoAllLists(Action<TabPage> action)
        {
            var list = GetAllQueues(this.settings);
            foreach(var qs in list)
            {
                var tabName = qs.GetTabName();
                var page = GetTabPageByName(tabName);
                if (page !=null)
                {
                    page.Tag = qs;
                    action(page);
                }
            }
        }

        private void ProcessAllLists()
        {
            // curry
            var a = new Action<TabPage>(x => { ProcessListEx(x, false); });
            DoAllLists(a);
        }

        private void RefreshAllLists()
        {
            DoAllLists(RefreshList);
        }

        private void RefreshList(System.Windows.Forms.TabPage page)
        {
            QueueSettings qs = page.Tag as QueueSettings;
            Dino.Reddit.Data.Listing listing = null;
            if (String.IsNullOrEmpty(qs.Name))
                listing = reddit.GetUserModQueue();
            else if (qs.WatchNewQueue)
                listing = reddit.GetNewQueue(qs.Name);
            else
                listing = reddit.GetModQueue(qs.Name);

            this.tabControl1.SelectedTab = page;
            this.Update();
            var dgv = page.Controls[0] as DataGridView;

            if (listing != null)
            {
                if (listing.data != null)
                {
                    var ds = listing.data.children.Select(x => x.data).ToList();
                    foreach (var x in ds)
                        x.markSpam = x.markRemove = x.markApprove = x.handled = false;

                    dgv.DataSource = ds;
                    for (int i = 0; i < dgv.ColumnCount; i++)
                    {
                        // enable the checkboxes
                        dgv.Columns[i].ReadOnly = (i != 1 && i != 2 && i != 3);
                    }
                    dgv.Columns[1].HeaderText = "S";
                    dgv.Columns[1].ToolTipText = "Report as Spam";
                    dgv.Columns[2].HeaderText = "R";
                    dgv.Columns[2].ToolTipText = "Remove the item";
                    dgv.Columns[3].HeaderText = "A";
                    dgv.Columns[3].ToolTipText = "Approve the item";
                    dgv.Columns[10].HeaderText = "\u2191"; // uparrow
                    var style = new DataGridViewCellStyle();
                    style.Font = new System.Drawing.Font("Lucida Sans Unicode", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                    dgv.Columns[10].HeaderCell.Style = style;
                    dgv.Columns[10].ToolTipText = "upvotes";
                    dgv.Columns[11].HeaderText = "\u2193"; // downarrow
                    dgv.Columns[11].HeaderCell.Style = style;
                    dgv.Columns[11].ToolTipText = "downvotes";
                    dgv.Columns[12].HeaderText = "#";
                    dgv.Columns[12].ToolTipText = "score";
                    dgv.Columns[13].HeaderText = "!";
                    dgv.Columns[13].ToolTipText = "reports";
                    dgv.Columns[15].HeaderText = "self";
                    dgv.Columns[16].HeaderText = "text";
                    dgv.Columns[16].ToolTipText = "self text or comment text";
                    dgv.ClearSelection();
                    SetRowHeaders();
                    UpdateStatus("ready.");
                    this.Update();
                }
                else
                {
                    dgv.DataSource = null;
                    UpdateStatus("error 1 retrieving the listing.");
                }
            }
            else
            {
                dgv.DataSource = null;
                UpdateStatus("error 2 retrieving the listing.");
            }
        }

        private void SetRowHeaders()
        {
            // I thought I was gonna have row headers, but then decided
            // against it.

            // foreach (DataGridViewRow row in dataGridView1.Rows)
            //     row.HeaderCell.Value = String.Format("{0}", row.Index + 1);
        }

        private void ProcessListEx(TabPage page, bool skipRefreshOnEmptyQueue)
        {
            this.tabControl1.SelectTab(page);
            this.Update();

            var dgv = page.Controls[0] as DataGridView;
            if (dgv == null || dgv.DataSource == null) return;
            var ds = (List<Dino.Reddit.Data.OneItemData>) dgv.DataSource;

            int initialCount = ds.Count();
            int handledCount = 0;
            if (initialCount > 0)
            {
                int c = 0;
                foreach (var x in ds) ProcessStage1(c++, x, dgv);
                ds = ds.Where(x => !x.handled).ToList();
                dgv.DataSource = ds;
                SetRowHeaders();
                dgv.ClearSelection();
                this.Update();

                c = 0;
                foreach (var x in ds) ProcessStage2(c++, x, dgv);
                dgv.ClearSelection();
                this.Update();
                log.Save();

                handledCount = ds.Where(x => x.handled).Count();
            }

            bool skipRefresh = ((initialCount == 0 || handledCount==0) && skipRefreshOnEmptyQueue);
            if (!skipRefresh)
            {
                // retrieve and display a new list
                this.lblStatus.Text = "refreshing...";
                this.Update();
                Application.DoEvents();
                RefreshList(page);
            }
        }

        private void HandleClick(Action action)
        {
            if (!reddit.LoggedIn)
            {
                if (String.IsNullOrEmpty(settings.RedditUser) ||
                       String.IsNullOrEmpty(settings.RedditPwd))
                    btnSettings_Click(null, null);

                if (!(String.IsNullOrEmpty(settings.RedditUser) ||
                       String.IsNullOrEmpty(settings.RedditPwd)))
                    reddit.Login(settings.RedditUser, settings.RedditPwd);
            }

            if (!reddit.LoggedIn) return;

            this.Cursor = Cursors.WaitCursor;
            this.Update();
            if (timer1 != null) timer1.Stop();
            DisableButtons();
            try
            {
                action();
            }
            finally
            {
                EnableButtons();
                if (timer1 != null)
                {
                    timerCyclesRemaining = settings.CheckInterval * 60;
                    timer1.Start();
                }
                this.Cursor = Cursors.Default;
            }
        }

        private void btnCheck_Click(object sender, EventArgs e)
        {
            HandleClick(RefreshAllLists);
        }

        private void btnProcess_Click(object sender, EventArgs e)
        {
            HandleClick(ProcessAllLists);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // remember how many posts the tool removed. Just for grins.
            settings.NumRemoved += nRemoved;

            // remember the width of the column for the post title,
            // for next time. If the user adjusts it, it will be restored
            // as he last had it, on app restart.
            settings.TitleWidth = TitleWidth;
            settings.Save();
        }

        public int TitleWidth
        {
            get
            {
                // I put a sanity check here, for use during
                // development.  I had been fiddling with the number and
                // order of columns, and yet I wanted to save the width
                // of the title column.  This check verifies that I am
                // saving the correct with.  It might be nice to allow
                // the user to define the set and order of columns to
                // display, but ... leave that for another day.
                DataGridViewColumn column = dataGridView1.Columns[7]; // title
                if (column.HeaderText != "title")
                    throw new Exception("incorrect configuration");
                return column.Width;
            }
            set
            {
                DataGridViewColumn column = dataGridView1.Columns[7]; // title
                if (column.HeaderText != "title")
                    throw new Exception("incorrect configuration");
                column.Width = value;
            }
        }


        private void btnSettings_Click(object sender, EventArgs e)
        {
            DisableButtons();
            var copy = settings.Copy();
            var dlg = new SettingsForm(copy);
            dlg.Client = reddit;

            if (timer1 != null) timer1.Stop();

            var result = dlg.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                reddit.MarkUsersUndead(settings.DeadUsers);
                reddit.MarkUsersDead(copy.DeadUsers);

                // If the user name / pwd has changed, need to logout, re-login
                if (copy.RedditUser != settings.RedditUser ||
                    copy.RedditPwd != settings.RedditPwd)
                {
                    DisableButtons();
                    reddit.Logout();
                    if (reddit.Login(copy.RedditUser, copy.RedditPwd))
                    {
                        EnableButtons();
                        ResetTimer();
                    }
                }
                else
                {
                    RetagSettings(copy);
                    EnableButtons();
                }
                copy.Parent = settings.Parent;
                copy.Save();
                settings = copy;
            }
            else
            {
                EnableButtons();
            }
            if (timer1 != null) timer1.Start();
        }


        private void button3_Click(object sender, EventArgs e)
        {
            var dlg = new AboutBoxForm();
            dlg.ShowDialog();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var dlg = new AuditLogForm(log, settings.PreferredBrowserExe);
            dlg.ShowDialog();
        }

        private void Form1_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
                HandleClick(RefreshAllLists);
        }

    }
}
