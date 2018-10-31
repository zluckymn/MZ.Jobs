using System;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using Yinhe.ProcessingCenter;
using MongoDB.Driver;

// ReSharper disable once CheckNamespace
namespace MZ.BusinessLogicLayer.Business
{
    /// <summary>
    /// job处理累
    /// </summary>
    public class CustomerInfoBll: BusinessBase
    {
        #region 构造函数
        /// <summary>
        /// 类私有变量
        /// </summary>
        

        private const string TableName = "CustomerInfo";
        private const string KeyFieldName = "customerId";
        /// <summary>
        /// 封闭当前默认构造函数
        /// </summary>
        private CustomerInfoBll() :base(TableName, KeyFieldName)
        {
             
        }
        private CustomerInfoBll(DataOperation dataOp) : base(TableName, KeyFieldName)
        {
            base.dataOp = dataOp;
        }
        public static CustomerInfoBll _() 
        {
            return new CustomerInfoBll();
        }
        public static CustomerInfoBll _(DataOperation dataOp)
        {
            return new CustomerInfoBll(dataOp);
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
        public bool IsRunning(BsonDocument customer)
        {
            var isRunning = customer.Date("serviceActiveDate") > DateTime.Now.AddMinutes(-10);
            return isRunning;
        }

        #endregion

        #region 表操作


        #endregion


    }

}
