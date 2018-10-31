using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using Yinhe.ProcessingCenter;
using MongoDB.Driver;
using MZ.Jobs.Core.Business.Info;
using System.Collections;
using System.Threading.Tasks;
using BusinessLogicLayer.Business;
using BusinessLogicLayer.Common;
using MZ.Jobs.Core;
using MZ.RabbitMQ;
using Yinhe.ProcessingCenter.DataRule;
using MessagePack;
using System.Web;

namespace MZ.BusinessLogicLayer.Business
{
    /// <summary>
    /// job处理累
    /// </summary>
    public class JobLogBll: BusinessBase
    {
        #region 构造函数
        /// <summary>
        /// 类私有变量
        /// </summary>
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static readonly EasyNetQHelper _easyNetQHelper = new EasyNetQHelper().VhGeneraterHelper(RabbitMqVirtualHostType.LogAnalyse);
        private const string TableName = "BackgroundJobLog";
        private const string KeyFieldName = "logId";
        /// <summary>
        /// 封闭当前默认构造函数
        /// </summary>
        private JobLogBll() :base(TableName, KeyFieldName)
        {
             
        }
        private JobLogBll(DataOperation dataOp) : base(TableName, KeyFieldName)
        {
            this.dataOp = dataOp;
        }
        public static JobLogBll _() 
        {
            return new JobLogBll();
        }
        public static JobLogBll _(DataOperation dataOp)
        {
            return new JobLogBll(dataOp);
        }
        #endregion

        #region 查询

        /// <summary>
        /// 通过Id字段查找
        /// </summary>
        /// <param name="customerCode"></param>
        /// <returns></returns>
        public MongoCursor<BsonDocument> FindByCustomerCode(string customerCode)
        {
              var obj = dataOp.FindAllByQuery(tableName, Query.EQ("customerCode", customerCode));
              return obj;
        }



        #endregion

        #region 表操作

        /// <summary>
        /// 对数据进行压缩保存
        /// </summary>
        /// <param name="bsonDoc"></param>
        /// <returns></returns>
        public BsonDocument LogDataMessagePack(BsonDocument bsonDoc)
        {
            //超过1000个字符串进行messagePack压缩
            if (bsonDoc.ContainsColumn("runLog")&& bsonDoc.Text("runLog").Length>=1000)
            {
                var runLog = MessagePackSerializer.Serialize(bsonDoc.Text("runLog"),
                    MessagePack.Resolvers.ContractlessStandardResolver.Instance);
                bsonDoc.Set("runLog", runLog);
                bsonDoc.Set("isMessagePack", "1");
            }
            return bsonDoc;
        }
        /// <summary>
        /// 对数据进行解压缩保存
        /// </summary>
        /// <param name="bsonDoc"></param>
        /// <returns></returns>
        public BsonDocument LogDataMessageDePack(BsonDocument bsonDoc)
        {
            if (bsonDoc.Text("isMessagePack") == "1"&&bsonDoc.ContainsColumn("runLog"))
            {
                var runLog = MessagePackSerializer.Deserialize<string>((byte[]) bsonDoc["runLog"],
                    MessagePack.Resolvers.ContractlessStandardResolver.Instance);
                bsonDoc.Set("runLog", runLog);
            }
            return bsonDoc;
        }

        public InvokeResult CreateLogQuick(BsonDocument bsonDoc)
        {
            var mongoOp=MongoOpCollection.GetMongoOp();
            var result=mongoOp.Save(tableName, LogDataMessagePack(bsonDoc));
            return result;
        }
        public InvokeResult CreateLog(BackgroundJobLog jobLog)
        {
            //记录日志到统一日志平台
            logger.Info(jobLog.runLog.Replace("\\",""));
            var bsonDoc = new BsonDocument
            {
                {"jobId", jobLog.jobId},
                {"jobType", jobLog.jobType},
                {"name", jobLog.name},
                {"runLog", jobLog.runLog},
                {"executionTime", jobLog.executionTime},
                {"executionDuration", jobLog.executionDuration},
                {"customerCode", jobLog.customerCode}
            };
           
             var result= CommonDbChangeHelper.SubmitChange(new StorageData()
                    {
                        Document = LogDataMessagePack(bsonDoc),
                        Name = tableName,
                        Type = StorageType.Insert
                    });
           
              
            return result;
        }

