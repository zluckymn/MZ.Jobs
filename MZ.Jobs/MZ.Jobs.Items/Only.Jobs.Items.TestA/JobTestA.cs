
using log4net;
using MongoDB.Bson;
using MZ.Jobs.Core;
using Quartz;
using System;
using System.Collections.Generic;

namespace MZ.Jobs.Items.TestA
{
    ///不允许此 Job 并发执行任务（禁止新开线程执行）
    /// 用于自动更新最新的开发程序配合autoUpdater使用，中控台开始只执行一次的jobUpdater调度任务，进行开一个新进程触发BaT操作
    /// 命令进行卸载后进行更新最新dll后，并进行重新安装操作，并且重新运行操作
    /// context.MergedJobDataMap.GetString("Parameters");获取数据库中配置的jobArgs参数 ，jobArgs  特殊的代码执行配置项，如统计的模块代码ID等
    /// 数据库连接字符串 CustomerConfig.DataBaseConnectionString
    /// 客户代码 CustomerConfig.CustomerCode
    [DisallowConcurrentExecution]
    public sealed class JobTestA : IJob
    {

        private static readonly ILog Log = LogManager.GetLogger(typeof(JobTestA));

        public void Execute(IJobExecutionContext context)
        {
           
            // Version Ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            Log.InfoFormat("JobA 开始执行 ");
            try 
            {
                var bsonDocList = new List<BsonDocument>
                {
                    new BsonDocument().Add("dateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                };
                var jobArgs = CustomerConfig.GetJobArgDoc(context.MergedJobDataMap.GetString("Parameters"));
                context.MergedJobDataMap.Put("execData", bsonDocList.ToJson());
               // context.MergedJobDataMap.Put("extend_run_result", "success");
               // _logger.InfoFormat("JobA Executing ...");
                 Console.WriteLine("JobA 开始执行");
            }
            catch (Exception ex)
            {
                Log.Error("JobTestA 执行过程中发生异常:" + ex.ToString());
            }
            finally
            {
                Log.Info("JobA 结束执行 ");
            }
        }
    }
}
