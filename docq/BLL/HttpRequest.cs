using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace docq.BLL
{
    class HttpRequest
    {
        public CookieContainer cookie = new CookieContainer();

        public HttpRequest(CookieContainer cookie)
        {
            this.cookie = cookie;
        }
        /// <summary>
        /// 模拟post请求可以使用WebClient与HttpWebRequest两种方式
        /// 但是HttpWebPost更加灵活，也更加强大，例如HttpWebRequest支持Cookie
        /// </summary>
        /// <param name="url">post-url</param>
        /// <param name="postData">post-data</param>
        /// <returns></returns>
        public HttpWebResponse HttpPost(string url, string postData)
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
        public HttpWebResponse HttpGet(string url)
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
        /// return the getresponsetext
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public string GetResponseText(HttpWebResponse response)
        {
            Stream responseStream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(responseStream, Encoding.Default);
            string retstring = streamReader.ReadToEnd();

            Console.WriteLine(retstring);
            return retstring;
        }

    }
}
