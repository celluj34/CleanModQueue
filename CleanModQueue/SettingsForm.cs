// SettingsForm.cs
// ------------------------------------------------------------------
//
// Part of CleanModQueue .
// A form to display and modify the tool settings.
//
// last saved: <2012-July-31 06:20:50>
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
using System.Data;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Linq;
using System.Windows.Forms;

namespace Dino.Tools.Reddit.CleanModQueue
{
    public partial class SettingsForm : Form
    {
        int markedInvalid = -1;
        TextBox[][] txts;
        Label[] warningLabels;
        List<string> orderableActions;
        Dino.Reddit.Client reddit;
        bool fillingForm;
        int oldIndex;

        public SettingsForm(AppSettings s)
        {
            InitializeComponent();

            fillingForm = true;
            this.Settings = s;
            this.lblMissingPwd.Visible = false;
            this.lblMissingUser.Visible = false;
            this.lblUserConflict.Visible = false;
            this.lblDomainConflict.Visible = false;
            this.lblTitleRegexInvalid.Visible = false;

            txts = new TextBox[][]
                { new TextBox[] { this.txtAuthorsSpam, this.txtAuthorsGood },
                  new TextBox[] { this.txtDomainsSpam, this.txtDomainsGood },
                  new TextBox[] { this.txtTitlesGood },
                  new TextBox[] { this.txtTitlesSpam } };

            warningLabels = new Label[] { this.lblUserConflict,
                                          this.lblDomainConflict,
                                          this.lblTitleRegexInvalid,
                                          this.lblTitleRegexInvalid };

            MapPropsToForm(s);

            oldIndex = -1;
            this.comboBox1.SelectedIndex = 0;
            fillingForm = false;
            comboBox1_SelectedIndexChanged(null, null);
        }

        public Dino.Reddit.Client Client
        {
            get
            {
                return reddit;
            }
            set
            {
                reddit = value;
            }
        }

        private static string SpacePascal(string s)
        {
            return Regex.Replace(s, "([a-z])([A-Z])", "$1 $2");
        }

        private static string CollapsePascal(string s)
        {
            return s.Replace(" ","");
        }

        private void MapPropsToForm(AppSettings s, string queueName=null)
        {
            this.txtUser.Text = s.RedditUser;
            this.txtPwd.Text = s.RedditPwd;
            this.chkSavePwd.Checked = s.SavePassword;
            this.chkSpamIsAsSpamDoes.Checked = s.SpamIsAsSpamDoes;
            this.txtInterval.Text = s.CheckInterval.ToString();

            if (queueName == null)
                MapQueueSpecificPropsToForm(s);
            else
            {
                var z = this.Settings.OtherQueues
                    .Where(x => x.Name == queueName)
                    .Select(x => x);

                if (z.Count() == 1)
                    MapQueueSpecificPropsToForm(z.First());
            }

            this.txtBrowserExe.Text = this.Settings.PreferredBrowserExe;
            txtUser_TextChanged(null, null);
            txtPwd_TextChanged(null, null);
        }

        private void MapQueueSpecificPropsToForm(QueueSettings s)
        {
            this.tabControl1.Tag = s; // for deserializing later

            this.txtUpvotes.Text = s.UpvoteThreshold.ToString();

            s.DeadUsers.Sort();
            this.txtAuthorsSpam.Text = String.Join("\r\n", s.DeadUsers);
            s.SpamDomains.Sort();
            this.txtDomainsSpam.Text = String.Join("\r\n", s.SpamDomains);
            s.SpamTitleRegexi.Sort();
            this.txtTitlesSpam.Text = String.Join("\r\n", s.SpamTitleRegexi);

            s.BlessedUsers.Sort();
            this.txtAuthorsGood.Text = String.Join("\r\n", s.BlessedUsers);
            s.BlessedDomains.Sort();
            this.txtDomainsGood.Text = String.Join("\r\n", s.BlessedDomains);
            s.BlessedTitleRegexi.Sort();
            this.txtTitlesGood.Text = String.Join("\r\n", s.BlessedTitleRegexi);

            this.chkDenyAllUsers.Checked = s.UserRules.DenyAll;
            this.chkAllowAllUsers.Checked = s.UserRules.AllowAll;
            this.chkDenyAllDomains.Checked = s.DomainRules.DenyAll;
            this.chkAllowAllDomains.Checked = s.DomainRules.AllowAll;
            this.chkDenyAllTitles.Checked = s.TitleRules.DenyAll;
            this.chkAllowAllTitles.Checked = s.TitleRules.AllowAll;

            this.chkEnabled.Checked = s.Enabled;

            this.spamAgeTextBox.Text = s.SpamAuthMaturity.Age.ToString();
            this.spamLinkKarmaTextBox.Text = s.SpamAuthMaturity.LinkKarma.ToString();
            this.spamCommentKarmaTextBox.Text = s.SpamAuthMaturity.CommentKarma.ToString();
            this.blessedAgeTextBox.Text = s.BlessedAuthMaturity.Age.ToString();
            this.blessedLinkKarmaTextBox.Text = s.BlessedAuthMaturity.LinkKarma.ToString();
            this.blessedCommentKarmaTextBox.Text = s.BlessedAuthMaturity.CommentKarma.ToString();

            orderableActions = s.OrderedActions
                .ConvertAll(z => z.ToString())
                .ConvertAll(SpacePascal);
            this.listBox1.DataSource = orderableActions;
        }


