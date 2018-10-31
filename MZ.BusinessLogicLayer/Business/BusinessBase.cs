using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BusinessLogicLayer.Business;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using Yinhe.ProcessingCenter;
using MongoDB.Driver;
using MZ.Jobs.Core;
using Yinhe.ProcessingCenter.DataRule;
using MZ.RabbitMQ;

// ReSharper disable once CheckNamespace
namespace MZ.BusinessLogicLayer.Business
{
    /// <summary>
    /// 流程步骤处理类
    /// </summary>
    public class BusinessBase
    {
        #region 构造函数
        /// <summary>
        /// 类私有变量
        /// </summary>
        // ReSharper disable once InconsistentNaming
        internal DataOperation dataOp = null;
         
        public static EasyNetQHelper NetQHelper { get; set; } = new EasyNetQHelper().VhGeneraterHelper(RabbitMqVirtualHostType.CommonDBChangeQueue);
        public static CommonDbChange CommonDbChangeHelper =null;
        internal string tableName = "";
        internal string keyFieldName = "";

        /// <summary>
        /// 封闭当前默认构造函数
        /// </summary>
        public BusinessBase()
        {
           
            dataOp = new DataOperation();
            CommonDbChangeHelper = CommonDbChange._(dataOp);
        }


        /// <summary>
        /// 封闭当前默认构造函数
        /// </summary>
        public BusinessBase(string tableName, string keyFieldName) 
        {
          
            this.tableName = tableName;
            this.keyFieldName = keyFieldName;
            dataOp = new DataOperation();
            CommonDbChangeHelper = CommonDbChange._(dataOp);
        }

        
        #endregion

        #region 查询




        /// <summary>
        /// 通过Id字段查找
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public BsonDocument FindById(string  id)
        {
              var obj = dataOp.FindOneByKeyVal(tableName, keyFieldName, id);
              return obj;
        }

        /// <summary>
        /// 通过Ids字段查找
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public MongoCursor<BsonDocument> FindByIds(params string[] ids)
        {
            return FindByQuery(Query.In(keyFieldName, ids.Select(c=>(BsonValue)c)));
           
        }

        /// <summary>
        /// 通过Ids字段查找
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public MongoCursor<BsonDocument> FindByQuery(QueryComplete query)
        {
           var obj = dataOp.FindAllByQuery(tableName, query);
            return obj;
        }

        /// <summary>
        /// 通过Ids字段查找
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool  HasExist(string id,string name)
        {
            var count = dataOp.FindCount(tableName,Query.And( Query.NE(keyFieldName,id),Query.EQ("name",name)));
            return count>0;
        }

        /// <summary>
        /// 通过Ids字段查找
        /// </summary>
        /// <returns></returns>
        public bool HasExist(BsonDocument  doc)
        {
            return HasExist(doc.Text(keyFieldName), doc.Text("name"));
        }
        #endregion

        #region 表操作
        /// <summary>
        /// 添加实例
        /// </summary>
        /// <returns></returns>
        public InvokeResult  Insert(BsonDocument pwdTip)
            {
                if (pwdTip == null)
                    throw new ArgumentNullException();
                var storageData = new StorageData() {Name = tableName, Document = pwdTip, Type = StorageType.Insert};
                var result= CommonDbChangeHelper.CommonSubmitChange(storageData);
                return result;
            }
       
        /// <summary>
        /// 更新事例实例
        /// </summary>
        /// <returns></returns>
        public InvokeResult Update(string id,BsonDocument pwdTip)
          {
              if (pwdTip == null)
              throw new ArgumentNullException();
              var storageData = new StorageData() { Name = tableName, Document = pwdTip, Type = StorageType.Update,Query = Query.EQ(keyFieldName, id) };
              var result = CommonDbChangeHelper.CommonSubmitChange(storageData);
              return result;
            
          }
            /// <summary>
            /// 更新事例实例
            /// </summary>
            /// <returns></returns>
            public InvokeResult Update(QueryComplete query, BsonDocument updateDoc)
            {
                if ( updateDoc == null)
                    throw new ArgumentNullException();
                var storageData = new StorageData() { Name = tableName, Document = updateDoc, Type = StorageType.Update, Query = query };
                var result = CommonDbChangeHelper.CommonSubmitChange(storageData);
                return result;
            }
        /// <summary>
        /// 更新事例实例
        /// </summary>
        /// <returns></returns>
        public InvokeResult Update(BsonDocument oldDoc, BsonDocument updateDoc)
        {
                if (oldDoc == null|| updateDoc==null)
                    throw new ArgumentNullException();
           
            var storageData = new StorageData() { Name = tableName, Document = updateDoc, Type = StorageType.Update, Query = Query.EQ(keyFieldName, oldDoc.Text(keyFieldName)) };
            var result = CommonDbChangeHelper.CommonSubmitChange(storageData);
            return result;
            
         }

        /// <summary>
        /// 更新事例实例
        /// </summary>
        /// <returns></returns>
        public InvokeResult Delete(BsonDocument doc)
        {
            var result = new InvokeResult() { Status = Status.Successful };
            if (doc == null)
                return result;
            //result = dataOp.Delete(tableName, );
            var storageData = new StorageData() { Name = tableName,Type = StorageType.Delete, Query = Query.EQ(keyFieldName, doc.Text(keyFieldName)) };
            result = CommonDbChangeHelper.CommonSubmitChange(storageData);
            return result;
        }

        /// <summary>
        /// 更新事例实例
        /// </summary>
        /// <returns></returns>
        public InvokeResult Delete(string id)
        {
            var storageData = new StorageData() { Name = tableName,Type = StorageType.Delete, Query = Query.EQ(keyFieldName, id) };
            var result = CommonDbChangeHelper.CommonSubmitChange(storageData);
            return result;
        }
        #endregion


    }

}
