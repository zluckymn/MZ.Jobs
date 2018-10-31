using EasyNetQ;
using System;
using System.Threading.Tasks;

using System.Configuration;

namespace MZ.RabbitMQ
{
    public enum RabbitMqVirtualHostType
    {
        /// <summary>
        /// 系统业务数据
        /// </summary>
        SystemDataAnalyse,
        /// <summary>
        /// 旧系统站点监控数据用来保存到230WorkPlanManage并且发送异常短信数据
        /// </summary>
        SystemHealth,
        /// <summary>
        /// 日志数据处理
        /// </summary>
        LogAnalyse,
        /// <summary>
        /// job更新
        /// </summary>
        JobUpdate,
        /// <summary>
        /// 通用队列更新
        /// </summary>
        // ReSharper disable once InconsistentNaming
        CommonDBChangeQueue
    }
    /// <summary>
    /// RabbitMQ配置帮助类
    /// Virtual Host:
    /// jobLog_LogAnalyse   日志保存分析统计队列
    /// jobLog_SystemDataAnalyse  系统业务数据
    /// jobLog_SystemHealth   旧系统站点监控数据用来保存到230WorkPlanManage并且发送异常短信数据
    /// </summary>
    public class RabbitMqConfig 
    {

        /// <summary>
        /// RabbitMQ配置
        /// </summary>
        public static string RabbitMqHostStr
        {
            get
            {
                if (ConfigurationManager.AppSettings["RabbitMQHostStr"] != null)
                {
                    return ConfigurationManager.AppSettings["RabbitMQHostStr"];
                }
                return "host=192.168.185.173:5672;username=antapos;password=antapos";
            }
        }
        /// <summary>
        /// 配置是否使用RabbitMQ
        /// </summary>
        public static bool RabbitMqAvaiable
        {
            get
            {
                if (ConfigurationManager.AppSettings["RabbitMqActive"] != null)
                {
                    return ConfigurationManager.AppSettings["RabbitMqActive"] =="true";
                }
                return true;
            }
        }
        /// <summary>
        /// RabbitMQ配置
        /// </summary>
        public static string RabbitMqVirtualHostStr() => RabbitMqHostStr;

        /// <summary>
        /// RabbitMQ配置
        /// </summary>
        public static string RabbitMqVirtualHostStr(RabbitMqVirtualHostType vhType) =>string.Format("{0};virtualhost={1}", RabbitMqHostStr, vhType.ToString());

        /// <summary>
        /// RabbitMQ配置
        /// </summary>
        public static string RabbitMqVirtualHostStr(string vhStr) => string.Format("{0};virtualhost={1}", RabbitMqHostStr, vhStr);

        public RabbitMqConfig()
        {

        }

    }
}
