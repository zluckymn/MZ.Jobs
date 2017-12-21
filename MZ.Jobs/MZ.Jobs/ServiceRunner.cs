
using log4net;
using MZ.Jobs.Core;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using System;
using System.Timers;
using Topshelf;

namespace MZ.Jobs
{
    public sealed class ServiceRunner : ServiceControl, ServiceSuspend
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(ServiceRunner));
        private readonly IScheduler scheduler;
       
        private string ServiceName
        {
            get
            {
                return "MZJobSheduler";
            }
        }

        public ServiceRunner()
        {
            
                scheduler = StdSchedulerFactory.GetDefaultScheduler();
               
        }

        public bool Start(HostControl hostControl)
        {

             
            scheduler.ListenerManager.AddJobListener(new SchedulerJobListener(), GroupMatcher<JobKey>.AnyGroup());
            scheduler.Start();
            new QuartzManager().JobSchedulerMain(scheduler);
            _logger.Info(string.Format("{0} Start", ServiceName));
            return true;
        }
         
        public bool Stop(HostControl hostControl)
        {
            scheduler.Shutdown(false);
            _logger.Info(string.Format("{0} Stop", ServiceName));
            return true;
        }

        public bool Continue(HostControl hostControl)
        {
            scheduler.ResumeAll();
            _logger.Info(string.Format("{0} Continue", ServiceName));
            return true;
        }

        public bool Pause(HostControl hostControl)
        {
            scheduler.PauseAll();
            _logger.Info(string.Format("{0} Pause", ServiceName));
            return true;
        }

    }
}
