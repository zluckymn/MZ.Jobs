using System.Configuration;
using System.Reflection;
using MongoDB.Bson;
using Yinhe.ProcessingCenter;

namespace MZ.Jobs.Core
{
    /// <summary>
    /// 客户配置项，定时获取客户配置项，并进行修改XML进行更新配置？在job中进行CustomerConfig 进行配置
    /// </summary>
    public class CustomerConfig
    {
       
        //public const string APIUrl = "http://webapi.meng-zheng.com.cn/api";//webapi地址
        public static readonly string ApiUrl = System.Configuration.ConfigurationManager.AppSettings["APIUrl"];//webapi地址
        public static readonly string ServiceName = System.Configuration.ConfigurationManager.AppSettings["ServiceName"];//webapi地址
        public static readonly string CustomerInfoUrl = "/Customer/Info";//获取客户配置详细信息。
        public static readonly string CustomerCfgChangeUrl = "/Customer/CFGChange";//获取客户配置是否发生了改变 {customerCode } 返回 bsonDocument 字段 needChange 是否改变 ,heartBeat功能
        public static readonly string CustomerJobListUrl = "/Jobs/JobList";//获取用户配置与status详细job列表 参数 customerCode  {customerCode }  返回值 BsonDocumentList，筛选staus为 1 3,5的job列表
        public static readonly string CustomerJobManageUrl = "/Jobs/JobManage";//根据customerCode获取对应客户的JobManage {customerCode,jobType } 返回值 BsonDocument
        public static readonly string CustomerJobInfoUrl = "/Jobs/JobInfo";//根据customerCode获取对应客户的JobManage {customerCode,jobType } 返回值 BsonDocument
        public static readonly string CustomerJobUpdateUrl = "/Jobs/UpdateJobState";//根据customerCode获取更新对应Job的执行状态，并更新改job执行次数 {customerCode,jobId,status }  
        public static readonly string CustomerJobLgUrl = "/Jobs/CreateJobLog";//根据customerCode记录job执行日志 {customerCode,jobLog字段 }  
        public static readonly string CustomerCode = SysAppConfig.CustomerCode;
        public static readonly string DataBaseConnectionString = SysAppConfig.DataBaseConnectionString;//数据库连接字符串
        public static readonly bool NeedSendLog = System.Configuration.ConfigurationManager.AppSettings.Get("NeedSendLog") ==null || System.Configuration.ConfigurationManager.AppSettings.Get("NeedSendLog")=="true";//是否需要发送执行日志
        //是否展示请求API日志
        public static readonly bool ShowApiLog = System.Configuration.ConfigurationManager.AppSettings.Get("ShowAPILog") == null || System.Configuration.ConfigurationManager.AppSettings.Get("ShowAPILog") == "true";//是否需要发送执行日志
        /// <summary>
        /// 默认的manaagerJobKey
        /// </summary>
        public static string Managerjobkey = System.Configuration.ConfigurationManager.AppSettings["DefaultManagerJobKey"]==null? "75DF1ACB-D1AB-46CD-83E5-0C295E688675": System.Configuration.ConfigurationManager.AppSettings.Get("DefaultManagerJobKey");
        /// <summary>
        /// joblist缓存时间，默认 1小时
        /// </summary>
        public static int JobListCacheTime = System.Configuration.ConfigurationManager.AppSettings["JobListCacheTime"] ==null? 60000*60 : int.Parse(System.Configuration.ConfigurationManager.AppSettings.Get("JobListCacheTime"));

        /// <summary>
        /// 获取对应的JobDoc参数配置对象
        /// </summary>
        /// <returns></returns>
        public static BsonDocument GetJobArgDoc(string bsonStr)
        {
           if (!string.IsNullOrEmpty(bsonStr)) { 
                var resultDoc = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(bsonStr);
                return resultDoc;
           }
           return new BsonDocument();
        }

       

    }
}
