using iTextSharp.text.pdf.parser;
using SAB.Library.Core.FileService;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;

namespace OnSign.Common.Helpers
{
    public class ConfigHelper
    {
        #region -- Static (implement Singleton pattern) --
        public static string ServiceName = ConfigurationManager.AppSettings["ServiceName"];



        public static string User = ConfigurationManager.AppSettings["SessionUser"];
        public static string KeyEncryptPassword = ConfigurationManager.AppSettings["KeyEncryptPassword"];
        public static string UsernameEmail = ConfigurationManager.AppSettings["UsernameEmail"];
        public static string PasswordEmail = ConfigurationManager.AppSettings["PasswordEmail"];
        public static string RootURL = ConfigurationManager.AppSettings["RootURL"];
        public static string RootFolder = ConfigurationManager.AppSettings["RootFolder"];
        public static string DocumentRootFolder = ConfigurationManager.AppSettings["DocumentRootFolder"];
        public static string FullDocument = $"{ RootFolder}{DocumentRootFolder}";


        public static string SignaturesRootFolder = ConfigurationManager.AppSettings["SignaturesRootFolder"];
        public static string HostEmail = ConfigurationManager.AppSettings["HostEmail"];
        public static string GhostScriptFolder = HostingEnvironment.MapPath($"~/{ConfigurationManager.AppSettings["GhostScriptFolder"]}");

        // Cấu hình đường dẫn HSM CyberLotus
        public static string APIID = ConfigurationManager.AppSettings["APIID"];
        public static string APIURL = ConfigurationManager.AppSettings["APIURL"];
        public static string SECRET = ConfigurationManager.AppSettings["SECRET"];

        // Config HSM - ONSIGN
        public static string APIURL_ONSIGN = ConfigurationManager.AppSettings["APIURL_ONSIGN"];
        public static string APIID_ONSIGN = ConfigurationManager.AppSettings["APIID_ONSIGN"];
        public static string SECRET_ONSIGN = ConfigurationManager.AppSettings["SECRET_ONSIGN"];

        public static string HSMApiSignPdf = ConfigurationManager.AppSettings["HSMApiSignPdf"];
        public static string HSMApiSignPdfHashData = ConfigurationManager.AppSettings["HSMApiSignPdfHashData"];
        public static string HSMApiSignGetCertificate = ConfigurationManager.AppSettings["HSMApiSignGetCertificate"];

        // Cấu hình đường dẫn OnFinance
        public static string AccessToken = ConfigurationManager.AppSettings["AccessToken"];
        public static string OnFinanceHost = ConfigurationManager.AppSettings["OnFinanceHost"];
        public static string OnFinanceAPIGetToken = ConfigurationManager.AppSettings["OnFinanceAPIGetToken"];
        public static string OnFinanceAPIGetListInvoice = ConfigurationManager.AppSettings["OnFinanceAPIGetListInvoice"];
        public static string OnFinanceAPISignInvoice = ConfigurationManager.AppSettings["OnFinanceAPISignInvoice"];
        public static string OnFinanceAPISignMultipleInvoice = ConfigurationManager.AppSettings["OnFinanceAPISignMultipleInvoice"];

        // Cấu hình Địa chỉ elasticsearch
        public static string ES_IP = ConfigurationManager.AppSettings["ES_IP"];

        // Cấu hình đường dẫn Domain SPA_SAInvoice
        public static string UriAppAddress = ConfigurationManager.AppSettings["UriAppAddress"];

        public static string RabbitMQUserName = ConfigurationManager.AppSettings["RabbitMQUserName"];
        public static string RabbitMQPassword = ConfigurationManager.AppSettings["RabbitMQPassword"];
        public static string RabbitMQHostName = ConfigurationManager.AppSettings["RabbitMQHostName"];
        public static string RabbitMQVirtualHost = ConfigurationManager.AppSettings["RabbitMQVirtualHost"];
        public static string RabbitMQRenderTopic = ConfigurationManager.AppSettings["RabbitMQRenderTopic"];
        public static string RabbitMQRequestTopic = ConfigurationManager.AppSettings["RabbitMQRequestTopic"];

        /// <summary>
        /// The instance
        /// </summary>
        private static volatile ConfigHelper _instance;

        /// <summary>
        /// The synchronize root
        /// </summary>
        private static readonly object _syncRoot = new object();

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static ConfigHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_syncRoot)
                    {
                        if (_instance == null)
                        {
                            _instance = new ConfigHelper();
                        }
                    }
                }
                return _instance;
            }
        }

        #endregion

        public string GetConnectionStringDS()
        {
            return ConfigurationManager.ConnectionStrings[Constants.CKEY_CONNECTIONDS].ConnectionString;
        }

        /// <summary>
        /// Ghi log Exception
        /// </summary>
        /// <param name="strTitle"></param>
        /// <param name="objEx"></param>
        /// <param name="strEvent"></param>
        /// <param name="strModuleName"></param>
        /// <param name="strUsername"></param>
        /// <param name="locationId"></param>
        /// <returns></returns>
        public ResultMessageBO WriteLogException(string strTitle, Exception objEx, string strEvent = null, string strModuleName = null, string strUsername = "system", int locationId = 0)
        {
            SAB.Library.Data.FileLogger.LogAction(objEx);
            return MethodHelper.Instance.FillResultMessage(true, ErrorTypes.Others, strTitle, objEx.ToString());
        }

        /// <summary>
        /// Ghi log string
        /// </summary>
        /// <param name="strTitle">Tiêu đề lỗi</param>
        /// <param name="strContent">Nội dung</param>
        /// <param name="strEvent">Tên sự kiện</param>
        /// <param name="strModuleName">NULL</param>
        /// <param name="strUsername">email user</param>
        /// <param name="user_id">id user</param>
        /// <returns></returns>
        public ResultMessageBO WriteLogString(string strTitle, string strContent, string strEvent, string strModuleName = null, string strUsername = "system", int user_id = 0)
        {
            //WriteLog(strTitle, strContent, strEvent, strUsername, intStoreId, strModuleName);
            SAB.Library.Data.FileLogger.LogAction(strTitle, strContent, strEvent, strUsername, user_id);
            return MethodHelper.Instance.FillResultMessage(true, ErrorTypes.Others, strTitle, strContent);
        }
    }

}
