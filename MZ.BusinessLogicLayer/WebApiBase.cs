using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http;
using MongoDB.Bson;
using System.Web;
using MongoDB.Driver.Builders;
using System.Net.Http;
using BusinessLogicLayer;
using Common.Logging;
using MZ.BusinessLogicLayer.Business;

namespace Yinhe.ProcessingCenter
{
    public partial class ApiControllerBase : ApiController
    {
        public static readonly JobBll _jobBll = JobBll._();
        public static readonly JobLogBll _jobLogBll = JobLogBll._();
        private DataOperation _dataOp = null;
        /// <summary>
        /// 数据加密,后续与客户端进行同步加密解密
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage DataEncode(ResultInfo resultInfo)
        {

            return new HttpResponseMessage() { Content = new JsonContent(resultInfo) };
        }
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
    /// 自定义此特性用于接口的身份验证
    /// </summary>
    public class RequestAuthorizeAttribute : AuthorizeAttribute
    {
        //重写基类的验证方式，加入我们自定义的验证
        public override void OnAuthorization(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            var curUrl = actionContext.Request.RequestUri.AbsoluteUri;
            if (!PageReq.UrlCheckSign(curUrl))
            {
                //log.Info("请求验证未通过,请求地址:" + actionContext.Request.RequestUri.AbsoluteUri);
                HandleUnauthorizedRequest(actionContext);
               // base.IsAuthorized(actionContext);
            }
            else
            {
                base.IsAuthorized(actionContext);
            }
        }
    }
}
