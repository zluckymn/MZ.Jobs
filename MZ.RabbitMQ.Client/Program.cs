using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using BusinessLogicLayer.Business;
using MongoDB.Bson;
using MZ.BusinessLogicLayer.Business;
using MZ.Jobs.Core.Business.Info;
using Yinhe.ProcessingCenter;
using Yinhe.ProcessingCenter.DataRule;

namespace MZ.RabbitMQ.Client
{
    class Program
    {

        /// <summary>
        /// 业务连接数据
        /// </summary>

        public static EasyNetQHelper EasyNetQHelper = new EasyNetQHelper();
        private static readonly JobBll jobBll = JobBll._();
        private static readonly JobLogBll jobLogBll = JobLogBll._();
        private static readonly CommonDbChange commonDbChangeBll = CommonDbChange._();
        private static List<string> queueList = new List<string>();
        private static Dictionary<string, int> idMap = new Dictionary<string, int>();
        private static Dictionary<string, EasyNetQHelper> queueMap = new Dictionary<string, EasyNetQHelper>();
        static string GetLocalIpv4()
        {
            string name = Dns.GetHostName();
            IPAddress[] ipadrlist = Dns.GetHostAddresses(name);
            foreach (IPAddress ipa in ipadrlist)
            {
                if (ipa.AddressFamily == AddressFamily.InterNetwork)
                    return ipa.ToString();
            }
            return "127.0.0.1";
        }
        static void Main(string[] args)
        {
           var ip = GetLocalIpv4();
            //Task.Run(() => new SmallHttpServer().Listen(ip, 5555));
            while (true)
            {
                if (queueList.Count < 5)
                {
                    ShowInfo();
                }
                var r = Console.ReadLine();
                if (r == "quit")
                {
                    break;
                }
                if (queueList.Contains(r))
                {
                    ShowQueueInfo(string.Empty);
                    continue;
                }
                switch (r)
                {
                    case "1":
                        var e1 = EasyNetQHelper.VhGeneraterHelper(RabbitMqVirtualHostType.SystemDataAnalyse);
                        e1.Listen<BackgroundJobLog>(String.Empty, OnDataAnalyse, () =>
                        {
                            var re = CallBack.OnListenSuccess(ip, (int)RabbitMqVirtualHostType.SystemDataAnalyse);
                            if (re.Status == Status.Successful)
                            {
                                var id = re.BsonInfo.Int("consumerId");
                                idMap.Add("1", id);
                                e1.Id = id.ToString();
                                e1.OnDispose = () => CallBack.OnQueueClose(id);
                                e1.SetLastSuccessTime(re.BsonInfo.Date("lastExecTime"));
                            }
                            queueList.Add("1");
                            queueMap.Add("1", e1);
                            ShowQueueInfo("Job执行业务数据");
                        });
                        break;
                    case "2":
                        var e2 = EasyNetQHelper.VhGeneraterHelper(RabbitMqVirtualHostType.LogAnalyse);
                        e2.Listen<BackgroundJobLog>(String.Empty, OnLogCreate, () =>
                        {
                            var re = CallBack.OnListenSuccess(ip, (int)RabbitMqVirtualHostType.LogAnalyse);
                            if (re.Status == Status.Successful)
                            {
                                var id = re.BsonInfo.Int("consumerId");
                                idMap.Add("2", id);
                                e2.Id = id.ToString();
                                e2.OnDispose = () => CallBack.OnQueueClose(id);
                                e2.SetLastSuccessTime(re.BsonInfo.Date("lastExecTime"));
                            }
                            queueList.Add("2");
                            queueMap.Add("2", e2);
                            ShowQueueInfo("日志保存");
                        });
                        break;
                    case "3":
                        var e3 = EasyNetQHelper.VhGeneraterHelper(RabbitMqVirtualHostType.JobUpdate);
                        e3.Listen<string>(String.Empty, OnJobUpdate, () =>
                        {
                            var re = CallBack.OnListenSuccess(ip, (int)RabbitMqVirtualHostType.JobUpdate);
                            if (re.Status == Status.Successful)
                            {
                                var id = re.BsonInfo.Int("consumerId");
                                idMap.Add("3", id);
                                e3.Id = id.ToString();
                                e3.OnDispose = () => CallBack.OnQueueClose(id);
                                e3.SetLastSuccessTime(re.BsonInfo.Date("lastExecTime"));
                            }
                            queueMap.Add("3", e3);
                            queueList.Add("3"); ShowQueueInfo("调度job状态更新");
                        });
                        break;
                    case "4":
                        var e4 = EasyNetQHelper.VhGeneraterHelper(RabbitMqVirtualHostType.SystemHealth);
                        e4.Listen<BackgroundJobLog>(String.Empty, OnSystemHealth, () =>
                        {
                            var re = CallBack.OnListenSuccess(ip, (int)RabbitMqVirtualHostType.SystemHealth);
                            if (re.Status == Status.Successful)
                            {
                                var id = re.BsonInfo.Int("consumerId");
                                idMap.Add("4", id);
                                e4.Id = id.ToString();
                                e4.OnDispose = () => CallBack.OnQueueClose(id);
                                e4.SetLastSuccessTime(re.BsonInfo.Date("lastExecTime"));
                            }
                            queueList.Add("4");
                            queueMap.Add("4", e4);
                            ShowQueueInfo("调度job站点监控数据回写");
                        });
                        break;
                    case "5":
                        var e5 = EasyNetQHelper.VhGeneraterHelper(RabbitMqVirtualHostType.CommonDBChangeQueue);
                        e5.Listen<StorageDataForSerialize>(String.Empty, OnCommonDbChange, () =>
                        {
                            var re = CallBack.OnListenSuccess(ip, (int)RabbitMqVirtualHostType.CommonDBChangeQueue);
                            if (re.Status == Status.Successful)
                            {
                                var id = re.BsonInfo.Int("consumerId");
                                idMap.Add("5", id);
                                e5.Id = id.ToString();
                                e5.OnDispose = () => CallBack.OnQueueClose(id);
                                e5.SetLastSuccessTime(re.BsonInfo.Date("lastExecTime"));
                            }
                            queueList.Add("5");
                            queueMap.Add("5", e5);
                            ShowQueueInfo("通用数据库更新");
                        });
                        break;
                }
                Thread.Sleep(50);
            }
            EasyNetQHelper.Dispose();
            foreach (var helper in queueMap.Values)
            {
                helper.Dispose();
            }
            Console.WriteLine("end");
        }
        private static void ShowInfo()
        {
            Console.WriteLine("请选则需要建立的队列:");
            Console.WriteLine("1.业务数据队列");
            Console.WriteLine("2.日志保存队列建立");
            Console.WriteLine("3.调度job状态更新队列");
            Console.WriteLine("4.调度job站点监控数据回写队列建立");
            Console.WriteLine("5.通用数据库更新回写队列建立");
        }
        private static void ShowQueueInfo(string queueName)
        {
            var message = ($"{queueName}队列已建立成功，准备消费 目前队列总数：{queueList.Count}\n在线队列ID:{string.Join(",", queueList.ToArray())}");
            ShowMessageInfo("注册", message);
        }