        public InvokeResult ProcessJobData(BackgroundJobLog dto)
        {
            if (CommonDbChangeHelper.NeedQueue)
            {
                return ProcessJobDataViaQueue(dto);
            }
            else
            {
                return ProcessJobDataImedicate(dto);
            }
        }

        /// <summary>
        /// 执行更新操作
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public InvokeResult ProcessJobDataViaQueue(BackgroundJobLog dto)
        {
            var systemHealthBll = SystemHealthBll._();
            var success = false;
            switch (int.Parse(dto.jobType))
            {
                case (int)BackgroundJobType.Normal://数据分析
                    success = new EasyNetQHelper().VhGeneraterHelper(RabbitMqVirtualHostType.SystemDataAnalyse).Broadcast(dto);
                    break;
                case (int)BackgroundJobType.SytemHealth://站点健康检测
                    success = new EasyNetQHelper().VhGeneraterHelper(RabbitMqVirtualHostType.SystemHealth).Broadcast(dto);
                    break;
                //管理器
                case (int)BackgroundJobType.Manager: break;
                //更新器
                case (int)BackgroundJobType.Updater: break;
                default:
                    break;

            }
            //添加日志处理队列
            Task.Run(() => { new EasyNetQHelper().VhGeneraterHelper(RabbitMqVirtualHostType.LogAnalyse).Broadcast<BackgroundJobLog>(dto); });
            return new InvokeResult() { Status = success ? Status.Successful : Status.Failed };
        }

        public InvokeResult ProcessJobDataImedicate(BackgroundJobLog jobLog)
        {
            var systemHealthBll = SystemHealthBll._();
            var success = false;
            var content = jobLog.runLog;
            BsonDocument bsonDoc =null;
            if (!string.IsNullOrEmpty(content)) {
                bsonDoc = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(jobLog.runLog);
            }
            if (bsonDoc == null) return new InvokeResult() { Status = Status.Successful };
            switch (int.Parse(jobLog.jobType))
            {
                case (int)BackgroundJobType.Normal://数据分析
                    var json = ReceiveSaveData.SaveData(bsonDoc.String("execData"), jobLog.customerCode);
                    break;
                case (int)BackgroundJobType.SytemHealth://站点健康检测，发送错误邮件
                   
                    if (bsonDoc != null)
                    {
                        if (!string.IsNullOrEmpty(bsonDoc.Text("execData")))
                        {
                            ///发送PushInfo异常信息到站点
                            var messageInfo = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<PushMessageInfo>(bsonDoc.Text("execData"));
                            ProcessPushMessageInfo(messageInfo);
                        }
                    }
                    break;
                //管理器
                case (int)BackgroundJobType.Manager: break;
                //更新器
                case (int)BackgroundJobType.Updater: break;
                default:
                    break;

            }
            //添加日志处理队列，发送到日志平台
            var result = CreateLog(jobLog);
             
            return new InvokeResult() { Status = success ? Status.Successful : Status.Failed };
        }

        /// <summary>
        /// 处理站点健康检查信息,一般为需要发送邮件的内容,后续定期从数据库中读取数据进行邮件发送
        /// </summary>
        /// <param name="jobLog"></param>
        /// <returns></returns>
        public InvokeResult ProcessPushMessageInfo(PushMessageInfo messageInfo)
        {
            var result = new InvokeResult() { Status=Status.Successful};
            if (!string.IsNullOrEmpty(messageInfo.errorMessage))
            {
                
                //messageInfo.errorMessage = HttpUtility.UrlEncode(messageInfo.errorMessage);
                //messageInfo.content = HttpUtility.UrlDecode(messageInfo.content);
                if (!string.IsNullOrEmpty(messageInfo.errorMessage))
                {
                    var bsonDoc = new BsonDocument
                    {
                        {"title",messageInfo.content},
                        {"content", messageInfo.errorMessage},
                        {"arrivedUserIds", messageInfo.approvalUserId},
                        {"sendUserId", messageInfo.approvalUserId},
                        {"sendType", "0"},
                        {"registerDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")},
                        {"sendDate",  DateTime.Now.AddMinutes(5).ToString("yyyy-MM-dd HH:mm:ss")},
                        {"isMessagePack",  1},//加密模式
                        
                    };
                    result = CommonDbChangeHelper.SubmitChange(new StorageData()
                    {
                        Document = bsonDoc,
                        Name = "SystemMessagePushQueue",
                        Type = StorageType.Insert
                    });
                }

            }
           

            return result;
        }
        #endregion


    }

}
