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
    /// 数据库死锁监控，每日检测到死锁数据的时候发送邮件通知运维人员查看
    /// 
    /// </summary>
    [DisallowConcurrentExecution]
    public sealed class JobDeadLockHealthCheck : JobBase,IJob
    {

         public void Execute(IJobExecutionContext context)
        {
            this.Start(context);//基类初始化对象;
         }
        /// <summary>
        /// 从数据库中调用EUR_DEADLOCK_DETAIL
        /// TimePoint  statement_parameter_k statement_k statement_parameter [statement] waitresource_k waitresource
        /// isolationlevel_k isolationlevel waittime_k waittime
        ///clientapp_k clientapp hostname_k hostname
        /// </summary>
        /// <returns></returns>
        internal override string GenerateData()
         {
            //获取异常数据的接口地址通过pos站点数据
            var posUrl = JobParamsDoc.Text("PosUrl");
            var result= GetHtml(posUrl);
            if (result.StatusCode==System.Net.HttpStatusCode.OK)
            {
                try
                {
                    var resultDoc=MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(result.Html);
                    if(!resultDoc.ContainsColumn("data")||string.IsNullOrEmpty(resultDoc.Text("data")))return string.Empty;
                    var detailDetailInfo = string.Empty;
                    var deadLockList = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<List<BsonDocument>>(resultDoc.Text("data"));
                    if (deadLockList.Count() > 0)
                    {
                        //返回解析后的内容字符串
                        detailDetailInfo = GetDetailFromJson(deadLockList);
                        detailDetailInfo += "详情请通过exec EUR_DEADLOCK_DETAIL 进行查询";
                        var messageInfo = new PushMessageInfo()
                        {
                            content = "数据库死锁预警",
                            approvalUserId = this.JobParamsDoc.Text("approvalUserId"),
                            errorMessage = detailDetailInfo,
                            customerCode = SysAppConfig.CustomerCode,
                            logType = "1"

                        };
                        return messageInfo.ToJson();
                    }
                   

                } catch (Exception ex)
                {

                }
            }
            return string.Empty;
         }

       
    }
}
