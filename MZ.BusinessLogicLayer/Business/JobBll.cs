using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using Yinhe.ProcessingCenter;
using MongoDB.Driver;
using MZ.Jobs.Core.Business.Info;
using System.Collections;
using MZ.RabbitMQ;
using Yinhe.ProcessingCenter.DataRule;

namespace MZ.BusinessLogicLayer.Business
{
    /// <summary>
    /// job处理累
    /// </summary>
    public class JobBll: BusinessBase
    {
        #region 构造函数
        /// <summary>
        /// 类私有变量
        /// </summary>

       
        private const string TableName = "BackgroundJob";
        private const string KeyFieldName = "jobId";
        
        /// <summary>
        /// 封闭当前默认构造函数
        /// </summary>
        private JobBll() :base(TableName, KeyFieldName)
        {
            NetQHelper = new EasyNetQHelper().VhGeneraterHelper(RabbitMqVirtualHostType.JobUpdate);

        }
        private JobBll(DataOperation dataOp) : base(TableName, KeyFieldName)
        {
            NetQHelper =new EasyNetQHelper().VhGeneraterHelper(RabbitMqVirtualHostType.JobUpdate);

            this.dataOp = dataOp;
        }
        public static JobBll _() 
        {
            return new JobBll();
        }
        public static JobBll _(DataOperation dataOp)
        {
            return new JobBll(dataOp);
        }

        
        #endregion

        #region 查询

        /// <summary>
        /// 通过Id字段查找
        /// </summary>
        /// <param name="customerCode"></param>
        /// <returns></returns>
        public MongoCursor<BsonDocument> FindByCustomerCode(string customerCode)
        {
              var obj = dataOp.FindAllByQuery(tableName, Query.EQ("customerCode", customerCode));
              return obj;
        }

        /// <summary>
        /// 通过Id字段查找
        /// </summary>
        /// <param name="customerCode"></param>
        /// <returns></returns>
        public BsonDocument FindManager(string customerCode)
        {
            string jobType = ((int)BackgroundJobType.Manager).ToString();
            BsonDocument job = dataOp.FindOneByQuery("BackgroundJob", Query.And(Query.EQ("customerCode", customerCode), Query.EQ("jobType", jobType)));
            var statement = dataOp.FindOneByQuery("Statement", Query.EQ("statementId", job.String("statementId")));
            var statementHeaders = dataOp.FindAllByQuery("StatementHeader", Query.EQ("statementId", statement.String("statementId"))).ToList();
            var statementLibs = dataOp.FindAllByQuery("StatementDataRuleLib", Query.In("mainTbName", statementHeaders.Select(c => (BsonValue)c.String("mainTbName")))).ToList();
            job.Set("id", job.String("_id")).Remove("_id");
            if (statement != null)
            {
                statementHeaders.ForEach(c => c.Set("id", c.String("_id")).Remove("_id"));
                statementLibs.ForEach(c => c.Set("id", c.String("_id")).Remove("_id"));
                statement.Set("id", statement.String("_id")).Remove("_id");
                statement.Set("statementHeaders", statementHeaders.ToJson());
                statement.Set("statementLibs", statementLibs.ToJson());
                job.Set("statementData", statement.ToJson());
            }
            //var statement = dataOp.FindOneByQuery("Statement", Query.EQ("statementId", job.String("statementId")));
            //var statementHeaders = dataOp.FindAllByQuery("StatementHeader", Query.EQ("statementId", job.String("statementId"))).ToList();
            //var statementLibs = dataOp.FindAllByQuery("StatementDataRuleLib", Query.In("mainTbName", statementHeaders.Select(c => (BsonValue)c.String("mainTbName")))).ToList();
            //var statementData = new BsonDocument();
            //statementData.Add("statement", statement.ToJson());
            //statementData.Add("stateMentHeader", statementHeaders.ToJson());
            //statementData.Add("stateMentLib", statementLibs.ToJson());
            //job.Set("id", job.String("_id")).Remove("_id");
            //job.Set("statementData", statementData.ToJson());
            //var obj = dataOp.FindOneByQuery(tableName, Query.And(Query.EQ("customerCode", customerCode),Query.EQ("jobType", jobType)));
            return job;
        }


