using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Qiniu.RS;
using Qiniu.IO;
using Qiniu.IO.Resumable;
using Qiniu.RPC;
using Microsoft.Win32;
using Qiniu.Auth;
using System.Threading;
using System.Runtime.InteropServices;
using docq.BLL;

namespace docq
{
    public partial class Form1 : Form
    {
        public static CookieContainer cookie = new CookieContainer();
        public static bool result = false;
        public static String pathNames = "";

        public Form1(String pathname)
        {
            pathNames = pathname;
            InitializeComponent();
            InitializeTextBox();
            //AddFileContextMenuItem("上传到 DocQ", Application.StartupPath + "\\docq.exe \"%1\"");
            Console.WriteLine(Application.StartupPath + "\\docq.exe");
        }

        /// <summary>
        /// initial the textbox for email and password
        /// </summary>
        private void InitializeTextBox()
        {
            FileInfo infoUser = new FileInfo(Application.StartupPath + "\\userInfo.txt");
            if (!infoUser.Exists)
            {
                textBox1.Text = "";
                textBox2.Text = "";
            }
            else
            {
                String[] userLine = File.ReadAllLines(Application.StartupPath + "\\userInfo.txt");
                if (userLine.Length > 0)
                {
                    textBox1.Text = userLine[0];
                    textBox2.Text = userLine[1];
                }
                this.Hide();
                button1_Click(button1, EventArgs.Empty); 
                if (result)
                {
                    this.WindowState = FormWindowState.Minimized;
                    this.ShowInTaskbar = false;
                }
                else
                {
                    this.WindowState = FormWindowState.Normal;
                    this.ShowInTaskbar = true;
                }
               
            }
        }

        private void autoUploaded(object o)
        {
            HttpRequest hr = new HttpRequest(cookie);
            string docsurl = "http://docq.cn/api/docs";
            string filename = "";
            string path = "";
            long fileSize = 0;
            Console.WriteLine(pathNames);
            int nameIndex = pathNames.LastIndexOf('\\') + 1;
            filename = pathNames.Substring(nameIndex);
            Console.WriteLine(filename);
            filename = System.Web.HttpUtility.UrlEncode(filename, Encoding.UTF8);
            string postData = "file=" + "%7B%22name%22%3A%22" + filename + "%22%7D";
            if (pathNames != "")
            {
                FileInfo fi = new FileInfo(pathNames);
                if (!fi.Exists)
                {
                    MessageBox.Show("文件不存在");
                    return;
                }
                fileSize = fi.Length;
                Console.WriteLine(fileSize);
                path = pathNames;
                if (filename == "")
                {
                    return;
                }
                else if (filename.EndsWith(".doc") || filename.EndsWith(".pdf") || 
                    filename.EndsWith(".docx") || filename.EndsWith(".ppt") || 
                    filename.EndsWith(".pptx") || filename.EndsWith(".xls") || 
                    filename.EndsWith(".xlsx"))
                {
                    HttpWebResponse upkeyResponse = hr.HttpPost(docsurl, postData);
                    string upkeyStr = hr.GetResponseText(upkeyResponse);
                    JObject jo = JObject.Parse(upkeyStr);
                    string[] valuesK = jo.Properties().Select(item => item.Value.ToString()).ToArray();
                    string upkey = valuesK[2];
                    Console.WriteLine("upkey: " + upkey);
                    upkeyResponse.Close();

                    string url = "http://docq.cn/api/uptoken";
                    HttpWebResponse uptokenResponse = hr.HttpGet(url);
                    string uptokenStr = hr.GetResponseText(uptokenResponse);
                    JObject joToken = JObject.Parse(uptokenStr);
                    string[] valuesT = joToken.Properties().Select(item => item.Value.ToString()).ToArray();
                    string uptoken = valuesT[0];
                    Console.WriteLine("uptoken: " + uptoken);
                    uptokenResponse.Close();

                    uploadedFile(uptoken, upkey, path);
                }
                else
                {
                    this.notifyIcon1.ShowBalloonTip(10, "注意", "上传格式出错，请重新上传", ToolTipIcon.Info);
                }
            }
        
        }

        /// <summary>
        /// create left menu to windows
        /// </summary>
        /// <param name="itemName">left name</param>
        /// <param name="associatedProgramFullPath">path-> docq</param>
        private void AddFileContextMenuItem(string itemName, string associatedProgramFullPath)
        {
            //创建项：shell 
            RegistryKey shellKey = Registry.ClassesRoot.OpenSubKey(@"*\shell", true);
            if (shellKey == null)
            {
                shellKey = Registry.ClassesRoot.CreateSubKey(@"*\shell");
            }

            //创建项：右键显示的菜单名称
            RegistryKey rightCommondKey = shellKey.CreateSubKey(itemName);
            RegistryKey associatedProgramKey = rightCommondKey.CreateSubKey("command");

            //创建默认值：关联的程序
            associatedProgramKey.SetValue(string.Empty, associatedProgramFullPath);

            //刷新到磁盘并释放资源
            associatedProgramKey.Close();
            rightCommondKey.Close();
            shellKey.Close();
        }
        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void VisitWebToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://docq.cn");
        }

