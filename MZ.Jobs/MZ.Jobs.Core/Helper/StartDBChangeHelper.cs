using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yinhe.ProcessingCenter;
using MongoDB.Driver;
using Yinhe.ProcessingCenter.DataRule;
using MongoDB.Driver.Builders;
using MongoDB.Bson;

namespace MZ.Jobs.Core
{
    public  class StartDbChangeHelper
    {
        /// <summary>
        /// 获取PKCount
        /// </summary>
        /// <param name="mongoDbOp"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static int GetMaxCount(MongoOperation mongoDbOp, string tableName)
         {

             var maxMatCountObj = mongoDbOp.FindOne("TablePKCounter", Query.EQ("tbName", tableName));
            if (maxMatCountObj != null)
            {
                return maxMatCountObj.Int("count");
            }
            else
            {
                var newObj = new BsonDocument().Add("tbName", tableName).Add("count", 1);
                var result = mongoDbOp.Save("TablePKCounter", newObj);
                return 2;
            }
         }

        /// <summary>
        /// 获取PKCount
        /// </summary>
        /// <param name="mongoDbOp"></param>
        /// <param name="tableName"></param>
        /// <param name="maxCount"></param>
        /// <returns></returns>
        public static void UpdateMaxCount(MongoOperation mongoDbOp, string tableName, int maxCount)
        {
            var maxMatCountObj = mongoDbOp.FindOne("TablePKCounter", Query.EQ("tbName", tableName));
            if (maxMatCountObj != null)
            {
                var newObj = new BsonDocument().Add("count", maxCount);
                mongoDbOp.Save("TablePKCounter", Query.EQ("tbName", tableName), newObj);
            }
            else
            {
                var newObj = new BsonDocument().Add("tbName", tableName).Add("count", maxCount);
                mongoDbOp.Save("TablePKCounter", newObj);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataop"></param>
        /// <param name="imd"></param>
        public static void StartDbChangeProcess(DataOperation dataop, bool imd = false)
        {
            //if (DBChangeQueue.Instance.Count < 0) return;
            List<StorageData> updateList = new List<StorageData>();
            while (DBChangeQueue.Instance.Count > 0)
            {
                var curStorage = DBChangeQueue.Instance.DeQueue();
                if (curStorage != null)
                {
                    updateList.Add(curStorage);
                }
            }
            if (updateList.Any())
            {
                var result = dataop.BatchSaveStorageData(updateList);
                if (result.Status != Status.Successful)//出错进行重新添加处理
                {
                    foreach (var storageData in updateList)
                    {
                        DBChangeQueue.Instance.EnQueue(storageData);
                    }
                }
            }
        }


        /// <summary>
        /// 对需要更新的队列数据更新操作进行批量处理,可考虑异步执行
        /// </summary>
        public static void StartDbChangeProcessQuick(MongoOperation mongoDbOp,bool isDefaultField=false)
        {
            if (mongoDbOp == null)
            {
                var connStr = "mongodb://MZsa:MZdba@192.168.1.230:37088/SimpleCrawler";

                mongoDbOp = new MongoOperation(connStr);
            }
            var result = new InvokeResult();
            List<StorageData> updateList = new List<StorageData>();
            while (DBChangeQueue.Instance.Count > 0)
            {

                var temp = DBChangeQueue.Instance.DeQueue();
                if (temp != null)
                {
                    var insertDoc = temp.Document;

                    switch (temp.Type)
                    {
                        case StorageType.Insert:
                            if (isDefaultField == true)
                            {
                                if (insertDoc.Contains("createDate") == false) insertDoc.Add("createDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));      //添加时,默认增加创建时间
                                if (insertDoc.Contains("createUserId") == false) insertDoc.Add("createUserId", "1");
                                //更新用户
                                if (insertDoc.Contains("underTable") == false) insertDoc.Add("underTable", temp.Name);
                                insertDoc.Set("updateDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));      //更新时间
                                insertDoc.Set("updateUserId", "1");
                            }
                            result = mongoDbOp.Save(temp.Name, insertDoc); ;
                            break;
                        case StorageType.Update:

                            result = mongoDbOp.Save(temp.Name, temp.Query, insertDoc);
                            break;
                        case StorageType.Delete:
                            result = mongoDbOp.Delete(temp.Name, temp.Query);
                            break;
                    }
                    //logInfo1.Info("");
                    if (result.Status == Status.Failed)
                    {

                        //throw new Exception(result.Message);
                    }

                }

            }

            if (DBChangeQueue.Instance.Count > 0)
            {
                StartDbChangeProcessQuick(mongoDbOp, isDefaultField);
            }
        }
    }
}
