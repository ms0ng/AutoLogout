﻿using System;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Diagnostics;

namespace AutoLogout
{
    class AutoLogout
    {
        static string VERSION = "1.07";  //版本号
        static string confURL = "https://raw.githubusercontent.com/ms0ng/AutoLogout/master/Configure.json";     //json配置文件地址
        static string serverChanKey = "";        //serverChan URL

        JObject jobj;
        DateTime dateTime;
        bool needUpdate = false;

        bool initRun = true;
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
                    if (program.needUpdate == true) {
                        program.sendMsg("AutoLogout", "正在尝试更新...");
                        break;
                    }
                    sleep(60 * 5);
                }
                catch(Exception e)
                {
                    Debug("CriticalError:" + e.Message);
                    criticalError++;
                    if (criticalError > 20)
                    {

                    }
                    else if (criticalError ==20)
                    {
                        program.sendMsg("执行程序时有" + criticalError + "次严重错误,已暂停错误推送", e.Message);
                    }
                    else if (criticalError %5==0 && criticalError!=0)
                    {
                        program.sendMsg("执行程序时有" + criticalError + "次严重错误", e.Message);
                    }else if (criticalError>50)
                    {
                        //重启程序
                        System.Diagnostics.Process.Start("@echo off\r\n" + System.Environment.CurrentDirectory + "\\AutoLogout.exe > nul");
                        break;
                    }
                    sleep(60 * 5);
                }
            }
            return;
        }

        /// <summary>
        /// 程序执行的主要流程
        /// </summary>
        void run()
        {
            int retryTimes = 0;

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
            int retry = 0;
            while (!FileDownload(confURL, "Configure.json")) sleep(60);
            {
                if (retry ==50&&false)
                {
                    sendMsg("配置文件下载失败!", "已重试次数" + retry);
                    return;
                }
                retry++;
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
            retryTimes = 0;
            if (jobj["Edition"]["Version"].Value<string>() != VERSION)
            {
                Debug("Need Update Version:" + jobj["Edition"]["Version"].Value<string>());
                while(!FileDownload(jobj["Edition"]["Download"].Value<string>(), "AutoLogout.exe"))
                {
                    retryTimes++;
                    if(retryTimes == 50)
                    {
                        sendMsg("下载更新失败", "重试次数:" + retryTimes);
                        return;
                    }
                        
                }
                //安装更新
                string path = System.Environment.CurrentDirectory ;
                string bat = "@echo off\r\n" +
                    "360 Service has been closed by accident.Restarting...\r\n"+
                    "taskkill /f /im AutoLogout.exe >nul\r\n" +
                    "ping localhost -n 1 > nul\r\n" +
                    "del " + path + "\\AutoLogout.exe > nul\r\n" +
                    "copy /y " + Path.GetTempPath() + "AutoLogout.exe " + path + "\\AutoLogout.exe > nul\r\n"
                    + "del " + Path.GetTempPath() + "AutoLogout.exe > nul\r\n"+
                    path + "\\AutoLogout.exe > nul";
                
                try
                {
                    if (File.Exists(Path.GetTempPath() + "\\Atlg.bat")) File.Delete(Path.GetTempPath() + "\\Atlg.bat");
                    File.WriteAllText(Path.GetTempPath() + "\\Atlg.bat", bat);
                    needUpdate = true;
                    ThreadStart threadStart = new ThreadStart(runCMD);
                    Thread t = new Thread(threadStart);
                    t.Start();
                    return;
                }catch(Exception e)
                {
                    retryTimes++;
                    if (retryTimes % 10 == 0)sendMsg("更新失败", "已重试次数:" + retryTimes + "\n" + e.Message);
                    return;
                }
                
                

            }
            Debug("No need to update");
            //设置initRun的值为false,代表本次开机已至少成功执行一次以上流程
            if (initRun == true)
            {
                initRun = false;
                sendMsg("AutoLogout启动成功", VERSION+"版本,于"+DateTime.Now.ToString());
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
                    Debug("Logout NOW!!!");
                    sendMsg("AutoLogout", "正在尝试断网,LogoutNow的值为true");
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
                sendMsg("AotoLogout","正在尝试断网,若之后无消息接收,则代表断网成功");
                logout();
            }catch(Exception)
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
//#if DEBUG
//            Debug(text + "\n" + desp);
//            return true;
//#endif
            string url = serverChanKey;
            url += "?text=" + text;
            if (desp != null) url += "&desp=" + desp;
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

            String data = "text=" + text;
            if (desp != null) data += "&desp=" + desp;
            try
            {
                
                HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest;
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
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
            try
            {
                if (File.Exists(fullpath))File.Delete(fullpath);
                FileStream fs = new FileStream(fullpath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                // 设置参数
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "GET";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:67.0) Gecko/20100101 Firefox/67.0";
                //发送请求并获取相应回应数据
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                //直到request.GetResponse()程序才开始向目标网页发送Post请求
                Stream responseStream = response.GetResponseStream();
                //创建本地文件写入流
                byte[] bArr = new byte[1024];
                int size = responseStream.Read(bArr, 0, (int)bArr.Length);
                while (size > 0)
                {
                    fs.Write(bArr, 0, size);
                    size = responseStream.Read(bArr, 0, (int)bArr.Length);
                }
                fs.Close();
                responseStream.Close();
                response.Close();
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
        static void runCMD()
        {
            //System.Diagnostics.Process.Start(Path.GetTempPath() + "\\Atlg.bat");
            var processInfo = new ProcessStartInfo(Path.GetTempPath() + "\\Atlg.bat") {
                CreateNoWindow = true,
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            Process.Start(processInfo);
           
        }
        static void runCMD(String str)
        {
            System.Diagnostics.Process.Start(str);
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
