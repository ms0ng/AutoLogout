using System;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Threading;


namespace AutoLogout
{
    class AutoLogout
    {
        static string VERSION = "1.0";  //版本号
        static string confURL = "https://raw.githubusercontent.com/ms0ng/AutoLogout/master/Configure.json";     //json配置文件地址
        static string serverChanKey = "";        //serverChan URL

        int retryTimes = 0;
        JObject jobj;
        DateTime dateTime;
        bool needUpdate = false;
        static void Main(string[] args)
        {
            AutoLogout program = new AutoLogout();
            int criticalError = 0;
            Debug("VERSION:"+VERSION);
            while (program.needUpdate==false)
            {
                try
                {
                    Debug("DEBUG RUN");
                    program.run();
                    sleep(60 * 5);
                }
                catch(Exception e)
                {
                    Debug(e.Message);
                    criticalError++;
                    if (criticalError >= 20)
                    {
                        program.sendMsg("执行程序时有" + criticalError + "次严重错误,已关闭程序", e.StackTrace);
                        return;
                    }
                    else if (criticalError >= 10)
                    {
                        program.sendMsg("执行程序时有" + criticalError + "次严重错误", e.StackTrace);
                        continue;
                    }
                    sleep(60 * 5);
                }
                
            }
        }

        /// <summary>
        /// 程序执行的主要流程
        /// </summary>
        void run()
        {
            
            //检查联网状态
            try
            {
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create("https://cn.bing.com/");
                WebResponse response = req.GetResponse();
                response.Close();
                Debug("Internet Connected!");
            } catch (Exception e)
            {
                Debug(e.StackTrace);
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
            Debug("Download Conf Complete!");
            //读取配置
            try
            {
                StreamReader streamReader = new StreamReader(Path.GetTempPath() + "\\Configure.json");
                string json = streamReader.ReadToEnd();
                streamReader.Close();
                jobj = (JObject)JsonConvert.DeserializeObject(json);
                Debug("Read Conf Success!");
            }
            catch (Exception e)
            {
                Debug(e.StackTrace);
                retryTimes++;
                return;
            }

            //检查更新
            if (jobj["Edition"]["Version"].Value<string>() != VERSION)
            {
                Debug("Need Update Version:" + jobj["Edition"]["Version"].Value<string>());
                while(!FileDownload(jobj["Edition"]["Download"].Value<string>(), "AutoLogout.exe"))
                {
                    retryTimes++;
                    if((retryTimes % 10) == 0)
                    {
                        sendMsg("下载更新失败", "重试次数:" + retryTimes);
                        return;
                    }
                        
                }
                //安装更新
                string path = System.Environment.CurrentDirectory ;
                string bat = "@echo off\r\n" +
                    //"ping localhost -n 5 > nul\r\n" +
                    "del " + path + "\\AutoLogout.exe > nul\r\n" +
                    "copy /y " + Path.GetTempPath() + "AutoLogout.exe " + path + "\\AutoLogout.exe > nul\r\n"
                    + "del " + Path.GetTempPath() + "AutoLogout.exe > nul\r\n"+
                    path + "\\AutoLogout.exe > nul";
                
                try
                {
                    if (File.Exists(Path.GetTempPath() + "\\Atlg.bat")) File.Delete(Path.GetTempPath() + "\\Atlg.bat");
                    File.WriteAllText(Path.GetTempPath() + "\\Atlg.bat", bat);
                    needUpdate = true;
                    System.Diagnostics.Process.Start(Path.GetTempPath() + "\\Atlg.bat");
                    return;
                }catch(Exception e)
                {
                    retryTimes++;
                    if (retryTimes % 10 == 0)sendMsg("更新失败", "已重试次数:" + retryTimes + "\n" + e.Message);
                    return;
                }
                
                

            }
            Debug("No need to update");
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
                    Debug("Logout NOW!!!");
                    logout();
                    return;
                } catch (Exception e)
                {
                    retryTimes++;
                    sendMsg("无法断网", "已重试次数:" + retryTimes + "\n" + e.Message);
                    return;
                }

            }
            //检查日期
            Debug("Checking Date");
            dateTime = DateTime.Now;
            Debug(dateTime.ToString());
            string dayOfWeek = dateTime.DayOfWeek.ToString();
            Debug("Today: " + dayOfWeek);
            int sleeptime = 60 * 60;        //单位:秒,暂停时间,设一个小时
            switch (dayOfWeek)
            {
                case "Monday":
                    if (jobj["Configure"]["Date"]["Mon"].Value<bool>() == false)
                    {
                        sleep(sleeptime);
                        return;
                    }
                    break;
                case "Tuesday":
                    if (jobj["Configure"]["Date"]["Tue"].Value<bool>() == false)
                    {
                        sleep(sleeptime);
                        return;
                    }
                    break;
                case "Wednesday":
                    if (jobj["Configure"]["Date"]["Wed"].Value<bool>() == false)
                    {
                        sleep(sleeptime);
                        return;
                    }
                    break;
                case "Thursday":
                    if (jobj["Configure"]["Date"]["Thu"].Value<bool>() == false)
                    {
                        sleep(sleeptime);
                        return;
                    }
                    break;
                case "Friday":
                    if (jobj["Configure"]["Date"]["Fri"].Value<bool>() == false)
                    {
                        sleep(sleeptime);
                        return;
                    }
                    break;
                case "Saturday":
                    if (jobj["Configure"]["Date"]["Sat"].Value<bool>() == false)
                    {
                        sleep(sleeptime);
                        return;
                    }
                    break;
                case "Sunday":
                    if (jobj["Configure"]["Date"]["Sun"].Value<bool>() == false)
                    {
                        sleep(sleeptime);
                        return;
                    }
                    break;
                default:
                    retryTimes++;
                    sendMsg("检查日期时出错", "已重试次数:" + retryTimes);
                    return;
            }

