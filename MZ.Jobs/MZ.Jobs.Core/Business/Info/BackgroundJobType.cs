 
namespace MZ.Jobs.Core.Business.Info
{

    /// <summary>
    ///job类型
    /// </summary>
    public enum BackgroundJobType
    {
        /// <summary>
        /// 普通类型job
        /// </summary>
        Normal = 0,
        /// <summary>
        /// 管理job
        /// </summary>
        Manager = 1,
        /// <summary>
        /// 更新期
        /// </summary>
        Updater = 2,
        /// <summary>
        /// 系统健康检测
        /// </summary>
        SytemHealth = 3,


    }
    
}
