using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;
using Jacket.Classes;
using System.Collections;
using System.Media;
using System.Net;
using System.Drawing;
using System.Globalization;

namespace Fire_Monitor
{
    public partial class Form1 : Form
    {        
        string xmlfile = Application.StartupPath + "\\db.xml";
        string schemafile = Application.StartupPath + "\\db.xsd";

        string thisversion;
        string ip;
        string community;
        int timeout;
        int retries;
        int run;
        int hist;
        bool firstrun;
        bool start = true;
        bool play_sounds;
        bool show_popups;
        string date = DateTime.Now.ToString();
        DateTime check_1 = new DateTime();
        List<List<int>> pins = new List<List<int>>();
        public string[] pins_last = new string[8];
        bool warning_tip_shown = false;
        public NotifyIcon[] pin_icons = new NotifyIcon[8];
        bool[] pin_warnings = new bool[8];
        bool[] pin_alarms = new bool[8];
        string tooltip_lines;
        bool rigid_graph;
        int upper_limit;
        int lower_limit;

        SoundPlayer sound_alarm = new SoundPlayer(Properties.Resources.alarm);
        SoundPlayer sound_warn = new SoundPlayer(Properties.Resources.warning);
        SoundPlayer sound_switch = new SoundPlayer(Properties.Resources.switch_sensor);

        public Form1()
        {
            InitializeComponent();
        }

        public void reload_db()
        {
            ip = dataSet1.Tables["settings"].Rows[0][0].ToString();
            community = dataSet1.Tables["settings"].Rows[0][1].ToString();
            timeout = Convert.ToInt32(dataSet1.Tables["settings"].Rows[0][2]);
            retries = Convert.ToInt32(dataSet1.Tables["settings"].Rows[0][3]);
            firstrun = Convert.ToBoolean(dataSet1.Tables["settings"].Rows[0]["first_run"]);
            hist = Convert.ToInt32(dataSet1.Tables["settings"].Rows[0]["hist"]);
            play_sounds = Convert.ToBoolean(dataSet1.Tables["settings"].Rows[0]["sounds"]);
            show_popups = Convert.ToBoolean(dataSet1.Tables["settings"].Rows[0]["popups"]);
            rigid_graph = Convert.ToBoolean(dataSet1.Tables["settings"].Rows[0][8]);
            upper_limit = Convert.ToInt32(dataSet1.Tables["settings"].Rows[0][9]);
            lower_limit = Convert.ToInt32(dataSet1.Tables["settings"].Rows[0][10]);
            notifyIcon1.Visible = true;
            for (int i = 0; i <= 7; i++)
            {
                if ((bool)dataSet1.Tables["sensors"].Rows[i]["tray"])
                {
                    pin_icons[i].Text = dataSet1.Tables["sensors"].Rows[i]["descr"].ToString();
                    pin_icons[i].ContextMenuStrip = contextMenuStrip1;
                    notifyIcon1.Visible = false;
                }
                else
                {
                    try
                    {
                        pin_icons[i].Visible = false;
                    }
                    catch (Exception)
                    {

                    }
                }
                pin_warnings[i] = true;
                pin_alarms[i] = true;
            }
            toolStripStatusLabel2.Text = "Свързване...";
            toolStripStatusLabel2.ForeColor = Color.Black;
            toolStripStatusLabel2.Font = new Font(toolStripStatusLabel2.Font, FontStyle.Regular);
            reload_table();
            draw_graph();
            start = true;
            date = DateTime.Now.ToString();
        }

        public void Form1_Load(object sender, EventArgs e)
        {
            Version ver = new Version(Application.ProductVersion);
            thisversion = ver.Major + "." + ver.Minor + ver.Build;
            
            dataSet1.ReadXmlSchema(schemafile);
            dataSet1.ReadXml(xmlfile);

            ip = dataSet1.Tables["settings"].Rows[0][0].ToString();
            community = dataSet1.Tables["settings"].Rows[0][1].ToString();
            timeout = Convert.ToInt32(dataSet1.Tables["settings"].Rows[0][2]);
            retries = Convert.ToInt32(dataSet1.Tables["settings"].Rows[0][3]);
            firstrun = Convert.ToBoolean(dataSet1.Tables["settings"].Rows[0]["first_run"]);
            hist = Convert.ToInt32(dataSet1.Tables["settings"].Rows[0]["hist"]);
            play_sounds = Convert.ToBoolean(dataSet1.Tables["settings"].Rows[0]["sounds"]);
            show_popups = Convert.ToBoolean(dataSet1.Tables["settings"].Rows[0]["popups"]);
            rigid_graph = Convert.ToBoolean(dataSet1.Tables["settings"].Rows[0][8]);
            upper_limit = Convert.ToInt32(dataSet1.Tables["settings"].Rows[0][9]);
            lower_limit = Convert.ToInt32(dataSet1.Tables["settings"].Rows[0][10]);
            for (int i = 0; i <= 7; i++)
            {
                pins.Add(new List<int>());
                pin_icons[i] = new NotifyIcon();
                if ((bool)dataSet1.Tables["sensors"].Rows[i]["tray"])
                {
                    pin_icons[i].Text = dataSet1.Tables["sensors"].Rows[i]["descr"].ToString();
                    pin_icons[i].MouseDoubleClick += new MouseEventHandler(notifyIcon1_DoubleClick);
                    pin_icons[i].ContextMenuStrip = contextMenuStrip1;
                    pin_icons[i].Visible = true;
                    if (notifyIcon1.Visible)
                    {
                        notifyIcon1.Visible = false;
                    }
                }
                else
                {
                    pin_icons[i].Visible = false;
                }
                pin_warnings[i] = true;
                pin_alarms[i] = true;
            }
            run = 0;
            check_1 = DateTime.Now;
            DateTime start_date = new DateTime();
            start_date = DateTime.Now.AddHours(-2.0);
            dateTimePicker1.Value = start_date;
            dateTimePicker1.Checked = false;

            Application.ApplicationExit += new EventHandler(Application_ApplicationExit);

            update_check(true);
        }

