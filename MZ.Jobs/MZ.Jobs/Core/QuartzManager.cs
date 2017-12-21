using MongoDB.Bson;
using MZ.Jobs.Core.Business.Info;
using MZ.Jobs.Core.Services;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using Quartz.Impl.Triggers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Web;

namespace MZ.Jobs.Core
{
    public class QuartzManager
    {
        /// <summary>
        /// 从程序集中加载指定类
        /// </summary>
        /// <param name="assemblyName">含后缀的程序集名</param>
        /// <param name="className">含命名空间完整类名</param>
        /// <returns></returns>
        private Type GetClassInfo(string assemblyName, string className)
        {
            Type type = null;
            try
            {
                assemblyName = GetAbsolutePath(assemblyName);
                Assembly assembly = null;
                assembly = Assembly.LoadFrom(assemblyName);
                type = assembly.GetType(className, true, true);
            }
            catch (Exception ex)
            {
            }
            return type;
        }

        /// <summary>
        /// 校验字符串是否为正确的Cron表达式
        /// </summary>
        /// <param name="cronExpression">带校验表达式</param>
        /// <returns></returns>
        public bool ValidExpression(string cronExpression)
        {
            return CronExpression.IsValidExpression(cronExpression);
        }

        /// <summary>
        ///  获取文件的绝对路径
        /// </summary>
        /// <param name="relativePath">相对路径</param>
        /// <returns></returns>
        public string GetAbsolutePath(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                throw new ArgumentNullException("参数relativePath空异常！");
            }
            relativePath = relativePath.Replace("/", "\\");
            if (relativePath[0] == '\\')
            {
                relativePath = relativePath.Remove(0, 1);
            }
            if (HttpContext.Current != null)
            {
                return Path.Combine(HttpRuntime.AppDomainAppPath, relativePath);
            }
            else
            {
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
            }
        }
        

        /// <summary>
        ///  默认调度器服务，需要长期一直运行，对于后台只进行，返回状态值更新
        /// </summary>
        /// <param name="Scheduler"></param>
        public void JobSchedulerMain(IScheduler Scheduler)
        {

            BackgroundJob jobInfo = new BackgroundJobService().GetBackgroundJobManage();
            if (jobInfo == null)//默认job
            {
                //throw new Exception("查找不到默认JobManager");
                ////禁止使用默认job 保值job调度正确运行
                //return;
                jobInfo = new BackgroundJob()
                {
                    assemblyName = "MZ.Jobs.exe",
                    className = "MZ.Jobs.JobItems.ManagerJob",
                    cron = "0/10 * * * * ?",//5秒钟调用
                    jobId = CustomerConfig.Managerjobkey,
                    name = "defaultMainJob",
                    jobType = ((int)BackgroundJobType.Manager).ToString()
                };
            }
            JobKey jobKey = new JobKey(jobInfo.jobId, "Group");

            if (Scheduler.CheckExists(jobKey) == false)
            {
                ScheduleJob(Scheduler, jobInfo);

            }

        }

        /// <summary>
        /// Job调度
        /// </summary>
        /// <param name="scheduler"></param>
        /// <param name="jobInfo"></param>
        public void ScheduleJob(IScheduler scheduler, BackgroundJob jobInfo)
        {
            JobLogger.Info("加入调度{0} ", jobInfo.name);
            if (ValidExpression(jobInfo.cron))
            {
                Type type = GetClassInfo(jobInfo.assemblyName, jobInfo.className);
                if (type != null)
                {
                    IJobDetail job = new JobDetailImpl(jobInfo.jobId.ToString(), "Group", type);
                    job.JobDataMap.Add("Parameters", jobInfo.jobArgs);
                    job.JobDataMap.Add("JobName", jobInfo.name);
                    job.JobDataMap.Add("JobType", jobInfo.jobType);//类型
                    job.JobDataMap.Add("ClassName", jobInfo.className);//
                    job.JobDataMap.Add("AssemblyName", jobInfo.assemblyName);//处理类
                    job.JobDataMap.Add("statementData", jobInfo.statementData);//处理类参数
                    CronTriggerImpl trigger = new CronTriggerImpl();
                    trigger.CronExpressionString = jobInfo.cron;
                    trigger.Name = jobInfo.jobId.ToString();
                    trigger.Description = jobInfo.description;
                    trigger.StartTimeUtc = DateTime.UtcNow;
                    trigger.Group = jobInfo.jobId + "TriggerGroup";
                    scheduler.ScheduleJob(job, trigger);
                }
                else
                {
                    var resultContent = new BsonDocument();
                    resultContent.Set("status", "false");
                    resultContent.Set("errorMessage", jobInfo.assemblyName + jobInfo.className + "无效，无法启动该任务");
                    new BackgroundJobService().WriteBackgroundJoLog(jobInfo, DateTime.Now, resultContent.ToJson());
                }
            }
            else
            {
                var resultContent = new BsonDocument();
                resultContent.Set("status", "false");
                resultContent.Set("errorMessage", string.Format($"{jobInfo.cron}不是正确的Cron表达式,无法启动该任务"));
                new BackgroundJobService().WriteBackgroundJoLog(jobInfo, DateTime.Now, resultContent.ToJson());
            }
        }

