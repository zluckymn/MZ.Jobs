using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using Yinhe.ProcessingCenter;
using BusinessLogicLayer;
using System.Net.Http;
using BusinessLogicLayer.Business;
using Yinhe.ProcessingCenter.DataRule;
using MZ.BusinessLogicLayer.Business;
 
namespace MZ.WebHost.Controllers
{
   
    [RequestAuthorize]
    public class CustomerController : ApiControllerBase
    {
      
        //
        // GET: /Jobs/
        public string Index()
        {
            return "Hello!";
        }
        /// <summary>
        /// 返回客户信息
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage Info()
        {
            var customerCode = PageReq.GetString("customerCode");
            var customerInfo = dataOp.FindOneByQuery("CustomerInfo", Query.EQ("customerCode", customerCode));
            customerInfo.Set("id", customerInfo.String("_id")).Remove("_id");
            var resultInfo = new ResultInfo
            {
                status = "true",
                message = "成功",
                data = customerInfo.ToJson()
            };
            return DataEncode(resultInfo);
        }
        /// <summary>
        /// 判断是否要更新,频繁读取的加入到队列中进行保存
        /// </summary>
        /// <returns></returns>
        // ReSharper disable once InconsistentNaming
        public HttpResponseMessage CFGChange()
        {
            var ipAddress = IpHelper.GetIPAddress; ;
            string customerCode = PageReq.GetString("customerCode");
            BsonDocument customerInfo = dataOp.FindOneFieldsByQuery("CustomerInfo", Query.EQ("customerCode", customerCode),new List<string> (){ "customerCode", "needChange" });
            var updateDoc = new BsonDocument().Add("serviceActiveDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Add("ip", ipAddress);
            if (customerInfo.Int("needChange") != 0)
            {
              
                //needChange后续优化在队列中进行更新
                updateDoc.Add("needChange", "0");
             
            }
            var storageData = new StorageData() { Document = updateDoc, Name = "CustomerInfo", Query = Query.EQ("customerCode", customerCode), Type = StorageType.Update };
            CommonDbChange._().SubmitChangeRouter(storageData);//通过配置是否进入队列更新或者直接更新
            //easyNetQHelper.VH_CommonDBChangeQueue.Broadcast<StorageDataForSerialize>(storageData.ToStorageDataForSerialize());
            //BsonDocument dataBson = result.BsonInfo;
            //customerInfo.Set("id", dataBson.String("_id")).Remove("_id");
            var resultInfo = new ResultInfo
            {
                status = "true",
                message = "成功",
                data = customerInfo.ToJson()
            };
            return DataEncode(resultInfo);
        }
    }
}