        private void update_check(bool first)
        {
            string url = "http://jacket.megalan.bg/fmon/?check_version";
            if (!first)
            {
                url += "&running";
            }
            string current_version = null;
            bool update_error = false;
            try
            {
                WebClient client = new WebClient();
                current_version = client.DownloadString(url);
            }
            catch (Exception)
            {
                //update_error = true;
                //notifyIcon2.Visible = true;
                //notifyIcon2.BalloonTipIcon = ToolTipIcon.Error;
                //notifyIcon2.BalloonTipText = "Няма връзка с ъпдейт сървъра:\n";
                //notifyIcon2.BalloonTipText += ex.Message.ToString();
                //notifyIcon2.BalloonTipTitle = "Fire Monitor";
                //notifyIcon2.ShowBalloonTip(5000);
            }
            if (!update_error)
            {
                if (current_version != thisversion)
                {
                    notifyIcon2.Visible = true;
                    notifyIcon2.BalloonTipIcon = ToolTipIcon.Info;
                    notifyIcon2.BalloonTipText = "Нова версия на програмата " + current_version + "\r\n";
                    notifyIcon2.BalloonTipText += "Текуща версия: " + thisversion + "\r\n";
                    notifyIcon2.BalloonTipText += "Натиснете за сваляне";
                    notifyIcon2.BalloonTipTitle = "Fire Monitor";
                    notifyIcon2.ShowBalloonTip(5000);
                }
            }
        }

        void Application_ApplicationExit(object sender, EventArgs e)
        {
            if (notifyIcon1 != null)
            {
                try
                {
                    notifyIcon1.Visible = false;
                    notifyIcon1.Dispose();
                    notifyIcon1 = null;
                }
                catch (Exception)
                {

                }
            }
            if (notifyIcon2 != null)
            {
                try
                {
                    notifyIcon2.Visible = false;
                    notifyIcon2.Dispose();
                    notifyIcon2 = null;
                }
                catch (Exception)
                {

                }
            }
            for (int i = 0; i <= 7; i++)
            {
                if (pin_icons[i] != null)
                {
                    pin_icons[i].Visible = false;
                    pin_icons[i].Dispose();
                    pin_icons[i] = null;
                }
            }
        }

