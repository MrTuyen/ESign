using Microsoft.SqlServer.Server;
using OnSign.BusinessObject.Permission;
using OnSign.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace OnSign.BusinessObject.Account
{
    [Serializable]
    public class AccountBO
    {
        private static AccountBO _instance;

        public static AccountBO Current
        {
            get { return _instance ?? (_instance = new AccountBO()); }
        }

        public AccountBO CurrentUser()
        {
            try
            {
                if (HttpContext.Current != null && HttpContext.Current.Session != null)
                {
                    HttpSessionStateBase session = new HttpSessionStateWrapper(HttpContext.Current.Session);
                    return session[ConfigHelper.User] as AccountBO;
                }
            }
            catch (Exception objEx)
            {
                ConfigHelper.Instance.WriteLogException("Không lấy được thông tin user", objEx, "CurrentUser", "AccountModel");
            }
            return null;
        }
        public List<PermissionGroupBO> PERMISSIONGROUP { get; set; }
        /// <remarks/>
        #region Thông tin cá nhân (thông tin đăng ký)
        public int ID { get; set; }
        public string FULLNAME { get; set; }
        public string CREATEDFULLNAME { get; set; }
        public string USERNAME { get; set; }
        public string PHONE { get; set; }
        public string AVATAR { get; set; }
        public string EMAIL { get; set; }
        public int CREATEDBYUSER { get; set; }
        public DateTime CREATEDATTIME { get; set; }
        public string CREATEDBYIP { get; set; }
        public string PASSWORD { get; set; }
        public string OLDPASSWORD { get; set; }
        public string NEWPASSWORD { get; set; }
        public string CONFIRMNEWPASSWORD { get; set; }
        public string PASSWORDTEMP { get; set; }
        public string SIGNATUREIMAGE { get; set; }
        public string IP { get; set; }
        public string TAXCODE { get; set; }
        public string COMPANY { get; set; }
        public string POSITION { get; set; }
        public string UTMSOURCE { get; set; }
        public string ADDRESS { get; set; }
        public bool ISACTIVED { get; set; }
        public bool ISDELETED { get; set; }
        public int DELETEDBYUSER { get; set; }
        public DateTime DELETEDATTIME { get; set; }
        public string DELETEDBYIP { get; set; }
        public bool ISACCEPTED { get; set; }
        public DateTime ACCEPTEDATTIME { get; set; }
        public bool IS_OWNER { get; set; }
        public int OWNER { get; set; }
        public int INVITATIONID { get; set; }

        //HSM Config
        public string APIID { get; set; }
        public string SECRET { get; set; }
        public string APIURL { get; set; }
        public string ACCESSTOKEN { get; set; }

        //Document config
        public int SIGNINDEX { get; set; }
        public bool ISCC { get; set; }

        public DateTime LASTACTIVITY { get; set; }

        public bool ISLOGGED { get; set; }
        public bool ISADMIN { get; set; }
        public bool ISUSEHSM { get; set; }
        public bool IS_USE_HSM_OF_OWNER { get; set; }

        public int TOTALROW { get; set; }
        public long COMPLETE { get; set; }
        public long WAITING { get; set; }
        public long DELETED { get; set; }
        #endregion
        public string DESCRIPTION { get; set; }

    }
}
