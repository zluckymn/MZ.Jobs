using MZ.Jobs.Core.Business.Info;
using System;
using System.Collections.Generic;

namespace MZ.Jobs.Core.Services
{
    /// <summary>
    /// 通过请求方式获取与中控站点进行交互
    /// </summary>
    public interface IBackgroundJob
    {
        /// <summary>
        /// Job修改
        /// </summary>
        /// <param name="backgroundJob"></param>
        /// <returns></returns>
        bool UpdateBackgroundJob(BackgroundJob backgroundJob);
        /// Job详情
        /// 
        /// <param name="backgroundJobId">Job ID</param>
        /// <returns></returns>
        BackgroundJob GetBackgroundJob(System.Guid backgroundJobId);
        /// <summary>
        /// 获取允许调度的Job集合
        /// </summary>
        /// <returns></returns>
        List<BackgroundJob> GeAllowScheduleJobInfoList();
        /// <summary>
        /// 更新Job状态
        /// </summary>
        /// <param name="backgroundJobId">Job ID</param>
        /// <param name="state">状态</param>
        /// <returns></returns>
        bool UpdateBackgroundJobState(System.Guid backgroundJobId, int state);
        /// <summary>
        /// 更新Job运行信息 
        /// </summary>
        /// <param name="backgroundJobId">Job ID</param>
        /// <param name="lastRunTime">最后运行时间</param>
        /// <param name="nextRunTime">下次运行时间</param>
         void UpdateBackgroundJobStatus(System.Guid backgroundJobId, DateTime lastRunTime, DateTime nextRunTime);
        /// <summary>
        /// 更新Job运行信息 
        /// </summary>
        /// <param name="backgroundJobId">Job ID</param>
        /// <param name="jobName">Job名称</param>
        /// <param name="lastRunTime">最后运行时间</param>
        /// <param name="nextRunTime">下次运行时间</param>
        /// <param name="executionDuration">运行时长</param>
        /// <param name="runLog">日志</param>
        void UpdateBackgroundJobStatus(System.Guid backgroundJobId, string jobName, DateTime lastRunTime, DateTime nextRunTime, double executionDuration, string runLog);
        /// <summary>
        /// Job运行日志记录
        /// </summary>
        /// <param name="backgroundJobId">Job ID</param>
        /// <param name="jobName">Job名称</param>
        /// <param name="executionTime">开始执行时间</param>
        /// <param name="runLog">日志内容</param>
        void WriteBackgroundJoLog(System.Guid backgroundJobId, string jobName, DateTime executionTime, string runLog);
        /// <summary>
        ///  Job运行日志记录
        /// </summary>
        /// <param name="backgroundJobId"></param>
        /// <param name="jobName"></param>
        /// <param name="executionTime"></param>
        /// <param name="executionDuration"></param>
        /// <param name="runLog"></param>
        void WriteBackgroundJoLog(System.Guid backgroundJobId, string jobName, DateTime executionTime, double executionDuration, string runLog);
        

    }
}
