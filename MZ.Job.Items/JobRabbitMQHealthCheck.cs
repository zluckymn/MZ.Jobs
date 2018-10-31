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
    ///不允许此 Job 并发执行任务（禁止新开线程执行）
    /// 配置项 jobargs limitCondition  空间剩余百分比，推送用户Id
    /// 用于自动更新最新的开发程序配合autoUpdater使用，中控台开始只执行一次的jobUpdater调度任务，进行开一个新进程触发BaT操作
    /// 命令进行卸载后进行更新最新dll后，并进行重新安装操作，并且重新运行操作
    /// context.MergedJobDataMap.GetString("Parameters");获取数据库中配置的jobArgs参数 以bsondoc方式存储，jobArgs  特殊的代码执行配置项，如统计的模块代码ID等
    /// 数据库连接字符串 CustomerConfig.DataBaseConnectionString
    /// 客户代码 CustomerConfig.CustomerCode
    [DisallowConcurrentExecution]
    public sealed class JobRabbitMQHealthCheck : JobBase,IJob
    {

         public void Execute(IJobExecutionContext context)
        {
            this.Start(context);//基类初始化对象;
         }
         /// <summary>
         /// 重载基类算法，查看队列服务是否正常
         /// </summary>
         /// <returns></returns>
          internal override string GenerateData()
         {
            var mqAddress = JobParamsDoc.Text("MQAddress");
            var mqPassWord= JobParamsDoc.Text("MQAuthorization");
            //http://192.168.185.173:15672/api/queues
            Core.HttpHelper http = new Core.HttpHelper();
            HttpItem item = new HttpItem() {
                URL = mqAddress,
                Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8",
                UserAgent= "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36",
                
            };
            item.Header.Add("Authorization", "Basic YW50YXBvczphbnRhcG9z");
            var result= http.GetHtml(item);
            if (result.StatusCode!=System.Net.HttpStatusCode.OK)
            {
                var messageInfo = new PushMessageInfo()
                {
                    content= "RabbitMQ服务停止预警",
                    approvalUserId = this.JobParamsDoc.Text("approvalUserId"),
                    errorMessage = $"{mqAddress}rabbitMQ队列服务出错",
                    customerCode = SysAppConfig.CustomerCode,
                    logType = "1"

                };
                return messageInfo.ToJson();
            }
            return string.Empty;
         }
    }
}
