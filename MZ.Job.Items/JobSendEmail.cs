using MongoDB.Bson;
using Quartz;
using System;
using System.Collections.Generic;
using MZ.Jobs.Core;

using MongoDB.Driver.Builders;
using System.Linq;
using System.Text;
using BusinessLogicLayer;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace MZ.Jobs.Items
{
    ///不允许此 Job 并发执行任务（禁止新开线程执行）
    /// 配置项 jobargs limitCondition  空间剩余百分比，推送用户Id
    /// 用于自动更新最新的开发程序配合autoUpdater使用，中控台开始只执行一次的jobUpdater调度任务，进行开一个新进程触发BaT操作
    /// 命令进行卸载后进行更新最新dll后，并进行重新安装操作，并且重新运行操作
    /// context.MergedJobDataMap.GetString("Parameters");获取数据库中配置的jobArgs参数 以bsondoc方式存储，jobArgs  特殊的代码执行配置项，如统计的模块代码ID等
    /// 数据库连接字符串 CustomerConfig.DataBaseConnectionString
    /// 客户代码 CustomerConfig.CustomerCode
    [DisallowConcurrentExecution]
    public sealed class JobSendEmail : JobBase,IJob
    {

         public void Execute(IJobExecutionContext context)
        {
            this.Start(context);//基类初始化对象;
         }
         /// <summary>
         /// 重载基类算 定时发送邮件
         /// </summary>
         /// <returns></returns>
          internal override string GenerateData()
         {
            var errorMessage=new StringBuilder();
            var RetryCount = JobParamsDoc.Text("RetryCount");
            var tableName = "SystemMessagePushQueue";
            var _MongoOp = MongoOpCollection.GetMongoOp(CusAppConfig.MasterDataBaseConnectionString);
            var _dataop = new Yinhe.ProcessingCenter.DataOperation(_MongoOp);
            var query = Query.And(Query.NE("sendStatus", "1"));//有错误消息
            var limit = 20;
            //获取当前登录状态
            var topRecoredList = _dataop.FindLimitFieldsByQuery(tableName, query, new MongoDB.Driver.SortByDocument() { }, 0, limit, new string[] { "title", "content", "arrivedUserIds", "sendDate" }).ToList();
            var sendUserIds = topRecoredList.Select(c => (BsonValue)c.Text("arrivedUserIds"));
            var sendUserList = _dataop.FindAll("SysUser").SetFields("emailAddr", "userId").ToList();
            var titileDistinct = new List<int>();
            var smtp = AntaSmtpHelper.loadSmtpInfo();
            var resultJson = string.Empty;
            //对重复的titile内容进行过滤减少展示
            foreach (var messageInfo in topRecoredList)
            {
                var sendDate = messageInfo.Text("sendDate");
                ///时间过滤控制
                if (!string.IsNullOrEmpty(sendDate))
                {
                    //时间生成
                    var curDate = DateTime.Now;
                  
                    if (sendDate.Contains("yyyy"))
                    {
                        sendDate = sendDate.Replace("yyyy", curDate.Year.ToString());
                    }
                    if (sendDate.Contains("MM"))
                    {
                        sendDate = sendDate.Replace("MM", curDate.Month.ToString());
                    }
                    if (sendDate.Contains("dd"))
                    {
                        sendDate = sendDate.Replace("dd", curDate.Day.ToString());
                    }
                    if (sendDate.Contains("hh"))
                    {
                        sendDate = sendDate.Replace("hh", curDate.Hour.ToString());
                    }
                    if (sendDate.Contains("mm"))
                    {
                        sendDate = sendDate.Replace("mm", curDate.Minute.ToString());
                    }
                    if (sendDate.Contains("ss"))
                    {
                        sendDate = sendDate.Replace("ss", curDate.Second.ToString());
                    }
                    DateTime messSendDate;
                    if (DateTime.TryParse(sendDate, out messSendDate))
                    {
                        //整小时发送，修正为匹配分钟，15分钟内
                        if (curDate.Year == messSendDate.Year && curDate.Month == messSendDate.Month && curDate.Day == messSendDate.Day && curDate.Hour == messSendDate.Hour && Math.Abs(curDate.Minute - messSendDate.Minute) <= 30)
                        {

                        }
                        else //时间不匹配需要进行重新轮询时间
                        {
                            continue;
                        }
                    }
                }
                var title = messageInfo.Text("title");
                var hasCode = title.GetHashCode();
               
                var content= messageInfo.Text("content");
                var hitUserIds = messageInfo.Text("arrivedUserIds");
                if (string.IsNullOrEmpty(hitUserIds))
                {
                    hitUserIds = "1";//发给管理员
                }
                var arrivedUserIds = hitUserIds.SplitParam(new string[] { ",","|"});//获取发送人的邮箱
                 
                
                var hitUsers = sendUserList.Where(c => arrivedUserIds.Any(d=>d==c.Text("userId"))).ToList();
                if (hitUsers == null) continue;

                //邮件地址隔开
                var mailAddressArr = new List<string>();
                hitUsers.ForEach(c => {
                    mailAddressArr.AddRange(c.Text("emailAddr").SplitParam(new string[] { ",", "|" }));
                });
                try
                {
                    title = title.Replace("\r", " ");
                    title = title.Replace("\n", " ");
                    var mailMessage = AntaSmtpHelper.buildMailMessage(mailAddressArr.Distinct().ToList(), content, title);
                    smtp.SendAsync(mailMessage,null);
                    DBChangeQueue.Instance.EnQueue(new Yinhe.ProcessingCenter.DataRule.StorageData()
                    {
                        Document = new BsonDocument().Add("sendStatus", "1"),
                        Name = tableName,
                        Query = Query.EQ("_id", ObjectId.Parse(messageInfo.Text("_id"))),
                        Type = Yinhe.ProcessingCenter.DataRule.StorageType.Update
                    });
                    
                }
                catch (Exception ex)
                {
                    errorMessage.AppendFormat("发送邮件错误:{0}，请查看调度器服务配置", ex.Message);
                    
                }
                //防止重复发送
                if (!titileDistinct.Contains(title.GetHashCode()))
                {
                    titileDistinct.Add(hasCode);
                }
                else
                {
                    continue;
                }
            }
            if (DBChangeQueue.Instance.Count > 0)
            {
                StartDbChangeHelper.StartDbChangeProcessQuick(_MongoOp);
            }
            if (!string.IsNullOrEmpty(errorMessage.ToString())) {
                var remoteMessageInfo = new Yinhe.ProcessingCenter.PushMessageInfo()
                {
                    content = "邮件发送预警",
                    approvalUserId = this.JobParamsDoc.Text("approvalUserId"),
                    errorMessage = errorMessage.ToString(),
                    customerCode = Yinhe.ProcessingCenter.SysAppConfig.CustomerCode,
                    logType = "1"

                };
                return remoteMessageInfo.ToJson();
            }
           
            return errorMessage.ToString();
        }
    }
}
