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

namespace docq
{
    public partial class Form1 : Form
    {
        public static CookieContainer cookie = new CookieContainer();
        public static bool result = false;
        public Form1()
        {
            InitializeComponent();
            InitializeTextBox();
            AddFileContextMenuItem("上传到docq", Application.StartupPath + "\\docqRC.exe");
            Console.WriteLine(Application.StartupPath + "\\docq.exe");
            //this.BackgroundImage = Image.FromFile("D:\\vsWorkspace\\docq\\background.png");
        }
        
        /// <summary>
        /// for auto start
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cb_zddl_CheckedChanged(object sender, EventArgs e)    
        {          
            if (cb_zddl.Checked)            
            {                
                MessageBox.Show("设置开机自启动，需要修改注册表", "提示");                
                string path = Application.ExecutablePath;                
                RegistryKey rk = Registry.LocalMachine;                
                RegistryKey rk2 = rk.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");               
                rk2.SetValue("Form1", path);                
                rk2.Close();                
                rk.Close();            
            }            
            else             
            {                
                MessageBox.Show("取消开机自启动，需要修改注册表", "提示");                
                string path = Application.ExecutablePath;               
                RegistryKey rk = Registry.LocalMachine;               
                RegistryKey rk2 = rk.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");             
                rk2.DeleteValue("Form1", false);              
                rk2.Close();               
                rk.Close();           
            }          
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
                textBox1.Text = userLine[0];
                textBox2.Text = userLine[1];
                button1_Click(button1, EventArgs.Empty);
                if (result)
                {
                    this.WindowState = FormWindowState.Minimized;
                    this.ShowInTaskbar = false;
                    //toolStripMenuItem1_Click(toolStripMenuItem1, EventArgs.Empty);
                }
                else
                {
                    this.WindowState = FormWindowState.Normal;
                    this.ShowInTaskbar = true;
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
            System.Diagnostics.Process.Start("http://docq.cn");
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://docq.cn");
        }

        /// <summary>
        /// Button for begin to use 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            string email = textBox1.Text.ToString();
            string password = textBox2.Text.ToString();
            string url = "http://docq.cn/api/auth/login";
            string postData = "email=" + email + "&" + "password=" + password;
            //string postData = "email=1157995231%40qq.com&password=123456";
            
            HttpWebResponse cookieResponse = HttpPost(url, postData);
             // Print the properties of each cookie.
            string test = GetResponseText(cookieResponse);
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
                this.Hide();
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
            }
            else
            {
                DialogResult dr = MessageBox.Show("用户名或者密码错误");
                System.IO.File.SetAttributes(Application.StartupPath + "\\userInfo.txt", System.IO.FileAttributes.Normal);
                File.Delete(Application.StartupPath + "\\userInfo.txt");
                textBox1.Text = "";
                textBox2.Text = "";
                result = false;
            }
        }

        /// <summary>
        /// 模拟post请求可以使用WebClient与HttpWebRequest两种方式
        /// 但是HttpWebPost更加灵活，也更加强大，例如HttpWebRequest支持Cookie
        /// </summary>
        /// <param name="url">post-url</param>
        /// <param name="postData">post-data</param>
        /// <returns></returns>
        private HttpWebResponse HttpPost(string url, string postData)
        {
            //request
            UTF8Encoding encoding = new UTF8Encoding();
            byte[] data = encoding.GetBytes(postData);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            request.ContentLength = data.Length;
            if (cookie.Count == 0)
            {
                CookieContainer cc = new CookieContainer();
                request.CookieContainer = cc;
            }
            else
            {
                request.CookieContainer = cookie;
            }
            Stream newStream = request.GetRequestStream();

            //send the data;
            newStream.Write(data, 0, data.Length);
            newStream.Close();
            //response
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            return response;
        }

        /// <summary>
        /// get
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private HttpWebResponse HttpGet(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            if (cookie.Count == 0)
            {
                CookieContainer cc = new CookieContainer();
                request.CookieContainer = cc;
            }
            else
            {
                request.CookieContainer = cookie;
            }
            request.Method = "GET";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            //GetResponseText(response);
            return response;
        }

        /// <summary>
        /// upload button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            string docsurl = "http://docq.cn/api/docs";
            string filename = "";
            string path = "";
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            dialog.ShowDialog();
            for (int i = 0; i < dialog.FileNames.Length; i++)
            {
                Console.WriteLine(dialog.SafeFileNames[i]);
                Console.WriteLine(dialog.FileNames[i]);
                path = dialog.FileNames[i];
                filename = System.Web.HttpUtility.UrlEncode(dialog.SafeFileNames[i], Encoding.UTF8);
                string postData = "file=" + "%7B%22name%22%3A%22" + filename + "%22%7D";
                if (filename == "")
                {
                    return;
                }
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
        }

        /// <summary>
        /// return the getresponsetext
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        private string GetResponseText(HttpWebResponse response)
        {
            Stream responseStream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(responseStream, Encoding.Default);
            string retstring = streamReader.ReadToEnd();

            Console.WriteLine(retstring);
            return retstring;
        }

        private void uploadedFile(string uptoken, string upkey, string path)
        {
            String hash = PutFile(uptoken, upkey, path);
            if (hash != "") 
            {
                string[] upkeyArr = upkey.Split('/');
                string url = "http://docq.cn/api/" + upkeyArr[0] + "/" + upkeyArr[1] + "/uploaded";
                Console.WriteLine(url);
                Console.WriteLine(hash);
                string postData = "_method=PUT&etag=" + hash; 
                HttpWebResponse uploadResponse = HttpPost(url, postData);
                string uploadStr = GetResponseText(uploadResponse);
                if (uploadStr == "{}")
                {
                    Console.WriteLine("upload success");
                    this.notifyIcon1.ShowBalloonTip(30, "注意", "上传成功", ToolTipIcon.Info);
                }
                else
                {
                    this.notifyIcon1.ShowBalloonTip(30, "注意", "上传出错，请重新上传", ToolTipIcon.Info);
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
        public static String PutFile(string upToken, string key, string fname)
        {
            // 初始化qiniu配置，主要是API Keys
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
        public static String ResumablePutFile(string upToken, string key, string fname)
        {
            Console.WriteLine("\n===> ResumablePutFile {0} fname:{1}", key, fname);
            Console.WriteLine(upToken);
            Settings setting = new Settings();
            ResumablePutExtra extra = new ResumablePutExtra();
            ResumablePut client = new ResumablePut(setting, extra);
            CallRet ret = client.PutFile(upToken, fname, Guid.NewGuid().ToString());         
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
    }
}
