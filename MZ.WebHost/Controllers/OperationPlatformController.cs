using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using MongoDB.Bson;
using System.Web;
using System.Web.Helpers;
using Yinhe.ProcessingCenter.Common;
using Yinhe.ProcessingCenter.Permissions;
using Yinhe.ProcessingCenter.SystemAuthority;
using MongoDB.Driver.Builders;
using Yinhe.ProcessingCenter;

namespace MZ.WebHost.Controllers
{
    [Authentication]
    public class OperationPlatformController : Yinhe.ProcessingCenter.ControllerBase
    {
        //
        // GET: /OperationPlatform/

        public ActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// 首页
        /// </summary>
        /// <returns></returns>
        public ActionResult PlatfromIndex()
        {
            var returnUrl = PageReq.GetString("returnUrl");
            var customerList = dataOp.FindAll("CustomerInfo").OrderBy(c => c.Int("order")).ToList();
            ViewData["customerList"] = customerList;
            ViewData["returnUrl"] = returnUrl;
            return View();
        }
        #region 客户设置
        /// <summary>
        /// 客户管理首页
        /// </summary>
        /// <returns></returns>
        public ActionResult CustomerIndex()
        {
            return View();
        }
        /// <summary>
        /// 客户列表
        /// </summary>
        /// <returns></returns>
        public ActionResult CustomerList()
        {
            var customerList = dataOp.FindAll("CustomerInfo").OrderBy(c => c.Int("order")).ToList();
            ViewData["customerList"] = customerList;
            return View();
        }
        /// <summary>
        /// 客户编辑
        /// </summary>
        /// <returns></returns>
        public ActionResult CustomerEdit()
        {
            var customerId = PageReq.GetParamInt("customerId");
            var customer = dataOp.FindOneByQuery("CustomerInfo", Query.EQ("customerId", customerId.ToString()));
            var webconfig = dataOp.FindOneByQuery("CustomerWebConfigInfo", Query.EQ("customerId", customerId.ToString()));
            ViewData["customer"] = customer;
            ViewData["webconfig"] = webconfig;
            return View();
        }
        /// <summary>
        /// 客户连接设置
        /// </summary>
        /// <returns></returns>
        public ActionResult CustomerConnectEdit()
        {
            var customerCode = PageReq.GetParam("customerCode");
            var customerConnect = dataOp.FindOneByQuery("CustomerConnect", Query.EQ("customerCode", customerCode));
            var fileRelList = dataOp.FindAllByQuery("FileRelation", Query.And(Query.EQ("tableName", "CustomerConnect"), Query.EQ("keyValue", customerConnect.String("connectId")))).ToList();
            var fileList = dataOp.FindAllByQuery("FileLibrary", Query.In("fileId", fileRelList.Select(c => (BsonValue)c.String("fileId")))).ToList();
            ViewData["customerConnect"] = customerConnect;
            ViewData["customerCode"] = customerCode;
            ViewData["fileRelList"] = fileRelList;
            ViewData["fileList"] = fileList;
            return View();
        }
        #endregion
        #region 服务器设置
        /// <summary>
        /// 服务器管理首页
        /// </summary>
        /// <returns></returns>
        public ActionResult ServerIndex()
        {
            var customerList = dataOp.FindAll("CustomerInfo").ToList();
            ViewData["customerList"] = customerList;
            return View();
        }
        /// <summary>
        /// 服务器列表
        /// </summary>
        /// <returns></returns>
        public ActionResult ServerList()
        {
            var customerCode = PageReq.GetParam("customerCode");
            var secretkey = PageReq.GetParam("secretkey");
            var serverList = dataOp.FindAllByQuery("ServerInfo", Query.EQ("customerCode", customerCode)).OrderBy(c => c.Int("order")).ToList();
            var customerConnect = dataOp.FindOneByQuery("CustomerConnect", Query.EQ("customerCode", customerCode));
            var fileRelList = dataOp.FindAllByQuery("FileRelation", Query.And(Query.EQ("tableName", "CustomerConnect"), Query.EQ("keyValue", customerConnect.String("connectId")))).ToList();
            var fileList = dataOp.FindAllByQuery("FileLibrary", Query.In("fileId", fileRelList.Select(c => (BsonValue)c.String("fileId")))).ToList();
            ViewData["serverList"] = serverList;
            ViewData["customerConnect"] = customerConnect;
            ViewData["customerCode"] = customerCode;
            ViewData["secretkey"] = secretkey;
            ViewData["fileRelList"] = fileRelList;
            ViewData["fileList"] = fileList;
            return View();
        }
        /// <summary>
        /// 服务器编辑
        /// </summary>
        /// <returns></returns>
        public ActionResult ServerEdit()
        {
            var customerCode = PageReq.GetParam("customerCode");
            var serverId = PageReq.GetParamInt("serverId");
            var server = dataOp.FindOneByQuery("ServerInfo", Query.EQ("serverId", serverId.ToString()));
            ViewData["server"] = server;
            ViewData["customerCode"] = customerCode;
            return View();
        }

