using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using BusinessLogicLayer;
using BusinessLogicLayer.Business;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Yinhe.ProcessingCenter;
using Yinhe.ProcessingCenter.Quick;

namespace MZ.WebHost.Controllers
{
    public class SystemMonitorController : Yinhe.ProcessingCenter.ControllerBase
    {
        public static string MQConsumerInfo = "MQConsumerInfo";
        public static string MQConsumerLog = "MQConsumerLog";

        public ActionResult QueueStatus()
        {
            return View();
        }

        public ActionResult MoreSystem()
        {
            return View();
        }
        public ActionResult QueueServerStatus()
        {
            return View();
        }
        public ActionResult ConsumeException()
        {
            return View();
        }
        /// <summary>
        /// Mq消费者client status
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        public ActionResult ConsumerInfo(int limit, int offset, string search = null)
        {
            var query = Query.Null;
            if (!string.IsNullOrEmpty(search))
            {
                query = Query.Matches("ip", BsonRegularExpression.Create($"/{search}/"));
            }
            var total = dataOp.FindCount(MQConsumerInfo, query);
            var rows = new List<dynamic>();
            if (total > 0)
            {
                var sortBy = new SortByDocument { { "isStart", -1 }, { "lastStartTime", -1 }, { "lastEndTime", -1 }, { "ip", 1 } };
                var docs = dataOp.FindLimitByQuery(MQConsumerInfo, query, sortBy, offset, limit);
                foreach (var doc in docs)
                {
                    var consumerId = doc.Int("consumerId");
                    var ip = doc.String("ip");
                    var queueType = doc.String("queueType");
                    var isStart = doc.Int("isStart");
                    if (isStart > 0)
                    {
                        SystemMonitorBll._().SetIp(ip);
                    }
                    var lastStartTime = doc.String("lastStartTime");
                    var lastEndTime = doc.String("lastEndTime");
                    var lastExecTime = doc.String("lastExecTime");
                    rows.Add(new { consumerId, ip, queueType, isStart, lastStartTime, lastEndTime, lastExecTime });
                }
            }
            return Json(new { total, rows }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Mq consume failed log
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        public ActionResult ConsumerLogInfo(int limit, int offset, string search = null)
        {
            var query = Query.Null;
            if (!string.IsNullOrEmpty(search))
            {
            }
            var total = dataOp.FindCount(MQConsumerLog, query);
            var rows = new List<dynamic>();
            if (total > 0)
            {
                var sortBy = new SortByDocument { { "time", -1 } };
                var docs = dataOp.FindLimitByQuery(MQConsumerLog, query, sortBy, offset, limit);
                foreach (var doc in docs)
                {
                    var logId = doc.Int("logId");
                    var consumerId = doc.String("consumerId");
                    var info = SystemMonitorBll._().GetConsumerInfo(consumerId);
                    var ip = info?.ip;
                    var queueType = info?.queueType;
                    var type = doc.Int("type");
                    var msg = doc.String("msg");
                    var sourceData = doc.String("sourceData");
                    if (sourceData.Length > 2500)
                    {
                        sourceData = RemoveRunLog(sourceData);
                    }
                    var time = doc.String("time");
                    var customer = SystemMonitorBll._().GetCustomerNameByJobId(GetJobId(sourceData));
                    rows.Add(new { logId, ip, queueType, type, msg, sourceData, time, customer });
                }
            }
            return Json(new { total, rows }, JsonRequestBehavior.AllowGet);
        }

        public string GetRabbitMQIP(string address)
        {
            var beginIndex = address.IndexOf("=");
            if (beginIndex == -1)
            { beginIndex = 0; }
            else
            {
                beginIndex++;
            }
            var endindex = address.IndexOf(";");
            if (endindex == -1)
            {
                endindex = address.Length - beginIndex;
            }
            var hitStr = address.Substring(beginIndex, endindex - beginIndex);
            return hitStr;
        }
        public ActionResult QueueRunStatus(int limit, int offset, string search = null)
        {
            var mqAddress = CusAppConfig.RabbitMQHostStr;
            QuickHttpHelper http = new QuickHttpHelper();
            ///固定模式后续需要修改对应地址authorization模拟登陆
            var addressList = mqAddress.Split(new string[] { "|"}, StringSplitOptions.RemoveEmptyEntries);
            var rows = new List<dynamic>();
            foreach (var address in addressList)
            {
                var begindate = DateTime.Now;
                //请求数据
                HttpItem item = new HttpItem()
                {
                    URL = address,
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8",
                    UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36",

                };
                 
                item.Header.Add("Authorization", "Basic YW50YXBvczphbnRhcG9z");
                var result = http.GetHtml(item);
                var endDate = DateTime.Now;
                var host = GetRabbitMQIP(address);
                var name = "ping侦测";
                var ping = begindate;
                var pong = endDate;
                var interval = (endDate - begindate).TotalSeconds;
                var status = 0;
                var diff = Math.Abs((ping - DateTime.Now).TotalSeconds) - interval > 20 ? 1 : 0;
                var threshold = 5;
                if (result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    status = 0;
                }
                rows.Add(new { host, name, ping = ping.ToString("yyyy-MM-dd HH:mm:ss.fff"), pong = pong.ToString("yyyy-MM-dd HH:mm:ss.fff"), interval, threshold, status, diff });


            }
            return Json(new { addressList.Length, rows }, JsonRequestBehavior.AllowGet);
        }

        static string RemoveRunLog(string source)
        {
            Regex regex = new Regex("\"runLog\" : \".*\"");
            source = regex.Replace(source, "\"runLog\" : \"内容过长,已省略输入\"");
            return source;
        }

        static string GetJobId(string source)
        {
            var r = "(?<=\"jobId\" : \").*?(?=\",)";
            //var r1 = "\"jobId\" : \"(.*)\",";
            Regex regex = new Regex(r);
            var match = regex.Match(source);
            return match.Value;
        }
    }
}