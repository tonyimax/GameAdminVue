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
using System.Data;
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
    public class api : WebService
    {
        private object ConvertDataSetToJson(DataSet ds)
        {
            if (ds == null || ds.Tables.Count == 0)
                return null;

            var tables = new List<object>();
            foreach (DataTable table in ds.Tables)
            {
                var rows = new List<Dictionary<string, object>>();
                foreach (DataRow row in table.Rows)
                {
                    var rowDict = new Dictionary<string, object>();
                    foreach (DataColumn col in table.Columns)
                    {
                        rowDict[col.ColumnName] = row[col] == DBNull.Value ? null : row[col];
                    }
                    rows.Add(rowDict);
                }
                tables.Add(new { TableName = table.TableName, Rows = rows });
            }

            return tables;
        }

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
                Session[AppConfig.UserSessionKey] = login;
                var x = Session[AppConfig.UserSessionKey];
                HttpCookie cookie = new HttpCookie(AppConfig.UserCookieKey, user.UserID.ToString());
                cookie.Expires = DateTime.Now.AddHours(1);
                Context.Response.Cookies.Add(cookie);
                var resCache = Fetch.SaveLoginResources(login.UserID);
                Session[AppConfig.LoginResources] = resCache;
                var y = Session[AppConfig.LoginResources];
                return 0;
            }
            else
            {
                return -3;
            }
        }


        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetMenuByUserId(int UserId)
        {
            try
            {
                var ds = FacadeManage.aidePlatformManagerFacade.GetMenuByUserId(UserId);
                var jsonResult = ConvertDataSetToJson(ds);
                return JsonConvert.SerializeObject(new
                {
                    Success = true,
                    Message = "获取菜单成功",
                    Data = jsonResult
                });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

    }


}
