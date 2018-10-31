using MongoDB.Bson;
using Quartz;
using System;
using System.Collections.Generic;
using MZ.Jobs.Core;
using Yinhe.ProcessingCenter;
using System.Linq;
using System.Text;

// ReSharper disable once CheckNamespace
namespace MZ.Jobs.Items
{
    ///不允许此 Job 并发执行任务（禁止新开线程执行）
    /// 用于自动更新最新的开发程序配合autoUpdater使用，中控台开始只执行一次的jobUpdater调度任务，进行开一个新进程触发BaT操作
    /// 命令进行卸载后进行更新最新dll后，并进行重新安装操作，并且重新运行操作
    /// context.MergedJobDataMap.GetString("Parameters");获取数据库中配置的jobArgs参数 以bsondoc方式存储，jobArgs  特殊的代码执行配置项，如统计的模块代码ID等
    /// 数据库连接字符串 CustomerConfig.DataBaseConnectionString
    /// 客户代码 CustomerConfig.CustomerCode

    public class JobBase 
    {
        DataOperation _dataOp;
        /// <summary>
        /// 获取对应数据查询器dataOp
        /// </summary>
        internal DataOperation dataOp
        {
            get {
                    try
                    {
                        if (_dataOp != null) {
                            return _dataOp;
                        }
                        _dataOp= MongoOpCollection.GetDataOp();
                        return _dataOp;
                    }
                    catch (Exception ex)
                    {
                        JobLogger.Info(ex.Message);
                        return null;
                    }
            }
        }
        /// <summary>
        /// 获取对应数据查询器mongoOp
        /// </summary>
        internal MongoOperation mongoOp
        {
            get {
                try
                {
                    return MongoOpCollection.GetMongoOp();
                }
                catch (Exception ex)
                {
                    JobLogger.Info(ex.Message);
                    return null;
                }
            }
        }
        internal BsonDocument _jobParamsDoc = new BsonDocument();
        internal  BsonDocument  _statementData = new  BsonDocument ();
        
        /// <summary>
        /// 获取对应数据查询器mongoOp
        /// </summary>
        internal BsonDocument JobParamsDoc => _jobParamsDoc;

        /// <summary>
        /// 获取对应数据查询器mongoOp
        /// </summary>
        internal BsonDocument StatementData
        {
            get
            {
                return _statementData;
            }
        }

        /// <summary>
        /// 获取对应数据查询器mongoOp
        /// </summary>
        internal IJobExecutionContext _IJobExecutionContext;
         
        
        /// <summary>
        /// 获取客户代码
        /// </summary>
        internal string CustomerCode => CustomerConfig.CustomerCode;

        /// <summary>
        /// 获取客户代码
        /// </summary>
        internal string ClassName
        {
            get
            {
                var accembly = System.Reflection.Assembly.GetExecutingAssembly().GetName();
                return accembly.Name;
            }
        }
       
        /// <summary>
        /// 获取客户代码
        /// </summary>
        internal void PushInfo(string msgData)
        {
            _IJobExecutionContext.MergedJobDataMap.Put("execData", msgData);
        }

        /// <summary>
        /// 获取客户代码
        /// </summary>
        internal void PushInfo(BsonDocument msgData)
        {
            _IJobExecutionContext.MergedJobDataMap.Put("execData", msgData.ToJson());
        }

        /// <summary>
        /// 获取客户代码
        /// </summary>
        internal void PushInfo(List<BsonDocument> msgData)
        {
            _IJobExecutionContext.MergedJobDataMap.Put("execData", msgData.ToJson());
        }
        /// <summary>
        /// 基类执行器初始化
        /// </summary>
        /// <param name="context"></param>
        internal void Init(IJobExecutionContext context)
        {
            try
            {
                    _IJobExecutionContext = context;
                    //初始化执行器
                    _jobParamsDoc = CustomerConfig.GetJobArgDoc(context.MergedJobDataMap.GetString("Parameters"));
                    _statementData = CustomerConfig.GetJobArgDoc(context.MergedJobDataMap.GetString("statementData"));
                
            }
            catch (Exception ex)
            {
                JobLogger.Info("Job基类 初始化发生异常:" + ex.ToString());
            }
            finally
            {
               // JobLogger.Info("Job基类 结束执行 ");
            }
        }

        /// <summary>
        /// 抽象方法
        /// </summary>
        /// <returns></returns>
        internal void Start(IJobExecutionContext context)
        {
            Init(context);//基类初始化对象;
            // Version Ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            //JobLogger.Info("{0} 开始执行", this.ClassName);
            try
            {
                var messageDoc = GenerateData();
                this.PushInfo(messageDoc);
            }
            catch (Exception ex)
            {
                JobLogger.Info("{0} 执行过程中发生异常:{1}", this.ClassName, ex.ToString());
            }
            finally
            {
               // JobLogger.Info("{0} 结束执行", this.ClassName);
            }
           
        }
        /// <summary>
        /// 抽象方法
        /// </summary>
        /// <returns></returns>
        internal  virtual string GenerateData()
        {
            return string.Empty;
        }

        internal HttpResult GetHtml(string url,string postData="",Dictionary<string,string>headers=null)
        {
            Core.HttpHelper http = new Core.HttpHelper();
            HttpItem item = new HttpItem()
            {
                URL = url,
                Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36",

            };
            if (!string.IsNullOrEmpty(postData))
            {
                item.Method = "post";
                item.Postdata = postData;
            }
            if (headers != null) {
                foreach (var header in headers)
                {
                    item.Header.Add(header.Key, header.Value);
                }
            }
            var result = http.GetHtml(item);
            return result;
        }

        /// <summary>
        /// bsonDoc中生成描述数据
        /// </summary>
        /// <param name="deadLockList"></param>
        /// <returns></returns>
        internal string GetDetailFromJson(IEnumerable<BsonDocument> docList)
        {
            var detailInfo = new StringBuilder();
            
            if (docList != null && docList.Count() > 0)
            {
                var index = 1;
                foreach (var doc in docList)
                {
                    detailInfo.AppendLine($"{index++}.---------------------");
                    foreach (var elem in doc.Elements)
                    {
                        detailInfo.AppendLine($"{elem.Name}:{doc.Text(elem.Name).Trim()}");
                    }
                    detailInfo.AppendLine("");
                    detailInfo.AppendLine("---------------------");
                }
            }
            return detailInfo.ToString();
        }
    }
}
