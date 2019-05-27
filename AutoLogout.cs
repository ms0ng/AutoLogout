using System;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Threading;

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
    class AutoLogout
    {
        static string VERSION = "1.0";  //版本号
        static string confURL = "https://raw.githubusercontent.com/ms0ng/AutoLogout/master/Configure.json";     //json配置文件地址
        static string serverChanKey = "https://sc.ftqq.com/SCU52366Taf8a7d0930b38f0a3f662566bdd26d185cebdd4986c77.send";        //serverChan URL

        int retryTimes = 0;
        JObject jobj;
        DateTime dateTime;
        bool needUpdate = false;
        static void Main(string[] args)
        {
            AutoLogout program = new AutoLogout();
            //while (program.needUpdate==false)
            {
                program.run();
            }
        }

        void run()
        {
            sleep(60 * 5);
            //检查联网状态
            try
            {
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create("https://cn.bing.com/");
                WebResponse response = req.GetResponse();
                response.Close();
            } catch (Exception)
            {
                sleep(60 * 5);
                return;
            }
            //下载配置
            while(!FileDownload(confURL, "Configure.json"))
            {
                if (retryTimes % 10 == 0)
                {
                    sendMsg("配置文件下载失败!", "已重试次数" + retryTimes);
                    return;
                }
                retryTimes++;
            }
            //读取配置
            try
            {
                StreamReader streamReader = new StreamReader(Path.GetTempPath() + "\\Configure.json");
                string json = streamReader.ReadToEnd();
                streamReader.Close();
                jobj = (JObject)JsonConvert.DeserializeObject(json);
            }
            catch (Exception)
            {
                retryTimes++;
                return;
            }

            //检查更新
            if (jobj["Edition"]["Version"].Value<string>() != VERSION)
            {
                
                while(!FileDownload(jobj["Edition"]["Download"].Value<string>(), "AutoLogoutTemp.exe"))
                {
                    retryTimes++;
                    if((retryTimes % 10) == 0)
                    {
                        sendMsg("下载更新失败", "重试次数:" + retryTimes);
                        return;
                    }
                        
                }
                //TODO:安装更新
                string path = System.Environment.CurrentDirectory ;
                string bat = "del " +path+"\\AutoLogout.exe"+
                    "copy ";
                needUpdate = true;
                return;

            }
            //检查配置文件激活
            if (jobj["Configure"]["Active"].Value<bool>() == false) return;

            //TODO:检查是否只执行一次
            //if (jobj["Configure"]["RunOnlyOnce"].Value<bool>() == true)
            {

            }

            //检查是否现在断网,**由于无法更改网页Json,故会一直断网**
            if (jobj["Configure"]["LogoutNow"].Value<bool>() == true) {
                try
                {
                    logout();
                } catch (Exception e)
                {
                    retryTimes++;
                    sendMsg("无法断网", "已重试次数:" + retryTimes + "\n" + e.Message);
                    return;
                }

            }
            //检查日期
            dateTime = DateTime.Now;
            string dayOfWeek = dateTime.DayOfWeek.ToString();
            int sleeptime = 60 * 60;        //单位:秒,暂停时间,设一个小时
            switch (dayOfWeek)
            {
                case "Monday":
                    if (jobj["Configure"]["Date"]["Mon"].Value<bool>() == false)
                    {
                        sleep(sleeptime);
                        break;
                    }
                    break;
                case "Tuesday":
                    if (jobj["Configure"]["Date"]["Tue"].Value<bool>() == false)
                    {
                        sleep(sleeptime);
                        break;
                    }
                    break;
                case "Wednesday":
                    if (jobj["Configure"]["Date"]["Wed"].Value<bool>() == false)
                    {
                        sleep(sleeptime);
                        break;
                    }
                    break;
                case "Thursday":
                    if (jobj["Configure"]["Date"]["Thu"].Value<bool>() == false)
                    {
                        sleep(sleeptime);
                        break;
                    }
                    break;
                case "Friday":
                    if (jobj["Configure"]["Date"]["Fri"].Value<bool>() == false)
                    {
                        sleep(sleeptime);
                        break;
                    }
                    break;
                case "Saturday":
                    if (jobj["Configure"]["Date"]["Sat"].Value<bool>() == false)
                    {
                        sleep(sleeptime);
                        break;
                    }
                    break;
                case "Sunday":
                    if (jobj["Configure"]["Date"]["Sun"].Value<bool>() == false)
                    {
                        sleep(sleeptime);
                        break;
                    }
                    break;
                default:
                    retryTimes++;
                    sendMsg("检查日期时出错", "已重试次数:" + retryTimes);
                    break;
            }

            //检查时间
            int[] now = { dateTime.Hour, dateTime.Minute };
            JArray array = (JArray)JsonConvert.DeserializeObject(jobj["Configure"]["Time"].ToString());
            string[] planTime = array[0].ToString().Split(":");
            int[] plan =
            {
                int.Parse(planTime[0]),
                int.Parse(planTime[1])
            };
            if (plan[0] != now[0]) return;
            int deltaTime = plan[1] - now[1];
            if (deltaTime < 0) deltaTime = -deltaTime;
            if (deltaTime > 5) return;
            try
            {
                logout();
                sendMsg("断网成功!");
            }catch(Exception e)
            {
                retryTimes++;
                sendMsg("断网失败", "检查时间出错,已重试次数:" + retryTimes);
                return;
            }

            retryTimes = 0;     //全部执行正常,错误次数归零
        }

        bool sendMsg(string text,string desp)
        {
            string url = serverChanKey;
            url += "?text=" + text;
            if (desp != null) url += "?desp=" + desp;
            try
            {
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
                WebResponse response = req.GetResponse();
                response.Close();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        bool sendMsg(string text)
        {
            return sendMsg(text, null);
        }

        /// <summary>
        /// 获取配置类
        /// </summary>
        /// <param name="url">URL地址</param>
        /// <returns>配置类JObject</returns>
        JObject readConf(string url)
        {
            StreamReader streamReader = new StreamReader(Path.GetTempPath()+"\\Configure.json");
            string json = streamReader.ReadToEnd();
            JObject jb=(JObject)JsonConvert.DeserializeObject(json);
            return jb;
            
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
                //Console.WriteLine(e.StackTrace);
            }
            return value;
        }
        /// <summary>
        /// 文件下载
        /// 下载到临时文件夹
        /// </summary>
        /// <param name="url">URL地址</param>
        /// <param name="name">文件名</param>
        /// <returns>运行结果</returns>
        bool FileDownload(string url, string name)
        {
            return FileDownload(url, name, "");
        }

        /// <summary>
        /// 自动下线
        /// </summary>
        /// <returns>运行结果</returns>
        bool logout() 
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

        void sleep(int s)
        {
            Thread.Sleep(1000 * s);
        }
    }
}
