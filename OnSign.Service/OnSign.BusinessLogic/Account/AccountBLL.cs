using Elasticsearch.Net;
using Nest;
using Newtonsoft.Json;
using OnSign.BusinessLogic.BusinessObjects;
using OnSign.BusinessObject.Account;
using OnSign.BusinessObject.Company;
using OnSign.BusinessObject.Email;
using OnSign.BusinessObject.Forms;
using OnSign.BusinessObject.Notifications;
using OnSign.BusinessObject.Permission;
using OnSign.Common;
using OnSign.Common.Helpers;
using OnSign.Common.Utilites;
using OnSign.DataObject.Account;
using SAB.Library.Core.Crypt;
using SAB.Library.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Hosting;

namespace OnSign.BusinessLogic.Account
{
    public class AccountBLL : BaseBLL
    {

        #region Fields
        protected IData objDataAccess = null;

        #endregion

        #region Properties

        #endregion

        #region Constructor
        public AccountBLL()
        {
        }

        public AccountBLL(IData objIData)
        {
            objDataAccess = objIData;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Thêm người dùng đăng nhập
        /// </summary>
        /// <returns></returns>
        public bool CreateUser(AccountBO reg)
        {
            try
            {
                AccountDAO objAccountDAO = new AccountDAO();
                return objAccountDAO.CreateUser(reg);
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi thêm người dùng đăng nhập");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                return false;
            }
        }

        public bool CreatePackage(PackageBO package)
        {
            try
            {
                AccountDAO objAccountDAO = new AccountDAO();
                return objAccountDAO.CreatePackage(package);
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi thêm người dùng đăng nhập");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                return false;
            }
        }

        public bool UpdatePackage(PackageBO package)
        {
            try
            {
                AccountDAO objAccountDAO = new AccountDAO();
                return objAccountDAO.UpdatePackage(package);
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi thêm người dùng đăng nhập");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                return false;
            }
        }

        public bool DeletePackage(PackageBO package)
        {
            try
            {
                AccountDAO objAccountDAO = new AccountDAO();
                return objAccountDAO.DeletePackage(package);
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi thêm người dùng đăng nhập");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                return false;
            }
        }
        //public AccountBO Login(AccountBO reg)
        //{
        //    try
        //    {
        //        AccountDAO objAccountDAO = new AccountDAO();
        //        var result = objAccountDAO.Login(reg);
        //        if (result.Count == 0)
        //            return null;
        //        return result.First();
        //    }
        //    catch (Exception objEx)
        //    {
        //        this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi lấy thông tin người dùng");
        //        objResultMessageBO = ConfigHelper.Instance.WriteLog(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
        //        return null;
        //    }
        //}

        /// <summary>
        /// Kiểm tra thông tin đăng nhập và trả về thông tin người dùng nếu đăng nhập thành công
        /// </summary>
        /// <param name="reg"></param>
        /// <returns></returns>
        public AccountBO UserLogin(AccountBO reg)
        {
            AccountDAO objAccountDAO = new AccountDAO();
            var resulLogin = objAccountDAO.Login(reg);

            var resultUser = resulLogin?.FirstOrDefault();
            if (resultUser != null)
            {
                //Nếu chữ ký mặc định rỗng  thì gán chữ ký bằng null
                if (!System.IO.File.Exists($"{ConfigHelper.RootFolder}{ConfigHelper.DocumentRootFolder}{resultUser.SIGNATUREIMAGE}"))
                    resultUser.SIGNATUREIMAGE = null;
                else
                    //ngược lại thì gán bằng 
                    resultUser.SIGNATUREIMAGE = $"{ConfigHelper.RootURL}{ConfigHelper.DocumentRootFolder}{resultUser.SIGNATUREIMAGE}";
                resultUser.IP = MethodHelper.GetIPClient();
            }
            return resultUser;
        }

        /// <summary>
        /// Lấy thông tin HSM từ user 
        /// </summary>
        /// <param name="reg"></param>
        /// <returns></returns>
        public AccountBO Get_HSM_Info(AccountBO reg)
        {
            //Đăng nhập lại để lấy thông tin user
            AccountDAO objAccountDAO = new AccountDAO();
            var objUser = objAccountDAO.Login(reg);
            var resultUser = objUser?.FirstOrDefault();

            //Nếu mà người khởi tạo được sử dụng HSM thì lấy thông tin HSM
            if (resultUser != null && resultUser.ISUSEHSM)
            {
                //Nếu không phải là chủ sở hữu => Lấy thông tin HSM của quản lý cấp trên
                if (!resultUser.IS_OWNER)
                {
                    //Lấy thông tin người quản lý
                    var owner = objAccountDAO.GetOwnerByUser(resultUser.ID, resultUser.TAXCODE);
                    if (owner != null)
                    {
                        //Nếu người quản lý thông tin không null thì gán giá trị vào 
                        resultUser.APIID = owner.APIID;
                        resultUser.APIURL = owner.APIURL;
                        resultUser.SECRET = owner.SECRET;
                    }
                }
                //Nếu là tài khoản demo (MST: 0101990346-999) thì gán HSM Test
                if (resultUser.TAXCODE == "0106579683-999" || resultUser.TAXCODE == "0101990346-999")
                {
                    resultUser.APIID = ConfigHelper.APIID;
                    resultUser.APIURL = ConfigHelper.APIURL;
                    resultUser.SECRET = ConfigHelper.SECRET;
                }
            }
            return resultUser;
        }

        public bool UpdateTimeOfUserAccessing(int UserId, string IPRemote)
        {
            try
            {
                AccountDAO objAccountDAO = new AccountDAO();
                return objAccountDAO.UpdateTimeOfUserAccessing(UserId, IPRemote);
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi cập nhật thông tin người dùng");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                return false;
            }
        }

        /// <summary>
        /// Quên mật khẩu
        /// </summary>
        /// <param name="reg"></param>
        /// <returns></returns>
        public bool ForgotPassword(AccountBO reg)
        {
            try
            {
                AccountDAO objAccountDAO = new AccountDAO();
                return objAccountDAO.ForgotPassword(reg);
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi quên mật khẩu.");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                return false;
            }
        }


        /// <summary>
        /// Cập nhật chữ ký và mật khẩu của người dùng
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public bool UpdateUserSignaturePassword(AccountBO account)
        {
            try
            {
                AccountDAO objAccountDAO = new AccountDAO();
                var result = objAccountDAO.UpdateUserSignaturePassword(account);
                return true;
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi cập nhật thông tin người dùng");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                throw objEx;
            }
            finally
            {
            }
        }

        #region Signature
        public bool AddSignature(SignatureBO signature)
        {
            try
            {
                AccountDAO objAccountDAO = new AccountDAO();
                return objAccountDAO.AddSignature(signature);
            }
            catch (Exception objEx)
            {
                string msg = "Lỗi cập nhật chữ ký";
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, msg);
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                throw objEx;
            }
        }

