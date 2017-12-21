 
namespace MZ.Jobs.Core.Business.Info
{

    /// <summary>
    ///指标数据源来源枚举项展示类型
    /// </summary>
    public enum BackgroundJobStateType
    {
        /// <summary>
        ///  状态  0-停止  1-运行   3-正在启动中...   5-停止中...
        /// </summary>
        /// <summary>
        /// 停止
        /// </summary>
        Stop = 0,
        /// <summary>
        /// 运行中
        /// </summary>
        Running = 1,
        /// <summary>
        /// 启动中
        /// </summary>
        Luanch = 3,
        /// <summary>
        /// 停止中
        /// </summary>
        Stopping = 5,
        

    }
    
}
