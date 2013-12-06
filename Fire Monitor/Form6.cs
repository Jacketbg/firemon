using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Fire_Monitor
{
    public partial class Form6 : Form
    {
        string url;
        public string Url
        {
            get
            {
                return url;
            }
            set
            {
                url = value;
            }
        }

        public Form6()
        {
            InitializeComponent();
        }

        private void Form6_Load(object sender, EventArgs e)
        {
            textBox1.Text = url;
            textBox2.Text = "[img]" + url + "[/img]";
            textBox3.Text = "<img src=\"" + url + "\">";
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(textBox1.Text);
            MessageBox.Show("Данните бяха копирани в Clipboard-а","ОК",MessageBoxButtons.OK,MessageBoxIcon.Information);
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(textBox2.Text);
            MessageBox.Show("Данните бяха копирани в Clipboard-а", "ОК", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(textBox3.Text);
            MessageBox.Show("Данните бяха копирани в Clipboard-а", "ОК", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(url); 
        }
    }
}