        public List<SignatureBO> GetSignatures(SignatureBO signature)
        {
            try
            {
                AccountDAO accountDAO = new AccountDAO();
                return accountDAO.GetSignatures(signature);

            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi lấy danh sách tài liệu");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                return new List<SignatureBO>();
            }
        }

        #endregion
        /// <summary>
        /// Gửi email
        /// </summary>
        /// <param name="emailDataList"></param>
        /// <returns></returns>
        public bool AddEmail(List<EmailDataBO> emailDataList)
        {
            IData objIData;
            if (objDataAccess == null)
                objIData = Data.CreateData(ConfigHelper.Instance.GetConnectionStringDS(), false);
            else
                objIData = objDataAccess;
            try
            {
                if (objDataAccess == null)
                    objIData.BeginTransaction();
                AccountDAO objAccountDAO = new AccountDAO(objDataAccess);
                foreach (var email in emailDataList)
                {
                    objAccountDAO.AddEmail(email);
                }
                if (objDataAccess == null)
                    objIData.CommitTransaction();
                return true;
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Gửi email không thành công!");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                if (objDataAccess == null)
                    objIData.RollBackTransaction();
                return false;
            }
            finally
            {
                if (objDataAccess == null)
                    if (objIData != null)
                        objIData.Disconnect();
            }
        }

        /// <summary>
        /// Cập nhật trạng thái email
        /// </summary>
        /// <param name="emailData"></param>
        /// <returns></returns>
        public bool UpdateEmail(EmailDataBO emailData)
        {
            try
            {
                AccountDAO objAccountDAO = new AccountDAO();
                return objAccountDAO.UpdateEmail(emailData);
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi cập nhật!");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                return false;
            }
        }

