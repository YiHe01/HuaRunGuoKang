using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NanjingGY
{
    class Helper
    {
        public static string wmsRespos(JObject jObject, string strMethod)
        {
            string url = System.Configuration.ConfigurationSettings.AppSettings["url"];
            url += strMethod;
            //IupJob.Log("URL IS:" + url);
            string strResultMsg = httpPost(url.ToString(), jObject.ToString());

            return strResultMsg;
        }

        public static string wmsRespos(JObject jObject, string url, string pjurl)
        {
            //ClientDataAdapter dataAdapter = ClientDataAdapter.GetInstance();
            //配置参数
            //string url = "http://api.tj.aqdpay.com/drugstore/api/erpApi";

            // 拼接后的 url 链接
            string wzurl = url + pjurl;
            //IupJob.Log("访问的url地址："+wzurl);
            string strResultMsg = DataHandle(jObject.ToString(), wzurl);

            return strResultMsg;
        }
        /// <summary>
        /// 加密签名路径
        /// </summary>
        /// <param name="jsonData"></param>
        public static string DataHandle(string jsonData, string url)
        {
            string returnMsg = "";
            try
            {
                GenerateSign genSign = new GenerateSign();
                //取当前时间
                String requestTime = GetTimeStamp().ToString();
                StringBuilder sbPostUrl = new StringBuilder();
                //拼接
                sbPostUrl.Append(url);
                //sbPostUrl.Append("?method=");
                //sbPostUrl.Append(method);
                //sbPostUrl.Append("&format=json&app_key=");
                //sbPostUrl.Append(appkey);
                //sbPostUrl.Append("&sign=");
                //sbPostUrl.Append(secretKey);
                //sbPostUrl.Append("&sign_method=md5");
                //sbPostUrl.Append(Sessionkey);
                //sbPostUrl.Append("&Sessionkey=Sessionkey");

                //IupJob.Log("url路径:" + sbPostUrl.ToString());
                returnMsg = httpPost(sbPostUrl.ToString().Trim('&'), jsonData);
                return returnMsg;
            }
            catch (Exception ex)
            {
                throw new IndexOutOfRangeException("接口访问异常：" + ex.Message);
            }
        }
        /// <summary>
        /// http传输
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postDataStr"></param>
        /// <returns></returns>
        public static string httpPost(string postUrl, string postDataStr)
        {

            string strValue = "";
            try
            {
                System.GC.Collect();
                System.Net.ServicePointManager.DefaultConnectionLimit = 50;

                CredentialCache mycache = new CredentialCache();
                DataTable dt = new DataTable();

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(postUrl);
                request.Method = "POST";
                request.ContentType = "application/json;charset=UTF-8";
                byte[] payload;

                payload = Encoding.UTF8.GetBytes(postDataStr);
                request.ReadWriteTimeout = 600 * 1000;
                request.Timeout = 600 * 1000;

                request.ContentLength = payload.Length;
                Stream writer = request.GetRequestStream();
                writer.WriteTimeout = 1800 * 1000;

                writer.Write(payload, 0, payload.Length);
                writer.Close();
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                Stream s = response.GetResponseStream();
                string StrDate = "";
                StreamReader Reader = new StreamReader(s, Encoding.UTF8);
                while ((StrDate = Reader.ReadLine()) != null)
                {
                    strValue += StrDate + "\r\n";
                }
                request.Abort();
            }
            catch (WebException ex)
            {
                throw new InvalidOperationException(ex.Message);
            }
            return strValue;
        }

        /// <summary>
        /// 创建RPC API接口授权码
        /// </summary>
        /// <param name="appID"></param>
        /// <param name="appSecret"></param>
        /// <param name="clientID"></param>
        /// <returns></returns>
        public static string CreateAuthCode(string dateTime)
        {
            string appID = System.Configuration.ConfigurationSettings.AppSettings["appId"];
            string appSecret = System.Configuration.ConfigurationSettings.AppSettings["appSecret"];
            string clientID = System.Configuration.ConfigurationSettings.AppSettings["clientID"];
            string text = string.Format("@APPID={0}|@APPSECRET={1}|@CLIENTID={2}|@TIME={3}",
                                         appID, appSecret, clientID, dateTime);
            string base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
            base64 = base64.Replace("+", "_").Replace("=", "-").Replace("/", "|");
            return base64;
        }

        /// <summary>
        /// MD5字符串加密
        /// </summary>
        /// <param name="txt"></param>
        /// <returns>加密后字符串</returns>
        public static string GenerateMD5(string txt)
        {
 
            string str = string.Empty;
            MD5 md5 = MD5.Create();//实例化一个md5对像
           // 加密后是一个字节类型的数组，这里要注意编码UTF8/Unicode等的选择　
            byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(txt));
            
            // 通过使用循环，将字节类型的数组转换为字符串，此字符串是常规字符格式化所得
            for (int i = 0; i < s.Length; i++)
            {
                // 将得到的字符串使用十六进制类型格式。格式后的字符是小写的字母，如果使用大写（X）则格式后的字符是大写字符
                str = str + s[i].ToString("X2");
            }
            
            return str;
        }
        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        public static string GetTimeStamp(string s, bool b = true)
        {
            string res = string.Empty;
            if (b)
            {
                TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
                if (!string.IsNullOrWhiteSpace(s))
                    ts = Convert.ToDateTime(s) - new DateTime(1970, 1, 1, 0, 0, 0, 0);
                res = Convert.ToInt64(ts.TotalSeconds).ToString();
            }
            else
                res = s;
            return res;
        }

        public static string GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalMilliseconds).ToString();
        }
    }
}