        private AppSettings Settings { get; set; }

        private List<String> Splitto(String x)
        {
            return x.Trim()
                .Split(new char[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries)
                .Where(s => !String.IsNullOrEmpty(s.Trim()))
                .Distinct()
                .ToList();
        }

        private void MarkInvalid(int flavor)
        {
            foreach (TextBox txt in txts[flavor])
            {
                txt.BackColor = System.Drawing.Color.LightSalmon;
            }

            warningLabels[flavor].Visible = true;
            markedInvalid = flavor;
        }


        private bool ValidateAndPreserveSettingsForOneQueue()
        {
            // validate
            var spamUsers = Splitto(this.txtAuthorsSpam.Text);
            var blessedUsers = Splitto(this.txtAuthorsGood.Text);
            var x1 = spamUsers.Intersect(blessedUsers);

            if (x1.Count()>0)
            {
                // there is an overlap between spam and blessed users
                MarkInvalid(0);
                return false;
            }
            var spamDomains = Splitto(this.txtDomainsSpam.Text);
            var blessedDomains = Splitto(this.txtDomainsGood.Text);
            x1 = spamDomains.Intersect(blessedDomains);

            if (x1.Count()>0)
            {
                // there is an overlap between spam and blessed domains
                MarkInvalid(1);
                return false;
            }

            var blessedTitles = Splitto(this.txtTitlesGood.Text);
            var spamTitles = Splitto(this.txtTitlesSpam.Text);

            // TODO: verify that the regexi are all valid.
            var groups = new List<String>[] { blessedTitles, spamTitles };
            for (int i=0; i < groups.Length; i++)
            {
                List<String> group  = groups[i];
                foreach (var regex in group)
                {
                    try
                    {
                        var re3 = new Regex(regex);
                    }
                    catch (System.Exception)
                    {
                        MarkInvalid(i+2);
                        return false;
                    }
                }
            }

            QueueSettings qs = this.tabControl1.Tag as QueueSettings;
            qs.Ordering =
                String.Join(",",
                            ((List<String>) this.listBox1.DataSource)
                            .ConvertAll(CollapsePascal));

            var getInt = new Func<TextBox,Int32,Int32>((tb,def) =>
                {
                    int value = def;
                    Int32.TryParse(tb.Text.Trim(), out value);
                    return value;
                });

            qs.UpvoteThreshold = getInt(this.txtUpvotes,300);
            qs.SpamAuthMaturity.Age = getInt(this.spamAgeTextBox,0);
            qs.SpamAuthMaturity.LinkKarma = getInt(this.spamLinkKarmaTextBox,0);
            qs.SpamAuthMaturity.CommentKarma = getInt(this.spamCommentKarmaTextBox,0);
            qs.BlessedAuthMaturity.Age = getInt(this.blessedAgeTextBox,-1);
            qs.BlessedAuthMaturity.LinkKarma = getInt(this.blessedLinkKarmaTextBox,-1);
            qs.BlessedAuthMaturity.CommentKarma = getInt(this.blessedCommentKarmaTextBox,-1);

            qs.Enabled = this.chkEnabled.Checked;

            qs.DeadUsers = spamUsers;
            qs.SpamDomains = spamDomains;
            qs.SpamTitleRegexi = spamTitles;
            qs.BlessedUsers = blessedUsers;
            qs.BlessedDomains = blessedDomains;
            qs.BlessedTitleRegexi = blessedTitles;

            qs.UserRules.DenyAll = this.chkDenyAllUsers.Checked;
            qs.UserRules.AllowAll = this.chkAllowAllUsers.Checked;
            qs.DomainRules.DenyAll = this.chkDenyAllDomains.Checked;
            qs.DomainRules.AllowAll = this.chkAllowAllDomains.Checked;
            qs.TitleRules.DenyAll = this.chkDenyAllTitles.Checked;
            qs.TitleRules.AllowAll = this.chkAllowAllTitles.Checked;

            return true;
        }


        private void btnOk_Click(object sender, EventArgs e)
        {
            if (!ValidateAndPreserveSettingsForOneQueue())
                return;

            this.Settings.RedditUser = this.txtUser.Text.Trim();
            this.Settings.RedditPwd = this.txtPwd.Text.Trim();
            this.Settings.SavePassword = this.chkSavePwd.Checked;
            this.Settings.SpamIsAsSpamDoes = this.chkSpamIsAsSpamDoes.Checked;
            int interval = 6; // minutes. the default if the parse fails.
            Int32.TryParse(this.txtInterval.Text.Trim(), out interval);
            this.Settings.CheckInterval = interval;
            this.Settings.PreferredBrowserExe = this.txtBrowserExe.Text.Trim();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var dlg = new System.Windows.Forms.OpenFileDialog();
            dlg.Filter = "Exes|*.exe";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                this.txtBrowserExe.Text = dlg.FileName;
            }
        }