        /// <summary>
        /// Lấy thông tin các hoạt động của người dùng khi đăng nhập vào hệ thống
        /// </summary>
        /// <param name="itemPerPage"></param>
        /// <param name="offset"></param>
        /// <param name="userID"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <returns></returns>
        public List<LTemplateBO> GetLog(int itemPerPage, int offset, int userID, DateTime fromDate, DateTime toDate)
        {
            try
            {
                AccountDAO accountDAO = new AccountDAO();
                var list = new List<LTemplateBO>();
                list = accountDAO.GetLog(itemPerPage, offset, userID, fromDate, toDate);

                return list;
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi lấy danh sách tài liệu");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                return new List<LTemplateBO>();
            }
            finally
            {

            }
        }

        public TemplateBO GetTemplateByMailType(string mailType)
        {
            try
            {
                AccountDAO accountDAO = new AccountDAO();
                var list = new TemplateBO();
                list = accountDAO.GetTemplateByMailType(mailType).FirstOrDefault();

                return list;
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi lấy danh sách tài liệu");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                return new TemplateBO();
            }
            finally
            {

            }
        }

        /// <summary>
        /// Thêm thông báo cho ngươi dùng
        /// </summary>
        /// <param name="notificationDataList"></param>
        /// <returns></returns>
        public bool AddNotification(List<NotificationItemBO> notificationDataList)
        {
            IData objIData;
            if (objDataAccess == null)
                objIData = Data.CreateData(ConfigHelper.Instance.GetConnectionStringDS(), false);
            else
                objIData = objDataAccess;
            try
            {
                if (objDataAccess == null)
                    objIData.BeginTransaction();
                AccountDAO objAccountDAO = new AccountDAO(objDataAccess);
                foreach (var notification in notificationDataList)
                {
                    objAccountDAO.AddNotification(notification);
                }
                return true;
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi cập nhật thông báo!");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                if (objDataAccess == null)
                    objIData.RollBackTransaction();
                return false;
            }
            finally
            {
                if (objDataAccess == null)
                    if (objIData != null)
                        objIData.Disconnect();
            }
        }

        /// <summary>
        /// Lấy thông báo 
        /// </summary>
        /// <param name="itemPerPage"></param>
        /// <param name="offset"></param>
        /// <param name="userID"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <returns></returns>
        public List<NTemplateBO> GetNotification(int itemPerPage, int offset, int userID, DateTime fromDate, DateTime toDate)
        {
            try
            {
                AccountDAO accountDAO = new AccountDAO();
                var list = new List<NTemplateBO>();
                list = accountDAO.GetNotification(itemPerPage, offset, userID, fromDate, toDate);

                return list;
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi lấy danh sách tài liệu");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                return new List<NTemplateBO>();
            }
            finally
            {

            }
        }

        /// <summary>
        /// Cập nhật thông báo
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="isseen"></param>
        /// <param name="isdeleted"></param>
        /// <returns></returns>
        public bool UpdateNotification(int UserId, bool isseen, bool isdeleted)
        {
            try
            {
                AccountDAO objAccountDAO = new AccountDAO();
                return objAccountDAO.UpdateNotification(UserId, isseen, isdeleted);
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi cập nhật!");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                return false;
            }
        }

        /// <summary>
        /// Cập nhật token Firebase
        /// </summary>
        /// <param name="id"></param>
        /// <param name="user_id"></param>
        /// <param name="token"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public bool UpdateTokenFirebase(int user_id, string token, int source)
        {
            try
            {
                AccountDAO objAccountDAO = new AccountDAO();
                return objAccountDAO.UpdateTokenFirebase(user_id, token, source);
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi cập nhật token Firebase!");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                return false;
            }
        }

        //public List<NotificationBO> GetNotificationOfUserId(int UserId)
        //{
        //    try
        //    {
        //        AccountDAO accountDAO = new AccountDAO();
        //        var list = new List<NotificationBO>();
        //        list = accountDAO.GetNotificationOfUserId(UserId);

        //        return list;
        //    }
        //    catch (Exception objEx)
        //    {
        //        this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi lấy danh sách tài liệu");
        //        objResultMessageBO = ConfigHelper.Instance.WriteLog(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
        //        return new List<NotificationBO>();
        //    }
        //    finally
        //    {

        //    }
        //}

        //public EmailDataBO GetEmailByLatestID()
        //{
        //    try
        //    {
        //        AccountDAO accountDAO = new AccountDAO();
        //        var list = new EmailDataBO();
        //        list = accountDAO.GetEmailByLatestID();

