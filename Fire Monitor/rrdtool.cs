using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Jacket
{
    namespace Classes
    {
        class rrdtool
        {
            string exepath = null;

            public void setExe(string exe)
            {
                exepath = exe;
            }

            public bool rrdExsists(string file)
            {
                if (File.Exists(file))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public void create(string file, string args)
            {
                if (exepath != null)
                {
                    Process p = new Process();
                    p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    p.StartInfo.UseShellExecute = true;
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.FileName = "\"" + exepath + "\"";
                    p.StartInfo.Arguments = " create " + "\"" + file + "\" " + args;
                    p.Start();
                }
            }
            public void update(string file, string args)
            {
                if (exepath != null)
                {
                    Process p = new Process();
                    p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    p.StartInfo.UseShellExecute = true;
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.FileName = "\"" + exepath + "\"";
                    p.StartInfo.Arguments = " update " + "\"" + file + "\" " + args;
                    p.Start();
                }
            }
            public void graph(string file, string args)
            {
                Process p = new Process();
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo.UseShellExecute = true;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.FileName = "\"" + exepath + "\"";
                p.StartInfo.Arguments = " graph " + "\"" + file + "\" " + args;
                p.Start();
                p.WaitForExit();
            }
        }
    }
}