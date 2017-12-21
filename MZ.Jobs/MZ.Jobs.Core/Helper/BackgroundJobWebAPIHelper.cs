using MongoDB.Bson;
using MZ.Jobs.Core.Business.Info;
using System;
using System.Collections.Generic;
using Yinhe.ProcessingCenter;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json.Linq;
using System.Web;

namespace MZ.Jobs.Core
{
    /// <summary>
    /// mongo 数据读取
    /// </summary>
    public class BackgroundJobWebAPIHelper
    {


        /// <summary>
        /// 对需要更新的队列数据更新操作进行批量处理,可考虑异步执行
        /// </summary>
        public static void StartUrlExecQuick(object state)
        {

            while (UrlExecQueue.Instance.Count > 0)
            {
                var temp = UrlExecQueue.Instance.DeQueue();
                if (temp != null)
                {
                    ExecHttpWebAPI(temp.UrlString, temp.ParamDoc);
                }
            }
            if (DBChangeQueue.Instance.Count > 0)
            {
                StartUrlExecQuick(state);
            }
        }
        /// <summary>
        /// 生成对应的post参数对象 后续进行MessagePack序列化进行解析与反序列化等进行https://github.com/neuecc/MessagePack-CSharp
        /// 进行序列化来加速代码压缩提高性能
        /// </summary>
        /// <param name="paramDoc"></param>
        /// <returns></returns>
        private static string GeneratePostData(BsonDocument paramDoc)
        {
            var sb = new StringBuilder();
            if (paramDoc == null)
            {
                paramDoc = new BsonDocument();
            }
            if (!paramDoc.ContainsColumn("customerCode"))
            {
                paramDoc.Set("customerCode", CustomerConfig.CustomerCode);
            }
            foreach (var elem in paramDoc.Elements)
            {
                if (elem.Name == "_id") continue;
                sb.AppendFormat("{0}={1}&", elem.Name, paramDoc.Text(elem.Name));
            }
            return sb.ToString().TrimEnd('&');
        }
        /// <summary>
        /// 通过执行webapi请求与中控数据库进行交互，进行token集成验证请求有效性
        /// </summary>
        /// <param name="url"></param>
        /// <param name="paramDoc"></param>
        /// <returns></returns>
        public static string ExecHttpWebAPI(string url, BsonDocument paramDoc)
        {
            var postData = string.Empty;
            var encodeUrl = string.Empty;
            try
            {

                //JobLogger.Info("开始进行url请求{0}\n{1}", url, GeneratePostData(paramDoc));
                encodeUrl = PageReq.UrlGenerateSign(CustomerConfig.ApiUrl + url);//获取加密后的url请求
                var http = new HttpHelper();                                           //创建Httphelper参数对象
                HttpItem item = new HttpItem()
                {
                    URL = encodeUrl,//URL     必需项    
                    UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36",//返回类型    可选项有默认值 
                    Timeout = 25000,
                    Accept = "*/*",
                    Method = "Get",
                    ContentType = "application/x-www-form-urlencoded; charset=UTF-8",  Expect100Continue = false, KeepAlive=true
                };
               
                item.Header.Add("Accept-Encoding", "gzip, deflate");//启用压缩
                postData = GeneratePostData(paramDoc);
                if (!string.IsNullOrEmpty(postData))
                {
                    item.Method = "Post";//URL     可选项 默认为Get
                    item.Postdata = postData;
                   
                }
                 
               
                var result = http.GetHtml(item);

                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    JobLogger.ApiExecInfo("{0},{1}\n{2}\n\r", encodeUrl, postData, result.Html);
                    return result.Html;
                }
                else
                {
                    JobLogger.ApiExecInfo("{0},{1}\n{2}\n\r", encodeUrl, postData, result.Html);
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                JobLogger.Info("{0},{1},{2}\n\r", encodeUrl, postData, ex.Message);
            }
            return string.Empty;
        }
        /// <summary>
        /// 数据解密,后续使用Base64方式进行解密，或者其他方式
        /// </summary>
        /// <returns></returns>
        public static string DataDecode(string resultHtml)
        {
            var html = resultHtml.Replace("\\\"", "\"").Trim('"');

            //if (html.Contains(_id))
            //{
            //    html = html.Replace("\"_id\"", "\"id\"");
            //    //ObjectId("5a097570bb1a598c34d1ba74")
            //    var replaceStr = CusStr(html, "ObjectId(\"", "\")");
            //    html = html.Replace($"ObjectId(\"{replaceStr}\")", $"\"{replaceStr}\"");

            //}
            return html;
        }

