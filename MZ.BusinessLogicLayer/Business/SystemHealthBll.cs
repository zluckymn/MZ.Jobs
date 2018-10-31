using System.Web;
using MongoDB.Bson;
using MZ.Jobs.Core.Business.Info;
using MZ.RabbitMQ;
using Yinhe.ProcessingCenter;

namespace BusinessLogicLayer.Business
{
    /// <summary>
    /// job处理累
    /// </summary>
    public class SystemHealthBll 
    {

        public static SystemHealthBll _()
        {
            return new SystemHealthBll();
        }
        private static readonly EasyNetQHelper EasyNetQHelper = new EasyNetQHelper().VhGeneraterHelper(RabbitMqVirtualHostType.SystemHealth);
        public bool NeedQueue { get; set; } = RabbitMqConfig.RabbitMqAvaiable;//默认VirtualHost
        /// <summary>
        /// 代表使用队列进行
        /// </summary>
        public   void ExecUnRegisterQueue()
        {
            NeedQueue = false;
        }

       
        /// <summary>
        /// 执行更新操作
        /// </summary>
        /// <param name="jobLog"></param>
        /// <returns></returns>
        public InvokeResult  ExecJob(BackgroundJobLog jobLog)
        {
           
            var postUrl = SysAppConfig.BugPushUrl;
                var bsonDoc = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(jobLog.runLog);
                if (bsonDoc != null)
                {
                    if (!string.IsNullOrEmpty(bsonDoc.Text("execData")))
                    {
                        var messageInfo =
                            MongoDB.Bson.Serialization.BsonSerializer.Deserialize<PushMessageInfo>(
                                bsonDoc.Text("execData"));
                        messageInfo.errorMessage = HttpUtility.UrlEncode(messageInfo.errorMessage);
                        messageInfo.content= HttpUtility.UrlEncode(messageInfo.content); 
                        HttpHelper helper = new HttpHelper();
                        helper.asyncHttpPost(postUrl, messageInfo);

                    }
                }
                return new InvokeResult() {Status = Status.Successful};
           
        }
        /// <summary>
        /// 执行更新操作
        /// </summary>
        /// <param name="jobLog"></param>
        /// <returns></returns>
        public InvokeResult ExecJobViaQueue(BackgroundJobLog jobLog)
        {
            var succeed= EasyNetQHelper.Broadcast(jobLog);
            if (succeed)
            {
                return new InvokeResult() {Status = Status.Successful};
            }
            return new InvokeResult() { Status = Status.Failed };
        }


    }

}
