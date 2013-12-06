using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Data;
using System.Diagnostics;
using System.IO;

namespace Fire_Monitor
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //string proc = Process.GetCurrentProcess().ProcessName;
            //Process[] processes = Process.GetProcessesByName(proc);
            //if (processes.Length > 1)
            //{
            //    foreach (Process clsProcess in Process.GetProcesses())
            //    {
            //        if (clsProcess.ProcessName.StartsWith(proc))
            //        {
            //            clsProcess.Kill();
            //        }
            //    }
            //}
            //foreach (string arg in Environment.GetCommandLineArgs())
            //{
            //    foreach (Process clsProcess in Process.GetProcesses())
            //    {
            //        if (clsProcess.ProcessName.StartsWith("Fire Monitor"))
            //        {
            //            if (arg == "-kill")
            //            {
            //                clsProcess.Kill();
            //                return;
            //            }
            //        }
            //    }
            //}

            DataSet1 dataSet1 = new DataSet1();
            string xmlfile = Application.StartupPath + "\\db.xml";
            string schemafile = Application.StartupPath + "\\db.xsd";
            try
            {
                dataSet1.ReadXmlSchema(schemafile);
                dataSet1.ReadXml(xmlfile);
                string dummy_check = dataSet1.Tables["sensor_types"].Rows[5]["cdef"].ToString();
            }
            catch (Exception ex)
            {
                dataSet1.Tables["sensor_types"].Clear();
                DataTable dt_sensors_type = dataSet1.Tables["sensor_types"];
                DataRow newrow1 = dt_sensors_type.NewRow();
                newrow1[0] = 0;
                newrow1[1] = "Свободен";
                newrow1[2] = "";
                newrow1[3] = "";
                newrow1[4] = "";
                dt_sensors_type.Rows.Add(newrow1);
                DataRow newrow2 = dt_sensors_type.NewRow();
                newrow2[0] = 1;
                newrow2[1] = "T LM35DZ";
                newrow2[2] = "3.16,/";
                newrow2[3] = "°C";
                newrow2[4] = "";
                dt_sensors_type.Rows.Add(newrow2);
                DataRow newrow3 = dt_sensors_type.NewRow();
                newrow3[0] = 2;
                newrow3[1] = "T LM335";
                newrow3[2] = "3.1,/,273.15,-";
                newrow3[3] = "°C";
                newrow3[4] = "";
                dt_sensors_type.Rows.Add(newrow3);
                DataRow newrow6 = dt_sensors_type.NewRow();
                newrow6[0] = 3;
                newrow6[1] = "H H5V6";
                newrow6[2] = "0.10624,*";
                newrow6[3] = "%";
                newrow6[4] = "";
                dt_sensors_type.Rows.Add(newrow6);
                DataRow newrow4 = dt_sensors_type.NewRow();
                newrow4[0] = 4;
                newrow4[1] = "Ключов";
                newrow4[2] = "";
                newrow4[3] = "";
                newrow4[4] = "";
                dt_sensors_type.Rows.Add(newrow4);
                DataRow newrow7 = dt_sensors_type.NewRow();
                newrow7[0] = 5;
                newrow7[1] = "12V Bat 1:5";
                newrow7[2] = "310,/,5,*";
                newrow7[3] = "V";
                newrow7[4] = "";
                dt_sensors_type.Rows.Add(newrow7);

                if (ex.Source == "mscorlib")
                {

                    DataTable dt_settings = dataSet1.Tables["settings"];
                    DataRow newrow5 = dt_settings.NewRow();
                    newrow5[0] = "172.16.100.2";
                    newrow5[1] = "000000000000";
                    newrow5[2] = 200;
                    newrow5[3] = 2;
                    newrow5[4] = 5;
                    newrow5[5] = true;
                    newrow5[6] = true;
                    newrow5[7] = true;
                    newrow5[8] = false;
                    newrow5[9] = 120.0;
                    newrow5[10] = 0.0;
                    dt_settings.Rows.Add(newrow5);

                    DataTable dt_sensors = dataSet1.Tables["sensors"];
                    for (int i = 1; i <= 8; i++)
                    {
                        DataRow newrow = dt_sensors.NewRow();
                        newrow[0] = i;
                        newrow[1] = 0;
                        newrow[2] = "";
                        newrow[3] = 80.00;
                        newrow[4] = 90.00;
                        newrow[5] = "#000000";
                        newrow[6] = false;
                        newrow[7] = "";
                        dt_sensors.Rows.Add(newrow);
                    }
                }

                dataSet1.WriteXmlSchema(schemafile);
                dataSet1.WriteXml(xmlfile);
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                Application.Run(new Form1());
            }
            catch (Exception e)
            {
                MessageBox.Show("Възникна неочаквана грешка и програмата ще се затвори!\nАко това се повтаря, изтрийте файла db.xml и настройте програмата отново.\nДетайли за грешката може да откриете във файла error.log.\n\nНакратко тя гласи:\n" + e.Message, "Опа", MessageBoxButtons.OK, MessageBoxIcon.Error);
                TextWriter tw = new StreamWriter("error.log");
                tw.Write(e.ToString());
                tw.Close();
            }
        }
    }
}
