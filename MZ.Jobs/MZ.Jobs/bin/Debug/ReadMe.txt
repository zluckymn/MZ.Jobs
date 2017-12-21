注意事项：
服务安装完后 MZ.jobs 可以的话设置管理员账号登陆

在进行autoUpdater的时候第一次更新需要将更新器更新中 出现360 权限设定需要设定为完全信任

autoUpdater 在中控发起调用后请及时关闭。用于更新整体服务情况，并在几分钟后查看日志 服务jobs是否正常运行

 autoUpdater 服务设定
 assemblyName = "MZ.Jobs.Items.JobUpdater.dll",
                className = "MZ.Jobs.Items.JobUpdater.JobUpdater",
                cron = "0/10 * * * * ?",
                jobId = "75DF1ACB-D1AB-46CD-83E5-0C295EJOBUPDATER",
                name = "JobUpdater",
                state = (int)BackgroundJobStateType.Running