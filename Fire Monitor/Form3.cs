using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Fire_Monitor
{
    public partial class Form3 : Form
    {
        string thisversion;
        
        public Form3()
        {
            InitializeComponent();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.napravisam.bg/forum/profile.php?mode=viewprofile&u=40667"); 
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            Version ver = new Version(Application.ProductVersion);
            thisversion = ver.Major + "." + ver.Minor + ver.Build;
            label3.Text = thisversion;
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://oss.oetiker.ch/rrdtool/");
        }
    }
}
