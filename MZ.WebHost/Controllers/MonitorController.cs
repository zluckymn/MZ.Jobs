using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using MongoDB.Bson;
using System.Web;
using Yinhe.ProcessingCenter.Common;
using Yinhe.ProcessingCenter.Permissions;
using Yinhe.ProcessingCenter.SystemAuthority;
using MongoDB.Driver.Builders;
using Yinhe.ProcessingCenter;
using BusinessLogicLayer.Business;
using BusinessLogicLayer;

namespace MZ.WebHost.Controllers
{
    [Authentication]
    public class MonitorController : Yinhe.ProcessingCenter.ControllerBase
    {
        //
        // GET: /OperationPlatform/

        public ActionResult Index()
        {
            return View();
        }
       
        /// <summary>
        /// 通用图标页
        /// </summary>
        /// <returns></returns>
        public ActionResult MonitorCommonChart() {
            var type = PageReq.GetParam("type");
            var path = PageReq.GetParam("path");
            ViewData["type"] = type;
            ViewData["path"] = path;
            return View();
        }
        #region 系统监控
        /// <summary>
        /// 系统监控
        /// </summary>
        /// <returns></returns>
        public ActionResult MonitorSystem()
        {
            var customerCode = PageReq.GetSession("CustomerCode");
            if (customerCode == "EAEACC31-F37F-430B-B320-37693BBIGDATA")//城市大数据
            {
                return Redirect("/Monitor/BigDataInfo");
            }
            var health = dataOp.FindAllByQuery("CSSysHealth", Query.EQ("customerCode", customerCode)).OrderByDescending(c => c.Date("createDate")).FirstOrDefault();
            if (health.IsNullOrEmpty())
            {
                health = new BsonDocument();
            }
            health.Set("cpu", health.Decimal("cpu").ToString("0.00"));
            var memory = health.Decimal("totalMemory") > 0 ? Math.Round((health.Decimal("totalMemory") - health.Decimal("freeMemory")) / health.Decimal("totalMemory"), 2, MidpointRounding.AwayFromZero) : 0;
            health.Set("memory", (memory*100).ToString());
            var time = Math.Round(health.Decimal("actRunTime") / 3000, 2, MidpointRounding.AwayFromZero);
            health.Set("time", (time*100).ToString());
            var bsonStr = health.String("disk");
            var bsonList = new List<BsonDocument>();
            if (!string.IsNullOrEmpty(bsonStr))
            {
                bsonList = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<List<BsonDocument>>(bsonStr);
            }
            var diskMemory = bsonList.Sum(c => c.Decimal("TotalSize"))>0? Math.Round(bsonList.Sum(c=>c.Decimal("TotalSize") - c.Decimal("FreeSize")) / bsonList.Sum(c => c.Decimal("TotalSize")), 2, MidpointRounding.AwayFromZero):0;
            health.Set("diskMemory", (diskMemory*100).ToString());
            health.Set("downUserCount", (health.Int("downUserCount")*-1).ToString());
            var logDataOp = new DataOperation(CusAppConfig.MasterDataBaseConnectionString, false);
            var customer = dataOp.FindOneByQuery("CustomerInfo", Query.EQ("customerCode", customerCode));
            var siteList = dataOp.FindAllByQuery("SiteInfo", Query.EQ("customerId", customer.String("customerId"))).ToList();
            var moduleList = dataOp.FindAll("CustomerModule").ToList();
            var exceptionLogList = logDataOp.FindAllByQuery("CommonExceptionLog",Query.And(Query.EQ("customerCode", customerCode),Query.Or(Query.Exists("logType",false),Query.EQ("logType", "")),Query.In("hostDomain", siteList.Select(c=>(BsonValue)c.String("siteDomain"))))).Where(c=>!c.String("errorMessage").Contains("未找到视图") && !c.String("errorMessage").Contains("执行处理程序")).OrderByDescending(c=>c.Date("date")).ToList();
            var allPathList = new List<string>();
            foreach (var module in moduleList)
            {
                var pathList = module.String("path").Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
                var moduleLogList = exceptionLogList.Where(c => pathList.Exists(x=> c.String("orininalString").ToLower().Contains(x.ToLower()))).ToList();
                var percent = exceptionLogList.Count > 0 ? Math.Round((decimal)moduleLogList.Count / exceptionLogList.Count, 2, MidpointRounding.AwayFromZero) : 0;
                module.Set("percent", (percent * 100).ToString());
                allPathList.AddRange(pathList);
            }
            var otherLogList = exceptionLogList.Where(c => !allPathList.Exists(x => c.String("orininalString").ToLower().Contains(x.ToLower()))).ToList();
            var otherPercent = exceptionLogList.Count > 0 ? Math.Round((decimal)otherLogList.Count / exceptionLogList.Count, 2, MidpointRounding.AwayFromZero) : 0;
            var otherModule = new BsonDocument().Add("moduleId","-1").Add("name", "其他").Add("percent", (otherPercent * 100).ToString());
            moduleList.Add(otherModule);
            ViewData["health"] = health;
            ViewData["exceptionLogList"] = exceptionLogList;
            ViewData["moduleList"] = moduleList;
           
            return View();
        }
        #endregion

      
    }
}