        private static string CusStr(string source, string beginStr, string endStr)
        {
            var beginIndex = source.IndexOf(beginStr,StringComparison.Ordinal) + beginStr.Length;
            var lastStr = source.Substring(beginIndex);
            var endIndex = lastStr.IndexOf(endStr, StringComparison.Ordinal);
            var target = source.Substring(beginIndex, endIndex);
            return target;

        }



        /// <summary>
        /// 通过执行webapi请求与中控数据库进行交互，进行token集成验证请求有效性
        /// </summary>
        /// <param name="url"></param>
        /// <param name="paramDoc"></param>
        /// <returns></returns>
        public static string ExecHttpWebAPIDoc(string url, BsonDocument paramDoc)
        {

            try
            {
                var resultHtml = ExecHttpWebAPI(url, paramDoc);
                if (!string.IsNullOrEmpty(resultHtml) && resultHtml.Contains("data"))
                {
                    //返回数据需要多次进行转义返回数据，后续当成加密方式使用
                    //var html = DataDecode(resultHtml);
                    JObject curData = JObject.Parse(resultHtml);

                    if (curData["status"] != null && curData["status"].ToString() == "true")
                    {
                        return curData["data"].ToString();
                    }
                    else
                    {
                        JobLogger.Info(curData["message"].ToString());
                    }

                }
            }
            catch (Exception ex)
            {
                JobLogger.Info(ex.Message);
            }
            return string.Empty;

        }
        /// <summary>
        /// 异步通过执行webapi请求与中控数据库进行交互，进行token集成验证请求有效性，需要进行异步进行，加入队列后马上返回不影响主流程进行
        /// </summary>
        /// <param name="url"></param>
        /// <param name="paramDoc"></param>
        /// <returns></returns>
        public static void ExecHttpWebAPIDocAsync(string url, BsonDocument paramDoc)
        {
            UrlExecQueue.Instance.EnQueue(new UrlExecInfo(url) { ParamDoc = paramDoc });
            //异步执行
            ThreadPool.QueueUserWorkItem((state) => { StartUrlExecQuick(state); });
            //Task.Run(()=>StartUrlExecQuick());//进程服务结束是否会继续运行？请注意
        }



        #region 对象转化

        /// <summary>
        /// 将job对象转化为bsondocument
        /// </summary>
        /// <param name="BackgroundJob"></param>
        /// <returns></returns>
        public static BsonDocument ConvertJobToBsonDoc(BackgroundJob BackgroundJob)
        {
            var bsonDoc = new BsonDocument();
            bsonDoc.Add("jobId", BackgroundJob.jobId);
            bsonDoc.Add("state", BackgroundJob.state);
            bsonDoc.Add("assemblyName", BackgroundJob.assemblyName);
            bsonDoc.Add("className", BackgroundJob.className);
            bsonDoc.Add("jobType", BackgroundJob.jobType);
            bsonDoc.Add("name", BackgroundJob.name);
            bsonDoc.Add("description", BackgroundJob.description);
            bsonDoc.Add("jobArgs", BackgroundJob.jobArgs);
            bsonDoc.Add("cron", BackgroundJob.cron);
            bsonDoc.Add("statementData", BackgroundJob.statementData);
            bsonDoc.Add("nextRunTime", BackgroundJob.nextRunTime);
            bsonDoc.Add("lastRunTime", BackgroundJob.lastRunTime);
            return bsonDoc;
        }

        /// <summary>
        /// 将job对象转化为bsondocument
        /// </summary>
        /// <param name="BackgroundJob"></param>
        /// <returns></returns>
        public static BackgroundJob ConvertBsonDocToJob(BsonDocument bsonDoc)
        {
            BackgroundJob curJobInfo = new BackgroundJob();
            curJobInfo.jobId = bsonDoc.Text("jobId");
            curJobInfo.state = bsonDoc.Int("state");
            curJobInfo.assemblyName = bsonDoc.Text("assemblyName");
            curJobInfo.className = bsonDoc.Text("className");
            curJobInfo.jobType = bsonDoc.Int("jobType").ToString();
            curJobInfo.name = bsonDoc.Text("name");
            curJobInfo.description = bsonDoc.Text("description");
            curJobInfo.jobArgs = bsonDoc.Text("jobArgs");
            curJobInfo.cron = bsonDoc.Text("cron").Trim();
            curJobInfo.statementData = bsonDoc.Text("statementData");
            return curJobInfo;
        }

