using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.GZip;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Yinhe.ProcessingCenter;

namespace 公交查询测试接口
{
    class Program
    {
        //GET http://www.doudou360.com:7011/bus/latestio.json?lid=1442&amp;sid=1129&amp;detail= HTTP/1.1
        //Authorization: Basic ZGQyX2lkOjU5NDM2Mg==
        //Accept-Encoding: gzip
        //User-Agent: Dalvik/1.6.0 (Linux; U; Android 4.2.2; Droid4X-WIN Build/JDQ39E)
        //Host: www.doudou360.com:7011
        //Connection: Keep-Alive
        //HTTP/1.1 200 OK
        //Cache-Control: private
        //Content-Type: text/plain; charset=utf-8
        //Content-Encoding: gzip
        //Vary: Accept-Encoding
        //Server: Microsoft-IIS/7.5
        //X-AspNet-Version: 4.0.30319
        //X-Powered-By: ASP.NET
        //Date: Tue, 04 Aug 2015 03:52:12 GMT
        //Content-Length: 1255

        //五缘湾到江头市场 6路 lineId 1441
        //http://www.doudou360.com:7011/bus/latestio.json?lid=1441&sid=3193&detail=&detail=HTTP/1.1
          static void Main(string[] args)
        {
            testDeCompresss();
            return;
            var curPositionStationName = "新亭";
            var alertStationCount = 3;//3站提醒;
                                      //天地花园到五缘湾运动馆 6路1442
            //下班
            //var url = "http://tx3.doudou360.com:25920/bus/latestio.json?lid=6241&sid=5342&sno=1&detail=HTTP/1.1";
            //上班
            var url = "http://tx3.doudou360.com:25920/bus/latestio.json?lid=6242&sid=5343&sno=9&detail=HTTP/1.1";

            var helper = new HttpHelper();
            var httpHeader = new HttpHelper.HttpHeader();
            httpHeader.method = "GET";
            httpHeader.contentType = "text/plain; charset=utf-8";
            httpHeader.userAgent = "Dalvik/1.6.0 (Linux; U; Android 4.2.2; Droid4X-WIN Build/JDQ39E)";
            httpHeader.accept = "image/gif, image/x-xbitmap, image/jpeg, image/pjpeg, application/x-shockwave-flash, application/vnd.ms-excel, application/vnd.ms-powerpoint, application/msword, */*";
            httpHeader.referer = "www.doudou360.com:7011";
            httpHeader.headerParam.Add("Authorization", "Basic ZGQyX2lkOjU5NDM2Mg==");
            // request.Headers.Add("Accept-Encoding", "gzip,deflate");
            var retString = helper.GetHtmlByCustomerHeader(url, httpHeader, null);
            if(!string.IsNullOrEmpty(retString))
            {
              JObject jsonObj = JObject.Parse(retString);
              if(jsonObj["success"].ToString()=="1")
                {
                    var lineHome = jsonObj["result"]["line_home"];
                    var lineInfo = lineHome["line"];
                    var stationList = lineHome["sta"].ToList();
                    foreach (var station in stationList)
                    {
                        Console.Write(station["name"]);
                    }
                    var hitStation = stationList.Where(c => c["name"].ToString().Contains(curPositionStationName)).FirstOrDefault();
                    var hitStationIndex =-1;
                    if (hitStation != null)
                    {
                         hitStationIndex = stationList.IndexOf(hitStation);
                        Console.Write("{0}:Id{1} sno:{2}", curPositionStationName,hitStation["id"], hitStationIndex);
                    }
                    var statusInfo = jsonObj["result"]["status"];
                    foreach (var hitBus in statusInfo)
                    {
                        var sno = 0;//正在前往，或者已经停靠的站点
                        int.TryParse(hitBus["sno"].ToString(), out sno);
                        if (hitBus["stop"].ToString() == "1")//已停靠
                        {
                           
                            if (int.TryParse(hitBus["sno"].ToString(), out sno))
                            {
                                if (sno >=0)
                                {
                                    Console.Write("\n\r一辆车到达【{0}】站点", stationList[sno-1]["name"].ToString());
                                }
                            }
                        }
                        else
                        {
                            if(stationList.Count()> sno + 1)
                            {
                                Console.Write("\n\r一辆车正在开往从【{0}】站点前往【{1}】", stationList[sno]["name"], stationList[sno + 1]["name"]);
                            }
                        }

                        var distanceStationCount = Math.Abs(hitStationIndex - sno);
                        if (sno <= hitStationIndex&&(distanceStationCount <= 3) )
                        {
                            Console.Write("\n\r距离【{2}】{3}站\n\r", stationList[sno]["name"], stationList[sno + 1]["name"],hitStation["name"], distanceStationCount);
                        }
                        Console.Write("\n\r");
                    }
                    Console.Write(jsonObj["result"]["status"]);
                }
             
            }
        
          
            Console.ReadLine();
        }

