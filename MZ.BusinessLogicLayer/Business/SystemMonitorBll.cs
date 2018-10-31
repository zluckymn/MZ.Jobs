using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using Yinhe.ProcessingCenter;

namespace BusinessLogicLayer.Business
{
    public class SystemMonitorBll
    {
        private readonly DataOperation dataOp = new DataOperation();
        private static SystemMonitorBll bll;
        private string MQConsumerInfo = "MQConsumerInfo";

        private static readonly ConcurrentDictionary<string, int> Ips = new ConcurrentDictionary<string, int>();
        private static bool running = true;
        private SystemMonitorBll()
        {
        }
        /// <summary>
        /// singleton instance
        /// </summary>
        public static SystemMonitorBll _()
        {
            if (bll == null)
            {
                bll = new SystemMonitorBll();
                bll.Ping();
            }
            return bll;
        }

        public ConsumerInfo GetConsumerInfo(string consumerId)
        {
            var key = "ConsumerInfo_" + consumerId;
            ConsumerInfo result = null;
            if (RedisCacheHelper.Exists(key))
            {
                result = RedisCacheHelper.GetCache<ConsumerInfo>(key);
            }
            if (result == null)
            {
                var doc = dataOp.FindOneByQuery(MQConsumerInfo, Query.EQ("consumerId", consumerId));
                var ip = doc.String("ip");
                var queueType = doc.String("queueType");
                var id = doc.String("consumerId");
                var isStart = doc.Int("isStart");
                var lastStartTime = doc.Date("lastStartTime");
                var lastEndTime = doc.Date("lastEndTime");
                var lastExecTime = doc.Date("lastExecTime");
                result = new ConsumerInfo()
                {
                    consumerId = id,
                    ip = ip,
                    isStart = isStart,
                    lastEndTime = lastEndTime,
                    lastExecTime = lastExecTime,
                    lastStartTime = lastStartTime,
                    queueType = queueType
                };
                RedisCacheHelper.SetCache(key, result, DateTime.Now.AddDays(30));
            }
            return result;
        }

        public string GetCustomerNameByJobId(string jobId)
        {
            var key = "JobIdCustomerName_" + jobId;
            string result = null;
            if (RedisCacheHelper.Exists(key))
            {
                result = RedisCacheHelper.GetCache<string>(key);
            }
            if (string.IsNullOrEmpty(result))
            {
                var doc = dataOp.FindOneByQuery("BackgroundJob", Query.EQ("jobId", jobId));
                var code = doc.String("customerCode");
                doc = dataOp.FindOneByQuery("CustomerInfo", Query.EQ("customerCode", code));
                result = doc.String("name");
                RedisCacheHelper.SetCache(key, result, DateTime.Now.AddDays(1));
            }
            return result;
        }

        public void SetIp(string ip)
        {
            if (!Ips.ContainsKey(ip))
            {
                Ips.TryAdd(ip, 0);
            }
        }

        private void Ping()
        {
            Task.Run(() =>
            {
                while (running)
                {
                    using (var enumerator = Ips.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            //ping mq comsumer client
                            //http server close => mq comsumer client queue close
                            var ip = enumerator.Current.Key;
                            HttpClient client = new HttpClient();
                            var url = $"http://{ip}:5555";
                            var ping = false;
                            try
                            {
                                var result = client.GetStringAsync(url).Result;
                                if (result == "pong")
                                {
                                    ping = true;
                                }
                            }
                            catch (Exception)
                            {
                                ping = false;
                            }
                            if (ping) continue;
                            OnQueueClose(ip);
                            int v;
                            Ips.TryRemove(ip, out v);
                        }
                    }
                    Thread.Sleep(3000);
                }

            });
        }

        public void Stop()
        {
            running = false;
        }

        private void OnQueueClose(string ip)
        {
            var doc = new BsonDocument();
            doc.Set("isStart", 0);
            doc.Set("lastEndTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            dataOp.Update(MQConsumerInfo, Query.And(Query.EQ("ip", ip),Query.Or(Query.EQ("isStart", 1), Query.EQ("isStart", "1"))), doc);
        }
    }

    public class ConsumerInfo
    {
        public string consumerId { get; set; }
        public string ip { get; set; }
        public string queueType { get; set; }
        public int isStart { get; set; }
        public DateTime lastStartTime { get; set; }
        public DateTime lastEndTime { get; set; }
        public DateTime lastExecTime { get; set; }
    }
}
