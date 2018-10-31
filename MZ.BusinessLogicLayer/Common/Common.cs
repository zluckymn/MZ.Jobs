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
using System.Collections;

namespace Yinhe.ProcessingCenter
{
    public partial class ControllerBase : Controller
    {
        /// <summary>
        /// 保存提交上来的数据
        /// </summary>
        /// <param name="saveForm"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult SavePostInfo(FormCollection saveForm)
        {
            NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
            InvokeResult result = new InvokeResult();
            #region 构建数据
            var formKeys = saveForm.AllKeys;
            string tbName = formKeys.Contains("tbName") ? saveForm["tbName"] : PageReq.GetForm("tbName");
            string queryStr = formKeys.Contains("queryStr") ? saveForm["queryStr"] : PageReq.GetForm("queryStr");
            string dataStr = formKeys.Contains("dataStr") ? saveForm["dataStr"] : PageReq.GetForm("dataStr");
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
        #region 删除方法
        /// <summary>
        /// 删除提交上来的信息
        /// </summary>
        /// <param name="saveForm"></param>
        /// <returns></returns>
        /// 
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult DelePostInfo(FormCollection saveForm)
        {
            InvokeResult result = new InvokeResult();

            string tbName = saveForm["tbName"] != null ? saveForm["tbName"] : "";
            string queryStr = saveForm["queryStr"] != null ? saveForm["queryStr"] : "";
            string dataStr = "";

            int primaryKey = 0;
            TableRule rule = new TableRule(tbName);
            string keyName = rule.GetPrimaryKey();
            var isLogicDel = rule.isLogicDel;
            if (!isLogicDel)
            {
                #region 删除文档

                if (!string.IsNullOrEmpty(queryStr))
                {
                    var query = TypeConvert.NativeQueryToQuery(queryStr);
                    var recordDoc = dataOp.FindOneByQuery(tbName, query);
                    saveForm["keyValue"] = result.BsonInfo.Text(keyName);
                    if (recordDoc != null)
                    {
                        primaryKey = recordDoc.Int(keyName);
                    }

                    FileOperationHelper opHelper = new FileOperationHelper();
                    result = opHelper.DeleteFile(tbName, keyName, primaryKey.ToString());
                }
                #endregion

                #region 删除数据
                BsonDocument curData = new BsonDocument();  //当前数据,即操作前数据

                if (queryStr.Trim() != "") curData = dataOp.FindOneByQuery(tbName, TypeConvert.NativeQueryToQuery(queryStr));

                dataOp.SetOperationData(tbName, queryStr, dataStr);

                result = dataOp.Delete();
                #endregion
                //删除文件
            }
            else
            {
                return logicDelePostInfo(saveForm);
            }

            return Json(TypeConvert.InvokeResultToPageJson(result));
        }
        /// <summary>
        /// 逻辑删除对应记录
        /// </summary>
        /// <param name="saveForm"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult logicDelePostInfo(FormCollection saveForm)
        {
            InvokeResult result = new InvokeResult();
            string tbName = saveForm["tbName"] != null ? saveForm["tbName"] : "";
            string queryStr = saveForm["queryStr"] != null ? saveForm["queryStr"] : "";
            string dataStr = "";
            #region 删除数据
            BsonDocument curData = new BsonDocument();  //当前数据,即操作前数据

            if (queryStr.Trim() != "") curData = dataOp.FindOneByQuery(tbName, TypeConvert.NativeQueryToQuery(queryStr));

            if (curData != null)
            {
                result = dataOp.Update(curData, "delStatus=1");
            }

            #endregion
            return Json(TypeConvert.InvokeResultToPageJson(result));
        }
        #endregion

        #region 获取简单json表单
        /// <summary>
        /// 获取简单表的Json列表
        /// </summary>
        /// <param name="tbName">表名</param>
        /// <param name="ps">每页条数(默认20,-1不翻页)</param>
        /// <param name="cu">当前页</param>
        /// <param name="qu">查询语句(原生查询)</param>
        /// <param name="of">排序字段</param>
        /// <param name="ot">排序类型(空正序,desc倒序)</param>
        /// <param name="key">关键词</param>
        /// <returns></returns>
        public ActionResult GetSingleTableJson(string tbName, int? ps, int? cu, string qu, string of, string ot, string key)
        {
            int pageSize = (ps != null && ps.HasValue && ps.Value != 0) ? ps.Value : 20;
            int current = (cu != null && cu.HasValue && cu.Value != 0) ? cu.Value : 1;

            string query = qu != null ? qu : "";
            string orderField = of != null ? of : "";
            string orderType = ot != null ? ot : "";

            var queryComp = TypeConvert.NativeQueryToQuery(query);

            List<BsonDocument> allDocList = new List<BsonDocument>();
            if (tbName != null && tbName != "")
            {
                allDocList = queryComp != null ? dataOp.FindAllByQuery(tbName, queryComp).ToList() : dataOp.FindAll(tbName).ToList();
            }
            if (!string.IsNullOrEmpty(key))
            {
                allDocList = allDocList.Where(c => c.String("name").Contains(key)).ToList();
            }
            int allCount = allDocList.Count();

            if (orderField != null && orderField != "")
            {
                if (orderType != null && orderType == "desc")
                {
                    allDocList = allDocList.OrderByDescending(t => t.String(orderField)).ToList();
                }
                else
                {
                    allDocList = allDocList.OrderBy(t => t.String(orderField)).ToList();
                }
            }


            List<Hashtable> retList = new List<Hashtable>();

            if (pageSize != -1)
            {
                allDocList = allDocList.Skip((current - 1) * pageSize).Take(pageSize).ToList();
            }

            foreach (var tempDoc in allDocList)
            {
                tempDoc.Add("allCount", allCount.ToString());
                tempDoc.Remove("_id");
                retList.Add(tempDoc.ToHashtable());
            }

            return this.Json(retList, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}