       private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            mysnmp snmp_obj = new mysnmp();
            string pattern = @"^([1-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])(\.([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])){3}$";
            Regex check = new Regex(pattern);
            bool valid = false;
            bool hasSensors = false;
            if (ip == "")
            {
                valid = false;
            }
            else
            {
                valid = true;
                if (!check.IsMatch(ip, 0))
                {
                    try
                    {
                        IPHostEntry he = Dns.GetHostEntry(ip);
                        IPAddress[] ip_addrs = he.AddressList;
                        foreach (IPAddress ipa in ip_addrs)
                        {
                            ip = ipa.ToString();
                        }
                    }
                    catch (System.Exception)
                    {
                        e.Result = "false|false|false|false|false|false|false|false|Unable to resolve " + ip + "!";
                        valid = false;
                        return;
                    }
                }
            }
            string[] results = new string[9];
            if (valid)
            {
                DataView dv = new DataView();
                dv.Table = dataSet1.Tables["sensors"];

                for (int i=1;i<=8;i++)
                {
                    if (Convert.ToInt32(dv[i - 1][1]) == 0)
                    {
                        results[i - 1] = "0";
                    }
                    else
                    {
                        hasSensors = true;
                        results[i - 1] = snmp_obj.get(ip, community, ".1.3.6.1.4.1.19865.1.2.3." + i.ToString() + ".0", timeout, retries);
                    }
                }
                results[8] = snmp_obj.getLastErr();
                string rrr = string.Join("|", results);
                e.Result = rrr;
            }
            else
            {
                e.Result = "false|false|false|false|false|false|false|false|Некоректен или невъведен IP адрес!";
            }
            if (!hasSensors)
            {
                e.Result = "false|false|false|false|false|false|false|false|Няма избрани датчици!";
            }
            if (backgroundWorker1.CancellationPending)
            {
                foreach (Process clsProcess in Process.GetProcesses())
                {
                    if (clsProcess.ProcessName.StartsWith("Fire Monitor"))
                    {
                        clsProcess.Kill();
                    }
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!backgroundWorker1.IsBusy)
            {
                backgroundWorker1.RunWorkerAsync(null);
            }
            else
            {
                if (!backgroundWorker1.CancellationPending)
                {
                    timer1.Stop();
                    timer1.Start();
                }
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            for (int i = 0; i <= 7; i++)
            {
                if ((bool)dataSet1.Tables["sensors"].Rows[i]["tray"])
                {
                    pin_icons[i].Visible = false;
                }
            }
            timer1.Stop();
            notifyIcon1.Visible = false;
            notifyIcon2.Visible = false;
            backgroundWorker1.CancelAsync();
            Application.Exit();
        }

        public void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            bool show_warning_tip = true;
            for (int i = 0; i <= 7; i++)
            {
                if (Convert.ToInt32(dataSet1.Tables["sensors"].Rows[i]["type"]) > 0)
                {
                    show_warning_tip = false;
                    break;
                }
            }
            if (show_warning_tip && !warning_tip_shown)
            {
                ToolTip tip = new ToolTip();
                tip.IsBalloon = true;
                tip.ToolTipIcon = ToolTipIcon.Info;
                tip.ToolTipTitle = "Датчици";
                tip.Show("Моля изберете датчици за мониторинг от настройките", label1, 0, -105, 10000);
                warning_tip_shown = true;
            }
            string[] results = e.Result.ToString().Split(new string[] { "|" }, StringSplitOptions.None);
            string lasterror = results[8];
            for (int i = 0; i <= 7; i++)
            {
                if (results[i] == "false")
                {
                    toolStripStatusLabel2.Text = "Грешка (" + lasterror + ")";
                    toolStripStatusLabel2.ForeColor = Color.Red;
                    toolStripStatusLabel2.Font = new Font(toolStripStatusLabel2.Font, FontStyle.Bold);
                    return;
                }
            }

            TimeSpan check_2 = DateTime.Now - check_1;

            if (check_2.Seconds < 20)
            {
                for (int i = 0; i <= 7; i++)
                {
                    pins[i].Add(Convert.ToInt32(results[i]));
                    if (pins_last[i] != null)
                    {
                        if (Convert.ToInt32(dataSet1.Tables["sensors"].Rows[i]["type"]) == 4)
                        {
                            if (Convert.ToInt32(results[i]) >= 1000 && Convert.ToInt32(pins_last[i]) < 1000 ||
                                Convert.ToInt32(results[i]) < 1000 && Convert.ToInt32(pins_last[i]) >= 1000)
                            {
                                ToolTip balloon = new ToolTip();
                                balloon.IsBalloon = true;
                                balloon.ToolTipIcon = ToolTipIcon.Info;
                                balloon.ToolTipTitle = "Информация";
                                string balloontext = "";
                                if (Convert.ToInt32(results[i]) >= 1000)
                                {
                                    balloontext = dataSet1.Tables["sensors"].Rows[i]["descr"].ToString() + " се включи...";
                                }
                                else
                                {
                                    balloontext = dataSet1.Tables["sensors"].Rows[i]["descr"].ToString() + " се изключи...";
                                }
                                if ((bool)dataSet1.Tables["sensors"].Rows[i]["tray"])
                                {
                                    pin_icons[i].ShowBalloonTip(5000, "Информация", balloontext, ToolTipIcon.Info);
                                }
                                else
                                {
                                    notifyIcon1.ShowBalloonTip(5000, "Информация", balloontext, ToolTipIcon.Info);
                                }
                                if (play_sounds)
                                {
                                    sound_switch.Play();
                                }
                                reload_table();
                            }
                        }
                    }
                }
                run++;
            }
            else
            {
                reload_table();
                check_1 = DateTime.Now;
                pins.Clear();
                for (int i = 0; i <= 7; i++)
                    pins.Add(new List<int>());
            }
            if (start)
            {
                reload_table();
                start = false;
            }

            TimeSpan total = DateTime.Now - DateTime.Parse(date);
            string tstripLabel = "Свързан от ";
            if (total.Hours == 0 && total.Minutes > 0)
            {
                if (total.Minutes == 1)
                {
                    tstripLabel += "1 минута";
                }
                else if (total.Minutes > 1)
                {
                    tstripLabel += "" + total.Minutes.ToString() + " минути";
                }
            }
            else if (total.Hours == 1)
            {
                tstripLabel += "1 час";
                if (total.Minutes == 1)
                {
                    tstripLabel += " и 1 минута";
                }
                else if (total.Minutes > 1)
                {
                    tstripLabel += " и " + total.Minutes.ToString() + " минути";
                }
            }
            else if (total.Hours > 1)
            {
                tstripLabel += total.Hours.ToString() + " часа";
                if (total.Minutes == 1)
                {
                    tstripLabel += " и 1 минута";
                }
                else if (total.Minutes > 1)
                {
                    tstripLabel += " и " + total.Minutes.ToString() + " минути";
                }
            }
            if (total.Hours == 0 && total.Minutes == 0)
            {
                if (total.Seconds == 1)
                {
                    tstripLabel += "1 секунда";
                }
                else if (total.Seconds > 1)
                {
                    tstripLabel += total.Seconds.ToString() + " секунди";
                }
            }

            toolStripStatusLabel2.Text = tstripLabel;
            toolStripStatusLabel2.Font = new Font(toolStripStatusLabel2.Font, FontStyle.Regular);
            if (toolStripStatusLabel2.ToolTipText == null)
            {
                toolStripStatusLabel2.ToolTipText = "Свързан на " + date;
            }
            toolStripStatusLabel2.ForeColor = Color.Green;
            for (int i = 0; i <= 7; i++)
            {
                pins_last[i] = results[i];
            }
        }

