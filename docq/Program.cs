using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace docq
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Form1 frm = new Form1();
            Sunisoft.IrisSkin.SkinEngine skin = new Sunisoft.IrisSkin.SkinEngine((System.ComponentModel.Component)frm);
            skin.SkinFile = "***.ssk"; // 指定皮肤文件
            skin.TitleFont = new System.Drawing.Font("微软雅黑", 10F);// 指定标题栏的Font。
            Application.Run(frm); 
            //Application.Run(new Form1());
        }
    }
}
