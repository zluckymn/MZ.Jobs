using MongoDB.Bson;
using Quartz;
using System;
using MZ.Jobs.Core;
using Yinhe.ProcessingCenter;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;

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
    public sealed class JobServerDiskConditionCheck : JobBase,IJob
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

            
            var now = DateTime.Now;//统一记录当前运行时刻

            var start = DateTime.Parse($"{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day}"); //统计当天时间
            var end = DateTime.Parse($"{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day + 1}").AddSeconds(-1);
            var startStr = DateTime.Now.ToString("yyyy-MM-dd 00:00:00"); //统计当天时间
            var endStr = end.ToString("yyyy-MM-dd-HH HH:mm:ss");
            var extArray = SysAppConfig.needCutThumbExtArray;

            var limitCondition = this.JobParamsDoc.Double("limitCondition");
            if (limitCondition <= 0)
            {
                limitCondition = 0.05;
            }
            //2015.4.13 新增错误日志发送到125.77.255.2:8023端口进行存储，后续迁移
            var serverDiskAlertLimit = SysAppConfig.ServerDiskAlertLimit;
            System.IO.DriveInfo[] drives = System.IO.DriveInfo.GetDrives();
            var pushInfoStr = new StringBuilder();
            var detaileContent = new StringBuilder();
            var needSend = false;
            foreach (System.IO.DriveInfo drive in drives.Where(c => c.DriveType == DriveType.Fixed || c.DriveType == DriveType.Network))
            {
                try
                {

                    if (drive.IsReady)
                    {
                        long totalSize = drive.TotalSize / 1024 / 1024 / 1024;//MB//总大小
                        long availableFreeSpace = drive.AvailableFreeSpace / 1024 / 1024 / 1024;//MB//可用大小
                        var usedSpace = totalSize - availableFreeSpace;//MB//
                        var avaiablePercent = double.Parse(availableFreeSpace.ToString()) / totalSize;

                        if (availableFreeSpace <= serverDiskAlertLimit || avaiablePercent <= limitCondition) //空间剩余5%报警
                        {
                            if (pushInfoStr.Length <= 0)
                            {
                                pushInfoStr.AppendFormat("磁盘空间提醒:", drive.Name);
                            }
                            pushInfoStr.AppendFormat("{0}空间不足 ", drive.Name);
                            detaileContent.AppendFormat("{0}总:{1}G 已用:{2}G 可用:{3}G\n\r", drive.Name, totalSize, usedSpace, availableFreeSpace);
                            needSend = true;
                        }
                    }
                }
                catch (System.IO.IOException ex)
                {
                    JobLogger.Info(ex.Message);

                }
                catch (Exception ex)
                {
                    JobLogger.Info(ex.Message);
                }

            }

             if (!needSend) return string.Empty;
             JobLogger.Info("磁盘进入警告");
             var errorMessage = $"截止{start.ToString()}至{end.ToString()} {DateTime.Now:ddd} {pushInfoStr}";
              
             var messageInfo = new PushMessageInfo()
             {
                 approvalUserId = this.JobParamsDoc.Text("approvalUserId"),
                 errorMessage = errorMessage ,
                 content = detaileContent.ToString(),
                 logType = "4",
                 customerCode = SysAppConfig.CustomerCode,
                 fileStatisDate = now.ToString("yyyy-MM-dd")
             };
             return messageInfo.ToJson();
             //模拟发送post请求
         }
    }
}
