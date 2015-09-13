namespace Dino.Tools.Reddit.CleanModQueue
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.btnCheck = new System.Windows.Forms.Button();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.bindingSource1 = new System.Windows.Forms.BindingSource(this.components);
            this.btnProcess = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btnSettings = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.index = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.markSpam = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.markRemove = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.markApprove = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.domainDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.subredditDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.idDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.titleDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.authorDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.createdTimeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.upsDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.downsDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.scoreDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.num_reports = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.nameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.isselfDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.selftextDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.permalinkDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.subredditidDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.urlDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.createdUtcTimeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.numcommentsDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).BeginInit();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCheck
            // 
            this.btnCheck.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCheck.Location = new System.Drawing.Point(429, 3);
            this.btnCheck.Name = "btnCheck";
            this.btnCheck.Size = new System.Drawing.Size(58, 23);
            this.btnCheck.TabIndex = 40;
            this.btnCheck.Text = "Check";
            this.toolTip1.SetToolTip(this.btnCheck, "Get the current list of items from\r\nthe modqueue on the server, and \r\nrefresh the" +
        " display with that list.");
            this.btnCheck.UseVisualStyleBackColor = true;
            this.btnCheck.Click += new System.EventHandler(this.btnCheck_Click);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.ShowImageMargin = false;
            this.contextMenuStrip1.Size = new System.Drawing.Size(128, 26);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.cms_Opening);
            // 
            // bindingSource1
            // 
            this.bindingSource1.DataSource = typeof(Dino.Reddit.Data.OneItemData);
            // 
            // btnProcess
            // 
            this.btnProcess.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnProcess.Location = new System.Drawing.Point(495, 3);
            this.btnProcess.Name = "btnProcess";
            this.btnProcess.Size = new System.Drawing.Size(58, 23);
            this.btnProcess.TabIndex = 50;
            this.btnProcess.Text = "Process";
            this.toolTip1.SetToolTip(this.btnProcess, "Process all items in the list, removing\r\nand approving as appropriate. Then\r\nre-c" +
        "heck for new items in the mod queue.");
            this.btnProcess.UseVisualStyleBackColor = true;
            this.btnProcess.Click += new System.EventHandler(this.btnProcess_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(12, 9);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(35, 13);
            this.lblStatus.TabIndex = 2;
            this.lblStatus.Text = "label1";
            // 
            // btnSettings
            // 
            this.btnSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSettings.Location = new System.Drawing.Point(363, 3);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(58, 23);
            this.btnSettings.TabIndex = 30;
            this.btnSettings.Text = "Settings";
            this.toolTip1.SetToolTip(this.btnSettings, "Display or edit the settings for the tool,\r\nincluding Reddit credentials, and the" +
        " list\r\nof known good and known spammer authors.");
            this.btnSettings.UseVisualStyleBackColor = true;
            this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);
            // 
            // button3
            // 
            this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button3.Location = new System.Drawing.Point(297, 3);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(58, 23);
            this.button3.TabIndex = 20;
            this.button3.Text = "About";
            this.toolTip1.SetToolTip(this.button3, "Show basic information about this tool.");
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button4.Location = new System.Drawing.Point(230, 3);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(61, 23);
            this.button4.TabIndex = 10;
            this.button4.Text = "View Log";
            this.toolTip1.SetToolTip(this.button4, "View the log of approve and remove actions taken\r\nby the tool.");
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.dataGridView1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(554, 202);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "modqueue";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AutoGenerateColumns = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.index,
            this.markSpam,
            this.markRemove,
            this.markApprove,
            this.domainDataGridViewTextBoxColumn,
            this.subredditDataGridViewTextBoxColumn,
            this.idDataGridViewTextBoxColumn,
            this.titleDataGridViewTextBoxColumn,
            this.authorDataGridViewTextBoxColumn,
            this.createdTimeDataGridViewTextBoxColumn,
            this.upsDataGridViewTextBoxColumn,
            this.downsDataGridViewTextBoxColumn,
            this.scoreDataGridViewTextBoxColumn,
            this.num_reports,
            this.nameDataGridViewTextBoxColumn,
            this.isselfDataGridViewCheckBoxColumn,
            this.selftextDataGridViewTextBoxColumn,
            this.permalinkDataGridViewTextBoxColumn,
            this.subredditidDataGridViewTextBoxColumn,
            this.urlDataGridViewTextBoxColumn,
            this.createdUtcTimeDataGridViewTextBoxColumn,
            this.numcommentsDataGridViewTextBoxColumn});
            this.dataGridView1.ContextMenuStrip = this.contextMenuStrip1;
            this.dataGridView1.DataSource = this.bindingSource1;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(3, 3);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersWidth = 24;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView1.Size = new System.Drawing.Size(548, 196);
            this.dataGridView1.TabIndex = 1;
            this.dataGridView1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.dataGridView_MouseUp);
            // 
            // index
            // 
            this.index.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
            this.index.HeaderText = "ix";
            this.index.Name = "index";
            this.index.ReadOnly = true;
            this.index.Width = 5;
            // 
            // markSpam
            // 
            this.markSpam.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
            this.markSpam.DataPropertyName = "markSpam";
            this.markSpam.HeaderText = "markSpam";
            this.markSpam.Name = "markSpam";
            this.markSpam.Width = 5;
            // 
            // markRemove
            // 
            this.markRemove.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
            this.markRemove.DataPropertyName = "markRemove";
            this.markRemove.HeaderText = "markRemove";
            this.markRemove.Name = "markRemove";
            this.markRemove.Width = 5;
            // 
            // markApprove
            // 
            this.markApprove.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
            this.markApprove.DataPropertyName = "markApprove";
            this.markApprove.HeaderText = "markApprove";
            this.markApprove.Name = "markApprove";
            this.markApprove.Width = 5;
            // 
            // domainDataGridViewTextBoxColumn
            // 
            this.domainDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.domainDataGridViewTextBoxColumn.DataPropertyName = "domain";
            this.domainDataGridViewTextBoxColumn.HeaderText = "domain";
            this.domainDataGridViewTextBoxColumn.Name = "domainDataGridViewTextBoxColumn";
            this.domainDataGridViewTextBoxColumn.ReadOnly = true;
            this.domainDataGridViewTextBoxColumn.Width = 66;
            // 
            // subredditDataGridViewTextBoxColumn
            // 
            this.subredditDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
            this.subredditDataGridViewTextBoxColumn.DataPropertyName = "subreddit";
            this.subredditDataGridViewTextBoxColumn.HeaderText = "subreddit";
            this.subredditDataGridViewTextBoxColumn.Name = "subredditDataGridViewTextBoxColumn";
            this.subredditDataGridViewTextBoxColumn.ReadOnly = true;
            this.subredditDataGridViewTextBoxColumn.Width = 5;
            // 
            // idDataGridViewTextBoxColumn
            // 
            this.idDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.idDataGridViewTextBoxColumn.DataPropertyName = "id";
            this.idDataGridViewTextBoxColumn.HeaderText = "id";
            this.idDataGridViewTextBoxColumn.Name = "idDataGridViewTextBoxColumn";
            this.idDataGridViewTextBoxColumn.ReadOnly = true;
            this.idDataGridViewTextBoxColumn.Width = 40;
            // 
            // titleDataGridViewTextBoxColumn
            // 
            this.titleDataGridViewTextBoxColumn.DataPropertyName = "title";
            this.titleDataGridViewTextBoxColumn.HeaderText = "title";
            this.titleDataGridViewTextBoxColumn.Name = "titleDataGridViewTextBoxColumn";
            this.titleDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // authorDataGridViewTextBoxColumn
            // 
            this.authorDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.authorDataGridViewTextBoxColumn.DataPropertyName = "author";
            this.authorDataGridViewTextBoxColumn.HeaderText = "author";
            this.authorDataGridViewTextBoxColumn.Name = "authorDataGridViewTextBoxColumn";
            this.authorDataGridViewTextBoxColumn.ReadOnly = true;
            this.authorDataGridViewTextBoxColumn.Width = 62;
            // 
            // createdTimeDataGridViewTextBoxColumn
            // 
            this.createdTimeDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.createdTimeDataGridViewTextBoxColumn.DataPropertyName = "createdTime";
            this.createdTimeDataGridViewTextBoxColumn.HeaderText = "createdTime";
            this.createdTimeDataGridViewTextBoxColumn.Name = "createdTimeDataGridViewTextBoxColumn";
            this.createdTimeDataGridViewTextBoxColumn.ReadOnly = true;
            this.createdTimeDataGridViewTextBoxColumn.Width = 91;
            // 
            // upsDataGridViewTextBoxColumn
            // 
            this.upsDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
            this.upsDataGridViewTextBoxColumn.DataPropertyName = "ups";
            this.upsDataGridViewTextBoxColumn.HeaderText = "ups";
            this.upsDataGridViewTextBoxColumn.Name = "upsDataGridViewTextBoxColumn";
            this.upsDataGridViewTextBoxColumn.ReadOnly = true;
            this.upsDataGridViewTextBoxColumn.Width = 5;
            // 
            // downsDataGridViewTextBoxColumn
            // 
            this.downsDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
            this.downsDataGridViewTextBoxColumn.DataPropertyName = "downs";
            this.downsDataGridViewTextBoxColumn.HeaderText = "downs";
            this.downsDataGridViewTextBoxColumn.Name = "downsDataGridViewTextBoxColumn";
            this.downsDataGridViewTextBoxColumn.ReadOnly = true;
            this.downsDataGridViewTextBoxColumn.Width = 5;
            // 
            // scoreDataGridViewTextBoxColumn
            // 
            this.scoreDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
            this.scoreDataGridViewTextBoxColumn.DataPropertyName = "score";
            this.scoreDataGridViewTextBoxColumn.HeaderText = "score";
            this.scoreDataGridViewTextBoxColumn.Name = "scoreDataGridViewTextBoxColumn";
            this.scoreDataGridViewTextBoxColumn.ReadOnly = true;
            this.scoreDataGridViewTextBoxColumn.Width = 5;
            // 
            // num_reports
            // 
            this.num_reports.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.num_reports.DataPropertyName = "num_reports";
            this.num_reports.HeaderText = "num_reports";
            this.num_reports.Name = "num_reports";
            this.num_reports.Width = 90;
            // 
            // nameDataGridViewTextBoxColumn
            // 
            this.nameDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.nameDataGridViewTextBoxColumn.DataPropertyName = "name";
            this.nameDataGridViewTextBoxColumn.HeaderText = "name";
            this.nameDataGridViewTextBoxColumn.Name = "nameDataGridViewTextBoxColumn";
            this.nameDataGridViewTextBoxColumn.ReadOnly = true;
            this.nameDataGridViewTextBoxColumn.Width = 58;
            // 
            // isselfDataGridViewCheckBoxColumn
            // 
            this.isselfDataGridViewCheckBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.isselfDataGridViewCheckBoxColumn.DataPropertyName = "is_self";
            this.isselfDataGridViewCheckBoxColumn.HeaderText = "is_self";
            this.isselfDataGridViewCheckBoxColumn.Name = "isselfDataGridViewCheckBoxColumn";
            this.isselfDataGridViewCheckBoxColumn.ReadOnly = true;
            this.isselfDataGridViewCheckBoxColumn.Width = 42;
            // 
            // selftextDataGridViewTextBoxColumn
            // 
            this.selftextDataGridViewTextBoxColumn.DataPropertyName = "selftext";
            this.selftextDataGridViewTextBoxColumn.HeaderText = "selftext";
            this.selftextDataGridViewTextBoxColumn.Name = "selftextDataGridViewTextBoxColumn";
            this.selftextDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // permalinkDataGridViewTextBoxColumn
            // 
            this.permalinkDataGridViewTextBoxColumn.DataPropertyName = "permalink";
            this.permalinkDataGridViewTextBoxColumn.HeaderText = "permalink";
            this.permalinkDataGridViewTextBoxColumn.Name = "permalinkDataGridViewTextBoxColumn";
            this.permalinkDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // subredditidDataGridViewTextBoxColumn
            // 
            this.subredditidDataGridViewTextBoxColumn.DataPropertyName = "subreddit_id";
            this.subredditidDataGridViewTextBoxColumn.HeaderText = "subreddit_id";
            this.subredditidDataGridViewTextBoxColumn.Name = "subredditidDataGridViewTextBoxColumn";
            this.subredditidDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // urlDataGridViewTextBoxColumn
            // 
            this.urlDataGridViewTextBoxColumn.DataPropertyName = "url";
            this.urlDataGridViewTextBoxColumn.HeaderText = "url";
            this.urlDataGridViewTextBoxColumn.Name = "urlDataGridViewTextBoxColumn";
            this.urlDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // createdUtcTimeDataGridViewTextBoxColumn
            // 
            this.createdUtcTimeDataGridViewTextBoxColumn.DataPropertyName = "createdUtcTime";
            this.createdUtcTimeDataGridViewTextBoxColumn.HeaderText = "createdUtcTime";
            this.createdUtcTimeDataGridViewTextBoxColumn.Name = "createdUtcTimeDataGridViewTextBoxColumn";
            this.createdUtcTimeDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // numcommentsDataGridViewTextBoxColumn
            // 
            this.numcommentsDataGridViewTextBoxColumn.DataPropertyName = "num_comments";
            this.numcommentsDataGridViewTextBoxColumn.HeaderText = "num_comments";
            this.numcommentsDataGridViewTextBoxColumn.Name = "numcommentsDataGridViewTextBoxColumn";
            this.numcommentsDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Location = new System.Drawing.Point(-1, 32);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(562, 228);
            this.tabControl1.TabIndex = 51;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(560, 262);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.btnSettings);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.btnProcess);
            this.Controls.Add(this.btnCheck);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Name = "Form1";
            this.Text = "CleanModQueue";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).EndInit();
            this.tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCheck;
        private System.Windows.Forms.BindingSource bindingSource1;
        private System.Windows.Forms.Button btnProcess;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btnSettings;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn index;
        private System.Windows.Forms.DataGridViewCheckBoxColumn markSpam;
        private System.Windows.Forms.DataGridViewCheckBoxColumn markRemove;
        private System.Windows.Forms.DataGridViewCheckBoxColumn markApprove;
        private System.Windows.Forms.DataGridViewTextBoxColumn domainDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn subredditDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn idDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn titleDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn authorDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn createdTimeDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn upsDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn downsDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn scoreDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn num_reports;
        private System.Windows.Forms.DataGridViewTextBoxColumn nameDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn isselfDataGridViewCheckBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn selftextDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn permalinkDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn subredditidDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn urlDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn createdUtcTimeDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn numcommentsDataGridViewTextBoxColumn;
        private System.Windows.Forms.TabControl tabControl1;
    }
}

