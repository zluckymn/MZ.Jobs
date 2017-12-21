using MZ.Jobs.Core;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace MZ.Jobs
{
    partial class MZJobService : ServiceBase
    {
       
        private readonly IScheduler scheduler;
        public MZJobService()
        {
            InitializeComponent();
            scheduler = StdSchedulerFactory.GetDefaultScheduler();
        }

        private string _ServiceName
        {
            get
            {
                return "MZJobSheduler";
            }
        }
        protected override void OnStart(string[] args)
        {
            //Debugger.Launch();
            JobLogger.Info(string.Format("ServiceBaseOnStartV2"));
            scheduler.ListenerManager.AddJobListener(new SchedulerJobListener(), GroupMatcher<JobKey>.AnyGroup());
            scheduler.Start();
            JobLogger.Info(string.Format("添加首次执行任务，判断需要执行的事物"));
            ///默认调度器
            new QuartzManager().JobSchedulerMain(scheduler);
            JobLogger.Info(string.Format("{0} Start", _ServiceName));
        }


        protected override void OnStop()
        {
            scheduler.Shutdown(false);
            JobLogger.Info(string.Format("{0} Stop", _ServiceName));
        }

        protected override void OnPause()
        {
            scheduler.PauseAll();
            JobLogger.Info(string.Format("{0} Pause", _ServiceName));
        }
        protected override void OnContinue()
        {
            scheduler.ResumeAll();
            JobLogger.Info(string.Format("{0} Continue", _ServiceName));
        }
    }
}