        private void txt_TextChanged(object sender, EventArgs e)
        {
            if (markedInvalid>=0)
            {
                foreach (TextBox txt in txts[markedInvalid])
                    txt.BackColor = System.Drawing.SystemColors.Window;
                warningLabels[markedInvalid].Visible = false;
                markedInvalid = -1;
            }
        }

        private void txtExe_TextChanged(object sender, EventArgs e)
        {
            var txt = this.txtBrowserExe;
            var exe = txt.Text.Trim();
            if (String.IsNullOrEmpty(exe) || System.IO.File.Exists(exe))
            {
                txt.BackColor = System.Drawing.SystemColors.Window;
            }
            else
            {
                txt.BackColor = System.Drawing.Color.LightSalmon;
            }
        }

        private void txtUser_TextChanged(object sender, EventArgs e)
        {
            TextBox txt = sender as TextBox;
            if (txt == null) return;
            if (String.IsNullOrEmpty(txt.Text))
            {
                txt.BackColor = System.Drawing.Color.LightSalmon;
                this.lblMissingUser.Visible = true;
            }
            else
            {
                txt.BackColor = System.Drawing.SystemColors.Window;
                this.lblMissingUser.Visible = false;
            }
        }

        private void txtPwd_TextChanged(object sender, EventArgs e)
        {
            TextBox txt = sender as TextBox;
            if (txt == null) return;
            if (String.IsNullOrEmpty(txt.Text))
            {
                txt.BackColor = System.Drawing.Color.LightSalmon;
                this.lblMissingPwd.Visible = true;
            }
            else
            {
                txt.BackColor = System.Drawing.SystemColors.Window;
                this.lblMissingPwd.Visible = false;
            }
        }

        private void Reorder(int ix1, int ix2)
        {
            var item = orderableActions[ix1];
            orderableActions.RemoveAt(ix1);
            orderableActions.Insert(ix2, item);
            this.listBox1.SelectedIndex = ix2;
            CurrencyManager cm = (BindingContext[listBox1.DataSource] as CurrencyManager);
            cm.Refresh();
        }

        private void btnUp_Click(object sender, EventArgs e)
        {
            var ix = this.listBox1.SelectedIndex;
            if (ix == 0) return;
            Reorder(ix, ix-1);
        }

        private void btnDn_Click(object sender, EventArgs e)
        {
            var ix = this.listBox1.SelectedIndex;
            if (ix >= orderableActions.Count() - 1) return;
            Reorder(ix, ix+1);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.btnUp.Enabled = (this.listBox1.SelectedIndex > 0);
            this.btnDn.Enabled = (this.listBox1.SelectedIndex < this.listBox1.Items.Count-1);
        }

