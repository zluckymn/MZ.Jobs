using MongoDB.Bson;
using MZ.Jobs.Core.Business.Info;
using MZ.Jobs.Core.Services;
using Quartz;
using System;
using System.Web;

namespace MZ.Jobs.Core
{
    public class SchedulerJobListener : IJobListener
    {
        public void JobExecutionVetoed(IJobExecutionContext context)
        {

        }

        public void JobToBeExecuted(IJobExecutionContext context)
        {

        }

        public void JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException)
        {
            var jobId = context.JobDetail.Key.Name;
            DateTime NextFireTimeUtc = TimeZoneInfo.ConvertTimeFromUtc(context.NextFireTimeUtc.Value.DateTime, TimeZoneInfo.Local);
            DateTime FireTimeUtc = TimeZoneInfo.ConvertTimeFromUtc(context.FireTimeUtc.Value.DateTime, TimeZoneInfo.Local);

            double TotalSeconds = context.JobRunTime.TotalSeconds;
            string JobName = string.Empty;
            string LogContent = string.Empty;
            var resultContent = new BsonDocument();

            var curJob = new BackgroundJob();
            curJob.jobId = jobId;
            if (context.MergedJobDataMap != null)
            {
                var jobType = context.MergedJobDataMap.GetString("JobType");//类型
                //调度服务已经由心跳进行发送运行不用进行添加状态，修改成按照jobType判断
                if (jobType != ((int)BackgroundJobType.Manager).ToString()&& jobType != ((int)BackgroundJobType.Updater).ToString())
                {
                    JobName = context.MergedJobDataMap.GetString("JobName");
                    LogContent = context.MergedJobDataMap.GetString("execData");
                   
                    var className= context.MergedJobDataMap.GetString("ClassName");//
                    var assemblyName= context.MergedJobDataMap.GetString("AssemblyName");//处理类
                    resultContent.Set("status", "true");
                    resultContent.Set("execData", LogContent);
                    if (jobException != null)
                    {
                        resultContent.Set("status", "false");
                        resultContent.Set("errorMessage", jobException.Message);
                    }
                    curJob.name = JobName;
                    curJob.className = className;
                    curJob.assemblyName = assemblyName;
                    curJob.jobType = jobType;
                    new BackgroundJobService().UpdateBackgroundJobStatus(curJob, FireTimeUtc, NextFireTimeUtc, TotalSeconds, resultContent.ToJson());
                }
                //JobLogger.Info("LogContent");
                //添加job运行情况日志发送中控
              
            }
          

        }

        public string Name
        {
            get { return "SchedulerJobListener"; }
        }
    }
}
