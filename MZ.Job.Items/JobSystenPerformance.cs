using BusinessLogicLayer.Common;
using MZ.Jobs.Items;
using Quartz;

namespace MZ.Jobs.Items
{
    [DisallowConcurrentExecution]
    public sealed class JobSystenPerformance : JobBase, IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            Start(context);
        }

        internal override string GenerateData()
        {
            return SystemInfo.GetSystemPerformance();
        }
    }
    
}
