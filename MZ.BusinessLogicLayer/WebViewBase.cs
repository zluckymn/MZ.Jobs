using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using MongoDB.Bson;
using System.Web;
using BusinessLogicLayer;
using MongoDB.Driver.Builders;

namespace Yinhe.ProcessingCenter
{
    /// <summary>
    /// 通用页面基类
    /// </summary>
    public partial class ControllerBase : Controller
    {
        private DataOperation _dataOp = null;

        /// <summary>
        /// 数据操作类
        /// </summary>
        public DataOperation dataOp
        {
            get
            {
                if (_dataOp == null) _dataOp = new DataOperation();
                return this._dataOp;
            }
        }
       

    }
    /// <summary>
    /// 验证登入
    /// </summary>
    public class AuthenticationAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// 登录验证跳转
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //var ssid = filterContext.HttpContext.Request.Cookies["token"]?.Value;
            //var userInfo = RedisCacheHelper.GetCache<UserInfoDto>(ssid);
            //if (string.IsNullOrEmpty(userInfo?.UserId))
            //{
            //    string returnUrl = SysAppConfig.LoginUrl;

            //    if (filterContext.HttpContext.Request.RawUrl != "/")
            //    {
            //        var urlReferrer = filterContext.HttpContext.Request.RawUrl;
            //        if (filterContext.HttpContext.Request.UrlReferrer != null
            //            && filterContext.HttpContext.Request.UrlReferrer.AbsolutePath.ToString() != returnUrl)
            //        {
            //            urlReferrer = filterContext.HttpContext.Request.UrlReferrer.PathAndQuery.ToString();
            //        }

            //        if (returnUrl.IndexOf("?") >= 0)
            //        {
            //            returnUrl += "&returnUrl=" + filterContext.HttpContext.Server.UrlEncode(urlReferrer);
            //        }
            //        else
            //        {
            //            returnUrl += "?returnUrl=" + filterContext.HttpContext.Server.UrlEncode(urlReferrer);
            //        }
            //    }

            //    ContentResult Content = new ContentResult();
            //    Content.Content = string.Format("<script type='text/javascript'>window.location.href='{0}';</script>", returnUrl);
            //    filterContext.Result = Content;
            //}
            //else
            //{
            //    PageReq.SetSession("UserId",userInfo.UserId);
            //    PageReq.SetSession("UserName", userInfo.UserName);
            //    PageReq.SetSession("LoginName", userInfo.LoginName);
            //    PageReq.SetSession("UserType", userInfo.UserType);
            //    PageReq.SetSession("token", userInfo.token);
            //    PageReq.SetSession("sessionToken", userInfo.sessionToken);

            //    var cacheKey = $"CustomerCode_{userInfo.sessionToken}";
            //    var customerCodeRedis= RedisCacheHelper.GetCache<string>(cacheKey);
            //    //如果当前customerCode不存在则从redis缓存中读去对应数据,问题同一用户浏览出现用户切换问题，如我正在看蓝光 后续自动切换到旭辉
            //    if (!string.IsNullOrEmpty(customerCodeRedis))
            //    {
            //        PageReq.SetSession("CustomerCode", customerCodeRedis);
            //    }
            //    //PageReq.SetSession("token", userInfo.token);
            //    if (string.IsNullOrEmpty(PageReq.GetSession("CustomerCode")) == true && filterContext.HttpContext.Request.RawUrl.Contains("/Monitor"))
            //    {
            //        string returnUrl = SysAppConfig.IndexUrl;
            //        if (filterContext.HttpContext.Request.RawUrl != "/")
            //        {
            //            var urlReferrer = filterContext.HttpContext.Request.RawUrl;
            //            //if (filterContext.HttpContext.Request.UrlReferrer != null
            //            //    && filterContext.HttpContext.Request.UrlReferrer.AbsolutePath.ToString() != returnUrl)
            //            //{
            //            //    urlReferrer = filterContext.HttpContext.Request.UrlReferrer.PathAndQuery.ToString();
            //            //}

            //            //if (returnUrl.IndexOf("?") >= 0)
            //            //{
            //            //    returnUrl += "&returnUrl=" + filterContext.HttpContext.Server.UrlEncode(urlReferrer);
            //            //}
            //            //else
            //            //{
            //            //    returnUrl += "?returnUrl=" + filterContext.HttpContext.Server.UrlEncode(urlReferrer);
            //            //}
            //        }

            //        ContentResult Content = new ContentResult();
            //        Content.Content = string.Format("<script type='text/javascript'>window.location.href='{0}';</script>", returnUrl);
            //        filterContext.Result = Content;
            //    }
            //}
            base.OnActionExecuting(filterContext);
        }

    }
}
