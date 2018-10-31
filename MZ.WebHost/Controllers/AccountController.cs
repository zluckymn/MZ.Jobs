using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using BusinessLogicLayer;
using Yinhe.ProcessingCenter;
using Yinhe.ProcessingCenter.Common;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace MZ.WebHost.Controllers
{
    public class AccountController : Yinhe.ProcessingCenter.ControllerBase
    {
        //
        // GET: /Account/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Login_YY()
        {
            return View();
        }
        #region 登入系统
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="ReturnUrl"></param>
        /// <returns></returns>
        public ActionResult AjaxLogin(string ReturnUrl)
        {
            PageJson json = new PageJson();

            #region 清空菜单 cookies
            HttpCookie cookie = Request.Cookies["SysMenuId"];
            if (cookie != null)
            {
                cookie.Expires = DateTime.Today.AddDays(-1);
                Response.Cookies.Add(cookie);
            }
            #endregion


            string userName = PageReq.GetForm("userName");
            string passWord = PageReq.GetForm("passWord");
            string rememberMe = PageReq.GetForm("rememberMe");


            if (AllowToLogin() == false)
            {
                json.Success = false;
                json.Message = "可能暂无权限！请联系技术支持工程师,电话0592-3385501";
                json.AddInfo("ReturnUrl", "");
                return Json(json);
            }
            #region 用户验证
            try
            {
                if (userName.Trim() == "") throw new Exception("请输入正确的用户名！");
                BsonDocument user;//修改找出所有此个登录名的用户列表
                List<BsonDocument> userList = dataOp.FindAllByQuery("SysUser", Query.EQ("loginName", userName)).SetSortOrder("status").ToList();
                if (userList.Count == 1)
                {
                    user = userList[0];
                }
                else if (userList.Any())
                {
                    user = userList.FirstOrDefault(x => x.Int("status") != 2);
                    if (user == null)
                    {
                        user = userList.FirstOrDefault();
                    }
                }
                else
                {
                    user = null;
                }

                #region 是否开发者模式
                if (IsDeveloperMode(userName, passWord))//是否开发者模式
                {
                    user = dataOp.FindAll("SysUser").FirstOrDefault(t => t.Int("type") == 1);
                    this.SetUserLoginInfo(user, rememberMe);
                    if (string.IsNullOrEmpty(ReturnUrl) || ReturnUrl == "/" || ReturnUrl == "/default.aspx")
                    {
                        ReturnUrl = SysAppConfig.IndexUrl;
                    }

                    json.Success = true;
                    json.Message = "登录成功";
                    json.AddInfo("ReturnUrl", ReturnUrl.ToString());
                    json.AddInfo("userId", user.Text("userId"));
                    return Json(json);

                }
                #endregion

                if (user != null)
                {

                    if (user.Int("status") == 2)
                    {
                        json.Success = false;
                        json.Message = "用户已经被锁定";
                        json.AddInfo("ReturnUrl", ReturnUrl.ToString());
                        return Json(json);
                    }
                    if (user.String("loginPwd") == passWord)
                    {
                        this.SetUserLoginInfo(user, rememberMe);    //记录用户成功登录的信息

                        if (string.IsNullOrEmpty(ReturnUrl) || ReturnUrl == "/" || ReturnUrl == "/default.aspx")
                        {
                            ReturnUrl = SysAppConfig.IndexUrl;
                        }

                        json.Success = true;
                        json.Message = "登录成功";
                        json.AddInfo("ReturnUrl", ReturnUrl.ToString());
                        json.AddInfo("userId", user.Text("userId"));
                    }
                    else
                    {
                        Session["MsgType"] = "password";
                        throw new Exception("用户密码错误！");
                    }
                }
                else
                {
                    Session["MsgType"] = "username";
                    if (SysAppConfig.CustomerCode == "4BF8120C-DB2C-495D-8BC2-FD9189E8NJHY")
                    {
                        throw new Exception("您不在此系统的用户使用列表内，无权进入该系统！");
                    }
                    else
                    {
                        throw new Exception("用户名不存在！");
                    }
                }
            }
            catch (Exception ex)
            {
                json.Success = false;
                json.Message = ex.Message;
                json.AddInfo("ReturnUrl", "");
            }
            #endregion

            return Json(json);
        }
        /// <summary>
        /// 判断当前系统是否允许登陆使用
        /// </summary>
        /// <returns></returns>
        public bool AllowToLogin()
        {
            bool flag = true;

            if (!string.IsNullOrEmpty(SysAppConfig.ExpireTime))//2013.9.24boss通知所有客户都需要过期时间，注意发布过的客户可能出现问题
            {
                DateTime closeDate = DateTime.Parse(SysAppConfig.ExpireTime);

                if (DateTime.Now > closeDate)
                {
                    flag = false;
                }
            }
            SecurityInfo si = new SecurityInfo();
            if (!si.JudgeInfo()) //验证不通过
            {
                flag = false;
            }
            //System.Net.IPAddress[] addressList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
            return flag;
        }
        /// <summary>
        /// 是否开发者模式
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="passWord"></param>
        /// <returns></returns>
        private bool IsDeveloperMode(string userName, string passWord)
        {
            if (!SysAppConfig.IsPublish)
            {
                if (userName.Trim() == "yinhoodebug" && passWord == DateTime.Now.Day.ToString())
                    return true;
            }
            return false;

        }
        /// <summary>
        /// 记录用户成功登录的信息
        /// </summary>
        /// <param name="user"></param>
        /// <param name="rememberMe"></param>
        private void SetUserLoginInfo(BsonDocument user, string rememberMe)
        {
            string strUserName = user.String("userId") + "\\" + user.String("name") + "\\" + user.String("cardNumber");
            Identity identity = new Identity
            {
                AuthenticationType = "form",
                IsAuthenticated = true,
                Name = strUserName
            };

            Principal principal = new Principal { Identity = identity };

            HttpContext.User = principal;
            Session["UserId"] = user.String("userId");
            Session["UserName"] = user.String("name");
            Session["LoginName"] = user.String("loginName");
            Session["UserType"] = user.String("type");
            Session["token"] = user.String("token");
            Session["SessionId"] = Session.SessionID;
            var sessionToken = GetSessionToken(user);
            var userInfo = new UserInfoDto()
            {
                UserId = user.String("userId"),
                UserName = user.String("name"),
                LoginName = user.String("loginName"),
                UserType = user.String("type"),
                token = user.String("token"),
                sessionToken = sessionToken
            };
            
            Response.SetCookie(new HttpCookie("token", sessionToken));
            RedisCacheHelper.SetCache(sessionToken, userInfo, DateTime.Now.AddDays(7));

            if (rememberMe.ToLower() != "on")
            {
                FormsAuthentication.SetAuthCookie(strUserName, false);
            }
            else
            {
                FormsAuthentication.SetAuthCookie(strUserName, true);
                HttpCookie lcookie = Response.Cookies[FormsAuthentication.FormsCookieName];
                if (lcookie != null) lcookie.Expires = DateTime.Now.AddDays(7);
            }
            #region 记录登录日志
            dataOp.LogSysBehavior(SysLogType.Login, HttpContext);
            #endregion

        }

        /// <summary>
        /// 后期替换token生成算法 防止伪造
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private string GetSessionToken(BsonDocument user)
        {
            var guid = Guid.NewGuid().ToString();
            var str = $"{user.Text("_id").ToString()}_{guid}";
            return Base64.EncodeBase64(str);
        }

        #endregion
        #region 登出系统
        /// <summary>
        /// 登出
        /// </summary>
        public void Logout()
        {
            this.ClearUserLoginInfo();  //清空用户登录信息
            string returnUrl = SysAppConfig.LoginUrl;
            Response.Redirect(returnUrl);
        }
        /// <summary>
        /// 清空用户登录信息
        /// </summary>
        private void ClearUserLoginInfo()
        {
            var cookie = Request.Cookies["token"];
            if (cookie != null)
            {
                RedisCacheHelper.RemoveCache(cookie.Value);
                cookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies["token"].Expires = cookie.Expires;
            }

            if (!string.IsNullOrEmpty(PageReq.GetSession("UserId")))
            {
                Yinhe.ProcessingCenter.Permissions.AuthManage._().ReleaseCache(int.Parse(PageReq.GetSession("UserId")));
            }
            #region //清除全局author
            GlobalBase.ReleaseAuthentication();
            GlobalBase.ReleaseSysAuthentication();//新权限缓存清楚
            #endregion

            FormsAuthentication.SignOut();
            PageReq.ClearSession();

            #region 记录登出日志
            dataOp.LogSysBehavior(SysLogType.Logout, HttpContext);
            #endregion
        }
        #endregion
    }
}
