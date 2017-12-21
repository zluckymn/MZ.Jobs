namespace MZ.Jobs.Core.Business.Info
{
    /// <summary>
    /// 后台任务执行日志
    /// </summary>
    public class BackgroundJobLog
    {
        /// <summary>
        /// JobID
        /// </summary>				
        public string logId { get; set; }

        /// <summary>
        /// JobID
        /// </summary>
        public string jobId { get; set; }

        /// <summary>
        /// JobID
        /// </summary>
        public string jobType { get; set; }

        /// <summary>
        /// className
        /// </summary>
        public string className { get; set; }

        /// <summary>
        /// 程序集
        /// </summary>
        public string accemblyName { get; set; }

        /// <summary>
        /// Job名称
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 执行时间
        /// </summary>				
        public string executionTime { get; set; }

        /// <summary>
        /// 执行持续时长
        /// </summary>				
        public string executionDuration { get; set; }

        /// <summary>
        /// 创建日期时间
        /// </summary>				
        public string createDate { get; set; }

        /// <summary>
        /// 日志内容
        /// </summary>				
        public string runLog { get; set; }

        /// <summary>
        /// 客户代码
        /// </summary>				
        public string customerCode { get; set; }

    }
}