        private void chkDenyAllUsers_CheckedChanged(object sender, EventArgs e)
        {
            this.txtAuthorsSpam.Enabled = !this.chkDenyAllUsers.Checked;
        }

        private void chkDenyAllDomains_CheckedChanged(object sender, EventArgs e)
        {
            this.txtDomainsSpam.Enabled = !this.chkDenyAllDomains.Checked;
        }

        private void chkAllowAllDomains_CheckedChanged(object sender, EventArgs e)
        {
            this.txtDomainsGood.Enabled = !this.chkAllowAllDomains.Checked;
        }

        private void chkAllowAllUsers_CheckedChanged(object sender, EventArgs e)
        {
            this.txtAuthorsGood.Enabled = !this.chkAllowAllUsers.Checked;
        }


        private void FillListOfQueues()
        {
            if (this.comboBox1.Items.Count > 1) return;
            if (reddit==null) return;

            this.Enabled = false;
            if (!reddit.LoggedIn)
                reddit.Login( this.txtUser.Text, this.txtPwd.Text);

            if (!reddit.LoggedIn)
            {
                this.Enabled = true;
                return;
            }

            // refresh the list of possible queues
            var list = reddit.GetSubsIMod();
            if (list==null || list.error != null || list.data == null)
            {
                this.Enabled = true;
                return;
            }

            var reddits = list.data.children
                .Where(x => x.kind=="t5")
                .Select(x => x.data)
                .ToList();
            this.comboBox1.Items.Clear();
            this.comboBox1.Items.Add("modqueue");
            foreach (var item in reddits)
            {
                this.comboBox1.Items.Add(item.display_name + " (new)");
                this.comboBox1.Items.Add(item.display_name + " (mod)");
            }
            this.Enabled = true;
        }


        private void comboBox1_DropDown(object sender, EventArgs e)
        {
            FillListOfQueues();
        }


        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!this.Enabled) return;
            if (fillingForm) return;
            var cmb = this.comboBox1;
            QueueSettings settings = null;

            if (!ValidateAndPreserveSettingsForOneQueue())
            {
                cmb.SelectedIndex = oldIndex; // revert
                return;
            }

            var queueName = cmb.Items[cmb.SelectedIndex].ToString();
            oldIndex = cmb.SelectedIndex;

            if (queueName == "modqueue")   // special case
            {
                settings = this.Settings;
                MapQueueSpecificPropsToForm(settings);
            }
            else
            {
                var parts = queueName.Split(' ');
                var z = this.Settings.OtherQueues
                    .Where(x => ((x.Name == parts[0]) &&
                                 ((x.WatchNewQueue && parts[1] == "(new)") ||
                                  (!x.WatchNewQueue && parts[1] == "(mod)"))))
                    .Select(x => x);

                if (z.Count() == 1)
                {
                    settings = z.First();
                    MapQueueSpecificPropsToForm(settings);
                }
                else
                {
                    // cons new
                    settings = new QueueSettings {
                        Name = parts[0],
                        WatchNewQueue = (parts[1]=="(new)")
                    };
                    this.Settings.OtherQueues.Add(settings);
                    MapQueueSpecificPropsToForm(settings);
                }
            }
        }


        private void LeftTabControl_DrawItem(Object sender, System.Windows.Forms.DrawItemEventArgs e)
        {
            TabControl ctlTab = sender as TabControl;
            Graphics g = e.Graphics;
            String tabText = ctlTab.TabPages[e.Index].Text;
            SizeF sizeText = g.MeasureString(tabText, ctlTab.Font);
            int iX = e.Bounds.Left + 6;
            int iY = (int) (e.Bounds.Top + (e.Bounds.Height - sizeText.Height) / 2);
            if (ctlTab.SelectedIndex == e.Index)
            {
                // make the selected Tab white
                g.FillRectangle(SystemBrushes.ControlLightLight, e.Bounds);
            }
            else
            {
                // This code is unnecessary because the control will
                // automatically paint non-selected tabs
                //    g.FillRectangle(New SolidBrush(Color.LightBlue), e.Bounds);
            }
            g.DrawString(tabText, ctlTab.Font, Brushes.Black, iX, iY);
        }

    }
}
