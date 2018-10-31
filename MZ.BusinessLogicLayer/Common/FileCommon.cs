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
        #region 上传文件
        /// <summary>
        /// 上传多个文件
        /// </summary>
        /// <param name="saveForm"></param>
        /// <returns></returns>
        public string SaveMultipleUploadFiles(FormCollection saveForm)
        {
            string tableName = PageReq.GetForm("tableName");
            tableName = !string.IsNullOrEmpty(PageReq.GetForm("tableName")) ? PageReq.GetForm("tableName") : PageReq.GetForm("tbName");
            var formKeys = saveForm.AllKeys;
            if (tableName == "" && formKeys.Contains("tableName")) { tableName = saveForm["tableName"]; }
            if (tableName == "" && formKeys.Contains("tbName")) { tableName = saveForm["tbName"]; }
            string keyName = formKeys.Contains("keyName") ? saveForm["keyName"] : PageReq.GetForm("keyName");
            string keyValue = formKeys.Contains("keyValue") ? saveForm["keyValue"] : PageReq.GetForm("keyValue");
            if (string.IsNullOrEmpty(keyName))
            {
                keyName = saveForm["keyName"];
            }
            if (string.IsNullOrEmpty(keyValue) || keyValue == "0")
            {
                keyValue = saveForm["keyValue"];
            }
            string localPath = saveForm.AllKeys.Contains("uploadFileList") ? saveForm["uploadFileList"] : PageReq.GetForm("uploadFileList");
            string fileSaveType = saveForm["fileSaveType"] != null ? saveForm["fileSaveType"] : "multiply";
            int fileTypeId = PageReq.GetFormInt("fileTypeId");
            if (formKeys.Contains("fileTypeId")) { int.TryParse(saveForm["fileTypeId"], out fileTypeId); }
            int fileObjId = PageReq.GetFormInt("fileObjId");
            if (formKeys.Contains("fileObjId")) { int.TryParse(saveForm["fileObjId"], out fileObjId); }
            int uploadType = PageReq.GetFormInt("uploadType");
            if (formKeys.Contains("uploadType")) { int.TryParse(saveForm["uploadType"], out uploadType); }
            int fileRel_profId = PageReq.GetFormInt("fileRel_profId");
            if (formKeys.Contains("fileRel_profId")) { int.TryParse(saveForm["fileRel_profId"], out fileRel_profId); }
            int fileRel_stageId = PageReq.GetFormInt("fileRel_stageId");
            if (formKeys.Contains("fileRel_stageId")) { int.TryParse(saveForm["fileRel_stageId"], out fileRel_stageId); }
            int fileRel_fileCatId = PageReq.GetFormInt("fileRel_fileCatId");
            if (formKeys.Contains("fileRel_fileCatId")) { int.TryParse(saveForm["fileRel_fileCatId"], out fileRel_fileCatId); }
            int structId = PageReq.GetFormInt("structId");
            if (formKeys.Contains("structId")) { int.TryParse(saveForm["structId"], out structId); }

            bool isPreDefine = saveForm["isPreDefine"] != null ? bool.Parse(saveForm["isPreDefine"]) : false;

            Dictionary<string, string> propDic = new Dictionary<string, string>();
            FileOperationHelper opHelper = new FileOperationHelper();
            List<InvokeResult<FileUploadSaveResult>> result = new List<InvokeResult<FileUploadSaveResult>>();

            //替换会到之网络路径错误，如：\\192.168.1.150\D\A\1.jpg
            //localPath = localPath.Replace("\\\\", "\\");

            #region 如果保存类型为单个single 则删除旧的所有关联文件
            if (!string.IsNullOrEmpty(fileSaveType))
            {
                if (fileSaveType == "single")
                {
                    //opHelper.DeleteFile(tableName, keyName, keyValue);
                    opHelper.DeleteFile(tableName, keyName, keyValue, fileObjId.ToString());
                }
            }
            #endregion

            #region 通过关联读取对象属性
            if (!string.IsNullOrEmpty(localPath.Trim()))
            {
                string[] fileStr = Regex.Split(localPath, @"\|H\|", RegexOptions.IgnoreCase);
                Dictionary<string, string> filePath = new Dictionary<string, string>();
                Dictionary<string, string> filePathInfo = new Dictionary<string, string>();
                string s = fileSaveType.Length.ToString();
                foreach (string file in fileStr)
                {
                    if (string.IsNullOrEmpty(file)) continue;// 防止空数据插入的情况
                    string[] filePaths = Regex.Split(file, @"\|Y\|", RegexOptions.IgnoreCase);

                    if (filePaths.Length > 0)
                    {
                        string[] subfile = Regex.Split(filePaths[0], @"\|Z\|", RegexOptions.IgnoreCase);
                        if (subfile.Length > 0)
                        {
                            if (!filePath.Keys.Contains(subfile[0]))
                            {
                                if (filePaths.Length == 3)
                                {
                                    filePath.Add(subfile[0], filePaths[1]);
                                    filePathInfo.Add(subfile[0], filePaths[2]);
                                }
                                else if (filePaths.Length == 2 || filePaths.Length > 3)
                                {
                                    filePath.Add(subfile[0], filePaths[1]);
                                }
                                else
                                {
                                    filePath.Add(subfile[0], "");
                                }
                            }
                        }
                    }
                }

                if (fileObjId != 0)
                {

                    List<BsonDocument> docs = new List<BsonDocument>();
                    docs = dataOp.FindAllByKeyVal("FileObjPropertyRelation", "fileObjId", fileObjId.ToString()).ToList();

                    List<string> strList = new List<string>();
                    strList = docs.Select(t => t.Text("filePropId")).Distinct().ToList();
                    var doccList = dataOp.FindAllByKeyValList("FileProperty", "filePropId", strList);
                    foreach (var item in doccList)
                    {
                        var formValue = saveForm[item.Text("dataKey")];
                        if (formValue != null)
                        {
                            propDic.Add(item.Text("dataKey"), formValue.ToString());
                        }
                    }
                }
                #region 文档直接关联属性

                foreach (var tempKey in saveForm.AllKeys)
                {
                    if (!string.IsNullOrEmpty(tempKey) && tempKey.Contains("Property_"))
                    {
                        var formValue = saveForm[tempKey];
                        propDic.Add(tempKey, formValue.ToString());
                    }

                }

                #endregion

                List<FileUploadObject> singleList = new List<FileUploadObject>();   //纯文档上传
                List<FileUploadObject> objList = new List<FileUploadObject>();      //当前传入类型文件上传
                foreach (var str in filePath)
                {
                    FileUploadObject obj = new FileUploadObject();
                    List<string> infoList = new List<string>();
                    Dictionary<string, string> infoDc = new Dictionary<string, string>();
                    if (filePathInfo.ContainsKey(str.Key))
                    {
                        infoList = Regex.Split(filePathInfo[str.Key], @"\|N\|", RegexOptions.IgnoreCase).ToList();
                        foreach (var tempInfo in infoList)
                        {
                            string[] tempSingleInfo = Regex.Split(tempInfo, @"\|-\|", RegexOptions.IgnoreCase);
                            if (tempSingleInfo.Length == 2)
                            {
                                infoDc.Add(tempSingleInfo[0], tempSingleInfo[1]);
                            }
                        }

                    }
                    if (infoDc.ContainsKey("fileTypeId"))
                    {
                        obj.fileTypeId = Convert.ToInt32(infoDc["fileTypeId"]);
                    }
                    else
                    {
                        obj.fileTypeId = fileTypeId;
                    }
                    if (infoDc.ContainsKey("fileObjId"))
                    {
                        obj.fileObjId = Convert.ToInt32(infoDc["fileObjId"]);
                    }
                    else
                    {
                        obj.fileObjId = fileObjId;
                    }
                    if (filePathInfo.ContainsKey(str.Key))
                    {
                        obj.localPath = Regex.Split(str.Key, @"\|N\|", RegexOptions.IgnoreCase)[0];
                    }
                    else
                    {
                        obj.localPath = str.Key;
                    }
                    if (infoDc.ContainsKey("tableName"))
                    {
                        obj.tableName = infoDc["tableName"];
                    }
                    else
                    {
                        obj.tableName = tableName;
                    }
                    if (infoDc.ContainsKey("keyName"))
                    {
                        obj.keyName = infoDc["keyName"];
                    }
                    else
                    {
                        obj.keyName = keyName;
                    }
                    if (infoDc.ContainsKey("keyValue"))
                    {
                        if (infoDc["keyValue"] != "0")
                        {
                            obj.keyValue = infoDc["keyValue"];
                        }
                        else
                        {
                            obj.keyValue = keyValue;
                        }

                    }
                    else
                    {
                        obj.keyValue = keyValue;
                    }
                    if (infoDc.ContainsKey("uploadType"))
                    {
                        if (infoDc["uploadType"] != null && infoDc["uploadType"] != "undefined")
                        {
                            obj.uploadType = Convert.ToInt32(infoDc["uploadType"]);
                        }
                        else
                        {
                            obj.uploadType = uploadType;
                        }
                    }
                    else
                    {
                        obj.uploadType = uploadType;
                    }
                    obj.isPreDefine = isPreDefine;
                    if (infoDc.ContainsKey("isCover"))
                    {
                        if (infoDc["isCover"] == "Yes") { obj.isCover = true; }
                        else
                        {
                            obj.isCover = false;
                        }
                    }
                    else
                    {
                        obj.propvalueDic = propDic;
                    }
                    if (infoDc.ContainsKey("structId"))
                    {
                        obj.structId = Convert.ToInt32(infoDc["structId"]);
                    }
                    else
                    {
                        obj.structId = structId;
                    }
                    obj.rootDir = str.Value;
                    obj.fileRel_profId = fileRel_profId.ToString();
                    obj.fileRel_stageId = fileRel_stageId.ToString();
                    obj.fileRel_fileCatId = fileRel_fileCatId.ToString();

                    if (uploadType != 0 && (obj.rootDir == "null" || obj.rootDir.Trim() == ""))
                    {
                        singleList.Add(obj);
                    }
                    else
                    {
                        objList.Add(obj);
                    }
                }

                result = opHelper.UploadMultipleFiles(objList, (UploadType)uploadType);//(UploadType)uploadType
                if (singleList.Count > 0)
                {
                    //result = opHelper.UploadMultipleFiles(singleList, (UploadType)0);
                    result.AddRange(opHelper.UploadMultipleFiles(singleList, (UploadType)0));
                }
            }
            else
            {
                PageJson jsonone = new PageJson();
                jsonone.Success = false;
                return jsonone.ToString() + "|";

            }
            #endregion

            PageJson json = new PageJson();
            var ret = opHelper.ResultConver(result);
            json.Success = ret.Status == Status.Successful ? true : false;
            var strResult = json.ToString() + "|" + ret.Value;
            return strResult;
        }
        #endregion
        #region 文件方法
        /// <summary>
        /// 文件下载次数
        /// </summary>
        /// <param name="saveForm"></param>
        /// <returns></returns>
        public ActionResult FileDownloadCount(FormCollection saveForm)
        {
            PageJson json = new PageJson();
            string fileIds = PageReq.GetForm("fileIds");
            try
            {
                string[] fileIdArray;
                if (fileIds.Length > 0)
                {
                    fileIdArray = fileIds.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var tempId in fileIdArray)
                    {
                        var tempArray = tempId.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                        if (tempArray.Length != 2)
                            continue;
                        var fileId = tempArray[0];
                        var version = tempArray[1];
                        var file = dataOp.FindOneByQuery("FileLibrary", Query.EQ("_id", ObjectId.Parse(fileId)));
                        var fileVer = dataOp.FindOneByQuery("FileLibVersion", Query.And(
                            Query.EQ("fileId", file.String("fileId")),
                            Query.EQ("version", version)));
                        var downloadCount = fileVer.Int("downloadCount");
                        var data = new BsonDocument().Set("downloadCount", ++downloadCount);

                        dataOp.Update("FileLibVersion", Query.EQ("fileVerId", fileVer.String("fileVerId")), data);
                    }
                }
                json.Success = true;

            }
            catch (Exception ex)
            {
                json.Success = false;
                json.Message = ex.Message;
            }

            return Json(json);
        }
        /// <summary>
        /// 文件查看次数
        /// </summary>
        /// <param name="saveForm"></param>
        /// <returns></returns>
        public ActionResult FileViewCount(FormCollection saveForm)
        {
            PageJson json = new PageJson();
            string fileId = PageReq.GetForm("fileId");
            string version = PageReq.GetForm("version");
            try
            {
                var file = dataOp.FindOneByQuery("FileLibrary", Query.EQ("_id", ObjectId.Parse(fileId)));
                var fileVer = dataOp.FindOneByQuery("FileLibVersion",
                        Query.And(
                            Query.EQ("fileId", file.String("fileId")),
                            Query.EQ("version", version)
                        )
                    );
                var viewCount = fileVer.Int("viewCount");
                var data = new BsonDocument().Set("viewCount", ++viewCount);
                dataOp.Update("FileLibVersion", Query.EQ("fileVerId", fileVer.Text("fileVerId")), data);
                json.Success = true;
            }
            catch (Exception ex)
            {
                json.Success = false;
                json.Message = ex.Message;
            }

            return Json(json);
        }
        #endregion
        #region 删除文件
        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="saveForm"></param>
        /// <returns></returns>
        public JsonResult DeleFiles(FormCollection saveForm)
        {
            string fileRelIds = saveForm["delFileRelIds"] != null ? saveForm["delFileRelIds"] : "";

            InvokeResult result = new InvokeResult();
            FileOperationHelper opHelper = new FileOperationHelper();
            try
            {
                string[] fileRelArray;
                if (fileRelIds.Length > 0)
                {
                    fileRelArray = fileRelIds.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                    List<BsonValue> fileRelIdList = new List<BsonValue>();
                    foreach (var tempId in fileRelArray) fileRelIdList.Add(tempId.Trim());

                    if (fileRelArray.Length > 0)
                    {
                        result = opHelper.DeleteFileByRelIdList(fileRelIdList);
                        if (result.Status == Status.Failed) throw new Exception(result.Message);
                    }
                }

            }
            catch (Exception ex)
            {
                result.Status = Status.Failed;
                result.Message = ex.Message;
            }

            return Json(TypeConvert.InvokeResultToPageJson(result), JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}