            //检查时间
            Debug("Check if it is the time...");
            int[] now = { dateTime.Hour, dateTime.Minute };
            JArray array = (JArray)JsonConvert.DeserializeObject(jobj["Configure"]["Time"].ToString());
            Debug("Json Planing Time:" + array.ToString());
            string conf1 = array[0].ToString();
            string[] planTime = conf1.Split(':');
            int[] plan =
            {
                int.Parse(planTime[0]),
                int.Parse(planTime[1])
            };
            int deltaTime = plan[1] - now[1];
            if (deltaTime < 0) deltaTime = -deltaTime;
            Debug((plan[0]-now[0]).ToString()+"hours and "+deltaTime.ToString() + "minutes left");
            if (plan[0] != now[0]) return;
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

            Debug("Task Over");
            retryTimes = 0;     //全部执行正常,错误次数归零
        }

        bool sendMsg(string text,string desp)
        {
#if DEBUG
            Debug(text + "\n" + desp);
            return true;
#endif
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
            catch (Exception)
            {
                return false;
            }
        }
        bool sendMsg(string text)
        {
            return sendMsg(text, null);
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

            //方法1
            if(false)
            {
                try
                {
                    WebClient webClient = new WebClient();
                    webClient.Encoding = Encoding.UTF8;
                    string outText = webClient.DownloadString(url);
                    File.WriteAllText(fullpath, outText);
                    value = true;
                }
                catch (Exception e)
                {
                    Debug(e.StackTrace);
                }
            }
            //方法2
            try
            {
                FileStream fs = new FileStream(fullpath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                // 设置参数
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "GET";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:67.0) Gecko/20100101 Firefox/67.0";
                //发送请求并获取相应回应数据
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                //直到request.GetResponse()程序才开始向目标网页发送Post请求
                Stream responseStream = response.GetResponseStream();
                //创建本地文件写入流
                //Stream stream = new FileStream(tempFile, FileMode.Create);
                byte[] bArr = new byte[1024];
                int size = responseStream.Read(bArr, 0, (int)bArr.Length);
                while (size > 0)
                {
                    //stream.Write(bArr, 0, size);
                    fs.Write(bArr, 0, size);
                    size = responseStream.Read(bArr, 0, (int)bArr.Length);
                }
                //stream.Close();
                fs.Close();
                responseStream.Close();
                value = true;
            }
            catch (Exception e)
            {
                Debug(e.StackTrace);
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
#if DEBUG
            Debug("Logout!");
            return true;
#endif
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
                    Debug(stream.ReadByte() + "");
                }

                response.Close();
                stream.Close();
            }
            catch (WebException e)
            {
                Debug(e.Message);
                if (e.Message == "The remote server returned an error: (404) Not Found.")return true;
                //成功断网
            }
            catch (Exception e)
            {
                throw e;
            }
            return false;
        }

        static void sleep(int s)
        {
#if DEBUG
            Thread.Sleep(1000*60*5);
#else
            Thread.Sleep(1000 * s);
#endif
        }
        static void Debug(string s)
        {
#if DEBUG
            Console.WriteLine(s);
#endif
        }
    }
}