        /// <summary>
        /// 业务数据分析
        /// </summary>
        /// <param name="jobLog"></param>
        private static void OnDataAnalyse(BackgroundJobLog jobLog)
        {
            try
            {
                var content = jobLog.runLog;
                var bsonContent = new BsonDocument();
                if (!string.IsNullOrEmpty(content))
                {
                    bsonContent = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(content);
                    var json = ReceiveSaveData.SaveData(bsonContent.String("execData"), jobLog.customerCode);
                    content = json.Message;
                    if (json.Success == false)
                    {
                        CallBack.OnExecFail(idMap["1"], 1, json.Message, jobLog.ToJson());
                    }
                    else
                    {
                        queueMap["1"].SuccessTime = DateTime.Now;
                    }
                }
                var message = ($"处理结果：{jobLog.jobId}_{jobLog.customerCode}:{content}\n");
                ShowMessageInfo("OnDataAnalyse", message);
            }
            catch (Exception e)
            {
                CallBack.OnExecFail(idMap["1"], 0, e.ToString(), jobLog.ToJson());
            }

        }
        /// <summary>
        /// 添加Job执行日志
        /// </summary>
        /// <param name="jobLog"></param>
        private static void OnLogCreate(BackgroundJobLog jobLog)
        {

            var bsonDoc = new BsonDocument
            {
                {"jobId", jobLog.jobId},
                {"jobType", jobLog.jobType},
                {"name", jobLog.name},
                {"runLog", jobLog.runLog},
                {"executionTime", jobLog.executionTime},
                {"executionDuration", jobLog.executionDuration},
                {"customerCode", jobLog.customerCode},
                {"createDate", DateTime.Now.ToString("yyy-MM-dd HH:mm:ss")},
                { "clientInfo",jobLog.clientInfo}
            };

            try
            {
                var result = jobLogBll.CreateLogQuick(bsonDoc);
                var msg = result.Status == Status.Successful ? "成功" : "失败";
                if (result.Status != Status.Successful)
                {
                    CallBack.OnExecFail(idMap["2"], 1, result.Message, jobLog.ToJson());
                }
                else
                {
                    queueMap["2"].SuccessTime = DateTime.Now;
                }
                var message = ($"{bsonDoc.Text("name")}_{bsonDoc.Text("jobId")}处理结果：{msg}\n");
                ShowMessageInfo("OnLogCreate", message);
            }
            catch (Exception e)
            {
                CallBack.OnExecFail(idMap["2"], 0, e.ToString(), jobLog.ToJson());
            }

        }