        //        return list;
        //    }
        //    catch (Exception objEx)
        //    {
        //        this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi lấy danh sách tài liệu");
        //        objResultMessageBO = ConfigHelper.Instance.WriteLog(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
        //        return new EmailDataBO();
        //    }
        //    finally
        //    {

        //    }
        //}
        //Thêm và hiệu chỉnh doanh nghiệp

        public List<AccountBO> GetCustomerInfo(int itemPerPage, int offset)
        {
            try
            {
                AccountDAO companyDAO = new AccountDAO();
                var list = new List<AccountBO>();
                list = companyDAO.GetCustomerInfo(itemPerPage, offset);

                return list;
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi lấy danh sách tài liệu");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                return new List<AccountBO>();
            }
            finally
            {

            }
        }

        public List<AccountBO> GetCompanyBySearching(int itemPerPage, int offset, string input)
        {
            try
            {
                AccountDAO companyDAO = new AccountDAO();
                var list = new List<AccountBO>();
                list = companyDAO.GetCompanyBySearching(itemPerPage, offset, input);
                return list;
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi lấy danh sách khách hàng");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                return new List<AccountBO>();
            }
        }

        /// <summary>
        /// Lấy danh sách gói hợp đồng theo user 
        /// </summary>
        /// <param name="itemPerPage"></param>
        /// <param name="offset"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        public List<PackageBO> GetPackageByUserId(int user_id, int page_size, int offset)
        {
            try
            {
                AccountDAO companyDAO = new AccountDAO();
                var list = new List<PackageBO>();
                list = companyDAO.GetPackageByUserId(user_id, page_size, offset);
                return list;
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi lấy danh sách gói sử dụng");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                return new List<PackageBO>();
            }
        }

        public List<AccountBO> GetUserById(string userID)
        {
            try
            {
                AccountDAO companyDAO = new AccountDAO();
                var list = new List<AccountBO>();
                list = companyDAO.GetUserById(userID);

                return list;
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi lấy danh sách tài liệu");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                return new List<AccountBO>();
            }
            finally
            {

            }
        }

        public bool AddingCompany(AccountBO comf)
        {
            try
            {
                AccountDAO companyDAO = new AccountDAO();
                return companyDAO.AddingCompany(comf);
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi thêm doanh nghiệp");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                return false;
            }
        }

        /// <summary>
        /// Cập nhật thông tin thành viên của người dùng đang đăng nhập
        /// </summary>
        /// <param name="company"></param>
        /// <returns></returns>
        public bool UpdateUser(AccountBO company)
        {
            try
            {
                if (!company.ISUSEHSM)
                {
                    company.APIURL = null;
                    company.APIID = null;
                    company.SECRET = null;
                }
                AccountDAO companyDAO = new AccountDAO();
                return companyDAO.UpdateUser(company);
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi cập nhật thông tin doanh nghiệp");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                return false;
            }
        }

