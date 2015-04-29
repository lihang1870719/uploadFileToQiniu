using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace docq
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            String pathNames = "";
            if (args.Length > 0)
            {
                pathNames = args[0];
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Form1 frm = new Form1(pathNames);
            //frm.CheckCreated();
            if (System.Diagnostics.Process.GetProcessesByName(System.Diagnostics.Process.GetCurrentProcess().ProcessName).Length < 2)
            {
                Application.Run(frm);
            }
            else
            {
                Process[] p = System.Diagnostics.Process.GetProcessesByName(System.Diagnostics.Process.GetCurrentProcess().ProcessName);
                for (int i = 0; i < p.Length; i++)
                {
                    if (p[i].Id != System.Diagnostics.Process.GetCurrentProcess().Id)
                    {
                        p[i].Kill();
                    }         
                }
                Application.Run(frm);
            }
            
        }
    }
}