        /// <summary>
        /// 服务器站点列表
        /// </summary>
        /// <returns></returns>
        public ActionResult ServerIISSiteList()
        {
            var serverId = PageReq.GetParamInt("serverId");
            var server = dataOp.FindOneByQuery("ServerInfo", Query.EQ("serverId", serverId.ToString()));

            log4net.LogManager.GetLogger("").Error("1111");
            MZ.BusinessLogicLayer.WebService.RemoteIISSites client = new BusinessLogicLayer.WebService.RemoteIISSites(server.String("serverAddress"));
            var getStr = client.GetSites();
            var sitesList = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<List<BsonDocument>>(getStr);

            ViewData["sitesList"] = sitesList;
            ViewData["server"] = server;
            return View();
        }

        /// <summary>
        /// 服务器站点编辑
        /// </summary>
        /// <returns></returns>
        public ActionResult ServerIISSiteEdit()
        {
            return View();
        }

        /// <summary>
        /// 服务器站点编辑创建
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult CreateSites()
        {
            //NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
            InvokeResult result = new InvokeResult();

            var name = PageReq.GetForm("name");
            var port = PageReq.GetForm("port");
            var path = PageReq.GetForm("path");

            var serverId = PageReq.GetFormInt("serverId");
            var server = dataOp.FindOneByQuery("ServerInfo", Query.EQ("serverId", serverId.ToString()));

            MZ.BusinessLogicLayer.WebService.RemoteIISSites client = new BusinessLogicLayer.WebService.RemoteIISSites(server.String("serverAddress"));
            var isSuccess = client.CreateSites("", port, "", name, path);

            if (isSuccess)
            {
                result.Status = Status.Successful;
            } else
            {
                result.Status = Status.Failed;
            }

            return Json(TypeConvert.InvokeResultToPageJson(result));
        }

        /// <summary>
        /// 服务器站点操作
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult OperateSite()
        {
            //NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
            InvokeResult result = new InvokeResult();

            var siteName = PageReq.GetForm("siteName");
            var opType = PageReq.GetForm("opType");
            var path = PageReq.GetForm("path");

            var serverId = PageReq.GetFormInt("serverId");
            var server = dataOp.FindOneByQuery("ServerInfo", Query.EQ("serverId", serverId.ToString()));

            MZ.BusinessLogicLayer.WebService.RemoteIISSites client = new BusinessLogicLayer.WebService.RemoteIISSites(server.String("serverAddress"));
            var isSuccess = client.OperateWebSite(siteName, opType);

            if (isSuccess)
            {
                result.Status = Status.Successful;
            }
            else
            {
                result.Status = Status.Failed;
            }
            return Json(TypeConvert.InvokeResultToPageJson(result));
        }

