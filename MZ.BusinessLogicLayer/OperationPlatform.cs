using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Collections.Specialized;
using MongoDB.Bson;
using System.Web;
using Yinhe.ProcessingCenter.MvcFilters;
using Yinhe.ProcessingCenter.Document;
using System.Text.RegularExpressions;
using Yinhe.ProcessingCenter.DataRule;
using Yinhe.ProcessingCenter;
using Yinhe.ProcessingCenter.Common;
using MongoDB.Driver;
using System.Net;
using System.IO;
using SysDirectory = System.IO.Directory;
using System.Diagnostics;
using System.Data;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Builders;
using BusinessLogicLayer;

namespace Yinhe.ProcessingCenter
{
    public partial class ControllerBase : Controller
    {
        #region 生成客户session code
        /// <summary>
        /// 生成客户session code
        /// </summary>
        /// <returns></returns>
        public ActionResult GetSystemCodeSession()
        {
            PageJson json = new PageJson();
            var customerCode = PageReq.GetString("customerCode");
            if (string.IsNullOrEmpty(customerCode))
            {
                json.Success = false;
                json.Message = "客户代码为设置";
                return Json(json);
            }
            //Session["CustomerCode"] = customerCode;


            if (!string.IsNullOrEmpty(PageReq.GetSession("sessionToken")))
            {
                var cacheKey = $"CustomerCode_{PageReq.GetSession("sessionToken")}";
                RedisCacheHelper.SetCache(cacheKey, customerCode, DateTime.Now.AddDays(30));
                PageReq.SetSession("CustomerCode", customerCode);
            }
            json.Success = true;
            return Json(json);
        }
        #endregion
        #region 保存客户
        /// <summary>
        /// 保存客户
        /// </summary>
        /// <returns></returns>
        public ActionResult SaveCustomer() {
            InvokeResult result = new InvokeResult();
            var customerId = PageReq.GetInt("customerId");
            //基本信息
            var name = PageReq.GetString("name");
            var customerCode = PageReq.GetString("customerCode");
            var bizGuid = PageReq.GetString("bizGuid");
            var remark = PageReq.GetString("remark");
            //配置信息
            var configName = PageReq.GetString("configName");
            var authorityControl = PageReq.GetString("authorityControl");
            var styleSheet = PageReq.GetString("styleSheet");
            var bson = new BsonDocument() { { "name", name }, { "customerCode", customerCode }, { "bizGuid", bizGuid }, { "remark", remark } };
            var configBson = new BsonDocument() { { "name", configName }, { "authorityControl", authorityControl }, { "styleSheet", styleSheet } };
            if (customerId == 0)
            {
                bson.Add("needChange", "0");
                result = dataOp.Insert("CustomerInfo", bson);
                if (result.Status == Status.Successful)
                {
                    configBson.Set("customerId", result.BsonInfo.String("customerId"));
                    result = dataOp.Insert("CustomerWebConfigInfo", configBson);
                }
            }
            else {
                bson.Add("needChange", "1");
                var webconfig = dataOp.FindOneByQuery("CustomerWebConfigInfo", Query.EQ("customerId", customerId.ToString()));
                result = dataOp.Update("CustomerInfo", Query.EQ("customerId", customerId.ToString()), bson);
                result = dataOp.Update("CustomerWebConfigInfo", Query.EQ("webConfigId", webconfig.String("webConfigId")), configBson);
            }
            return Json(TypeConvert.InvokeResultToPageJson(result));
        }
        #endregion
        #region 保存关键人员
        /// <summary>
        /// 保存关键人员
        /// </summary>
        /// <returns></returns>
        public ActionResult SaveImportUser() {
            var userIds = PageReq.GetString("userIds");
            var customerCode = PageReq.GetString("customerCode");
            var userIdList = userIds.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var bsonList = new List<BsonDocument>();
            var result = new InvokeResult();
            var oldUserList = dataOp.FindAllByQuery("CustomerImportUser", Query.EQ("customerCode", customerCode)).ToList();
            var delUserList = oldUserList.Where(c => !userIdList.Exists(x => x == c.String("userId"))).ToList();
            var insertList = userIdList.Where(c => !oldUserList.Exists(x => x.String("userId") == c)).ToList();
            var storList = new List<StorageData>();
            foreach (var userId in insertList)
            {
                var bson = new BsonDocument();
                bson.Set("userId", userId);
                bson.Set("customerCode", customerCode);
                bsonList.Add(bson);
            }
            if (delUserList.Count>0) {
                result = dataOp.Delete("CustomerImportUser", Query.In("relId", delUserList.Select(c => (BsonValue)c.String("relId"))));
            }
            if (bsonList.Count > 0)
            {
                result = dataOp.QuickInsert("CustomerImportUser", bsonList);
            }
            return Json(TypeConvert.InvokeResultToPageJson(result));
        }
        #endregion
        #region 保存任务
        /// <summary>
        /// 保存任务
        /// </summary>
        /// <returns></returns>
        public ActionResult SaveStageTask() {
            InvokeResult result = new InvokeResult();
            var tasks = PageReq.GetString("tasks");
            var customerCode = PageReq.GetString("customerCode");
            var taskList = tasks.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
            result = dataOp.Delete("CustomerStageTask", Query.EQ("customerCode", customerCode));
            var storList = new List<StorageData>();
            foreach (var task in taskList)
            {
                var taskArr = task.Split(new string[] { "|#|" }, StringSplitOptions.RemoveEmptyEntries);
                if (taskArr.Count() == 2)
                {
                    StorageData stor = new StorageData();
                    stor.Name = "CustomerStageTask";
                    stor.Type = StorageType.Insert;
                    stor.Document = new BsonDocument().Add("name", taskArr[0]).Add("nodeKey", taskArr[1]).Add("customerCode", customerCode);
                    storList.Add(stor);
                }
            }
            if (storList.Count > 0)
            {
                result = dataOp.BatchSaveStorageData(storList);
            }
            return Json(TypeConvert.InvokeResultToPageJson(result));
        }
        #endregion
        #region 批量修改cron表达式
        public ActionResult BatchUpdateCron() {
            var cron = PageReq.GetString("cron");
            var customerCode = PageReq.GetString("customerCode");
            var jobList = dataOp.FindAllByQuery("BackgroundJob", Query.And(Query.EQ("customerCode", customerCode),Query.Or(Query.EQ("jobType","0"),Query.EQ("jobType","3")))).ToList();
            var result = dataOp.Update("BackgroundJob", Query.In("jobId", jobList.Select(c => (BsonValue)c.String("jobId"))),new BsonDocument().Add("cron", cron));
            return Json(TypeConvert.InvokeResultToPageJson(result));
        }
        #endregion