        public void reload_table()
        {
            dataSet1.tmp_data.Clear();
            string[] avg_pins = new string[8];
            tooltip_lines = "Fire monitor\n";
            for (int i = 0, pin = 1; i <= 7; i++, pin++)
            {
                int sensor_type = (int)dataSet1.Tables["sensors"].Rows[i]["type"];
                if (sensor_type == 0)
                {
                    avg_pins[i] = "0";
                    continue;
                }
                bool show_tray = (bool)dataSet1.Tables["sensors"].Rows[i]["tray"];
                double warn = (double)dataSet1.Tables["sensors"].Rows[i]["warn"];
                double alarm = (double)dataSet1.Tables["sensors"].Rows[i]["alarm"];
                bool sounds = (bool)dataSet1.Tables["settings"].Rows[0]["sounds"];
                string descr = dataSet1.Tables["sensors"].Rows[i]["descr"].ToString();
                string sensor_cdef = dataSet1.Tables["sensor_types"].Rows[sensor_type]["cdef"].ToString();
                string symbol = dataSet1.Tables["sensor_types"].Rows[sensor_type]["symbol"].ToString();
                string cdef = dataSet1.Tables["sensors"].Rows[i]["cdef"].ToString();
                string sensor_color = dataSet1.Tables["sensors"].Rows[i]["color"].ToString();
                int avg_pin = 0;
                for (int s = 0; s < pins[i].Count; s++)
                {
                    avg_pin += pins[i][s];
                }
                if(pins[i].Count > 0){
                    avg_pin = avg_pin / pins[i].Count;
                }

                DataRow row = dataSet1.tmp_data.NewRow();

                double pin_result = Convert.ToDouble(avg_pin);

                if (cdef.Length > 0)
                {
                    pin_result = calculate_cdef(pin_result, cdef);
                }

                if (sensor_cdef.Length > 0)
                {
                    pin_result = calculate_cdef(pin_result, sensor_cdef);
                }

                Bitmap color_rect = new Bitmap(16, 16);
                Color bgcolor = System.Drawing.ColorTranslator.FromHtml(sensor_color);
                Color bordercolor = ContrastColor(bgcolor);
                SolidBrush bru = new SolidBrush(bgcolor);
                Graphics graphicss = Graphics.FromImage(color_rect);
                graphicss.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                graphicss.FillRectangle(bru, 0, 0, 16, 16);
                try
                {
                    byte[] color_img = new byte[0];
                    MemoryStream mss = new MemoryStream();
                    color_rect.Save(mss, System.Drawing.Imaging.ImageFormat.Gif);
                    color_img = mss.ToArray();
                    row["color"] = color_img;
                }
                catch (Exception)
                {
                    //toolStripStatusLabel2.Text = "Грешка " + e;
                }
            

                pin_result = Math.Round(pin_result, 1);
               
                if (sensor_type == 1 || sensor_type == 2)
                {
                    byte[] img = new byte[0];
                    MemoryStream ms = new MemoryStream();
                    Properties.Resources.temperature.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
                    img = ms.ToArray();
                    row["image"] = img;
                    row["sensor"] = descr;
                    dataSet1.tmp_data.Rows.Add(row);
                    if (show_tray)
                    {
                        Bitmap bitmap = new Bitmap(16, 16);
                        SolidBrush brush = new SolidBrush(System.Drawing.ColorTranslator.FromHtml(sensor_color));
                        Graphics graphics = Graphics.FromImage(bitmap);
                        graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit; ;
                        graphics.DrawString(Math.Round(pin_result).ToString(), new Font("Tahoma", 8, FontStyle.Regular), brush, 0, 0);
                        IntPtr hIcon = new IntPtr();
                        try
                        {
                            hIcon = bitmap.GetHicon();
                            Icon icon = Icon.FromHandle(hIcon);
                            pin_icons[i].Icon = icon;
                            if (!pin_icons[i].Visible)
                            {
                                pin_icons[i].Visible = true;
                            }
                        }
                        catch (Exception)
                        {
                            //toolStripStatusLabel2.Text = "Грешка " + e;
                        }
                    }
                    row["result"] = pin_result.ToString() + symbol;
                    tooltip_lines += row["sensor"] + ": " + row["result"] + "\n";
                    if (pin_icons[i] != null)
                        pin_icons[i].Text = row["sensor"] + ": " + row["result"];
                    if (pin_result >= warn && pin_result < alarm && pin_warnings[i])
                    {
                        show_baloon(false, descr + ": Температура " + row["result"], show_tray, i);
                        pin_warnings[i] = false;
                    }
                    else if (pin_result >= alarm && pin_alarms[i])
                    {
                        show_baloon(true, descr + ": Температура " + row["result"], show_tray, i);
                        if (show_popups)
                        {
                            Form4 f4 = new Form4();
                            f4.Descr = descr;
                            f4.Sound = sounds;
                            f4.Value = pin_result.ToString();
                            f4.Show();
                        }
                        pin_alarms[i] = false;
                    }
                    if (!pin_warnings[i] && pin_result < warn - hist)
                    {
                        pin_warnings[i] = true;
                    }
                    if (!pin_alarms[i] && pin_result < alarm - hist)
                    {
                        Form4 f4 = new Form4();
                        f4.Close();
                        pin_alarms[i] = true;
                    }
                    avg_pins[i] = avg_pin.ToString();
                }
                else if (sensor_type == 3)
                {
                    byte[] img = new byte[0];
                    MemoryStream ms = new MemoryStream();
                    Properties.Resources.hum_old.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
                    img = ms.ToArray();
                    row["image"] = img;
                    row["sensor"] = descr;
                    row["result"] = pin_result.ToString() + symbol;
                    dataSet1.tmp_data.Rows.Add(row);
                    if (pin_icons[i] != null)
                        pin_icons[i].Text = row["sensor"] + ": " + row["result"];
                    if (show_tray)
                    {
                        Bitmap bitmap = new Bitmap(16, 16);
                        SolidBrush brush = new SolidBrush(System.Drawing.ColorTranslator.FromHtml(sensor_color));
                        Graphics graphics = Graphics.FromImage(bitmap);
                        graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit; ;
                        graphics.DrawString(Math.Round(pin_result).ToString(), new Font("Tahoma", 8, FontStyle.Regular), brush, 0, 0);
                        IntPtr hIcon = new IntPtr();
                        try
                        {
                            hIcon = bitmap.GetHicon();
                            Icon icon = Icon.FromHandle(hIcon);
                            pin_icons[i].Icon = icon;
                            if (!pin_icons[i].Visible)
                            {
                                pin_icons[i].Visible = true;
                            }
                        }
                        catch (Exception)
                        {
                            //toolStripStatusLabel2.Text = "Грешка " + e;
                        }
                        pin_icons[i].Text = row["sensor"] + ": " + row["result"];
                    }
                    if (pin_result >= warn && pin_result < alarm && pin_warnings[i])
                    {
                        show_baloon(false, descr + ": Влажност " + row["result"], show_tray, i);
                        pin_warnings[i] = false;
                    }
                    else if (pin_result >= alarm && pin_alarms[i])
                    {
                        if (show_popups)
                        {
                            Form4 f4 = new Form4();
                            f4.Descr = descr;
                            f4.Sound = sounds;
                            f4.Value = pin_result.ToString();
                            f4.Show();
                        }
                        pin_alarms[i] = false;
                    }
                    if (!pin_warnings[i] && pin_result < warn - hist)
                    {
                        pin_warnings[i] = true;
                    }
                    if (!pin_alarms[i] && pin_result < alarm - hist)
                    {
                        Form4 f4 = new Form4();
                        f4.Close();
                        pin_alarms[i] = true;
                    }
                    avg_pins[i] = avg_pin.ToString();
                }
                else if (sensor_type == 5)
                {
                    byte[] img = new byte[0];
                    MemoryStream ms = new MemoryStream();
                    Properties.Resources.icon_battery.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
                    img = ms.ToArray();
                    row["image"] = img;
                    row["sensor"] = descr;
                    row["result"] = pin_result.ToString() + symbol;
                    dataSet1.tmp_data.Rows.Add(row);
                    if (pin_icons[i] != null)
                        pin_icons[i].Text = row["sensor"] + ": " + row["result"];
                    if (show_tray)
                    {
                        Bitmap bitmap = new Bitmap(16, 16);
                        SolidBrush brush = new SolidBrush(System.Drawing.ColorTranslator.FromHtml(sensor_color));
                        Graphics graphics = Graphics.FromImage(bitmap);
                        graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                        brush.Color = Color.Orange;
                        graphics.DrawString(Math.Round(pin_result).ToString(), new Font("Tahoma", 8, FontStyle.Regular), brush, 0, 0);
                        IntPtr hIcon = new IntPtr();
                        try
                        {
                            hIcon = bitmap.GetHicon();
                            Icon icon = Icon.FromHandle(hIcon);
                            pin_icons[i].Icon = icon;
                            if (!pin_icons[i].Visible)
                            {
                                pin_icons[i].Visible = true;
                            }
                        }
                        catch (Exception)
                        {
                            //toolStripStatusLabel2.Text = "Грешка " + e;
                        }
                        pin_icons[i].Text = row["sensor"] + ": " + row["result"];
                    }
                    avg_pins[i] = avg_pin.ToString();
                }
                else if (sensor_type == 4)
                {
                    if (show_tray)
                    {
                        Bitmap bitmap = new Bitmap(16, 16);
                        SolidBrush brush = new SolidBrush(System.Drawing.ColorTranslator.FromHtml(sensor_color));
                        Graphics graphics = Graphics.FromImage(bitmap);
                        graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit; ;
                        Font fnt = new Font("tahoma", 8, FontStyle.Regular);
                        if (pins[i].Count > 0)
                        {
                            if (pins[i][pins[i].Count - 1] >= 1000)
                            {
                                graphics.DrawString("On", fnt, brush, 0, 0);
                            }
                            else
                            {
                                graphics.DrawString("Off", fnt, brush, 0, 0);
                            }
                        }
                        IntPtr hIcon = new IntPtr();
                        try
                        {
                            hIcon = bitmap.GetHicon();
                            Icon icon = Icon.FromHandle(hIcon);
                            pin_icons[i].Icon = icon;
                            if (!pin_icons[i].Visible)
                            {
                                pin_icons[i].Visible = true;
                            }
                        }
                        catch (Exception)
                        {
                            //toolStripStatusLabel2.Text = "Грешка " + e;
                        }
                    }
                    byte[] img = new byte[0];
                    MemoryStream ms = new MemoryStream();
                    if (pins[i].Count > 0)
                    {
                        if (pins[i][pins[i].Count - 1] >= 1000)
                        {
                            Properties.Resources.switch_on.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
                            row["result"] = "Работи";
                            avg_pins[i] = "10";
                        }
                        else
                        {
                            Properties.Resources.switch_off.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
                            row["result"] = "Не работи";
                            avg_pins[i] = "0";
                        }
                    }
                    row["sensor"] = descr;
                    tooltip_lines += row["sensor"] + ": " + row["result"] + "\n";
                    img = ms.ToArray();
                    row["image"] = img;
                    if (pin_icons[i] != null)
                        pin_icons[i].Text = row["sensor"] + ": " + row["result"];
                    dataSet1.tmp_data.Rows.Add(row);
                }
                else
                {
                    avg_pins[i] = "0";
                }
            }
            //notifyIcon1.Text = tooltip_lines.Substring(0, tooltip_lines.Length - 1
            run = 0;
            tmpdataBindingSource.DataSource = dataSet1.tmp_data;
            rrdtool rt = new rrdtool();
            rt.setExe(Application.StartupPath + "\\rrdtool.exe");
            if (!rt.rrdExsists(Application.StartupPath + "\\sensors.rrd"))
            {
                string command = "--start N ";
                command += "--step=20 ";
                command += "DS:pin1:GAUGE:80:-1000:U ";
                command += "DS:pin2:GAUGE:80:-1000:U ";
                command += "DS:pin3:GAUGE:80:-1000:U ";
                command += "DS:pin4:GAUGE:80:-1000:U ";
                command += "DS:pin5:GAUGE:80:-1000:U ";
                command += "DS:pin6:GAUGE:80:-1000:U ";
                command += "DS:pin7:GAUGE:80:-1000:U ";
                command += "DS:pin8:GAUGE:80:-1000:U ";
                command += "RRA:AVERAGE:0.5:1:60540 ";
                command += "RRA:AVERAGE:0.5:3:43800 ";
                command += "RRA:AVERAGE:0.5:15:52560 ";
                command += "RRA:AVERAGE:0.5:30:52596 ";
                command += "RRA:AVERAGE:0.5:180:43830 ";
                rt.create(Application.StartupPath + "\\sensors.rrd", command);
            }
            else
            {
                string command = "N:" + avg_pins[0] + ":" + avg_pins[1] + ":" + avg_pins[2] + ":" + avg_pins[3] + ":" + avg_pins[4] + ":" + avg_pins[5] + ":" + avg_pins[6] + ":" + avg_pins[7];
                rt.update(Application.StartupPath + "\\sensors.rrd", command);
            }
            bool dtcheck = dateTimePicker2.Checked;
            if (!dtcheck)
            {
                dateTimePicker2.Enabled = false;
                dateTimePicker2.Value = DateTime.Now;
                dateTimePicker2.Checked = dtcheck;
                dateTimePicker2.Enabled = true;
            }

            bool dtcheck1 = dateTimePicker1.Checked;
            if (!dtcheck1)
            {
                dateTimePicker1.Enabled = false;
                dateTimePicker1.Value = DateTime.Now.AddHours(-2.0);
                dateTimePicker1.Checked = dtcheck1;
                dateTimePicker1.Enabled = true;
            }
        }