        /// <summary>
        /// 转化为bsonDoc对象
        /// </summary>
        /// <param name="docStr"></param>
        /// <returns></returns>
        private static BsonDocument ConvertToBsonDoc(string docStr)
        {
            try
            {
                var dataDoc = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(docStr);
                return dataDoc;
            }
            catch (Exception ex)
            {
                JobLogger.Info(ex.Message);
            }
            return new BsonDocument();
        }
        /// <summary>
        ///  转化为bsonDocList对象
        /// </summary>
        /// <param name="docStr"></param>
        /// <returns></returns>
        private static List<BsonDocument> ConvertToBsonDocList(string docStr)
        {
            try
            {
                var dataDoc = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<List<BsonDocument>>(docStr);
                return dataDoc;
            }
            catch (Exception ex)
            {
                JobLogger.Info(ex.Message);
            }
            return new List<BsonDocument>();
        }
        #endregion
        #region job实例操作
        #region BackgroundJob



        /// <summary>
        /// 获取用户配置情况
        /// </summary>
        /// <param name="jobId">Job ID</param>
        /// <returns></returns>
        public static bool ConfigurationHasChange()
        {
            var doc = new BsonDocument();

            var resultDoc = ExecHttpWebAPIDoc(CustomerConfig.CustomerCfgChangeUrl, doc);
            if (!string.IsNullOrEmpty(resultDoc))
            {
                var dataDoc = ConvertToBsonDoc(resultDoc);
                return dataDoc.Text("needChange") == "1";
            }
            return false;
        }


        /// <summary>
        /// Job详情
        /// </summary>
        /// <param name="jobId">Job ID</param>
        /// <returns></returns>
        public static BackgroundJob GetBackgroundJob(System.Guid jobId)
        {
            var doc = new BsonDocument();
            doc.Set("jobId", jobId.ToString());
            var resultDoc = ExecHttpWebAPIDoc(CustomerConfig.CustomerJobInfoUrl, doc);
            if (!string.IsNullOrEmpty(resultDoc))
            {
                var dataDoc = ConvertToBsonDoc(resultDoc);
                return ConvertBsonDocToJob(dataDoc);
            }
            return null;
        }

        /// <summary>
        /// Job详情
        /// </summary>
        /// <param name="jobId">Job ID</param>
        /// <returns></returns>
        public static BackgroundJob GetBackgroundJobManage()
        {
            var doc = new BsonDocument();
            doc.Add("jobType", (int)BackgroundJobType.Manager);
            var resultDoc = ExecHttpWebAPIDoc(CustomerConfig.CustomerJobManageUrl, doc);
            if (!string.IsNullOrEmpty(resultDoc))
            {
                var dataDoc = ConvertToBsonDoc(resultDoc);
                return ConvertBsonDocToJob(dataDoc);
            }
            return null;
        }