        private static void testDeCompresss()
        {
            var gzipFileName = "c://fidreq.txt";//fidreq.txt
            var targetDir = "c://";
            byte[] dataBuffer = new byte[4096];
         
            using (System.IO.Stream fs = new FileStream(gzipFileName, FileMode.Open, FileAccess.Read))
            {
                using (GZipInputStream gzipStream = new GZipInputStream(fs))
                {

                    // Change this to your needs
                    string fnOut = Path.Combine(targetDir, Path.GetFileNameWithoutExtension(gzipFileName));

                    using (FileStream fsOut = File.Create(fnOut))
                    {
                        StreamUtils.Copy(gzipStream, fsOut, dataBuffer);
                    }
                }
            }

        }
 
        CookieContainer cookie = new CookieContainer();
        private string HttpPost(string Url, string postDataStr)
        {
            Url = "http://0592.mygolbs.com:8081/XMMyGoWeb/servlet/MyGoServer.HttpPool.HttpHandlerServlet";
            postDataStr = "AAAA77 + 9AAAAGAAAAAEAAAAOAAY5MTHot68AATEAAAAAAA ==";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = Encoding.UTF8.GetByteCount(postDataStr);
            request.CookieContainer = cookie;
            request.Accept = "Accept-Encoding";
            request.Host = "0592.mygolbs.com:8081";
            request.UserAgent = "Dalvik/1.6.0 (Linux; U; Android 4.4.4; XXEmulator Build/KTU84Q)";
           
            request.Headers.Add("sign", "4a8e0a8a9a9a4cc8a7c9fb9f161299ea993222245e44711d523628d0");
            request.Headers.Add("mybus - phoneid", "15959266823");
            request.Headers.Add("handset-type", "XXEmulator");
            request.Headers.Add("handset-os", "4.4.4");
            request.Headers.Add("version-name", "2.2.5");
            request.Headers.Add("version-code", "58");
            request.Headers.Add("attachs", "eyJjaXR5Y29kZSI6IjA1OTIiLCJ2aXNpdHR5cGUiOjMsImNpdHluYW1lIjoi5Y6m6Zeo5biCIiwiaW1laSI6IjcyMjU0MzE1NDY4MzQwMiJ9");
            request.Headers.Add("timeStamp", DateTime.Now.ToString("yyyyMMddHHmmss"));
            Stream myRequestStream = request.GetRequestStream();
            StreamWriter myStreamWriter = new StreamWriter(myRequestStream, Encoding.GetEncoding("gb2312"));
            myStreamWriter.Write(postDataStr);
            myStreamWriter.Close();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            response.Cookies = cookie.GetCookies(response.ResponseUri);
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();
            //Stream stm = new System.IO.Compression.GZipStream(resp.GetResponseStream(), System.IO.Compression.CompressionMode.Decompress)

            myStreamReader.Close();
            myResponseStream.Close();

            return retString;
        }

    }
}
