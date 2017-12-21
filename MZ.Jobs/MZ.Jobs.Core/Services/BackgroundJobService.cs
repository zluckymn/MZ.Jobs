using MZ.Jobs.Core.Business.Info;
using System;
using System.Collections.Generic;

namespace MZ.Jobs.Core.Services
{
    /// <summary>
    /// 通过请求方式获取与中控站点进行交互
    /// </summary>
    public class BackgroundJobService
    {
        public BackgroundJobService() { }

        /// <summary>
        /// 配置是否发生改变了
        /// </summary>
        /// <returns></returns>
        public bool ConfigurationHasChange()
        {
            return BackgroundJobWebAPIHelper.ConfigurationHasChange();
        }

        /// <summary>
        /// Job修改
        /// </summary>
        /// <param name="backgroundJob"></param>
        /// <returns></returns>
        public bool UpdateBackgroundJob(BackgroundJob backgroundJob)
        {
            return BackgroundJobWebAPIHelper.UpdateBackgroundJob(backgroundJob);
        }
         
        /// <summary>
        /// Job详情
        /// </summary>
        /// <param name="jobId">Job ID</param>
        /// <returns></returns>
        public BackgroundJob GetBackgroundJob(System.Guid jobId)
        {
            return BackgroundJobWebAPIHelper.GetBackgroundJob(jobId);
        }

        /// <summary>
        /// Job详情
        /// </summary>
        /// <returns></returns>
        public BackgroundJob GetBackgroundJobManage()
        {
            return BackgroundJobWebAPIHelper.GetBackgroundJobManage();
        }


        /// <summary>
        /// 获取允许调度的Job集合
        /// </summary>
        /// <returns></returns>
        public List<BackgroundJob> GeAllowScheduleJobInfoList()
        {
            return BackgroundJobWebAPIHelper.GeAllowScheduleJobInfoList();
        }

        /// <summary>
        /// 更新Job状态
        /// </summary>
        /// <param name="jobId">Job ID</param>
        /// <param name="state">状态</param>
        /// <returns></returns>
        public bool UpdateBackgroundJobState(string jobId, int state)
        {
            return BackgroundJobWebAPIHelper.UpdateBackgroundJobState(jobId, state);
        }

        /// <summary>
        /// 更新Job运行信息 
        /// </summary>
        /// <param name="jobId">Job ID</param>
        /// <param name="lastRunTime">最后运行时间</param>
        /// <param name="nextRunTime">下次运行时间</param>
        public void UpdateBackgroundJobStatus(string jobId, DateTime lastRunTime, DateTime nextRunTime)
        {
            BackgroundJobWebAPIHelper.UpdateBackgroundJobStatus(jobId, lastRunTime, nextRunTime);
        }

        /// <summary>
        /// 更新Job运行信息 
        /// </summary>
        /// <param name="job"></param>
        public void UpdateBackgroundJobStatus(BackgroundJob job)
        {
            BackgroundJobWebAPIHelper.UpdateBackgroundJobStatus(job);
        }

        /// <summary>
        /// 更新Job运行信息 
        /// </summary>
        /// <param name="jobInfo"></param>
        /// <param name="lastRunTime">最后运行时间</param>
        /// <param name="nextRunTime">下次运行时间</param>
        /// <param name="executionDuration">运行时长</param>
        /// <param name="runLog">日志</param>
        public void UpdateBackgroundJobStatus(BackgroundJob jobInfo, DateTime lastRunTime, DateTime nextRunTime, double executionDuration, string runLog)
        {
            UpdateBackgroundJobStatus(jobInfo.jobId, lastRunTime, nextRunTime);
            WriteBackgroundJoLog(jobInfo, lastRunTime, executionDuration, runLog);
        }


        /// <summary>
        /// Job运行日志记录
        /// </summary>
        /// <param name="jobInfo"></param>
        /// <param name="executionTime">开始执行时间</param>
        /// <param name="runLog">日志内容</param>
        public void WriteBackgroundJoLog(BackgroundJob jobInfo , DateTime executionTime, string runLog)
        {
           WriteBackgroundJoLog(jobInfo, executionTime, 0, runLog);
        }

        /// <summary>
        /// Job运行日志记录,SchedulerJobListener的运行日志
        /// </summary>
        /// <param name="jobInfo"></param>
        /// <param name="executionTime"></param>
        /// <param name="executionDuration"></param>
        /// <param name="runLog"></param>
        public void WriteBackgroundJoLog(BackgroundJob jobInfo, DateTime executionTime, double executionDuration, string runLog)
        {
             JobLogger.Info($"WriteBackgroundJoLog调度{jobInfo.name} job内容：{runLog.Length}");
            //默认发送通过配置文件判断是否发送日志信息，后续可通过任务进行调度进行配置文件修改，更新各大站点最新的配置信息
            if (CustomerConfig.NeedSendLog)
            {
                BackgroundJobLog backgroundJobLog = new BackgroundJobLog
                {
                    logId = System.Guid.NewGuid().ToString(),
                    jobId = jobInfo.jobId,
                    name = jobInfo.name,
                    accemblyName = jobInfo.assemblyName,
                    className = jobInfo.className,
                    jobType = jobInfo.jobType,
                    executionTime = executionTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    // ReSharper disable once SpecifyACultureInStringConversionExplicitly
                    executionDuration = executionDuration.ToString(),
                    createDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    runLog = runLog
                };
                BackgroundJobWebAPIHelper.WriteBackgroundJoLog(backgroundJobLog);
            }
        }

    }
}
