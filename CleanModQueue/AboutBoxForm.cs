// AboutBoxForm.cs
// ------------------------------------------------------------------
//
// Part of CleanModQueue .
// A form to display an about box.
//
// last saved: <2012-May-05 11:38:56>
// ------------------------------------------------------------------
//
// Copyright (c) 2012 Dino Chiesa
// All rights reserved.
//
// This code is Licensed under the New BSD license.
// See the License.txt file that accompanies this module.
//
// ------------------------------------------------------------------

using System.Windows.Forms;

namespace Dino.Tools.Reddit.CleanModQueue
{
    public partial class AboutBoxForm : Form
    {
        public AboutBoxForm()
        {
            InitializeComponent();

            this.lblVersion.Text = "v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var link = sender as LinkLabel;
            if (link != null)
                System.Diagnostics.Process.Start(link.Text);
        }
    }
}