        /// <summary>
        /// 处理job更新
        /// </summary>
        /// <param name="jobDocStr"></param>
        private static void OnJobUpdate(string jobDocStr)
        {

            try
            {
                var dataDoc = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(jobDocStr);
                var jobId = dataDoc.Text("jobId");
                var customerCode = dataDoc.Text("customerCode");
                dataDoc.Remove("jobId");
                dataDoc.Remove("customerCode");
                var result = jobBll.Update(jobId, dataDoc);
                if (result.Status != Status.Successful)
                {
                    CallBack.OnExecFail(idMap["3"], 1, result.Message, jobDocStr);
                }
                else
                {
                    queueMap["3"].SuccessTime = DateTime.Now;
                }
                var msg = result.Status == Status.Successful ? "成功" : "失败";
                var message = ($"{dataDoc.Text("jobId")}处理结果：{msg}\n{jobDocStr}\n");
                ShowMessageInfo("OnJobUpdate", message);
            }
            catch (Exception e)
            {
                CallBack.OnExecFail(idMap["3"], 0, e.ToString(), jobDocStr);
            }
        }

        /// <summary>
        /// 处理系统健康状态
        /// </summary>
        /// <param name="jobLog"></param>
        private static void OnSystemHealth(BackgroundJobLog jobLog)
        {
        
            if (jobLog != null)
            {
                try
                {
                    var bsonDoc = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(jobLog.runLog);
                    if (bsonDoc != null)
                    {
                        if (!string.IsNullOrEmpty(bsonDoc.Text("execData")))
                        {
                            var messageInfo = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<PushMessageInfo>(bsonDoc.Text("execData"));
                            jobLogBll.ProcessPushMessageInfo(messageInfo);
                        }
                    }
                    var message = ($"{jobLog.jobId}{jobLog.name}{jobLog.customerCode}{jobLog.runLog}\n");
                    ShowMessageInfo("OnSystemHealth", message);
                }
                catch (Exception e)
                {
                    CallBack.OnExecFail(idMap["4"], 0, e.ToString(), jobLog.ToJson());
                }

            }

        }

        /// <summary>
        /// 通用数据更新
        /// </summary>
        /// <param name="storageDataForSerialize"></param>
        private static void OnCommonDbChange(StorageDataForSerialize storageDataForSerialize)
        {
            try
            {
                var result = commonDbChangeBll.SubmitChange(storageDataForSerialize.ToStorageData());
                if (result.Status != Status.Successful)
                {
                    CallBack.OnExecFail(idMap["5"], 1, result.Message, storageDataForSerialize.ToJson());
                }
                else
                {
                    queueMap["5"].SuccessTime = DateTime.Now;
                }
                var msg = result.Status == Status.Successful ? "成功" : "失败";
                var message = ($"{msg} \n{storageDataForSerialize.ToJson()}\n");
                ShowMessageInfo("OnCommonDBChange", message);
            }
            catch (Exception e)
            {
                CallBack.OnExecFail(idMap["5"], 0, e.ToString(), storageDataForSerialize.ToJson());
            }

        }

        /// <summary>
        /// 消息输出
        /// </summary>
        /// <param name="funcName"></param>
        /// <param name="msg"></param>
        private static void ShowMessageInfo(string funcName, string msg)
        {
            Console.WriteLine($"{DateTime.Now.ToString()} {funcName}:{msg}");
        }

    }
    [MessagePack.MessagePackObject(true)]
    public class Message
    {
        public int Id { get; set; }
        public string Text { get; set; }

    }
}
