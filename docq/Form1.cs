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
        public Form1()
        {
            InitializeComponent();
            //AddFileContextMenuItem("上传到docq","D:\\vsWorkspace\\docq\\docq\\bin\\Debug\\docq.exe");
            //this.BackgroundImage = Image.FromFile("D:\\vsWorkspace\\docq\\background.png");
        }

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
            foreach (Cookie cook in cookieResponse.Cookies)
            {
                cookieRs.Add(cook);
                Console.WriteLine(cook.Name + cook.Value);
                Console.WriteLine(cookieResponse.StatusCode);            
            }
            cookie = cookieRs;
            if (test.Contains("login-success"))
            {
                this.Hide();
            }
            else
            {
                DialogResult dr = MessageBox.Show("用户名或者密码错误");
                textBox1.Text = "";
                textBox2.Text = "";
            }
        }

        /*
         * 模拟post请求可以使用WebClient与HttpWebRequest两种方式
         * 但是HttpWebPost更加灵活，也更加强大，例如HttpWebRequest支持Cookie
         * */
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
        /************************************************************************/
        /* 上传文档入口程序                                                                     */
        /************************************************************************/
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            string docsurl = "http://docq.cn/api/docs";
            string filename = "";
            string path = "";
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.ShowDialog();
            if (!string.IsNullOrEmpty(dialog.FileName))
            {
                path = dialog.FileName;
                filename = dialog.SafeFileName;
            }
            filename = System.Web.HttpUtility.UrlEncode(filename, Encoding.UTF8);
            string postData = "file=" + "%7B%22name%22%3A%22" + filename + "%22%7D";
            Console.WriteLine(filename);
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

        /************************************************************************/
        /* 得到Http返回的内容                                                                     */
        /************************************************************************/
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

        /************************************************************************/
        /*普通上传                                                                      */
        /************************************************************************/
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

        /************************************************************************/
        /*    断点续传                                                                  */
        /************************************************************************/
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
