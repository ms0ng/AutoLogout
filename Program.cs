using System;
using System.Net;
using System.IO;

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

                for (int i = 0; i < stream.Length; i++)
                {
                    Console.WriteLine(stream.ReadByte() + "");
                }

                response.Close();
                stream.Close();
            }catch(WebException e)
            {
                Console.WriteLine(e.Message);
                if(e.Message== "The remote server returned an error: (404) Not Found.")
                {

                }
            }catch(Exception e)
            {
                string msg = e.Message;
            }
            

            


        }
    }
}