        private double calculate_cdef(double pin_result, string cdef)
        {
            cdef = cdef.Replace(" ","");
            string[] cdef_arr = new string[10];
            Stack stack = new Stack();
            cdef_arr = cdef.Split(new string[] { "," }, StringSplitOptions.None);
            stack.Push(pin_result);
            for (int i = 0; i <= cdef_arr.Length-1; i++)
            {
                switch (cdef_arr[i])
                {
                    case "+":
                        try
                        {
                            double operand2 = (double)stack.Pop();
                            double operand1 = (double)stack.Pop();
                            stack.Push(operand1 + operand2);
                        }
                        catch (Exception) { }
                        break;
                    case "-":
                        try
                        {
                            double operand2 = (double)stack.Pop();
                            double operand1 = (double)stack.Pop();
                            stack.Push(operand1 - operand2);
                        }
                        catch (Exception) { }
                        break;
                    case "*":
                        try
                        {
                            double operand2 = (double)stack.Pop();
                            double operand1 = (double)stack.Pop();
                            stack.Push(operand1 * operand2);
                        }
                        catch (Exception) { }
                        break;
                    case "/":
                        try
                        {
                            double operand2 = (double)stack.Pop();
                            double operand1 = (double)stack.Pop();
                            stack.Push(operand1 / operand2);
                        }
                        catch (Exception) { }
                        break;
                    default:
                        try
                        {
                            string sep = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
                            if (sep != ".")
                            {
                                cdef_arr[i] = cdef_arr[i].Replace(".", sep);
                            }
                            double tmp_operand = Convert.ToDouble(cdef_arr[i]);
                            stack.Push(tmp_operand);
                        }
                        catch (Exception e)
                        {
                            
                            StreamWriter sw;
                            sw = File.AppendText("cdef_errors.log");
                            sw.WriteLine(DateTime.Now + ": " + "cdef error: '" + cdef + "': " + e.Message.ToString() + " Last operand:" + cdef_arr[i]);
                            sw.Close();
                        }
                        break;
                }
            }
            try
            {
                pin_result = Convert.ToDouble(stack.Pop());
            }
            catch (Exception)
            {
            }
            return pin_result;
        }

