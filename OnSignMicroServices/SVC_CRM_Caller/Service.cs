using Newtonsoft.Json;
using Npgsql;
using OnSign.BusinessLogic;
using OnSign.BusinessLogic.Account;
using OnSign.BusinessLogic.Document;
using OnSign.BusinessLogic.Email;
using OnSign.BusinessLogic.Notifications;
using OnSign.BusinessObject.Account;
using OnSign.BusinessObject.Document;
using OnSign.BusinessObject.Email;
using OnSign.BusinessObject.Forms;
using OnSign.BusinessObject.Sign;
using OnSign.Common;
using OnSign.Common.Helpers;
using SAB.Library.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
//using Newtonsoft.Json;

namespace SVC_CRM_Caller
{
    public partial class Service : ServiceBase
    {
        private readonly NpgsqlConnection conn = new NpgsqlConnection(ConfigHelper.Instance.GetConnectionStringDS());

        private bool IsExistAccessToken { get; set; }
        private bool IsNotFound { get; set; }

        #region CRM Caller\
        private static readonly string CALLER_ACTION = "talk";
        private static readonly string CALLER_FROM_EXTERNAL = "external";
        private static HttpClient client = new HttpClient();


        private static string baseAddress = ConfigurationManager.AppSettings["baseCaller"];
        private static string oauthCaller = ConfigurationManager.AppSettings["OauthCaller"];
        private static string callOutCaller = ConfigurationManager.AppSettings["CallOutCaller"];



        private static string client_id = ConfigurationManager.AppSettings["client_id"];
        private static string client_secret = ConfigurationManager.AppSettings["client_secret"];
        private static string grant_type = ConfigurationManager.AppSettings["grant_type"];
        private static string default_from_number = ConfigurationManager.AppSettings["default_from_number"];
        private static string from_number_viettel = ConfigurationManager.AppSettings["from_number_viettel"];
        private static string from_number_mobile = ConfigurationManager.AppSettings["from_number_mobile"];
        private static string from_number_vina = ConfigurationManager.AppSettings["from_number_vina"];
        #endregion CRM Caller

        public Service()
        {
            InitializeComponent();
        }

        public void OnDebug()
        {
            OnStart(null);
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                CallerOauth();
                Thread newProcess = new Thread(ListenForNotifications)
                {
                    IsBackground = true
                };
                newProcess.Start();
            }
            catch (Exception ex)
            {
                FileLogger.LogAction($"OnStart - Lỗi Khởi động : {ex}");
            }
        }


        /// <summary>
        /// Lấy access token CRM Caller
        /// </summary>
        private async Task<bool> CallerOauth()
        {
            try
            {
                OauthRequestBO oauthRequest = new OauthRequestBO
                {
                    client_id = client_id,
                    client_secret = client_secret,
                    grant_type = grant_type
                };
                var objToStr = JsonConvert.SerializeObject(oauthRequest);

                var formContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", client_id),
                    new KeyValuePair<string, string>("client_secret", client_secret),
                    new KeyValuePair<string, string>("grant_type", grant_type)
                });

