using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Yinhe.ProcessingCenter;

using log4net;
using MZ.BusinessLogicLayer.Business;
using System.DirectoryServices;
using MongoDB.Bson;

namespace MZ.WebHost.Controllers
{
    public class HomeController : Yinhe.ProcessingCenter.ControllerBase
    {
        //
        // GET: /Home/
        [Authentication]
        public ActionResult Index()
        {
            return View();
        }



        /// <summary>
        /// 创建新的站点
        /// </summary>
        /// <param name="hostIP">站点Ip</param>
        /// <param name="portNum">端口号</param>
        /// <param name="descOfWebSite">站点描述</param>
        /// <param name="commentOfWebSite">站点名称</param>
        /// <param name="webPath">站在所在文件夹物理路径</param>
        /// <returns></returns>
      
        public ActionResult CreateSites(string hostIP, string portNum, string descOfWebSite, string commentOfWebSite, string webPath)
        {
            try
            {
                var webSiteInfo = new IISHelper.NewWebSiteInfo(hostIP, portNum, descOfWebSite, commentOfWebSite, webPath);
                webSiteInfo.MineDic.Add(".dwf", "drawing/x-dwf");
                webSiteInfo.MineDic.Add(".dwg", "application/autocad");
                webSiteInfo.MineDic.Add(".dxf", "application/dxf");
                IISHelper helper = new IISHelper();
                DirectoryEntry newSiteEntry = null;
                newSiteEntry = helper.CreateNewWebSite(webSiteInfo);
             
            }
            catch (Exception ex)
            {
                log4net.LogManager.GetLogger("").Error(ex.Message);
                log4net.LogManager.GetLogger("").Error(ex.StackTrace);
                
            }
            return View();
        }

        /// <summary>
        /// 获取当前所有站点信息
        /// </summary>
        /// <returns></returns>
        
        public string GetSites()
        {
            try
            { 
                IISHelper helper = new IISHelper();
                var WebSiteInfoList = helper.GetSites();
                return WebSiteInfoList.ToJson();
            }
            catch (Exception ex)
            {
                log4net.LogManager.GetLogger("").Error(ex.Message);
                log4net.LogManager.GetLogger("").Error(ex.StackTrace);
                return "false";
            }
        }


        /// <summary>
        /// 站点操作
        /// </summary>
        /// <param name="siteName">站点名称</param>
        /// <param name="opType">1.启动 2.通知 3.重启 4.删除</param>
        /// <returns></returns>
      
        public ActionResult OperateWebSite(string siteName, string opType)
        {

            try
            {
                IISHelper helper = new IISHelper();
                var result = helper.OperateWebSite(siteName, opType);
                return View();
            }
            catch (Exception ex)
            {
                log4net.LogManager.GetLogger("").Error(ex.Message);
                log4net.LogManager.GetLogger("").Error(ex.StackTrace);
                return View();
            }
        }

        /// <summary>
        /// 应用程序池操作
        /// </summary>
        /// <param name="appPoolName"></param>
        /// <param name="opType">1：回收2:启动3：停止</param>
        /// <returns></returns>
        public ActionResult OperateAppPools(string appPoolName, string opType)
        {

            try
            {
                IISHelper helper = new IISHelper();
                var result = helper.OperateAppPools(appPoolName, opType);
                return View();
            }
            catch (Exception ex)
            {
                log4net.LogManager.GetLogger("").Error(ex.Message);
                log4net.LogManager.GetLogger("").Error(ex.StackTrace);
                return View();
            }
        }
    }
}
