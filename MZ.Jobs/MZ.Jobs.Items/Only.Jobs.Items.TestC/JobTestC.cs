using log4net;
using Quartz;
using System;

namespace MZ.Jobs.Items.TestC
{
    //不允许此 Job 并发执行任务（禁止新开线程执行）
    [DisallowConcurrentExecution]
    public sealed class JobTestC : IJob
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(JobTestC));

        public void Execute(IJobExecutionContext context)
        {
            Version Ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            _logger.Info("JobC 开始执行 ");
            try
            {
               // _logger.InfoFormat("JobTestC Executing ...");
               // Console.WriteLine("---------------------");
            }
            catch (Exception ex)
            {
                _logger.Error("JobTestC 执行过程中发生异常:" + ex.ToString());
            }
            finally
            {
                _logger.Info("JobC 结束执行 ");
            }
        }
    }
}