        /// <summary>
        /// 通过Id字段查找
        /// </summary>
        /// <param name="customerCode"></param>
        /// <returns></returns>
        public List<BsonDocument> FindCustomerRunningJobs(string customerCode)
        {
            List<BsonDocument> jobList = dataOp.FindAllByQuery("BackgroundJob", Query.And(Query.EQ("customerCode", customerCode), Query.Or(Query.EQ("state", ((int)BackgroundJobStateType.Running).ToString()), Query.EQ("state", ((int)BackgroundJobStateType.Luanch).ToString()), Query.EQ("state", ((int)BackgroundJobStateType.Stopping).ToString())))).ToList();
            List<BsonDocument> statementList = dataOp.FindAllByQuery("Statement", Query.In("statementId", jobList.Select(c => (BsonValue)c.String("statementId")))).ToList();
            List<BsonDocument> statementHeaderList = dataOp.FindAllByQuery("StatementHeader", Query.In("statementId", statementList.Select(c => (BsonValue)c.String("statementId")))).ToList();
            List<BsonDocument> statementLibList = dataOp.FindAllByQuery("StatementDataRuleLib", Query.In("mainTbName", statementHeaderList.Select(c => (BsonValue)c.String("mainTbName")))).ToList();
            foreach (var job in jobList)
            {
                var statement = statementList.FirstOrDefault(c => c.String("statementId") == job.String("statementId"));
                if (statement != null)
                {
                    var statementHeaders = statementHeaderList.Where(c => c.String("statementId") == statement.String("statementId")).ToList();
                    var statementLibs = statementLibList.Where(c => statementHeaders.Exists(x => x.String("mainTbName") == c.String("mainTbName"))).ToList();
                    statementHeaders.ForEach(c => c.Set("id", c.String("_id")).Remove("_id"));
                    statementLibs.ForEach(c => c.Set("id", c.String("_id")).Remove("_id"));
                    statement.Set("id", statement.String("_id")).Remove("_id");
                    statement.Set("statementHeaders", statementHeaders.ToJson());
                    statement.Set("statementLibs", statementLibs.ToJson());
                    job.Set("statementData", statement.ToJson());
                }
                job.Set("id", job.String("_id")).Remove("_id");
            }
            //List<BsonDocument> statementList = dataOp.FindAllByQuery("Statement", Query.In("statementId", jobList.Select(c => (BsonValue)c.String("statementId")))).ToList();
            //List<BsonDocument> statementHeaderList = dataOp.FindAllByQuery("StatementHeader", Query.In("statementId", statementList.Select(c => (BsonValue)c.String("statementId")))).ToList();
            //List<BsonDocument> statementLibList = dataOp.FindAllByQuery("StatementDataRuleLib", Query.In("mainTbName", statementHeaderList.Select(c => (BsonValue)c.String("mainTbName")))).ToList();
            //var htList = new List<Hashtable>();
            //foreach (var job in jobList)
            //{
            //    var statement = statementList.FirstOrDefault(c => c.String("statementId") == job.String("statementId"));
            //    var statementHeaders = statementHeaderList.Where(c => c.String("statementId") == statement.String("statementId")).ToList();
            //    var statementLibs = statementLibList.Where(c => statementHeaders.Exists(x => x.String("mainTbName") == c.String("mainTbName"))).ToList();
            //    var statementData = new BsonDocument();
            //    statementData.Add("statement", statement.ToJson());
            //    statementData.Add("stateMentHeader", statementHeaders.ToJson());
            //    statementData.Add("stateMentLib", statementLibs.ToJson());
            //    job.Set("id", job.String("_id")).Remove("_id");
            //    job.Set("statementData", statementData.ToJson());
            //    htList.Add(job.ToHashtable());
            //}

            return jobList;
        }
        #endregion

        #region 表操作

        /// <summary>
        /// 执行更新操作
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public InvokeResult Update(BsonDocument doc)
        {
            var storageData = new StorageData() { Name = tableName, Document = doc, Type = StorageType.Update, Query = Query.EQ(keyFieldName, doc.Text(keyFieldName)) };
            //调用基类的_commonDbChangeHelper进行保存
            // var result = CommonDbChangeHelper.CommonSubmitChange(storageData);
            if (CommonDbChangeHelper.NeedQueue)
            {
                var result = NetQHelper.Broadcast<string>(doc.ToJson());
                return new InvokeResult() { Status = Status.Successful };
            }
            else
            {
                var result = CommonDbChangeHelper.CommonSubmitChange(storageData);
                return new InvokeResult() { Status = Status.Successful };
            }
           
        }
        #endregion


    }

}
