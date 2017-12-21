using MZ.Jobs.Core;
using System;
using System.IO;
using System.ServiceProcess;
using Topshelf;

namespace MZ.Jobs
{
    class Program
    {
        static void Main(string[] args)
        {
            //log4net.Config.XmlConfigurator.ConfigureAndWatch(new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "log4net.config"));
            //MZServiceRunner();
            //return;
            JobLogger.Info(string.Format("TopshelfOnStart"));
            HostFactory.Run(x =>
            {
                x.RunAsLocalSystem();
                x.Service<ServiceRunner>();
                x.SetDescription(string.Format("{0} Ver:{1}", System.Configuration.ConfigurationManager.AppSettings.Get("ServiceName"), System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()));
                x.SetDisplayName(System.Configuration.ConfigurationManager.AppSettings.Get("ServiceDisplayName"));
                x.SetServiceName(System.Configuration.ConfigurationManager.AppSettings.Get("ServiceName"));
                x.EnablePauseAndContinue();
            });
        }
        /// <summary>
        /// 进行服务安装
        /// </summary>
        public static void MZServiceRunner()
        { 
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new MZJobService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