        /// <summary>
        /// 测试job返回
        /// </summary>
        /// <returns></returns>
        public static List<BackgroundJob> GetJobInfo_Test()
        {
            List<BackgroundJob> jobList = new List<BackgroundJob>();
            //jobList.Add(new BackgroundJob() {
            //    assemblyName = "MZ.Jobs.Items.TestA.dll",
            //    className = "MZ.Jobs.Items.TestA.JobTestA",
            //    cron = "0/10 * * * * ?",
            //    jobId = "75DF1ACB-D1AB-46CD-83E5-0C295E68TESTA",
            //    name = "testA",
            //    state =(int)BackgroundJobStateType.Running });
            // 在中控进行运行服务设定
            jobList.Add(new BackgroundJob()
            {
                assemblyName = "MZ.Jobs.Items.dll",
                className = "MZ.Jobs.Items.JobFileUploadErrorStatic",
                cron = "0/30 * * * * ?",
                jobId = "75DF1ACB-D1AB-46CD-83E5-0C295EJOBUPDATER",
                name = "JobFileUploadErrorStatic",
                state = (int)BackgroundJobStateType.Running
            });
            return jobList;
        }
        /// <summary>
        /// 获取允许调度的Job集合
        /// </summary>
        /// <returns></returns>
        public static List<BackgroundJob> GeAllowScheduleJobInfoList()
        {
            // return GetJobInfo_Test();
            List<BackgroundJob> jobList = new List<BackgroundJob>();
            try
            {
                var doc = new BsonDocument();
                var resultDoc = ExecHttpWebAPIDoc(CustomerConfig.CustomerJobListUrl, doc);
                if (!string.IsNullOrEmpty(resultDoc))
                {
                    var itemList = ConvertToBsonDocList(resultDoc);

                    foreach (var item in itemList)
                    {
                        jobList.Add(ConvertBsonDocToJob(item));
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return jobList;
        }

        /// <summary>
        /// 更新Job状态 更新命令只加入队列等待操作无论失败与否
        /// </summary>
        /// <param name="jobId">Job ID</param>
        /// <param name="State">状态</param>
        /// <returns></returns>
        public static bool UpdateBackgroundJobState(string jobId, int State)
        {
            var updateDoc = new BsonDocument();
            updateDoc.Set("jobId", jobId.ToString());
            updateDoc.Set("state", State.ToString());
            ExecHttpWebAPIDocAsync(CustomerConfig.CustomerJobUpdateUrl, updateDoc);
            return true;
        }

        /// <summary>
        /// Job修改
        /// </summary>
        /// <param name="BackgroundJob">BackgroundJob实体</param>
        /// <returns></returns>
        public static bool UpdateBackgroundJob(BackgroundJob BackgroundJob)
        {
            var doc = ConvertJobToBsonDoc(BackgroundJob);
            var updateDoc = new BsonDocument();
            ExecHttpWebAPIDocAsync(CustomerConfig.CustomerJobUpdateUrl, doc);
            return true;
        }

        /// <summary>
        /// 更新Job运行信息 
        /// </summary>
        /// <param name="jobId">Job ID</param>
        /// <param name="LastRunTime">最后运行时间</param>
        /// <param name="NextRunTime">下次运行时间</param>
        public static void UpdateBackgroundJobStatus(string jobId, DateTime LastRunTime, DateTime NextRunTime)
        {
            var updateDoc = new BsonDocument();
            updateDoc.Set("jobId", jobId.ToString());
            updateDoc.Set("lastRunTime", LastRunTime.ToString("yyyy-MM-dd HH:mm:ss"));
            updateDoc.Set("nextRunTime", NextRunTime.ToString("yyyy-MM-dd HH:mm:ss"));
            ExecHttpWebAPIDocAsync(CustomerConfig.CustomerJobUpdateUrl, updateDoc);
        }
        /// <summary>
        /// 更新Job运行信息 
        /// </summary>
        /// <param name="jobId">Job ID</param>
        /// <param name="LastRunTime">最后运行时间</param>
        /// <param name="NextRunTime">下次运行时间</param>
        public static void UpdateBackgroundJobStatus(BackgroundJob job)
        {
            var updateDoc = ConvertJobToBsonDoc(job);
            ExecHttpWebAPIDocAsync(CustomerConfig.CustomerJobUpdateUrl, updateDoc);
        }


        #endregion

        #region BackgroundJobLogInfo

        /// <summary>
        /// Job日志记录
        /// </summary>
        /// <param name="jobId">Job ID</param>
        /// <param name="JobName">Job名称</param>
        /// <param name="ExecutionTime">开始执行时间</param>
        /// <param name="ExecutionDuration">执行时长</param>
        /// <param name="RunLog">日志内容</param>
        public static void WriteBackgroundJoLog(BackgroundJobLog backgroundJobLogInfo)
        {
            var updateDoc = new BsonDocument();
            updateDoc.Set("jobId", backgroundJobLogInfo.jobId.ToString());
            updateDoc.Set("name", HttpUtility.UrlEncode(backgroundJobLogInfo.name));
            updateDoc.Set("execDateTime", backgroundJobLogInfo.executionTime);
            updateDoc.Set("execDuration", backgroundJobLogInfo.executionDuration);
            updateDoc.Set("jobType", backgroundJobLogInfo.jobType);
            updateDoc.Set("accemblyName", backgroundJobLogInfo.accemblyName);
            updateDoc.Set("className", backgroundJobLogInfo.className);
            updateDoc.Set("remark", HttpUtility.UrlEncode(backgroundJobLogInfo.runLog));
            
            ExecHttpWebAPIDocAsync(CustomerConfig.CustomerJobLgUrl, updateDoc);
        }
        #endregion 

        #endregion


    }
}
