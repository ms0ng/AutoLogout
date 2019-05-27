using System;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Text;
using Newtonsoft.Json.Linq;

//http://58.240.51.118/logoutServlet
/*
Host: 58.240.51.118
User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:67.0) Gecko/20100101 Firefox/67.0
Accept: text/html,application/xhtml+xml,application/xml;q=0.9,* /*;q=0.8
Accept-Language: zh-CN,zh;q=0.8,zh-TW;q=0.7,zh-HK;q=0.5,en-US;q=0.3,en;q=0.2
Accept-Encoding: gzip, deflate
Referer: http://58.240.51.118/style/portalv2_cujs/logon.jsp?paramStr=`````
Content-Type: application/x-www-form-urlencoded
Content-Length: 293
    */

namespace AutoLogout
{
    class Program
    {
        
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            string confurl = "https://raw.githubusercontent.com/ms0ng/AutoLogout/master/Configure.json";
            Program program = new Program();
            JObject jb = program.readConf(confurl);

            string b = jb["Edition"]["Download"].Value<string>();
            Console.WriteLine(b);
        }

        /// <summary>
        /// 获取配置类
        /// </summary>
        /// <param name="url">URL地址</param>
        /// <returns>配置类JObject</returns>
        JObject readConf(string url)
        {

            if (!FileDownload(url, "Configure.json")) return null;
            StreamReader streamReader = new StreamReader(Path.GetTempPath()+"\\Configure.json");
            string json = streamReader.ReadToEnd();
            JObject jb=(JObject)JsonConvert.DeserializeObject(json);
            return jb;
            
        }
        void Download(string url)
        {
            WebClient webClient = new WebClient();            webClient.Encoding = Encoding.UTF8;            string outText = webClient.DownloadString(url);            File.WriteAllText(".\\Configuretest.json", outText);
        }

        /// <summary>
        /// 文件下载
        /// </summary>
        /// <param name="url">URL地址</param>
        /// <param name="name">文件名</param>
        /// <param name="path">路径</param>
        /// <returns>运行结果</returns>
        bool FileDownload(string url, string name, string path)
        {
            bool value = false;
            if (path == "" || path == null)
            {
                //path = "C:\\Users\\Administrator\\Music";
                //path = ".\\";
                path = Path.GetTempPath();
            }
            if (!path.EndsWith("\\"))
            {
                path += "\\";
            }
            string fullpath = path + name;
            if (name.LastIndexOf(".") == -1) fullpath = path + name + url.Substring(url.LastIndexOf("."));
            if (File.Exists(fullpath))
            {
                File.Delete(fullpath);
            }
            try
            {
                WebClient webClient = new WebClient();
                webClient.Encoding = Encoding.UTF8;
                string outText = webClient.DownloadString(url);
                File.WriteAllText(fullpath, outText);
                value = true;
            }catch(Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
            return value;
        }
        bool FileDownload(string url, string name)
        {
            return FileDownload(url, name, "");
        }

        /// <summary>
        /// 自动下线
        /// </summary>
        /// <returns>运行结果</returns>
        Boolean logout() 
        {
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create("http://58.240.51.118/logoutServlet");
            req.Method = "GET";
            req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:67.0) Gecko/20100101 Firefox/67.0";
            req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            req.Referer = "http://58.240.51.118/style/portalv2_cujs/logon.jsp?paramStr=xxxxxxx";
            req.ContentType = "application/x-www-form-urlencoded";

            try
            {
                WebResponse response = req.GetResponse();

                Stream stream = response.GetResponseStream();
                //在这个地方应该会抛出404错误
                //for (int i = 0; i < stream.Length; i++)
                {
                    Console.WriteLine(stream.ReadByte() + "");
                }

                response.Close();
                stream.Close();
            }
            catch (WebException e)
            {
                Console.WriteLine(e.Message);
                if (e.Message == "The remote server returned an error: (404) Not Found.")return true;
                //成功断网
            }
            catch (Exception e)
            {
                throw e;
            }
            return false;
        }

    }
}