        public void draw_graph()
        {
            rrdtool rt = new rrdtool();
            rt.setExe(Application.StartupPath + "\\rrdtool.exe");
            string graph_command = "--imgformat=PNG ";
            if (!dateTimePicker1.Checked)
            {
                graph_command += "--start=-7200 ";
            }
            else
            {
                int unixTime = (int)(dateTimePicker1.Value.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds;
                graph_command += "--start=" + unixTime.ToString() + " ";
            }
            if (!dateTimePicker2.Checked)
            {
                graph_command += "--end=N ";
            }
            else
            {
                int unixTime = (int)(dateTimePicker2.Value.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds;
                graph_command += "--end=" + unixTime.ToString() + " ";

            }

            int graph_width = pictureBox1.Width - 68;
            int graph_height = pictureBox1.Height - 61;

            graph_command += "--height=" + graph_height.ToString() + " ";
            graph_command += "--width=" + graph_width.ToString() + " ";
            if (rigid_graph)
            {
                graph_command += "--rigid ";
                graph_command += "--upper-limit=" + upper_limit + " ";
                graph_command += "--lower-limit=" + lower_limit + " ";
            }
            else
            {
                graph_command += "--alt-autoscale-max ";
                graph_command += "--lower-limit=0 ";
            }
            graph_command += "--slope-mode ";
            graph_command += "--no-gridfit ";
            graph_command += "--font DEFAULT:8:\"" + Application.StartupPath.Replace("\\", "/").ToLower() + "/consola.ttf\" ";
            graph_command += "--color BACK#" + string.Format("{0:X2}{1:X2}{2:X2}", System.Drawing.SystemColors.Control.R, System.Drawing.SystemColors.Control.G, System.Drawing.SystemColors.Control.B) + " ";
            graph_command += "--color FONT#" + string.Format("{0:X2}{1:X2}{2:X2}", System.Drawing.SystemColors.ControlText.R, System.Drawing.SystemColors.ControlText.G, System.Drawing.SystemColors.ControlText.B) + " ";
            graph_command += "--color SHADEA#" + string.Format("{0:X2}{1:X2}{2:X2}", System.Drawing.SystemColors.Control.R, System.Drawing.SystemColors.Control.G, System.Drawing.SystemColors.Control.B) + " ";
            graph_command += "--color SHADEB#" + string.Format("{0:X2}{1:X2}{2:X2}", System.Drawing.SystemColors.Control.R, System.Drawing.SystemColors.Control.G, System.Drawing.SystemColors.Control.B) + " ";
            for (int i = 0, pin = 1; i <= 7; i++, pin++)
            {
                if (Convert.ToInt32(dataSet1.Tables["sensors"].Rows[i]["type"]) > 0 && Convert.ToBoolean(dataSet1.Tables["sensors"].Rows[i]["graphing"]))
                {
                    string defname = "";
                    switch (i)
                    {
                        case 0:
                            defname = "a";
                            break;
                        case 1:
                            defname = "b";
                            break;
                        case 2:
                            defname = "c";
                            break;
                        case 3:
                            defname = "d";
                            break;
                        case 4:
                            defname = "e";
                            break;
                        case 5:
                            defname = "f";
                            break;
                        case 6:
                            defname = "g";
                            break;
                        case 7:
                            defname = "h";
                            break;
                    }
                    int sensor_type = Convert.ToInt32(dataSet1.Tables["sensors"].Rows[i]["type"]);
                    string sensor_cdef = dataSet1.Tables["sensor_types"].Rows[sensor_type]["cdef"].ToString();
                    string extra_cdef = dataSet1.Tables["sensors"].Rows[i]["cdef"].ToString();
                    int line_area = Convert.ToInt32(dataSet1.Tables["settings"].Rows[0]["line_area"]);
                    string line_area_string;
                    graph_command += "DEF:" + defname + "=\"" + Application.StartupPath.Replace("\\", "/").Replace(":", "\\:").ToLower() + "/sensors.rrd\":pin" + pin + ":AVERAGE ";
                    sensor_cdef = (sensor_cdef == "") ? "" : "," + sensor_cdef;
                    extra_cdef = (extra_cdef == "") ? "" : "," + extra_cdef;
                    graph_command += "CDEF:cdef" + defname + "=" + defname + extra_cdef + sensor_cdef + " ";
                    graph_command += "LINE" + dataSet1.Tables["settings"].Rows[0]["line_width"].ToString() + ":cdef" + defname + dataSet1.Tables["sensors"].Rows[i]["color"] + " ";
                    if (line_area > 0)
                    {
                        if(line_area < 10)
                        {
                            line_area_string = "0" + line_area.ToString();
                        }else
                        {
                            line_area_string = line_area.ToString();
                        }

                        graph_command += "AREA:cdef" + defname + dataSet1.Tables["sensors"].Rows[i]["color"].ToString() + line_area_string + " ";
                    }
                }
            }
            rt.graph(Application.StartupPath + "\\graph.png", graph_command);
            if (File.Exists(Application.StartupPath + "\\graph.png"))
            {
                FileStream fs = new FileStream(Application.StartupPath + "\\graph.png", FileMode.Open, FileAccess.Read);
                Image graph_img = Image.FromStream(fs);
                fs.Close();
                pictureBox1.Image = graph_img;
            }
            pictureBox1.Refresh();
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Form2 f2 = new Form2(this);
            f2.ShowDialog();
        }

        private void dateTimePicker1_ValueChanged(object sender,EventArgs e)
        {
            draw_graph();
        }

        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            draw_graph();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                WindowState = FormWindowState.Minimized;
                Hide();
            }
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            if (FormWindowState.Normal == WindowState)
            {
                Hide();
                WindowState = FormWindowState.Minimized;
            }
            else if (FormWindowState.Minimized == WindowState)
            {
                Show();
                WindowState = FormWindowState.Normal;
            }
        }

        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            for (int i = 0; i <= 7; i++)
            {
                if ((bool)dataSet1.Tables["sensors"].Rows[i]["tray"])
                {
                    pin_icons[i].Visible = false;
                }
            }
            timer1.Stop();
            notifyIcon1.Visible = false;
            notifyIcon2.Visible = false;
            backgroundWorker1.CancelAsync();
            Application.Exit();
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            Form2 f2 = new Form2(this);
            f2.ShowDialog();
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            Form3 f3 = new Form3();
            f3.ShowDialog();
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            Form3 f3 = new Form3();
            f3.ShowDialog();
        }

