
using MZ.Jobs.Core;
using Quartz;
using System;

namespace MZ.Jobs.JobItems
{
    /// <summary>
    /// 调度管理器job,目前设定每3秒调度一次 后续设定1分钟调度一次？
    /// 加入缓存设定判断服务端控制台是否有更改，有更改才进行读取系统列表进行更新
    /// 
    /// </summary>
    [DisallowConcurrentExecution]
    public class ManagerJob : IJob
    {
       

        public void Execute(IJobExecutionContext context)
        {
            
            
            try
            {
               // JobLogger.Info("ManagerJob Execute Begin ");
                new QuartzManager().JobScheduler(context.Scheduler);
                
            }
            catch (Exception ex)
            {
                JobExecutionException e2 = new JobExecutionException(ex);
                e2.RefireImmediately = true;
            }
        }
    }
}
