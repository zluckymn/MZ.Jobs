using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Web;

namespace BusinessLogicLayer
{
    /// <summary>
    /// 系统配置项处理类,继承于SysAppConfig 可通过CusAppconfig.CustomerCode调用SysAppconfig中的所有对象，以后的新配置加到该类下，后续可迁移到到SysAppconfig
    /// </summary>
    public class CusAppConfig : Yinhe.ProcessingCenter.SysAppConfig
    {

        /// <summary>
        /// 获取秘钥
        /// </summary>
        public static string Secretkey
        {
            get
            {
                if (ConfigurationManager.AppSettings["Secretkey"] != null)
                {
                    return ConfigurationManager.AppSettings["Secretkey"].ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
        }
        /// <summary>
        /// 获取秘钥
        /// </summary>
        public static string BigDataBaseConnectionString
        {
            get
            {
                if (ConfigurationManager.AppSettings["BigDataBaseConnectionString"] != null)
                {
                    return ConfigurationManager.AppSettings["BigDataBaseConnectionString"].ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// 获取秘钥
        /// </summary>
        public static string MasterDataBaseConnectionString
        {
            get
            {
                if (ConfigurationManager.AppSettings["MasterDataBaseConnectionString"] != null)
                {
                    return ConfigurationManager.AppSettings["MasterDataBaseConnectionString"].ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// RabbitMQ配置
        /// </summary>
        public static string RabbitMQHostStr
        {
            get
            {
                if (ConfigurationManager.AppSettings["RabbitMQHostStr"] != null)
                {
                    return ConfigurationManager.AppSettings["RabbitMQHostStr"];
                }
                return "host=192.168.1.173:5672;username=X;password=X";
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
                    return ConfigurationManager.AppSettings["RabbitMqActive"] == "true";
                }
                return true;
            }
        }

        

        public static string SendMailAddress
        {
            get
            {
                if (ConfigurationManager.AppSettings["SendMailAddress"] != null)
                {
                    return ConfigurationManager.AppSettings["SendMailAddress"] ;
                }
                return "1";
            }
        }
        public static string SendMailPassWord
        {
            get
            {
                if (ConfigurationManager.AppSettings["SendMailPassWord"] != null)
                {
                    return ConfigurationManager.AppSettings["SendMailPassWord"] ;
                }
                return "1";
            }
        }
    }
}
