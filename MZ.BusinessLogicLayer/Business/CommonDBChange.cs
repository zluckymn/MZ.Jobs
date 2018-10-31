using System.Collections.Generic;
using MZ.RabbitMQ;
using Yinhe.ProcessingCenter;
using Yinhe.ProcessingCenter.DataRule;

namespace BusinessLogicLayer.Business
{
    /// <summary>
    /// 通用数据库更新器
    /// </summary>
    public class CommonDbChange
    {
        #region 构造函数
        /// <summary>
        /// 类私有变量
        /// </summary>
        readonly DataOperation  _dataOp = null;

        public static EasyNetQHelper NetQHelper { get; set; } = new EasyNetQHelper().VhGeneraterHelper(RabbitMqVirtualHostType.CommonDBChangeQueue);//默认VirtualHost

        public bool  NeedQueue { get; set; } = RabbitMqConfig.RabbitMqAvaiable;//默认VirtualHost
        /// <summary>
        /// 封闭当前默认构造函数
        /// </summary>
        private CommonDbChange() 
        {
            _dataOp = new DataOperation();
        }


      
        /// <summary>
        /// 封闭当前默认构造函数
        /// </summary>
        private CommonDbChange(DataOperation dataOp)
        {
            this._dataOp = dataOp;
        }
        public static CommonDbChange _() 
        {
            return new CommonDbChange();
        }
        public static CommonDbChange _(DataOperation dataOp)
        {
            return new CommonDbChange(dataOp);
        }
        
        #endregion



        #region 表操作

        /// <summary>
        /// 执行更新操作
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public InvokeResult CommonSubmitChange(StorageData doc)
        {
            //if (RabbitMqConfig.RabbitMqAvaiable)
            //{
            //    return SubmitChangeViaQueue(doc);
            //}
            var result = _dataOp.BatchSaveStorageData(new List<StorageData> { doc });
            return result;
        }

        /// <summary>
        /// 通过队列执行更新操作
        /// </summary>
        /// <param name="docList"></param>
        /// <returns></returns>
        public InvokeResult CommonSubmitChange(List<StorageData> docList)
        {
            //if (RabbitMqConfig.RabbitMqAvaiable)
            //{
            //    return SubmitChangeViaQueue(docList);
            //}
            var result = _dataOp.BatchSaveStorageData(docList);
            return result;
        }

        /// <summary>
        /// 执行更新操作
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public InvokeResult SubmitChange(StorageData doc)
        {
           var result= _dataOp.BatchSaveStorageData(new List<StorageData> { doc });
           return result;
        }

        /// <summary>
        /// 通过队列执行更新操作
        /// </summary>
        /// <param name="docList"></param>
        /// <returns></returns>
        public InvokeResult SubmitChange(List<StorageData> docList)
        {
            var result = _dataOp.BatchSaveStorageData(docList);
            return result;
        }


        /// <summary>
        /// 执行更新操作
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public InvokeResult SubmitChangeRouter(StorageData doc)
        {
            if (NeedQueue)
            {
                return SubmitChangeViaQueue(doc);
            }
            else
            {
                return SubmitChange(doc);
            }
        }

        /// <summary>
        /// 执行更新操作
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public InvokeResult SubmitChangeViaQueue(StorageData doc)
        {
           var success = NetQHelper.Broadcast<StorageDataForSerialize>(doc.ToStorageDataForSerialize());
           return new InvokeResult() {Status= success ? Status.Successful:Status.Failed };
        }

        /// <summary>
        /// 通过队列执行更新操作
        /// </summary>
        /// <param name="docList"></param>
        /// <returns></returns>
        public InvokeResult SubmitChangeViaQueue(List<StorageData> docList)
        {
            var success = false;
            docList.ForEach((doc) => {
                success=SubmitChangeViaQueue(doc).Status==Status.Successful;
            });
            return new InvokeResult() { Status = success ? Status.Successful : Status.Failed };
        }
        #endregion


    }

}