        #region 保存数据（加密重要数据）
        /// <summary>
        /// 保存数据（加密重要数据）
        /// </summary>
        /// <param name="saveForm"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult SavePostInfoEncode(FormCollection saveForm)
        {
            NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
            InvokeResult result = new InvokeResult();
            #region 构建数据
            var formKeys = saveForm.AllKeys;
            string tbName = formKeys.Contains("tbName") ? saveForm["tbName"] : PageReq.GetForm("tbName");
            string queryStr = formKeys.Contains("queryStr") ? saveForm["queryStr"] : PageReq.GetForm("queryStr");
            string dataStr = formKeys.Contains("dataStr") ? saveForm["dataStr"] : PageReq.GetForm("dataStr");
            string encodeColumns = formKeys.Contains("encodeColumns") ? saveForm["encodeColumns"] : PageReq.GetForm("encodeColumns");
            TableRule rule = new TableRule(tbName);
            BsonDocument dataBson = new BsonDocument();

            bool columnNeedConvert = false;
            if (dataStr.Trim() == "")
            {
                if (saveForm.AllKeys.Contains("fileObjId"))
                {
                    columnNeedConvert = true;
                }
                foreach (var tempKey in saveForm.AllKeys)
                {
                    if (tempKey == "tbName" || tempKey == "queryStr" || tempKey.Contains("fileList[") || tempKey.Contains("param.")) continue;
                    //2016.1.25添加数据转换过滤,
                    //由于前端通用TableManage需要上传可能会内置tableName字段，如果表中页游tableName字段可能会冲突保存不了
                    //目前做法前段替换，后端转化COLUMNNEEDCONVERT_
                    var curFormValue = saveForm[tempKey];
                    var curColumnName = tempKey;
                    if (columnNeedConvert && tempKey.Contains("COLUMNNEEDCONVERT_"))
                    {
                        curColumnName = curColumnName.Replace("COLUMNNEEDCONVERT_", string.Empty);
                    }
                    dataBson.Set(curColumnName, curFormValue);
                }
            }
            else
            {
                dataBson = TypeConvert.ParamStrToBsonDocument(dataStr);
            }
            #endregion
            #region 加密数据
            var AesHelp = new AESHelper();
            var encodeColumnList = encodeColumns.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var bson = queryStr != "" ? dataOp.FindOneByQuery(tbName, TypeConvert.NativeQueryToQuery(queryStr)) : new BsonDocument();
            foreach (var encodeColumn in encodeColumnList)
            {
                if (dataBson.String(encodeColumn) != bson.String(encodeColumn) && !string.IsNullOrEmpty(dataBson.String(encodeColumn)))
                {
                    dataBson.Set(encodeColumn, AESHelper.AESEncrypt(dataBson.String(encodeColumn), dataBson.String("customerCode")));
                }
            }
            #endregion
            #region 保存数据
            result = dataOp.Save(tbName, queryStr != "" ? TypeConvert.NativeQueryToQuery(queryStr) : Query.Null, dataBson);
            #endregion

            #region 文件上传
            int primaryKey = 0;
            ColumnRule columnRule = rule.ColumnRules.Where(t => t.IsPrimary == true).FirstOrDefault();
            string keyName = columnRule != null ? columnRule.Name : "";
            if (!string.IsNullOrEmpty(queryStr))
            {
                var query = TypeConvert.NativeQueryToQuery(queryStr);
                var recordDoc = dataOp.FindOneByQuery(tbName, query);
                saveForm["keyValue"] = result.BsonInfo.Text(keyName);
                if (recordDoc != null)
                {
                    primaryKey = recordDoc.Int(keyName);
                }
            }

            if (primaryKey == 0)//新建
            {
                if (saveForm["tableName"] != null)
                {
                    saveForm["keyValue"] = result.BsonInfo.Text(keyName);

                }
            }
            else//编辑
            {
                #region 删除文件
                string delFileRelIds = saveForm["delFileRelIds"] != null ? saveForm["delFileRelIds"] : "";
                if (!string.IsNullOrEmpty(delFileRelIds))
                {
                    FileOperationHelper opHelper = new FileOperationHelper();
                    try
                    {
                        string[] fileArray;
                        if (delFileRelIds.Length > 0)
                        {
                            fileArray = delFileRelIds.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                            if (fileArray.Length > 0)
                            {
                                var fileRelIdList = fileArray.Select(t => (BsonValue)t).ToList();
                                var result1 = opHelper.DeleteFileByRelIdList(fileRelIdList);
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        result.Status = Status.Failed;
                        result.Message = ex.Message;
                        return Json(TypeConvert.InvokeResultToPageJson(result));
                    }
                }
                #endregion

                saveForm["keyValue"] = primaryKey.ToString();
            }
            result.FileInfo = SaveMultipleUploadFiles(saveForm);
            #endregion

            return Json(TypeConvert.InvokeResultToPageJson(result));
        }
        #endregion
    }
}