        private void show_baloon(bool alarm, string text, bool show_tray, int i)
        {
            if (show_tray)
            {
                if (alarm)
                {
                    pin_icons[i].BalloonTipIcon = ToolTipIcon.Error;
                    pin_icons[i].BalloonTipTitle = "Аларма";
                }
                else
                {
                    pin_icons[i].BalloonTipIcon = ToolTipIcon.Warning;
                    pin_icons[i].BalloonTipTitle = "Предупреждение";
                    if (play_sounds)
                        sound_warn.Play();
                }
                pin_icons[i].BalloonTipText = text;
                pin_icons[i].ShowBalloonTip(10000);
            }
            else
            {
                if (alarm)
                {
                    notifyIcon1.BalloonTipIcon = ToolTipIcon.Error;
                    notifyIcon1.BalloonTipTitle = "Аларма";
                }
                else
                {
                    notifyIcon1.BalloonTipIcon = ToolTipIcon.Warning;
                    notifyIcon1.BalloonTipTitle = "Предупреждение";
                    if (play_sounds)
                        sound_warn.Play();
                }
                notifyIcon1.BalloonTipText = text;
                notifyIcon1.ShowBalloonTip(5000);
            }
        }
        public static Form IsFormAlreadyOpen(Type FormType)
        {
            foreach (Form OpenForm in Application.OpenForms)
            {
                if (OpenForm.GetType() == FormType)
                    return OpenForm;
            }
            return null;
        }

        private void notifyIcon2_BalloonTipClicked(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://jacket.megalan.bg/fmon?dl");
            notifyIcon2.Visible = false;
        }

