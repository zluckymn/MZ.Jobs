using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Configuration.Install;
using System.Linq;
using System.Reflection;
using MZ.Jobs.Core;
using Yinhe.ProcessingCenter;

namespace MZ.Jobs
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
            var serviceName = ServiceName();
            if (!string.IsNullOrEmpty(serviceName))
            {
                this.serviceInstaller1.ServiceName = serviceName;
           }
           
        }
        public string ServiceName()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(
                Assembly.GetExecutingAssembly().Location); ;
            return config.AppSettings.Settings["ServiceName"].Value;
        }
        private void serviceInstaller1_AfterInstall(object sender, InstallEventArgs e)
        {

        }
    }
}