        /// <summary>
        /// 服务器应用程序池操作
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult OperateAppPools()
        {
           // NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
            InvokeResult result = new InvokeResult();

            var appPoolName = PageReq.GetForm("appPoolName");
            var opType = PageReq.GetForm("opType");

            var serverId = PageReq.GetFormInt("serverId");
            var server = dataOp.FindOneByQuery("ServerInfo", Query.EQ("serverId", serverId.ToString()));

            MZ.BusinessLogicLayer.WebService.RemoteIISSites client = new BusinessLogicLayer.WebService.RemoteIISSites(server.String("serverAddress"));
            var isSuccess = client.OperateAppPools(appPoolName, opType);

            if (isSuccess)
            {
                result.Status = Status.Successful;
            }
            else
            {
                result.Status = Status.Failed;
            }
            return Json(TypeConvert.InvokeResultToPageJson(result));
        }
        #endregion
        #region 数据库设置
        /// <summary>
        /// 数据库管理首页
        /// </summary>
        /// <returns></returns>
        public ActionResult DataBaseIndex()
        {
            var customerList = dataOp.FindAll("CustomerInfo").ToList();
            ViewData["customerList"] = customerList;
            return View();
        }
        /// <summary>
        /// 数据库列表
        /// </summary>
        /// <returns></returns>
        public ActionResult DataBaseList()
        {
            var customerCode = PageReq.GetParam("customerCode");
            var secretkey = PageReq.GetParam("secretkey");
            var dataBaseList = dataOp.FindAllByQuery("DataBase", Query.EQ("customerCode", customerCode)).OrderBy(c => c.Int("order")).ToList();
            var serverList = dataOp.FindAllByQuery("ServerInfo", Query.In("serverId", dataBaseList.Select(c => (BsonValue)c.String("serverId")))).OrderBy(c => c.Int("order")).ToList();
            ViewData["dataBaseList"] = dataBaseList;
            ViewData["serverList"] = serverList;
            ViewData["customerCode"] = customerCode;
            ViewData["secretkey"] = secretkey;
            return View();
        }
        /// <summary>
        /// 数据库编辑
        /// </summary>
        /// <returns></returns>
        public ActionResult DataBaseEdit()
        {
            var dataBaseId = PageReq.GetParamInt("dataBaseId");
            var customerCode = PageReq.GetParam("customerCode");
            var dataBase = dataOp.FindOneByQuery("DataBase", Query.EQ("dataBaseId", dataBaseId.ToString()));
            var serverList = dataOp.FindAllByQuery("ServerInfo", Query.EQ("customerCode", customerCode)).OrderBy(c => c.Int("order")).ToList();
            ViewData["dataBase"] = dataBase;
            ViewData["serverList"] = serverList;
            ViewData["customerCode"] = customerCode;
            return View();
        }
        #endregion
        #region 站点设置
        /// <summary>
        /// 站点管理首页
        /// </summary>
        /// <returns></returns>
        public ActionResult SiteInfoIndex()
        {
            var customerList = dataOp.FindAll("CustomerInfo").ToList();
            ViewData["customerList"] = customerList;
            return View();
        }
        /// <summary>
        /// 站点列表
        /// </summary>
        /// <returns></returns>
        public ActionResult SiteInfoList()
        {
            var customerCode = PageReq.GetParam("customerCode");
            var siteList = dataOp.FindAllByQuery("SiteInfo", Query.EQ("customerCode", customerCode)).ToList();
            var dataBaseList = dataOp.FindAllByQuery("DataBase", Query.In("dataBaseId", siteList.Select(c => (BsonValue)c.String("dataBaseId")))).ToList();
            var serverList = dataOp.FindAllByQuery("ServerInfo", Query.In("serverId", siteList.Select(c => (BsonValue)c.String("serverId")))).ToList();
            ViewData["siteList"] = siteList;
            ViewData["dataBaseList"] = dataBaseList;
            ViewData["serverList"] = serverList;
            ViewData["customerCode"] = customerCode;
            return View();
        }
        /// <summary>
        /// 站点编辑
        /// </summary>
        /// <returns></returns>
        public ActionResult SiteInfoEdit()
        {
            var customerCode = PageReq.GetParam("customerCode");
            var siteId = PageReq.GetParamInt("siteId");
            var site = dataOp.FindOneByQuery("SiteInfo", Query.EQ("siteId", siteId.ToString()));
            var dataBaseList = dataOp.FindAll("DataBase").ToList();
            var serverList = dataOp.FindAll("ServerInfo").ToList();
            ViewData["site"] = site;
            ViewData["dataBaseList"] = dataBaseList;
            ViewData["serverList"] = serverList;
            ViewData["customerCode"] = customerCode;
            return View();
        }
        #endregion
        #region 系统配置页设置
        /// <summary>
        /// 系统配置汇聚主页
        /// </summary>
        /// <returns></returns>
        public ActionResult InitPageUrlManage()
        {
            var groupList = dataOp.FindAll("InitPageUrlGroup").OrderBy(c => c.Int("order")).ToList();
            ViewData["groupList"] = groupList;
            return View();
        }
        /// <summary>
        /// 系统配置汇聚列表页
        /// </summary>
        /// <returns></returns>
        public ActionResult InitPageUrlManageList()
        {
            var groupId = PageReq.GetParamInt("groupId");
            var urlList = dataOp.FindAllByQuery("InitPageUrl", Query.EQ("groupId", groupId.ToString())).OrderBy(c => c.Int("order")).ToList();
            ViewData["urlList"] = urlList;
            return View();
        }
        /// <summary>
        /// 系统配置链接组别编辑页
        /// </summary>
        /// <returns></returns>
        public ActionResult PageUrlGroupEdit()
        {
            var groupId = PageReq.GetParamInt("groupId");
            var groupInfo = dataOp.FindOneByKeyVal("InitPageUrlGroup", "groupId", groupId.ToString());
            var queryStr = string.Empty;
            if (!groupInfo.IsNullOrEmpty())
            {
                queryStr = "db.InitPageUrlGroup.distinct('_id',{'groupId':'" + groupId + "'})";
            }
            ViewData["groupInfo"] = groupInfo;
            ViewData["queryStr"] = queryStr;
            return View();
        }
        /// <summary>
        /// 系统配置链接编辑页
        /// </summary>
        /// <returns></returns>
        public ActionResult PageUrlEdit()
        {
            var urlId = PageReq.GetParamInt("urlId");
            var groupId = PageReq.GetParamInt("groupId");
            var urlInfo = dataOp.FindOneByKeyVal("InitPageUrl", "urlId", urlId.ToString());
            var queryStr = string.Empty;
            if (!urlInfo.IsNullOrEmpty())
            {
                queryStr = "db.InitPageUrl.distinct('_id',{'urlId':'" + urlId + "'})";
            }
            ViewData["groupId"] = groupId.ToString();
            ViewData["urlInfo"] = urlInfo;
            ViewData["queryStr"] = queryStr;
            return View();
        }
        #endregion
        #region 监控配置
        /// <summary>
        /// 原始调度事务
        /// </summary>
        /// <returns></returns>
        public ActionResult BackgroundJobIndex() {
            var customerList = dataOp.FindAll("CustomerInfo").OrderBy(c => c.Int("order")).ToList();
            ViewData["customerList"] = customerList;
            return View();
        }
        /// <summary>
        /// 原始调度事务列表
        /// </summary>
        /// <returns></returns>
        public ActionResult BackgroundJobList()
        {
            var customerCode = PageReq.GetParam("customerCode");
            var backgroundJobList = new List<BsonDocument>();
            var customerList = new List<BsonDocument>();
            if (string.IsNullOrEmpty(customerCode))
            {
                backgroundJobList = dataOp.FindAllByQuery("BackgroundJob", Query.Or(Query.Exists("customerCode", false), Query.EQ("customerCode", ""))).OrderBy(c => c.Int("jobType")).ToList();
            }
            else
            {
                backgroundJobList = dataOp.FindAllByQuery("BackgroundJob", Query.And(Query.Exists("customerCode", true), Query.EQ("customerCode", customerCode))).OrderBy(c => c.Int("jobType")).ToList();
                customerList = dataOp.FindAll("CustomerInfo").OrderBy(c => c.Int("order")).ToList();
                var templet = dataOp.FindAllByQuery("BackgroundJob", Query.Or(Query.Exists("customerCode", false), Query.EQ("customerCode", ""))).OrderBy(c => c.Int("order")).ToList();
                ViewData["customerJob"] = GetCustomerJobInfo();
                ViewData["templet"] = templet;
            }
            var statementList = dataOp.FindAllByQuery("Statement", Query.In("statementId", backgroundJobList.Select(c => (BsonValue)c.String("statementId")))).ToList();
            ViewData["backgroundJobList"] = backgroundJobList;
            ViewData["customerList"] = customerList;
            ViewData["statementList"] = statementList;
            ViewData["customerCode"] = customerCode;
            return View();
        }
        /// <summary>
        /// 原始调度事务编辑
        /// </summary>
        /// <returns></returns>
        public ActionResult BackgroundJobEdit()
        {
            var jobId = PageReq.GetParam("jobId");
            var customerCode = PageReq.GetParam("customerCode");
            var job = dataOp.FindOneByQuery("BackgroundJob", Query.EQ("jobId", jobId));
            //type==2 编辑
            //type==3 创建 从jobId导入
            //customerList = dataOp.FindAll("CustomerInfo").OrderBy(c => c.Int("order")).ToList();
            var stateMentList = dataOp.FindAll("Statement").OrderBy(c => c.Int("order")).ToList();
            ViewData["job"] = job;
            ViewData["stateMentList"] = stateMentList;
            ViewData["customerCode"] = customerCode;
            return View();
        }
        /// <summary>
        /// 事务同步
        /// </summary>
        /// <returns></returns>
        public ActionResult BackgroundJobSyn() {
            var customerCode = PageReq.GetString("customerCode");
            List<BsonDocument> jobList = new List<BsonDocument>();
            List<BsonDocument> customerList = new List<BsonDocument>();
            if (string.IsNullOrEmpty(customerCode))
            {
                customerList = dataOp.FindAll("CustomerInfo").ToList();
                jobList = dataOp.FindAllByQuery("BackgroundJob", Query.EQ("customerCode", "")).ToList();
            }
            else {
                customerList = dataOp.FindAllByQuery("CustomerInfo", Query.NE("customerCode", customerCode)).ToList();
                jobList = dataOp.FindAllByQuery("BackgroundJob", Query.EQ("customerCode", customerCode)).ToList();
            }
            ViewData["customerList"] = customerList;
            ViewData["jobList"] = jobList;
            return View();
        }
        /// <summary>
        /// 事务日志查看
        /// </summary>
        /// <returns></returns>
        public ActionResult BackgroundJobLogIndex() {
            var customerList = dataOp.FindAll("CustomerInfo").OrderBy(c => c.Int("order")).ToList();
            ViewData["customerList"] = customerList;
            return View();
        }
        /// <summary>
        /// 事务日志查看
        /// </summary>
        /// <returns></returns>
        public ActionResult BackgroundJobLogList()
        {
            var customerCode = PageReq.GetParam("customerCode");
            var startStr = PageReq.GetParam("startStr");
            var endStr = PageReq.GetParam("endStr");
            var nowDate = DateTime.Now.Date;
            if (string.IsNullOrEmpty(startStr) && string.IsNullOrEmpty(endStr))
            {
                DateTime startWeek = nowDate.AddDays(1 - Convert.ToInt32(nowDate.DayOfWeek.ToString("d"))); //本周周一
                DateTime endWeek = startWeek.AddDays(6); //本周周日
                startStr = startWeek.ToString("yyyy-MM-dd");
                endStr = endWeek.ToString("yyyy-MM-dd");
            }
            var limit = 20;
            var sort = new MongoDB.Driver.SortByDocument {  { "createDate",-1 } };
            var query = Query.And(Query.GTE("createDate", startStr), Query.LTE("createDate", endStr), Query.EQ("customerCode", customerCode));
            var backgroundJobLogList = dataOp.FindLimitByQuery("BackgroundJobLog", query, sort, 0,limit) .ToList();
            ViewData["backgroundJobLogList"] = backgroundJobLogList;
            ViewData["startStr"] = startStr;
            ViewData["endStr"] = endStr;
            return View();
        }
        public JsonResult BackgroundJobInfo(string code)
        {
            var jobs = dataOp.FindFieldsByQuery("BackgroundJob", Query.EQ("customerCode", code),new[]{"jobId","name"}).ToList();
            List<object>data = new List<object>();
            foreach (var job in jobs)
            {
                data.Add(new {id=job.Text("jobId"),name=job.Text("name")});
            }

            return Json(new {error=0,data});
        }
        /// <summary>
        /// 客户任务调度统计
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, int> GetCustomerJobInfo()
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            var col = new MongoOperation().GetCollection("BackgroundJob");
            Dictionary<string, int> dicInitial = new Dictionary<string, int>();
            dicInitial["count"] = 0;
            var r = col.Group(
                Query.And(Query.NE("customerCode", null), Query.NE("customerCode", ""), Query.Exists("customerCode", true)),
                "customerCode",
                BsonDocument.Create(dicInitial),
                BsonJavaScript.Create("function(doc,prev){prev.count++;}"),
                null
            ).ToList();
            if (r.Count > 0)
            {
                foreach (var item in r)
                {
                    result.Add(item.Text("customerCode"), item.Int("count"));
                }
            }
            return result;
        }
        public JsonResult BackgroundJobImport(List<string> ids,string code)
        {
            var jobs = dataOp
                .FindAllByQuery("BackgroundJob", Query.In("jobId", TypeConvert.StringListToBsonValueList(ids)))
                .ToList();
            List<BsonDocument>list = new List<BsonDocument>();
            foreach (var job in jobs)
            {
                var count = dataOp.FindCount("BackgroundJob",
                    Query.And(Query.EQ("name", job.Text("name")), Query.EQ("customerCode", code)));
                if (count<=0)
                {
                    var doc = new BsonDocument();
                    doc.Set("jobId", Guid.NewGuid().ToString());
                    doc.Set("customerCode", code);
                    doc.Set("assemblyName", job.Text("assemblyName"));
                    doc.Set("className", job.Text("className"));
                    doc.Set("cron", job.Text("cron"));
                    doc.Set("cronDesc", job.Text("cronDesc"));
                    doc.Set("descrption", job.Text("descrption"));
                    doc.Set("jobArgs", job.Text("jobArgs"));
                    doc.Set("name", job.Text("name"));
                    doc.Set("state", job.Text("state"));
                    doc.Set("jobType", job.Text("jobType"));
                    doc.Set("status", job.Text("status"));
                    doc.Set("statementId", job.Text("statementId"));
                    doc.Set("scrId", job.Text("jobId"));
                    doc.Set("sourceId", string.IsNullOrEmpty(job.Text("sourceId"))?job.Text("jobId"):job.Text("sourceId"));
                    list.Add(doc);
                }
            }
            if (list.Count>0)
            {
                var result = dataOp.BatchInsert("BackgroundJob", list);
                return Json(TypeConvert.InvokeResultToPageJson((result)));
            }
            else
            {
                return Json(new PageJson(false,"选择导入的数据已存在"));
            }
        }
        #endregion
        #region 报表数据配置
        /// <summary>
        /// 报表配置首页
        /// </summary>
        /// <returns></returns>
        public ActionResult StatementIndex() {
            return View();
        }
        /// <summary>
        /// 报表列表
        /// </summary>
        /// <returns></returns>
        public ActionResult StatementList()
        {
            var statementList = dataOp.FindAll("Statement").OrderBy(c => c.Int("order")).ToList();
            ViewData["statementList"] = statementList;
            return View();
        }
        /// <summary>
        /// 报表编辑
        /// </summary>
        /// <returns></returns>
        public ActionResult StatementEdit()
        {
            var statementId = PageReq.GetParamInt("statementId");
            var statement = dataOp.FindOneByQuery("Statement", Query.EQ("statementId", statementId.ToString()));
            ViewData["statement"] = statement;
            return View();
        }
        /// <summary>
        /// 报表表头管理
        /// </summary>
        /// <returns></returns>
        public ActionResult StatementHeaderIndex()
        {
            var statementId = PageReq.GetParamInt("statementId");
            var statement = dataOp.FindOneByQuery("Statement", Query.EQ("statementId", statementId.ToString()));
            var headerList = dataOp.FindAllByQuery("StatementHeader", Query.EQ("statementId", statementId.ToString())).OrderBy(c=>c.String("nodeKey")).ToList();
            ViewData["statement"] = statement;
            ViewData["headerList"] = headerList;
            return View();
        }
        /// <summary>
        /// 报表表头编辑
        /// </summary>
        /// <returns></returns>
        public ActionResult StatementHeaderEdit()
        {
            var statementId = PageReq.GetParamInt("statementId");
            var headerId = PageReq.GetParamInt("headerId");
            var nodePid = PageReq.GetString("nodePid");
            var header = dataOp.FindOneByQuery("StatementHeader", Query.EQ("headerId", headerId.ToString()));
            var methodList = dataOp.FindAll("StatementFieldGetMethod").ToList();
            ViewData["methodList"] = methodList;
            ViewData["header"] = header;
            ViewData["nodePid"] = nodePid;
            ViewData["statementId"] = statementId;
            return View();
        }
        /// <summary>
        /// 报表规则
        /// </summary>
        public ActionResult StatementLibIndex() {
            return View();
        }
        /// <summary>
        /// 报表规则
        /// </summary>
        public ActionResult StatementLibList()
        {
            var statementLibList = dataOp.FindAll("StatementDataRuleLib").ToList();
            ViewData["statementLibList"] = statementLibList;
            return View();
        }
        /// <summary>
        /// 报表规则
        /// </summary>
        public ActionResult StatementLibEdit()
        {
            var libId = PageReq.GetParamInt("libId");
            var statementLib = dataOp.FindOneByQuery("StatementDataRuleLib",Query.EQ("libId", libId.ToString()));
            ViewData["statementLib"] = statementLib;
            return View();
        }
        #endregion
        #region 系统模块
        /// <summary>
        /// 客户模块
        /// </summary>
        /// <returns></returns>
        public ActionResult CustomerModuleIndex() {
            //var customerList = dataOp.FindAll("CustomerInfo").OrderBy(c => c.Int("order")).ToList();
            //ViewData["customerList"] = customerList;
            return View();
        }
        /// <summary>
        /// 客户模块
        /// </summary>
        /// <returns></returns>
        public ActionResult CustomerModuleList()
        {
            var moduleList = dataOp.FindAll("CustomerModule").OrderBy(c => c.Int("order")).ToList();
            ViewData["moduleList"] = moduleList;
            return View();
        }
        /// <summary>
        /// 客户模块
        /// </summary>
        /// <returns></returns>
        public ActionResult CustomerModuleEdit()
        {
            var moduleId = PageReq.GetParamInt("moduleId");
            var module = dataOp.FindOneByQuery("CustomerModule", Query.EQ("moduleId", moduleId.ToString()));
            ViewData["module"] = module;
            return View();
        }
        #endregion
    }
}
