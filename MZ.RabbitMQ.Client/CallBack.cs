using System;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using Yinhe.ProcessingCenter;

namespace MZ.RabbitMQ.Client
{
    public class CallBack
    {
        public static DataOperation dataOp = new DataOperation();
        public static string MQConsumerInfo = "MQConsumerInfo";
        public static string MQConsumerLog = "MQConsumerLog";
        public static InvokeResult OnListenSuccess(string ip, int queueType)
        {
            var query = Query.And(Query.EQ("ip", ip), Query.EQ("queueType", queueType.ToString()));
            bool isExist = dataOp.FindCount(MQConsumerInfo, query) > 0;
            InvokeResult result;
            if (isExist)
            {
                var doc = new BsonDocument();
                doc.Set("isStart", 1);
                doc.Set("lastStartTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                result = dataOp.Update(MQConsumerInfo, query, doc);
            }
            else
            {
                var doc = new BsonDocument();
                doc.Set("ip", ip);
                doc.Set("queueType", queueType);
                doc.Set("isStart", 1);
                doc.Set("lastStartTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                result = dataOp.Insert(MQConsumerInfo, doc);
            }
            return result;
        }
        public static void OnExecFail(int id, int type, string msg, string sourceData)
        {
            var doc = new BsonDocument();
            doc.Set("consumerId", id);
            doc.Set("type", type);
            doc.Set("msg", msg);
            doc.Set("sourceData", sourceData);
            doc.Set("time", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            dataOp.Insert(MQConsumerLog, doc);
        }

        public static void OnQueueClose(int id)
        {
            var doc = new BsonDocument();
            doc.Set("isStart", 0);
            doc.Set("lastEndTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            dataOp.Update(MQConsumerInfo, Query.EQ("consumerId", id.ToString()), doc);
        }
    }
}
