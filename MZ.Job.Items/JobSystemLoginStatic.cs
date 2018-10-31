using MongoDB.Bson;
using Quartz;
using System;
using System.Collections.Generic;
using MZ.Jobs.Core;
using Yinhe.ProcessingCenter;
using MongoDB.Driver.Builders;
using System.Linq;

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
    public sealed class JobSystemLoginStatic : JobBase,IJob
    {

         public void Execute(IJobExecutionContext context)
        {
            this.Start(context);//基类初始化对象;
         }
         /// <summary>
         /// 重载基类算法
         /// </summary>
         /// <returns></returns>
          internal override string GenerateData()
         {

            var start = DateTime.Parse($"{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day}"); //统计当天时间
            var end = DateTime.Parse($"{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day + 1}").AddSeconds(-1);
            var startStr = DateTime.Now.ToString("yyyyMMdd000000"); //统计当天时间
            var endStr = end.ToString("yyyyMMddHHmmss");
            var week = DateTime.Now.DayOfWeek;
            if (week == DayOfWeek.Saturday || week == DayOfWeek.Sunday)//周末跳过
            {
                return string.Empty;
            }
            //获取当前登录状态
            List<BsonDocument> loginList = dataOp.FindAllByQuery("SysBehaviorLog", Query.And(Query.GTE("timeSort", startStr), Query.LTE("timeSort", endStr))).ToList();
            //2015.4.13 新增错误日志发送到125.77.255.2:8023端口进行存储，后续迁移
             JobLogger.Info($"登陆统计个数：{loginList.Count()}");
             if (loginList.Any()) return string.Empty;
             var errorMessage =
                 $"截止{start.ToString()}至{end.ToString()} {DateTime.Now:ddd} 系统目前无任何用户登录进行操作请注意";
                
             var messageInfo = new PushMessageInfo()
             {
                 content="系统登录预警",
                 approvalUserId = this.JobParamsDoc.Text("approvalUserId"),
                 errorMessage =errorMessage,
                 customerCode = SysAppConfig.CustomerCode,
                 logType = "1"

             };
             return messageInfo.ToJson();
         }
    }
}