        public static DateTime lastExecDate = DateTime.Now.AddHours(-1);
        /// <summary>
        ///1.请求客户配置是否发生了改变,没改变保持当前运行，通知服务进行更新\
        ///2.判断配置是否超过指定时间，后需要刷新下最新的Configuration
        ///3.当获取的List为0的时候如何判断是否因为网络原因而不是因为服务端无配置
        /// </summary>
        /// <returns></returns>
        public bool CanContinueJobScheduler()
        {
            
                var dateExipre = false;
                if ((DateTime.Now - lastExecDate).TotalSeconds >= CustomerConfig.JobListCacheTime / 1000)//1小时执行一次最新情况
                {
                    dateExipre = true;
                    return true;
                }
                var hasChange = new BackgroundJobService().ConfigurationHasChange();
                return hasChange || dateExipre;
           
        }
        /// <summary>
        /// 检测无效任务并进行删除操作，当任务正在进行然后手动将状态设置为0的时候因为获取不到任务，但是本地服务仍然在运行，也无法更新状态回中控，
        /// 因此需要将该任务与中控同步
        /// 当服务正在跑但是寻找不到任何list，此时不应该影响现有正在跑的调度服务，发送一条日志服务取不到任何ListJob回中控
        /// </summary>
        public void InvalidJobCheck(IScheduler Scheduler,List<JobKey> runningJobKeylist)
        {
            try
            {
                if (runningJobKeylist.Count > 0)
                {
                    var allSchedulerKeys = Scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals("Group"));
                    ///正在进行的jobKey比回传回来的并且运行的Jobkey多，将多余的JobKeyDelete掉并保持与中控服务同步
                    if (allSchedulerKeys.Count > runningJobKeylist.Count)
                    {
                        foreach (var jobKey in allSchedulerKeys)
                        {
                            if (!runningJobKeylist.Contains(jobKey))
                            {
                                Scheduler.DeleteJob(jobKey);//删除job
                                JobLogger.Info($"{ jobKey.Name}被远程直接设置为stop");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                JobLogger.Info("InvalidJobCheck:"+ex.Message);
            }
        }

        /// <summary>
        /// Job状态管控
        /// </summary>
        /// <param name="Scheduler"></param>
        public void JobScheduler(IScheduler Scheduler)
        {
            //请求客户配置是否发生了改变,没改变保持当前运行，通知服务进行更新
            var canContinue = CanContinueJobScheduler();
            if (canContinue == false)
            {
                 return;
            }

            
            List<BackgroundJob> list = new BackgroundJobService().GeAllowScheduleJobInfoList();
            JobLogger.Info($"获得Job个数:{list.Count}");
            //定时获取job数据，此处需要判断数据为0的情况是因为网络情况导致的异常
            if (list != null && list.Count > 0)
            {
                var allRunningJobKeys = new List<JobKey>();
                lastExecDate = DateTime.Now;
                foreach (BackgroundJob jobInfo in list)
                {
                     
                    ///保证只有一个managerjob运行，且管理job不能停止;默认运行器的ID将联系对应管理JOB删除默认job
                    if (jobInfo.jobType == ((int)BackgroundJobType.Manager).ToString()&& jobInfo.jobId != CustomerConfig.Managerjobkey)//有当前的默认job
                    {
                        JobKey managerJobKey = new JobKey(CustomerConfig.Managerjobkey, "Group");
                        if (Scheduler.CheckExists(managerJobKey))
                        {
                            Scheduler.DeleteJob(managerJobKey);//删除默认job
                        }
                   }
                    JobKey jobKey = new JobKey(jobInfo.jobId.ToString(), "Group");
                     
                    ///job不存在
                    if (Scheduler.CheckExists(jobKey) == false)
                    {
                        if (jobInfo.state == (int)BackgroundJobStateType.Running || jobInfo.state == (int)BackgroundJobStateType.Luanch)
                        {
                            ScheduleJob(Scheduler, jobInfo);
                            if (Scheduler.CheckExists(jobKey) == false)
                            {
                                //设置为停止
                                new BackgroundJobService().UpdateBackgroundJobState(jobInfo.jobId,(int)BackgroundJobStateType.Stop);
                            }
                            else
                            {
                                if (!allRunningJobKeys.Contains(jobKey)) { 
                                allRunningJobKeys.Add(jobKey);
                                }
                                ////设置为运行中
                                new BackgroundJobService().UpdateBackgroundJobState(jobInfo.jobId, (int)BackgroundJobStateType.Running);
                            }
                        }
                        else if (jobInfo.state == (int)BackgroundJobStateType.Stopping)
                        {
                            ////设置为停止
                            new BackgroundJobService().UpdateBackgroundJobState(jobInfo.jobId, (int)BackgroundJobStateType.Stop);
                        }
                    }
                    else//已经在运行
                    {
                        if (jobInfo.state == (int)BackgroundJobStateType.Stopping)
                        {
                            Scheduler.DeleteJob(jobKey);
                            ////设置为停止
                            new BackgroundJobService().UpdateBackgroundJobState(jobInfo.jobId, (int)BackgroundJobStateType.Stop);
                        }
                        else if (jobInfo.state == (int)BackgroundJobStateType.Luanch)
                        {
                            if (!allRunningJobKeys.Contains(jobKey))
                            {
                                allRunningJobKeys.Add(jobKey);
                            }
                            ////设置为运行
                            new BackgroundJobService().UpdateBackgroundJobState(jobInfo.jobId, (int)BackgroundJobStateType.Running);
                        }
                        else if (jobInfo.state == (int)BackgroundJobStateType.Running)
                        {
                            
                            if (!allRunningJobKeys.Contains(jobKey))
                            {
                                allRunningJobKeys.Add(jobKey);
                            }
                        }
                    }
                }
                //检查无效状态的任务
                InvalidJobCheck(Scheduler, allRunningJobKeys);
            }
           

        }

    }
}