                ConfigurationManager.AppSettings["Authorization"] = string.Empty;
                using (client = new HttpClient())
                {
                    client.BaseAddress = new Uri(baseAddress);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var response = await client.PostAsync(oauthCaller, formContent);
                    if (!response.IsSuccessStatusCode)
                    {
                        //Nếu domain sai, không tìm thấy domain
                        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            IsNotFound = true;
                            FileLogger.LogAction($"CallerOauth - {response.ReasonPhrase}: {response.RequestMessage.RequestUri}");
                            return false;
                        }

                        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            FileLogger.LogAction($"CallerOauth - {response.ReasonPhrase}: {response.RequestMessage.RequestUri} - {{client_id:{client_id}, client_secret: {client_secret}, grant_type: {grant_type}}}");
                            return false;
                        }
                    }

                    var responseString = await response.Content.ReadAsStringAsync();
                    OauthResponseBO oauthResponse = JsonConvert.DeserializeObject<OauthResponseBO>(responseString);
                    if (!string.IsNullOrEmpty(oauthResponse.access_token))
                    {
                        IsExistAccessToken = true;
                        ConfigurationManager.AppSettings["Authorization"] = $"Bearer {oauthResponse.access_token}";
                        return IsExistAccessToken;
                    }
                    else
                    {
                        IsExistAccessToken = false;
                        ConfigurationManager.AppSettings["Authorization"] = string.Empty;
                        FileLogger.LogAction($"CallerOauth - Lỗi lấy access Token CRM Caller: {response.RequestMessage.RequestUri} - {{client_id:{client_id}, client_secret: {client_secret}, grant_type: {grant_type}}}");
                        return IsExistAccessToken;
                    }
                }
            }
            catch (Exception ex)
            {
                IsExistAccessToken = false;
                FileLogger.LogAction($"CallerOauth - Lỗi lấy access Token CRM Caller: {ex}");
                return IsExistAccessToken;
            }
        }

        private async Task<bool> CallerCallOut(VerifyCodeBO verifyCode)
        {
            try
            {
                string auth = ConfigurationManager.AppSettings["Authorization"];
                if (IsNotFound || !IsExistAccessToken || string.IsNullOrEmpty(auth))
                    return false;

                verifyCode.PHONE_NUMBER_CALL_OUT = string.IsNullOrEmpty(verifyCode.PHONE_NUMBER_CALL_OUT) ?
                    default_from_number : MethodHelper.CleanNumber(verifyCode.PHONE_NUMBER_CALL_OUT);
                List<string> strCode = MethodHelper.CleanNumber(verifyCode.CODE).Select(c => c.ToString()).ToList(); string codeOTP = string.Join(" ! ", strCode);
                CallOutRequestBO callOutRequest = new CallOutRequestBO()
                {
                    from = new CallOutRequestFromBO()
                    {
                        alias = verifyCode.PHONE_NUMBER_CALL_OUT,
                        number = verifyCode.PHONE_NUMBER_CALL_OUT,
                        type = CALLER_FROM_EXTERNAL
                    },
                    to = new List<CallOutRequestToBO>()
                    {
                        new CallOutRequestToBO()
                        {
                            type = CALLER_FROM_EXTERNAL,
                            alias = verifyCode.PHONE_NUMBER,
                            number = verifyCode.PHONE_NUMBER
                        }
                    },
                    actions = new List<CallOutRequestActionBO>()
                    {
                        new CallOutRequestActionBO()
                        {
                            action = CALLER_ACTION,
                            text = $"On Sai chào bạn! Mã xác thực của bạn là: {codeOTP}. Xin nhắc lại, mã xác thực của bạn là: {codeOTP}. " +
                            $"Vui lòng không cung cấp mã này cho bất kỳ ai. Xin cảm ơn"
                        }
                    }
                };
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(baseAddress);
                    client.DefaultRequestHeaders.Add("Authorization", auth);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    using (var content = new StringContent(JsonConvert.SerializeObject(callOutRequest), System.Text.Encoding.UTF8, "application/json"))
                    {
                        var response = await client.PostAsync(callOutCaller, content);
                        if (!response.IsSuccessStatusCode)
                        {
                            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                            {
                                IsNotFound = true;
                                FileLogger.LogAction($"CallerCallOut - {response.ReasonPhrase}: {response.RequestMessage.RequestUri}");
                                return false;
                            }

                            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                            {
                                FileLogger.LogAction($"CallerCallOut - {response.ReasonPhrase}: {response.RequestMessage.RequestUri} - {{client_id:{client_id}, client_secret: {client_secret}, grant_type: {grant_type}}}");
                                await this.CallerOauth();
                                await CallerCallOut(verifyCode);
                            }
                        }
                        var responseString = await response.Content.ReadAsStringAsync();
                        CallOutResponseBO oauthResponse = JsonConvert.DeserializeObject<CallOutResponseBO>(responseString);
                        if (oauthResponse.success)
                        {
                            VerifyCodeBLL verifyCodeBLL = new VerifyCodeBLL();
                            return verifyCodeBLL.UpdateCallerCalled(verifyCode);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return true;
        }

        private void ListenForNotifications()
        {
            ExecuteListenNotify();
            conn.Notification += PostgresNotificationReceived;
            while (true)
            {
                try
                {
                    if (!(conn.State == ConnectionState.Open))
                    {
                        ExecuteListenNotify();
                    }
                    conn.Wait();
                }
                catch (Exception ex)
                {
                    Thread.Sleep(10000);
                    FileLogger.LogAction($"Lỗi kết nối Trigger Database: {ex}");
                }
            }
        }

        private void ExecuteListenNotify()
        {
            conn.Open();
            var listenCommand = conn.CreateCommand();
            listenCommand.CommandText = $"listen otp_notify";
            listenCommand.ExecuteNonQuery();
        }

        private void PostgresNotificationReceived(object sender, NpgsqlNotificationEventArgs e)
        {
            try
            {
                string data = e.Payload;
                var verifyCode = JsonConvert.DeserializeObject<VerifyCodeBO>(e.Payload, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                var result = CallerCallOut(verifyCode);
            }
            catch (Exception ex)
            {
                FileLogger.LogAction($"Lỗi gửi email: {ex}", "");
            }
        }


    }

    public class OauthRequestBO
    {
        public string client_id { get; set; }
        public string client_secret { get; set; }
        public string grant_type { get; set; }
    }

    public class OauthResponseBO
    {
        public string token_type { get; set; }
        public long expires_in { get; set; }
        public string access_token { get; set; }
    }


    public class CallOutRequestBO
    {
        public CallOutRequestFromBO from { get; set; }
        public List<CallOutRequestToBO> to { get; set; }
        public List<CallOutRequestActionBO> actions { get; set; }

    }
    public class CallOutRequestFromBO
    {
        public string type { get; set; }
        public string number { get; set; }
        public string alias { get; set; }
    }

    public class CallOutRequestToBO
    {
        public string type { get; set; }
        public string number { get; set; }
        public string alias { get; set; }
    }

    public class CallOutRequestActionBO
    {
        public string action { get; set; }
        public string text { get; set; }
    }



    public class CallOutResponseBO
    {
        public bool success { get; set; }
        public int code { get; set; }
        public string locale { get; set; }
        public string message { get; set; }
        public object data { get; set; }
    }

}