        private void FeedBackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            textBox2.Text = "";
            pathNames = "";
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://docq.cn");
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.Enter && (this.ActiveControl == textBox2 || this.ActiveControl == button1))
            {
                button1_Click(button1, EventArgs.Empty);
                this.ActiveControl = textBox1;
                return true;
            }
            else if (keyData == Keys.Tab)
            {
                if (this.ActiveControl == textBox1)
                {
                    textBox2.Focus();
                    return true;
                }
                else if (this.ActiveControl == textBox2)
                {
                    button1.Focus();
                    return true;
                }
                else if (this.ActiveControl == button1)
                {
                    textBox1.Focus();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Button for begin to use 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            FileInfo infoUser = new FileInfo(Application.StartupPath + "\\userInfo.txt");
            if (pathNames == "" || !infoUser.Exists)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(this.BeginUse));
            } 
            else
            {
                BeginUse("");
            }        
        }

        private void BeginUse(object o)
        {
            HttpRequest hr = new HttpRequest(cookie);
            string exeRoad = System.Environment.CurrentDirectory.ToString();
            Console.WriteLine(exeRoad);
            string email = textBox1.Text.ToString();
            string password = textBox2.Text.ToString();
            string url = "http://docq.cn/api/auth/login";
            string postData = "email=" + email + "&" + "password=" + password;
            //string postData = "email=1157995231%40qq.com&password=123456";

            HttpWebResponse cookieResponse = hr.HttpPost(url, postData);
            // Print the properties of each cookie.
            string test = hr.GetResponseText(cookieResponse);
            Console.WriteLine(test);
            CookieContainer cookieRs = new CookieContainer();
            FileInfo infoCookie = new FileInfo(Application.StartupPath + "\\cookie.txt");
            if (infoCookie.Exists)
            {
                System.IO.File.SetAttributes(Application.StartupPath + "\\cookie.txt", System.IO.FileAttributes.Normal);
                infoCookie.Delete();
            }
            foreach (Cookie cook in cookieResponse.Cookies)
            {
                cookieRs.Add(cook);
                FileStream fsCookie = File.Open(Application.StartupPath + "\\cookie.txt",
                    FileMode.Append, FileAccess.Write);
                String data = cook.ToString() + "\r\n";
                fsCookie.Write(Encoding.Default.GetBytes(data), 0, Encoding.Default.GetBytes(data).Length);
                fsCookie.Close();
                Console.WriteLine(cook.Name + cook.Value);
                Console.WriteLine(cookieResponse.StatusCode);
            }
            cookie = cookieRs;
            if (test.Contains("login-success"))
            {
                if (this.InvokeRequired)
                {
                    //Action<string> actionDelegate = (x) => { this.Hide(); };
                    //this.Invoke(actionDelegate, o);
                    this.BeginInvoke(new Action(() => { this.Hide(); }));
                }
                else
                {
                    this.Hide();
                }
                FileInfo infoUser = new FileInfo(Application.StartupPath + "\\userInfo.txt");
                if (infoUser.Exists)
                {
                    System.IO.File.SetAttributes(Application.StartupPath + "\\userInfo.txt", System.IO.FileAttributes.Normal);
                    infoUser.Delete();
                }
                FileStream fs = File.Open(Application.StartupPath + "\\userInfo.txt",
                    FileMode.Append, FileAccess.Write);
                String emailInfo = textBox1.Text + "\r\n";
                String passwordInfo = textBox2.Text + "\r\n";
                fs.Write(Encoding.Default.GetBytes(emailInfo), 0, Encoding.Default.GetBytes(emailInfo).Length);
                fs.Write(Encoding.Default.GetBytes(passwordInfo), 0, Encoding.Default.GetBytes(passwordInfo).Length);
                fs.Close();
                result = true;
                if (pathNames == "")
                {
                    return;
                }
                //注册进程OnProgramStarted
                /*ThreadPool.RegisterWaitForSingleObject(appStarted,
                    (obj, timeout) => { autoUploaded(); },
                    null, -1, false);*/
                ThreadPool.QueueUserWorkItem(new WaitCallback(this.autoUploaded));

            }
            else
            {
                DialogResult dr = MessageBox.Show("用户名或者密码错误");
                FileInfo infoUser = new FileInfo(Application.StartupPath + "\\userInfo.txt");
                if (infoUser.Exists)
                {
                    System.IO.File.SetAttributes(Application.StartupPath + "\\userInfo.txt", System.IO.FileAttributes.Normal);
                    infoUser.Delete();
                }
                textBox1.Text = "";
                textBox2.Text = "";
                result = false;
            }
        }

        /// <summary>
        /// upload button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /*private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            string docsurl = "http://docq.cn/api/docs";
            string filename = "";
            string path = "";
            long fileSize = 0;
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            dialog.ShowDialog();
            for (int i = 0; i < dialog.FileNames.Length; i++)
            {
                Console.WriteLine(dialog.SafeFileNames[i]);
                Console.WriteLine(dialog.FileNames[i]);
                FileInfo fi = new FileInfo(dialog.FileNames[i]);
                fileSize = fi.Length;
                Console.WriteLine(fileSize);
                path = dialog.FileNames[i];
                //filename = System.Web.HttpUtility.UrlEncode(dialog.SafeFileNames[i], Encoding.UTF8);
                int nameIndex = path.LastIndexOf('\\') + 1;
                filename = path.Substring(nameIndex);
                Console.WriteLine(filename);
                string postData = "file=" + "%7B%22name%22%3A%22" + filename + "%22%7D";
                if (filename == "")
                {
                    return;
                }
                else if (filename.EndsWith(".doc") || filename.EndsWith(".pdf") || filename.EndsWith(".docx")
                    || filename.EndsWith(".ppt") || filename.EndsWith(".pptx") || filename.EndsWith(".xls")
                    || filename.EndsWith(".xlsx"))
                {
                    HttpWebResponse upkeyResponse = HttpPost(docsurl, postData);
                    string upkeyStr = GetResponseText(upkeyResponse);
                    JObject jo = JObject.Parse(upkeyStr);
                    string[] valuesK = jo.Properties().Select(item => item.Value.ToString()).ToArray();
                    string upkey = valuesK[2];
                    Console.WriteLine("upkey: " + upkey);
                    upkeyResponse.Close();

                    string url = "http://docq.cn/api/uptoken";
                    HttpWebResponse uptokenResponse = HttpGet(url);
                    string uptokenStr = GetResponseText(uptokenResponse);
                    JObject joToken = JObject.Parse(uptokenStr);
                    string[] valuesT = joToken.Properties().Select(item => item.Value.ToString()).ToArray();
                    string uptoken = valuesT[0];
                    Console.WriteLine("uptoken: " + uptoken);
                    uptokenResponse.Close();

                    uploadedFile(uptoken, upkey, path);
                }
                else
                {
                    this.notifyIcon1.ShowBalloonTip(10, "注意", "上传格式出错，请重新上传", ToolTipIcon.Info);
                }
            }
        }*/

        private void uploadedFile(string uptoken, string upkey, string path)
        {
            HttpRequest hr = new HttpRequest(cookie);
            String hash = this.ResumablePutFile(uptoken, upkey, path);
            if (hash != "") 
            {
                string[] upkeyArr = upkey.Split('/');
                string url = "http://docq.cn/api/" + upkeyArr[0] + "/" + upkeyArr[1] + "/uploaded";
                Console.WriteLine(url);
                Console.WriteLine(hash);
                string postData = "_method=PUT&etag=" + hash; 
                HttpWebResponse uploadResponse = hr.HttpPost(url, postData);
                string uploadStr = hr.GetResponseText(uploadResponse);
                if (uploadStr == "{}")
                {
                    Console.WriteLine("upload success");
                    this.toolStripMenuItem2.Visible = false;
                    this.notifyIcon1.ShowBalloonTip(10, "注意", "上传成功", ToolTipIcon.Info);
                }
                else
                {
                    this.notifyIcon1.ShowBalloonTip(10, "注意", "上传出错，请重新上传", ToolTipIcon.Info);
                }
            }
        }

        /// <summary>
        /// normal uploaded
        /// </summary>
        /// <param name="upToken"></param>
        /// <param name="key"></param>
        /// <param name="fname"></param>
        /// <returns></returns>
        public String PutFile(string upToken, string key, string fname)
        {
            Console.WriteLine("\n===> PutFile {0} fname:{1}", key, fname);
            PutExtra extra = new PutExtra();
            IOClient client = new IOClient();
            CallRet ret = client.PutFile(upToken, key, fname, extra);
            if (ret.OK)
            {
                Console.WriteLine("success");
                string result = ret.Response;
                Console.WriteLine(result);
                JObject jo = JObject.Parse(result);
                string[] valuesH = jo.Properties().Select(item => item.Value.ToString()).ToArray();
                string hash = valuesH[0];
                return hash;
            }
            else
            {
                Console.WriteLine("fail");
                return "";
            }
        }

        /// <summary>
        /// resumable uploaded
        /// </summary>
        /// <param name="upToken"></param>
        /// <param name="key"></param>
        /// <param name="fname"></param>
        /// <returns></returns>
        String uploadingFName = "";
        public String ResumablePutFile(string upToken, string key, string fname)
        {
            Console.WriteLine("\n===> ResumablePutFile {0} fname:{1}", key, fname);
            Console.WriteLine(upToken);

            int nameIndex = fname.LastIndexOf('\\') + 1;
            uploadingFName = fname.Substring(nameIndex);

            Settings setting = new Settings();
            ResumablePutExtra extra = new ResumablePutExtra();
            extra.Notify += this.showNotify;
            extra.NotifyErr += this.showNotifyErr;
            extra.PutSchedule += this.showSchedule;
            this.notifyIcon1.ShowBalloonTip(10, "注意", uploadingFName + " 正在上传", ToolTipIcon.Info);
            ResumablePut client = new ResumablePut(setting, extra);
            //ThreadPool.QueueUserWorkItem(new WaitCallback(this.changeIco), "uploading");
            Thread t = new Thread(new ParameterizedThreadStart(this.changeIco));
            t.Start("uploading");
            CallRet ret = client.PutFile(upToken, fname, key);
            t.Abort();
            changeIco("normal");
            if (ret.OK)
            {
                Console.WriteLine("success");
                string result = ret.Response;
                Console.WriteLine(result);
                JObject jo = JObject.Parse(result);
                string[] valuesH = jo.Properties().Select(item => item.Value.ToString()).ToArray();
                string hash = valuesH[0];
                return hash;
            }
            else
            {
                Console.WriteLine("fail");
                return "";
            }
        }

        int c = 0;
        private void changeIco(object o)
        {
            switch (o.ToString())
            {
                case "uploading":
                    while (true)
                    {
                        if (c == 0) { notifyIcon1.Icon = Properties.Resources.circle1; c++; }
                        else if (c == 1) { notifyIcon1.Icon = Properties.Resources.circle2; c++; }
                        else if (c == 2) { notifyIcon1.Icon = Properties.Resources.circle3; c++; }
                        else if (c == 3) { notifyIcon1.Icon = Properties.Resources.circle4; c++; }
                        else if (c == 4) { notifyIcon1.Icon = Properties.Resources.circle5; c = 0; }
                    } 
                case "normal":
                    notifyIcon1.Icon = Properties.Resources.dcoq;
                    break;
            }
        }

        private void showNotifyErr(object sender, PutNotifyErrorEvent e)
        {
            System.Diagnostics.Debug.WriteLine("-showNotifyErr");
            System.Diagnostics.Debug.WriteLine("-blkIdx:" + e.BlkIdx);
            System.Diagnostics.Debug.WriteLine("-blkSize:" + e.BlkSize);
            System.Diagnostics.Debug.WriteLine("-error:" + e.Error);
        }

        private void showNotify(object sender, PutNotifyEvent e)
        {
            System.Diagnostics.Debug.WriteLine("=showNotify");
            System.Diagnostics.Debug.WriteLine("=blkIdx:" + e.BlkIdx);
            System.Diagnostics.Debug.WriteLine("=blkSize:" + e.BlkSize);
            System.Diagnostics.Debug.WriteLine("=Now in percent:" + Math.Floor((double)(e.BlkIdx*100 / e.BlkSize)) + "%");
            System.Diagnostics.Debug.WriteLine("=ret.ctx:" + e.Ret.ctx);
            System.Diagnostics.Debug.WriteLine("=ret.checkSum:" + e.Ret.checkSum);
            System.Diagnostics.Debug.WriteLine("=ret.crc32:" + e.Ret.crc32);
            System.Diagnostics.Debug.WriteLine("=ret.offset:" + e.Ret.offset);
        }

        private void showSchedule(object sender, PutScheduleEvent e)
        {
            System.Diagnostics.Debug.WriteLine("-showSchedule");
            System.Diagnostics.Debug.WriteLine(e.GetSchedule);
            this.toolStripMenuItem2.Visible = true;
            this.toolStripMenuItem2.Text = "正在上传 " + uploadingFName + " " + e.GetSchedule;
            //this.notifyIcon1.ShowBalloonTip(30, "注意", uploadingFName + " 正在上传" + e.GetSchedule, ToolTipIcon.Info);
        }

    }
}