        public bool UpdateUserActive(AccountBO company)
        {
            try
            {
                AccountDAO companyDAO = new AccountDAO();
                return companyDAO.UpdateUserActive(company);
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi cập nhật thông tin doanh nghiệp");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accountBO"></param>
        /// <returns></returns>
        public string GetStringToken(AccountBO accountBO)
        {
            try
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(accountBO, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore
                });
                return TokenUtility.EncryptToken(json);
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi tạo token");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                throw;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public AccountBO GetAccountByToken(string json)
        {
            AccountBO currentUser;
            try
            {
                if (string.IsNullOrEmpty(json))
                    return null;
                currentUser = JsonConvert.DeserializeObject<AccountBO>(json, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore
                });
                AccountBLL objAccountBLL = new AccountBLL();
                var resultCurrentUser = objAccountBLL.UserLogin(currentUser);
                if (objAccountBLL.ResultMessageBO.IsError)
                {
                    ConfigHelper.Instance.WriteLogString(objAccountBLL.ResultMessageBO.Message, objAccountBLL.ResultMessageBO.MessageDetail, MethodBase.GetCurrentMethod().Name, "UserLogin");
                    return null;
                }
                if (resultCurrentUser == null)
                {
                    return null;
                }
                else
                {
                    //mật khẩu lấy từ db so với mk lưu token
                    if (currentUser.PASSWORD != resultCurrentUser.PASSWORD)
                        return null;
                    if (resultCurrentUser.ISDELETED || !resultCurrentUser.ISACTIVED)
                        return null;
                }
                currentUser = resultCurrentUser;
            }
            catch (Exception)
            {
                return null;
            }
            return currentUser;
        }

        /// <summary>
        /// Gửi lời mời thêm thành viên
        /// </summary>
        /// <param name="employees"></param>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        #region Owner
        public bool InviteUserByOwner(List<AccountBO> employees, AccountBO currentUser)
        {
            IData objIData;
            if (objDataAccess == null)
                objIData = Data.CreateData(ConfigHelper.Instance.GetConnectionStringDS(), false);
            else
                objIData = objDataAccess;
            try
            {
                if (objDataAccess == null)
                    objIData.BeginTransaction();
                AccountDAO objAccountDAO = new AccountDAO(objIData);
                foreach (var emp in employees)
                {
                    emp.EMAIL = emp.EMAIL.ToLower();
                    emp.CREATEDBYUSER = currentUser.ID;
                    emp.CREATEDBYIP = MethodHelper.GetIPClient();
                    emp.TAXCODE = currentUser.TAXCODE;
                    //Nếu được sử dụng HSM
                    if (emp.ISUSEHSM)
                    {
                        emp.APIURL = currentUser.APIURL;
                        emp.APIID = currentUser.APIID;
                        emp.SECRET = currentUser.SECRET;
                    }
                    var result = objAccountDAO.InviteUserByOwner(emp);
                    if (result > 0)
                    {
                        EmailDataBO emailData = new EmailDataBO()
                        {
                            EmailType = Constants.TEMPLATE_MAILTYPE_INVITATION,
                            CreatedByEmail = currentUser.EMAIL.ToLower(),
                            CreatedByUser = currentUser.ID,
                            CreatedByIP = MethodHelper.GetIPClient(),
                            FromEmail = "onfinance@novaon.asia",
                            FromName = "OnSign",
                            MailTo = emp.EMAIL.ToLower(),
                            MailName = emp.FULLNAME,
                            Subject = string.Format(Constants.INVITE_SUBJECT, currentUser.FULLNAME),
                            DocumentLinkViewer = GenerateLinkInvitation(result, currentUser.ID, currentUser.FULLNAME, currentUser.EMAIL, emp.EMAIL),
                            DocumentMessage = string.Format(Constants.INVITE_MESSAGE, currentUser.FULLNAME, currentUser.EMAIL).Replace("@", "&#64;").Replace(".", "&#46;")
                        };
                        objAccountDAO.AddEmail(emailData);
                    }
                }
                if (objDataAccess == null)
                    objIData.CommitTransaction();
                return true;
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi thêm thành viên");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                if (objDataAccess == null)
                    objIData.RollBackTransaction();
                throw objEx;
            }
            finally
            {
                if (objDataAccess == null)
                    if (objIData != null)
                        objIData.Disconnect();
            }
        }

        /// <summary>
        /// Gửi lời mời thông qua việc gửi email và đồng thời cập nhật quyền của người được mời ở trạng thái chờ xác nhận (fasle)
        /// </summary>
        /// <param name="emp"></param>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        public bool InviteEmployeeByOwner(AccountBO emp, AccountBO currentUser)
        {
            IData objIData;
            if (objDataAccess == null)
                objIData = Data.CreateData(ConfigHelper.Instance.GetConnectionStringDS(), false);
            else
                objIData = objDataAccess;
            try
            {
                if (objDataAccess == null)
                    objIData.BeginTransaction();

                AccountDAO accountDAO = new AccountDAO(objIData);

                emp.EMAIL = emp.EMAIL.ToLower();
                emp.TAXCODE = currentUser.TAXCODE;
                emp.CREATEDBYUSER = currentUser.ID;
                emp.CREATEDBYIP = MethodHelper.GetIPClient();

                var resultInvitation = accountDAO.InviteUserByOwner(emp);

                EmailDataBO emailData = new EmailDataBO()
                {
                    EmailType = Constants.TEMPLATE_MAILTYPE_INVITATION,
                    CreatedByEmail = currentUser.EMAIL.ToLower(),
                    CreatedByUser = currentUser.ID,
                    CreatedByIP = MethodHelper.GetIPClient(),
                    FromEmail = "onfinance@novaon.asia",
                    FromName = "OnSign",
                    MailTo = emp.EMAIL.ToLower(),
                    MailName = emp.FULLNAME,
                    Subject = string.Format(Constants.INVITE_SUBJECT, currentUser.FULLNAME),
                    DocumentLinkViewer = GenerateLinkInvitation(resultInvitation, currentUser.ID, currentUser.FULLNAME, currentUser.EMAIL, emp.EMAIL),
                    DocumentMessage = string.Format(Constants.INVITE_MESSAGE, currentUser.FULLNAME, currentUser.EMAIL).Replace("@", "&#64;").Replace(".", "&#46;")
                };
                accountDAO.AddEmail(emailData);


                if (objDataAccess == null)
                    objIData.CommitTransaction();
                return true;
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi lấy danh sách thành viên");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                if (objDataAccess == null)
                    objIData.RollBackTransaction();
                return false;
            }
            finally
            {
                if (objDataAccess == null)
                    if (objIData != null)
                        objIData.Disconnect();
            }
        }

        /// <summary>
        /// lấy danh sách thành viên
        /// </summary>
        /// <param name="itemPerPage"></param>
        /// <param name="offset"></param>
        /// <param name="owner"></param>
        /// <returns></returns>
        public List<AccountBO> GetUserByOwner(int itemPerPage, int offset, int owner)
        {
            try
            {
                AccountDAO companyDAO = new AccountDAO();
                var list = new List<AccountBO>();
                list = companyDAO.GetUserByOwner(itemPerPage, offset, owner);
                return list;
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi lấy danh sách thành viên");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                return new List<AccountBO>();
            }
        }

        public AccountBO GetOwnerByUser(int user_id, string taxcode)
        {
            try
            {
                AccountDAO accountDAO = new AccountDAO();
                var result = accountDAO.GetOwnerByUser(user_id, taxcode);
                return result;
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi lấy người quản lý");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                throw objEx;
            }
        }

        #endregion Owner


        #region Permission
        /// <summary>
        /// Lấy danh sách quyền trên hệ thống
        /// </summary>
        /// <returns></returns>
        public List<PermissionGroupBO> GetPermissionGroup()
        {
            try
            {
                AccountDAO AccountDAO = new AccountDAO();
                var list = AccountDAO.GetAllPermission();

                return list.GroupBy(x => x.PERMISSIONGROUPID)
                    .Select(x => new PermissionGroupBO
                    {
                        ID = x.Key,
                        PERMISSIONGROUPNAME = x.First().PERMISSIONGROUPNAME,
                        PERMISSIONLIST = x.ToList()
                    }).ToList();
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi lấy danh sách quyền");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                return new List<PermissionGroupBO>();
            }
        }

        /// <summary>
        /// lấy list quyền của user
        /// </summary>
        /// <param name="invitationID">id của lời mời</param>
        /// <param name="createdbyuser">id người tạo</param>
        /// <returns></returns>
        public List<PermissionUserBO> GetPermissionForUser(int invitationID, int createdbyuser)
        {
            try
            {
                AccountDAO AccountDAO = new AccountDAO();
                return AccountDAO.GetPermissionForUser(invitationID, createdbyuser);
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi lấy danh sách quyền");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                return new List<PermissionUserBO>();
            }
        }

        /// <summary>
        /// thêm quyền cho user
        /// </summary>
        /// <param name="permissionUsers"></param>
        /// <returns></returns>
        public bool AddingPermissionForUser(List<PermissionUserBO> permissionUsers)
        {
            try
            {
                AccountDAO AccountDAO = new AccountDAO();
                foreach (var per in permissionUsers)
                {
                    var result = AccountDAO.AddingPermissionForUser(per);
                }
                return true;
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi thêm quyền người dùng!");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                throw objEx;
            }
        }

        /// <summary>
        /// chấp nhận quyền cho user
        /// </summary>
        /// <param name="invitationId">id của lời mời</param>
        /// <param name="createdByUser">id user khởi tạo</param>
        /// <returns></returns>
        public ObjectResult AcceptPermissionForUser(dynamic invite)
        {
            ObjectResult objectResult = new ObjectResult();
            IData objIData;
            if (objDataAccess == null)
                objIData = Data.CreateData(ConfigHelper.Instance.GetConnectionStringDS(), false);
            else
                objIData = objDataAccess;
            try
            {
                if (objDataAccess == null)
                    objIData.BeginTransaction();
                AccountDAO accountDAO = new AccountDAO(objIData);

                //Lấy thông tin lời mời
                AccountBO resultInvitaion = accountDAO.GetInvitaionForUser(invite.invitationid, invite.emailto);
                if (resultInvitaion.ISACCEPTED)
                {
                    if (objDataAccess == null)
                        objIData.CommitTransaction();
                    objectResult.rs = false;
                    objectResult.msg = $"Lời mời này đã được bạn chấp nhận trước đó!";
                    return objectResult;
                }

                if (resultInvitaion.ISDELETED)
                {
                    if (objDataAccess == null)
                        objIData.CommitTransaction();
                    objectResult.rs = false;
                    objectResult.msg = $"Lời mời này đã hết hiệu lực!";
                    return objectResult;
                }
                //Kiểm tra user này đã tồn tại trên hệ thống hay chưa, nếu chưa thì tạo mới
                var checkLogin = accountDAO.Login(new AccountBO() { USERNAME = resultInvitaion.EMAIL }).FirstOrDefault();
                //Nếu user chưa tồn tại trên hệ thống thì khởi tạo user
                if (checkLogin == null)
                {
                    string randPassword = MethodHelper.RandomString(12);
                    var user = new AccountBO
                    {
                        EMAIL = resultInvitaion.EMAIL,
                        INVITATIONID = resultInvitaion.ID,
                        TAXCODE = resultInvitaion.TAXCODE,
                        ISUSEHSM = resultInvitaion.ISUSEHSM,
                        FULLNAME = resultInvitaion.FULLNAME,
                        OWNER = resultInvitaion.CREATEDBYUSER,
                        CREATEDBYIP = MethodHelper.GetIPClient(),
                        CREATEDBYUSER = resultInvitaion.CREATEDBYUSER,
                        PASSWORD = MethodHelper.StringEncryptPassword(randPassword, ConfigHelper.KeyEncryptPassword, resultInvitaion.CREATEDBYUSER.ToString())
                    };
                    //Tạo mới user
                    var resultCreateUser = accountDAO.CreateUser(user);

                    //Gửi email chào mừng tới hệ thống và cấp password
                    EmailDataBO emailData = new EmailDataBO
                    {
                        Content = randPassword,
                        MailTo = resultInvitaion.EMAIL,
                        MailName = resultInvitaion.FULLNAME,
                        Subject = "Chào mừng bạn đến với OnSign",
                        CreatedByEmail = invite.createdbyuseremail,
                        CreatedByIP = MethodHelper.GetIPClient(),
                        EmailType = Constants.TEMPLATE_MAILTYPE_FORGOT_PASSWORD,
                    };
                    var sendEmail = accountDAO.AddEmail(emailData);
                }
                else
                {
                    //Cập nhật user
                    var user = new AccountBO
                    {
                        ID = checkLogin.ID,
                        EMAIL = resultInvitaion.EMAIL,
                        INVITATIONID = resultInvitaion.ID,
                        ISUSEHSM = resultInvitaion.ISUSEHSM,
                        OWNER = resultInvitaion.CREATEDBYUSER,
                        TAXCODE = resultInvitaion.TAXCODE,
                    };
                    //Cập nhật lại thông tin user
                    var resultCreateUser = accountDAO.UpdateUserInvitaion(user);
                }

                //Chấp nhận lời mời
                var acceptInvitation = new PermissionUserBO()
                {
                    INVITATIONID = resultInvitaion.ID,
                    EMAIL = resultInvitaion.EMAIL,
                    CREATEDBYUSER = resultInvitaion.CREATEDBYUSER
                };

                var resltAcceptInvitation = accountDAO.AcceptInvitationForUser(acceptInvitation);

                //Chấp nhận quyền
                //var resltAcceptPermission = accountDAO.AcceptPermissionForUser(acceptInvitation);

                if (objDataAccess == null)
                    objIData.CommitTransaction();
                objectResult.rs = true;
                return objectResult;
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi thêm quyền người dùng!");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                if (objDataAccess == null)
                    objIData.RollBackTransaction();
                throw objEx;
            }
            finally
            {
                if (objDataAccess == null)
                    if (objIData != null)
                        objIData.Disconnect();
            }
        }

        public bool UpdatePermission(string email)
        {
            try
            {
                AccountDAO AccountDAO = new AccountDAO();
                return AccountDAO.UpdatePermission(email);
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi cập nhật!");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                return false;
            }
        }

        #endregion Permission
        #endregion Methods
    }
}

