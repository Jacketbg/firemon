using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Media;

namespace Fire_Monitor
{
    public partial class Form4 : Form
    {
        string sensor_descr;
        string sensor_value;
        bool sound;
        SoundPlayer alarm = new SoundPlayer(Properties.Resources.alarm_bell);

        public string Descr
        {
            get
            {
                return sensor_descr;
            }
            set
            {
                sensor_descr = value;
            }
        }

        public string Value
        {
            get
            {
                return sensor_value;
            }
            set
            {
                sensor_value = value;
            }
        }

        public bool Sound
        {
            get
            {
                return sound;
            }
            set
            {
                sound = value;
            }
        }

        public Form4()
        {
            InitializeComponent();
        }

        private void Form4_Load(object sender, EventArgs e)
        {
            label4.Text = sensor_descr;
            label5.Text = sensor_value;
            if (sound)
                alarm.PlayLooping();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (sound)
                alarm.Stop();
            this.Close();
        }

        private void Form4_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (sound)
                alarm.Stop();
        }
    }
}
