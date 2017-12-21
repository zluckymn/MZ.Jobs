
using log4net;
using MZ.Jobs.Core.Business.Info;
using MZ.Jobs.Core.Services;
using Quartz;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace MZ.Jobs.Items.JobUpdater
{
    /// <summary>
    /// 用于自动更新最新的开发程序配合autoUpdater使用，中控台开始只执行一次的jobUpdater调度任务，进行开一个新进程触发BaT操作
    /// 命令进行卸载后进行更新最新dll后，并进行重新安装操作，并且重新运行操作
    /// context.MergedJobDataMap.GetString("Parameters");获取数据库中配置的jobArgs参数 ，jobArgs  特殊的代码执行配置项，如统计的模块代码ID等
    /// 数据库连接字符串 CustomerConfig.DataBaseConnectionString
    /// 客户代码 CustomerConfig.CustomerCode
    /// </summary>
    //不允许此 Job 并发执行任务（禁止新开线程执行）
    [DisallowConcurrentExecution]
    public sealed class JobUpdater : IJob
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(JobUpdater));

        public void Execute(IJobExecutionContext context)
        {
            
            //Version Ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            Logger.Info("JobUpdaterV1 更新器开始执行 ");
            var updaterConsoleName = string.Empty;
            try
            {
               
                //执行父父目录的自动更新程序,失败也会更新
                var curDir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
                if (curDir.Parent != null)
                {
                    var autoUpdater = curDir.GetFiles().FirstOrDefault(c => c.Name == "autoUpdater.bat") ?? curDir.Parent?.GetFiles().FirstOrDefault(c => c.Name == "autoUpdater.bat");
                    //自动更新器
                    var updaterConsole = (curDir.GetFiles()).FirstOrDefault(c => c.Name == "WebSiteUpdateConsole.exe");
                    if (updaterConsole == null)
                    {
                        updaterConsole = curDir.Parent.GetFiles().FirstOrDefault(c => c.Name == "WebSiteUpdateConsole.exe");
                    }

                    if (autoUpdater != null&& updaterConsole!=null)
                    {
                        //ExecProcess(hitWEBSiteUpdate.FullName);
                        var thread = new Thread(delegate ()
                        {
                            ExecProcess(autoUpdater.FullName);
                          
                        });
                        thread.Start();
                    }
                }
               
            }
            catch (Exception ex)
            {
                Logger.Error("JobUpdaterV1 执行过程中发生异常:" + ex.ToString());
            }
            finally
            {
                var jobId = context.JobDetail.Key.Name;
                var managerJobKey = new JobKey(jobId, "Group");
                var jobInfo = new BackgroundJob() { jobId = jobId, state = (int)BackgroundJobStateType.Stop, lastRunTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                //设置为停止
                new BackgroundJobService().UpdateBackgroundJobStatus(jobInfo);
                Logger.Info("JobUpdaterV5 正在停止所有jobs ");
                context.Scheduler.PauseAll();//防止运行过程中出错
                Logger.Info("JobUpdaterV5 正在删除jobUpdater");
                context.Scheduler.DeleteJob(managerJobKey);//防止一直运行
                Logger.Info("JobUpdaterV5 更新期结束执行 ");

               
            }
        }

        /// <summary>
        /// 执行可执行文件
        /// </summary>
        /// <param name="exeFilePath"></param>
        /// <param name="arguments"></param>
        public static string ExecProcess(string exeFilePath, string arguments = "")
        {

            // 执行exe文件
            var process = new Process
            {
                StartInfo =
                {
                    FileName = exeFilePath,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    // ReSharper disable once AssignNullToNotNullAttribute
                    WorkingDirectory = Path.GetDirectoryName(exeFilePath)
                }
            };
            // 不显示闪烁窗口

            // 注意，参数需用引号括起来，因为路径中可能有空格
            if (!string.IsNullOrEmpty(arguments))
            {
                process.StartInfo.Arguments = arguments;
            }
            try
            {
                Logger.Info("JobUpdaterV3正在执行新进程" + exeFilePath );
                process.Start();


            }
            catch (OutOfMemoryException ex)
            {
                return ex.Message;

            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            finally
            {
             
                    process.Close();

            }
            return string.Empty;
        }
    }
}
