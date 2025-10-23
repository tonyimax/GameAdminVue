using Game.Entity.PlatformManager;
using Game.Facade;
using Game.Facade;
using Game.Kernel;
using Game.Utils;
using Game.Utils;
using Game.Utils.Cache;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;
using System.Web.UI;
using System.Xml.Linq;

namespace Game.Web
{
    public class LoginData
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Captcha { get; set; }
    }

    /// <summary>
    /// WebServiceLogin 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [ScriptService]
    public class WebServiceLogin : WebService
    {
        [WebMethod(EnableSession = true)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public int Login() {
            //'Content-Type': 'application/x-www-form-urlencoded'获取POST过来的数据 可以读取
            string json;
            using (var reader = new StreamReader(Context.Request.InputStream))
            {
                json = reader.ReadToEnd();
            }
            if (string.IsNullOrEmpty(json))
            {
                return -1;
            }
            var req = JsonConvert.DeserializeObject<LoginData>(json);
            string username = req.UserName;
            string password = req.Password;
            string captcha = req.Captcha;
            //====================================================================================

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return -1;
            }
            var sessionCaptcha = Session[AppConfig.VerifyCodeKey] as string;
            if (string.IsNullOrEmpty(captcha) || null == sessionCaptcha)
            {
                return -2;
            }
            

            Base_Users user = new Base_Users();
            user.Username = username;
            user.Password = Utility.MD5(password);
            user.LastLoginIP = GameRequest.GetUserIP();

            var msg = FacadeManage.aidePlatformManagerFacade.UserLogon(user);
            if (msg.Success)
            {
                LoginUser login = msg.EntityList[0] as LoginUser;
                //缓存账号信息
                Session[AppConfig.UserSessionKey] = login;
                var x = Session[AppConfig.UserSessionKey];
                HttpCookie cookie = new HttpCookie(AppConfig.UserCookieKey, user.UserID.ToString());
                cookie.Expires = DateTime.Now.AddHours(1);
                Context.Response.Cookies.Add(cookie);
                //缓存资源信息
                var resCache = Fetch.SaveLoginResources(login.UserID);
                Session[AppConfig.LoginResources] = resCache;
                var y = Session[AppConfig.LoginResources];
                /*var result = new
                {
                    data = "success",
                };
                System.Web.Script.Serialization.JavaScriptSerializer serializer =
                    new System.Web.Script.Serialization.JavaScriptSerializer();
                return serializer.Serialize(result);*/
                return 0;
            }
            else
            {
                return -3;
            }
        }
    }
}
