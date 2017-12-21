namespace MZ.Jobs.Core.Business.Info
{
    /// <summary>
    /// Job信息
    /// </summary>
    public class BackgroundJob
    {
        public string customerCode { get; set; }
        /// <summary>
        /// JobID
        /// </summary>				
        public string jobId { get; set; }


        /// <summary>
        /// Job表头类型
        /// </summary>
        public string statementData { get; set; }
        
        /// <summary>
        /// Job类型
        /// </summary>
        public string jobType { get; set; }

        /// <summary>
        /// Job名称
        /// </summary>				
        public string name { get; set; }

        /// <summary>
        /// 描述
        /// </summary>				
        public string description { get; set; }

        /// <summary>
        /// 程序集名称(所属程序集)
        /// </summary>				
        public string assemblyName { get; set; }

        /// <summary>
        /// 类名(完整命名空间的类名)
        /// </summary>				
        public string className { get; set; }

        /// <summary>
        /// 参数
        /// </summary>				
        public string jobArgs { get; set; }

        /// <summary>
        /// Cron表达式
        /// </summary>				
        public string cron { get; set; }

        /// <summary>
        /// Cron表达式描述
        /// </summary>				
        public string cronDesc { get; set; }

        /// <summary>
        /// 最后运行时间
        /// </summary>				
        public string lastRunTime { get; set; }

        /// <summary>
        /// 下次运行时间
        /// </summary>				
        public string nextRunTime { get; set; }

        /// <summary>
        /// 运行次数
        /// </summary>
        public int runCount { get; set; }

        /// <summary>
        /// 状态  0-停止  1-运行   3-正在启动中...   5-停止中...
        /// </summary>
        public int state { get; set; }

        /// <summary>
        /// 排序
        /// </summary>				
        public int order { get; set; }

        /// <summary>
        /// job调用webAPi地址
        /// </summary>				
        public int webApiUrl { get; set; }

        
        /// <summary>
        /// 创建人ID
        /// </summary>				
        public string createUserId { get; set; }

        /// <summary>
        /// 创建人名称
        /// </summary>				
        public string createUserName { get; set; }

        /// <summary>
        /// 创建日期时间
        /// </summary>				
        public string createdDate { get; set; }

        /// <summary>
        /// 最后更新人ID
        /// </summary>				
        public string updateUserId { get; set; }

        /// <summary>
        /// 最后更新人名称
        /// </summary>				
        public string updateUserName { get; set; }

        ///// <summary>
        ///// 最后更新时间
        ///// </summary>
        public string updateDate { get; set; }

        /// <summary>
        /// 是否删除 0-未删除   1-已删除
        /// </summary>				
        public int isDelete { get; set; }

        
    }
}