        private void notifyIcon2_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://jacket.megalan.bg/fmon?dl");
            notifyIcon2.Visible = false;
        }

        private void notifyIcon2_BalloonTipClosed(object sender, EventArgs e)
        {
            notifyIcon2.Visible = false;
        }

        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {
            saveFileDialog1.FileName = "graph_" + (int)(dateTimePicker1.Value.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds + "_" + (int)(dateTimePicker2.Value.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds + ".png";
            saveFileDialog1.ShowDialog();
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            string imagefile = saveFileDialog1.FileName;
            pictureBox1.Image.Save(saveFileDialog1.FileName, System.Drawing.Imaging.ImageFormat.Png);
        }

        private void toolStripMenuItem8_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            int unixStart = (int)(dateTimePicker1.Value.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds;
            int unixEnd = (int)(dateTimePicker2.Value.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds;
            string html = "";
            bool[] pins = new bool[8];

            Process p = new Process();
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.FileName = Application.StartupPath + "\\rrdtool.exe";
            p.StartInfo.Arguments = " fetch sensors.rrd AVERAGE -s " + unixStart.ToString() + " -e " + unixEnd.ToString();
            p.StartInfo.RedirectStandardOutput = true; 
            p.Start();
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            string[] results = output.ToString().Split(new string[] { "\n" }, StringSplitOptions.None);

            DataView dv = new DataView();
            dv.Table = dataSet1.Tables["sensors"];
            html += "<tr class=\"th\"><td>Дата</td>";
            List<List<double>> avg_pins = new List<List<double>>();
            for (int i = 1; i <= 8; i++)
            {
                avg_pins.Add(new List<double>());
                if (Convert.ToInt32(dv[i - 1]["type"]) != 0)
                {
                    html += "<td>" + dv[i - 1]["descr"].ToString() + "</td>";
                    pins[i-1] = true;
                }else
                {
                    pins[i-1] = false;
                }
            }
            html += "</tr>";
            string[] line_arr = new string[10];
            string trclass = "tr2";
            for (int i = 0; i < results.Length; i++)
            {
                trclass = (trclass == "tr2") ? "tr1" : "tr2";
                line_arr = results[i].Split(new string[] { " " }, StringSplitOptions.None);
                if (i <= 2 || line_arr.Length < 8 || i >= results.Length - 2) continue;
                html += "<tr class=\"" + trclass + "\"><td>" + new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(Convert.ToDouble(line_arr[0].Replace(":", "")) + 7200) + "</td>";
                for (int c = 1; c <= 8; c++)
                {
                    if (pins[c-1])
                    {
                        if (line_arr[c] == "-1.#IND000000e+000")
                        {
                            html += "<td>N/A</td>";
                        }
                        else
                        {
                            line_arr[c] = line_arr[c].Replace("\r", "");
                            string sep = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
                            if (sep != ".")
                            {
                                line_arr[c] = line_arr[c].Replace(".", sep);
                            }
                            int sensor_type = (int)dataSet1.Tables["sensors"].Rows[c - 1]["type"];
                            string cdef = dataSet1.Tables["sensors"].Rows[c - 1]["cdef"].ToString();
                            string sensor_cdef = dataSet1.Tables["sensor_types"].Rows[sensor_type]["cdef"].ToString();
                            double pin_result = Convert.ToDouble(line_arr[c]);
                            string symbol = dataSet1.Tables["sensor_types"].Rows[sensor_type]["symbol"].ToString();

                            if (cdef.Length > 0)
                            {
                                pin_result = calculate_cdef(pin_result, cdef);
                            }

                            if (sensor_cdef.Length > 0)
                            {
                                pin_result = calculate_cdef(pin_result, sensor_cdef);
                            }

                            else if (sensor_type == 4)
                            {
                                if (pin_result == 10)
                                {
                                    html += "<td>Вкл.</td>";
                                    continue;
                                }
                                else
                                {
                                    html += "<td>Изкл.</td>";
                                    continue;
                                }
                            }

                            avg_pins[c - 1].Add(pin_result);

                            pin_result = Math.Round(pin_result, 1);

                            html += "<td>" + pin_result.ToString() + symbol + "</td>";
                        }
                    }
                }
                html += "</tr>";
            }
            html += "<tr class=\"tf\">";
            html += "<td>Средно:</td>";
            for (int c = 1; c <= 8; c++)
            {
                if (pins[c - 1])
                {
                    int sensor_type = (int)dataSet1.Tables["sensors"].Rows[c - 1]["type"];
                    string symbol = dataSet1.Tables["sensor_types"].Rows[sensor_type]["symbol"].ToString();
                    if (sensor_type == 4)
                    {
                        html += "<td>N/A</td>";
                        continue;
                    }

                    double avg_pin = 0;
                    for (int s = 0; s < avg_pins[c - 1].Count; s++)
                    {
                        avg_pin += avg_pins[c - 1][s];
                    }
                    if (avg_pins[c - 1].Count > 0)
                    {
                        avg_pin = avg_pin / avg_pins[c - 1].Count;
                    }
                    avg_pin = Math.Round(avg_pin, 1);
                    html += "<td>" + avg_pin.ToString() + symbol + "</td>";
                }
            }
            html += "</tr>";
            string template = "";
            try
            {
                StreamReader sr = new StreamReader(Application.StartupPath + "\\export_template.html");
                template = sr.ReadToEnd();
                sr.Close();
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("Не е открит template файла. Моля преинсталирайте!", "Грешка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            template = template.Replace("<!--[template]!-->", html);
            StreamWriter sw = new StreamWriter(Application.StartupPath + "\\export.html");
            sw.Write(template);
            sw.Close();
            System.Diagnostics.Process.Start(Application.StartupPath + "\\export.html");
            this.Cursor = Cursors.Default;
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            update_check(false);
        }

        private void toolStripMenuItem9_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            string imagefile = Application.StartupPath + "\\graph.png";
            ImageshackUpload uploader = new ImageshackUpload();
            string url = uploader.UploadFileToImageShack(imagefile);
            Form6 f6 = new Form6();
            f6.Url = url;
            f6.Show();
            this.Cursor = Cursors.Default;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            draw_graph();
        }

        Color ContrastColor(Color color)
        {
            var c = new int[] { 255 - color.R, 255 - color.G, 255 - color.B };
            int d = 0;

            // Counting the perceptive luminance - human eye favors green color... 
            double a = 1 - (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;

            if (a < 0.5)
                d = 0; // bright colors - dark font
            else
                d = 255; // dark colors - white font

            return Color.FromArgb(d, d, d);
        }

        private void dataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {

        }

    }
}
