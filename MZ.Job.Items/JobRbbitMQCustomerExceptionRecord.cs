using MongoDB.Bson;
using Quartz;
using System;
using System.Collections.Generic;
using MZ.Jobs.Core;
using Yinhe.ProcessingCenter;
using MongoDB.Driver.Builders;
using System.Linq;
using System.Text;

// ReSharper disable once CheckNamespace
namespace MZ.Jobs.Items
{
    /// <summary>
    /// 该job通过连接mongoDB数据库查看队列消费失败的
    /// </summary>
    [DisallowConcurrentExecution]
    public sealed class JobRbbitMQCustomerExceptionRecord : JobBase,IJob
    {

         public void Execute(IJobExecutionContext context)
        {
            this.Start(context);//基类初始化对象;
         }
         /// <summary>
         /// 重载基类算法,注意如果重复读取可能发生数据重复发送邮件
         /// </summary>
         /// <returns></returns>
          internal override string GenerateData()
         {
           var rabbitQueueMessageType = JobParamsDoc.Text("MQMessageType");
           var rabbitMQVirtualHost= JobParamsDoc.Text("MQVirtualHost");
           var RetryCount= JobParamsDoc.Text("RetryCount");
           var tableName = $"{rabbitQueueMessageType}_{rabbitMQVirtualHost}_{DateTime.Now.ToString("yyyy-MM")}";

            var week = DateTime.Now.DayOfWeek;
            var retryTimeQuery = Query.And(Query.NE("DeQueueStage", 1), Query.GTE("EnQueueStage", 3));//重试入队多次
            var query = Query.Or(retryTimeQuery, Query.Exists("errorMsgs", true));//有错误消息
            var limit = 20;
            //获取当前登录状态
            var recordCount = dataOp.FindCount(tableName, query);
            var topRecoredList = dataOp.FindLimitFieldsByQuery(tableName, query, new MongoDB.Driver.SortByDocument() { }, 0, limit, new string[] { "messageID", "errorMsgs", "MessageRouter", "MessageBody","EnQueueStage" }).ToList();

            //2015.4.13 新增错误日志发送到125.77.255.2:8023端口进行存储，后续迁移
            if (!topRecoredList.Any()) return string.Empty;
            JobLogger.Info($"队列执行错误个数：{recordCount}");
            var errorMessage = new StringBuilder();
            errorMessage.AppendLine($"{DateTime.Now.ToString("yyyy-MM")}捕获到队列执行异常数据${recordCount}条.前{limit}内容如下:\n\r");
            topRecoredList.ForEach(doc =>
            {
                errorMessage.AppendLine($"{doc.ToJson()}");
            });
            var approvalUserId = this.JobParamsDoc.Text("approvalUserId");
            
            var messageInfo = new PushMessageInfo()
             {
                 content = $"小票上传重试错误预警,失败条数:{recordCount}",
                 approvalUserId = approvalUserId,
                 errorMessage =errorMessage.ToString(),
                 customerCode = SysAppConfig.CustomerCode,
                 logType = "1"

             };
             return messageInfo.ToJson();
         }
    }
}
