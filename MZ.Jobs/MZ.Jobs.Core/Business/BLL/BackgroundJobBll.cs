using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using MZ.Jobs.Core.Business.Info;
using Yinhe.ProcessingCenter;
using Yinhe.ProcessingCenter.DataRule;

namespace MZ.Jobs.Core.Business.BLL
{
    /// <summary>
    /// mongo 数据读取
    /// </summary>
    public class BackgroundJobBll
    {
        #region jobInfo

        readonly DataOperation _dataOp = null;
        readonly MongoOperation _mongoOp = null;
        /// <summary>
        /// 构造函数初始化
        /// </summary>
        public BackgroundJobBll()
        {
            _mongoOp = MongoOpCollection.GetMongoOp();
            _dataOp = new DataOperation(_mongoOp);
        }
        public static BackgroundJobBll _()
        {
           
            return new BackgroundJobBll();
        }
        const string TableName = "BackgroundJob";
        const string KeyFieldName = "jobId";
        const string LogTableName = "BackgroundJobLog";
        const string LogKeyFieldName = "logId";
        /// <summary>
        /// 保存更改
        /// </summary>
        public void SubmitChange()
        {
            StartDbChangeHelper.StartDbChangeProcess(_dataOp);
        }

        /// <summary>
        /// Job新增
        /// </summary>
        /// <param name="jobInfo">jobInfo实体</param>
        /// <returns></returns>
        public void Insert(BsonDocument jobInfo)
        {
            DBChangeQueue.Instance.EnQueue(new StorageData() { Document = jobInfo, Name = TableName, Type = StorageType.Insert });
            SubmitChange();
        }

        /// <summary>
        /// Job修改
        /// </summary>
        /// <param name="jobInfo">jobInfo实体</param>
        /// <returns></returns>
        public void Updatejob(BsonDocument jobInfo)
        {
            DBChangeQueue.Instance.EnQueue(new StorageData() { Document = jobInfo, Name = TableName,Query=Query.EQ(KeyFieldName,jobInfo.Text(KeyFieldName)), Type = StorageType.Update });
            SubmitChange();
        }

        /// <summary>
        /// Job删除
        /// </summary>
        /// <param name="jobId">Job ID</param>
        /// <returns></returns>
        public void Deletejob(string jobId)
        {
             DBChangeQueue.Instance.EnQueue(new StorageData() {  Name = TableName, Query = Query.EQ(KeyFieldName, jobId), Type = StorageType.Delete });
             SubmitChange();
        }

        /// <summary>
        /// Job详情
        /// </summary>
        /// <param name="jobId">Job ID</param>
        /// <returns></returns>
        public BsonDocument GetJobInfo(string jobId)
        {
            return _dataOp.FindOneByQuery(TableName, Query.EQ(KeyFieldName, jobId));
        }

        
        /// <summary>
        /// 获取允许调度的Job集合
        /// </summary>
        /// <returns></returns>
        public List<BsonDocument> GeAllowSchedulejobInfoList()
        {
            List<BsonDocument> list = null;
            var avaiableStateList = new List<int>();
            avaiableStateList.Add((int)BackgroundJobStateType.Running);
            avaiableStateList.Add((int)BackgroundJobStateType.Luanch);
            avaiableStateList.Add((int)BackgroundJobStateType.Stopping);

            var query = Query.And(Query.NE("IsDelete", "1"), Query.In("state", avaiableStateList.Select(c => (BsonValue)c.ToString())));
            list = _dataOp.FindAllByQuery(TableName, query).OrderByDescending(c => c.Date("createDate")).ToList();
            return list;
        }

        /// <summary>
        /// 更新Job状态
        /// </summary>
        /// <param name="jobId">jobId</param>
        /// <param name="state">状态</param>
        /// <returns></returns>
        public void UpdatejobState(string jobId, string state)
        {
            var curObj = new BsonDocument {{"jobId", jobId}, {"state", state}};
            Updatejob(curObj);
        }

        /// <summary>
        /// 更新Job运行信息 
        /// </summary>
        /// <param name="jobId">Job ID</param>
        /// <param name="lastRunTime">最后运行时间</param>
        /// <param name="nextRunTime">下次运行时间</param>
        public void UpdatejobStatus(string jobId, DateTime lastRunTime, DateTime nextRunTime)
        {
            // ReSharper disable once UseObjectOrCollectionInitializer
            var curObj = new BsonDocument();
            curObj.Add("jobId", jobId);
            curObj.Add("lastRunTime", lastRunTime.ToString("yyyy-MM-dd HH:mm:ss"));
            curObj.Add("nextRunTime", nextRunTime.ToString("yyyy-MM-dd HH:mm:ss"));
            Updatejob(curObj);
        }
        #endregion

        #region jobLogInfo


        /// <summary>
        /// Job日志详情
        /// </summary>
        /// <param name="jobLogId">日志ID</param>
        /// <returns></returns>
        public BsonDocument GetJobLogInfo(string jobLogId)
        {
            var  jobLogInfo = _dataOp.FindOneByQuery(LogTableName, Query.EQ(LogKeyFieldName, jobLogId));
            return jobLogInfo;
        }

      

        /// <summary>
        /// Job日志删除
        /// </summary>
        /// <param name="jobLogId">日志ID</param>
        /// <returns></returns>
        public void DeleteJobLog(string jobLogId)
        {
            DBChangeQueue.Instance.EnQueue(new StorageData() { Name = LogTableName, Query = Query.EQ(LogKeyFieldName, jobLogId), Type = StorageType.Delete });
            SubmitChange();
        }

        /// <summary>
        /// 编写日志
        /// </summary>
        /// <param name="jobLogInfo"></param>
        public void WriteBackgroundJoLog(BsonDocument jobLogInfo)
        {
            DBChangeQueue.Instance.EnQueue(new StorageData() { Document = jobLogInfo, Name = LogTableName, Type = StorageType.Insert });
            SubmitChange();
        }
        #endregion
    }
}
