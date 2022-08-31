using OnSign.BusinessObject.Account;
using OnSign.BusinessObject.Company;
using OnSign.BusinessObject.Email;
using OnSign.BusinessObject.Forms;
using OnSign.BusinessObject.Notifications;
using OnSign.BusinessObject.Permission;
using SAB.Library.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OnSign.DataObject.Account
{
    public class AccountDAO : BaseDAO
    {
        #region Constructor

        public AccountDAO() : base()
        {
        }

        public AccountDAO(IData objIData)
            : base(objIData)
        {
        }

        #endregion

        #region Methods
        /// <summary>
        /// Thêm mới người dùng
        /// </summary>
        /// <param name="reg"></param>
        /// <returns></returns>
        public bool CreateUser(AccountBO reg)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.system_user_add");
                objIData.AddParameter("p_fullname", reg.FULLNAME);
                objIData.AddParameter("p_email", reg.EMAIL);
                objIData.AddParameter("p_password", reg.PASSWORD);
                objIData.AddParameter("p_createdbyuser", reg.CREATEDBYUSER);
                objIData.AddParameter("p_createdbyip", reg.CREATEDBYIP);

                objIData.AddParameter("p_address", reg.ADDRESS);
                objIData.AddParameter("p_taxcode", reg.TAXCODE);
                objIData.AddParameter("p_phone", reg.PHONE);
                objIData.AddParameter("p_position", reg.POSITION);
                objIData.AddParameter("p_isadmin", reg.ISADMIN);
                objIData.AddParameter("p_isusehsm", reg.ISUSEHSM);
                objIData.AddParameter("p_apiid", reg.APIID);
                objIData.AddParameter("p_secret", reg.SECRET);

                objIData.AddParameter("p_owner", reg.OWNER);
                objIData.AddParameter("p_invitaionid", reg.INVITATIONID);

                var reader = objIData.ExecNonQuery();
                CommitTransactionIfAny(objIData);

                return true;
            }
            catch (Exception objEx)
            {
                RollBackTransactionIfAny(objIData);
                throw objEx;
            }
            finally
            {
                this.DisconnectIData(objIData);
            }
        }

        public bool CreatePackage(PackageBO package)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.pm_package_add");
                objIData.AddParameter("p_packagename", package.PACKAGE_NAME);
                objIData.AddParameter("p_userid", package.USER_ID);
                objIData.AddParameter("p_starttime", package.USING_FROM_DATE);
                objIData.AddParameter("p_endtime", package.USING_TO_DATE);
                objIData.AddParameter("p_createdbyuser", package.CREATEDBYUSER);
                var reader = objIData.ExecNonQuery();
                CommitTransactionIfAny(objIData);
                return true;
            }
            catch (Exception objEx)
            {
                RollBackTransactionIfAny(objIData);
                throw objEx;
            }
            finally
            {
                this.DisconnectIData(objIData);
            }
        }

        public bool DeletePackage(PackageBO package)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.pm_package_delete");
                objIData.AddParameter("p_id", package.ID);
                var reader = objIData.ExecNonQuery();
                CommitTransactionIfAny(objIData);
                return true;
            }
            catch (Exception objEx)
            {
                RollBackTransactionIfAny(objIData);
                throw objEx;
            }
            finally
            {
                this.DisconnectIData(objIData);
            }
        }

        public bool UpdatePackage(PackageBO package)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.pm_package_update");
                objIData.AddParameter("p_id", package.ID);
                objIData.AddParameter("p_createdbyip", package.CREATEDBYIP);
                objIData.AddParameter("p_createdbyuser", package.CREATEDBYUSER);
                var reader = objIData.ExecNonQuery();
                CommitTransactionIfAny(objIData);
                return true;
            }
            catch (Exception objEx)
            {
                RollBackTransactionIfAny(objIData);
                throw objEx;
            }
            finally
            {
                this.DisconnectIData(objIData);
            }
        }

        /// <summary>
        /// Kiểm tra thông tin đăng nhập để xác định có tồn tại người dùng trong hệ thống không
        /// </summary>
        /// <param name="reg"></param>
        /// <returns></returns>
        public List<AccountBO> Login(AccountBO reg)
        {

            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.system_user_login");
                objIData.AddParameter("p_username", reg.USERNAME?.ToLower().Trim());
                var reader = objIData.ExecStoreToDataReader();
                var list = new List<AccountBO>();
                ConvertToObject(reader, list);
                reader.Close();
                CommitTransactionIfAny(objIData);
                return list;
            }
            catch (Exception objEx)
            {
                RollBackTransactionIfAny(objIData);
                throw objEx;
            }
            finally
            {
                this.DisconnectIData(objIData);
            }
        }

        /// <summary>
        /// Cập nhật thời điểm gần nhất của người dùng truy cập vào hệ thống
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="IPRemote"></param>
        /// <returns></returns>
        public bool UpdateTimeOfUserAccessing(int UserId, string IPRemote)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.system_user_update_activity");
                objIData.AddParameter("p_user_id", UserId);
                objIData.AddParameter("p_createdbyip", IPRemote);
                var reader = objIData.ExecNonQuery();
                CommitTransactionIfAny(objIData);

                return true;
            }
            catch (Exception objEx)
            {
                RollBackTransactionIfAny(objIData);
                throw objEx;
            }
            finally
            {
                this.DisconnectIData(objIData);
            }
        }
        //Quên mật khẩu -> Tạo ra 1 mật khẩu tạm lưu vào pm_customer_password

        public bool ForgotPassword(AccountBO reg)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.system_user_password_add");
                objIData.AddParameter("p_email", reg.EMAIL);
                objIData.AddParameter("p_password", reg.PASSWORD);
                var reader = objIData.ExecNonQuery();
                CommitTransactionIfAny(objIData);
                return true;
            }
            catch (Exception objEx)
            {
                RollBackTransactionIfAny(objIData);
                throw objEx;
            }
            finally
            {
                DisconnectIData(objIData);
            }
        }

        public List<TemplateBO> GetTemplateByMailType(string mailType)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.pm_templates_get_by_mailtype");
                objIData.AddParameter("p_mailtype", mailType);
                var reader = objIData.ExecStoreToDataReader();
                var list = new List<TemplateBO>();
                ConvertToObject(reader, list);
                reader.Close();
                CommitTransactionIfAny(objIData);
                return list;
            }
            catch (Exception objEx)
            {
                RollBackTransactionIfAny(objIData);
                throw objEx;
            }
            finally
            {
                this.DisconnectIData(objIData);
            }
        }

        /// <summary>
        /// Lấy thông tin các hoạt động của người dùng đang truy cập vào hệ thống
        /// </summary>
        /// <param name="itemPerPage"></param>
        /// <param name="offset"></param>
        /// <param name="userID"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <returns></returns>
        public List<LTemplateBO> GetLog(int itemPerPage, int offset, int userID, DateTime fromDate, DateTime toDate)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.pm_logs_get");
                objIData.AddParameter("p_pagesize", itemPerPage);
                objIData.AddParameter("p_offset", offset);
                objIData.AddParameter("p_createdbyuser", userID);
                objIData.AddParameter("p_fromdate", fromDate);
                objIData.AddParameter("p_todate", toDate);
                var reader = objIData.ExecStoreToDataReader();
                var list = new List<LTemplateBO>();
                ConvertToObject(reader, list);
                reader.Close();
                CommitTransactionIfAny(objIData);
                return list;
            }
            catch (Exception objEx)
            {
                RollBackTransactionIfAny(objIData);
                throw objEx;
            }
            finally
            {
                this.DisconnectIData(objIData);
            }
        }

        /// <summary>
        /// Cập nhật chữ ký và mật khẩu của người dùng đăng nhập
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public bool UpdateUserSignaturePassword(AccountBO account)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.system_user_update_signature_password");
                objIData.AddParameter("p_id", account.ID);
                objIData.AddParameter("p_password", account.PASSWORD);
                objIData.AddParameter("p_signatureimage", account.SIGNATUREIMAGE);

                var reader = objIData.ExecNonQuery();
                CommitTransactionIfAny(objIData);

                return true;
            }
            catch (Exception objEx)
            {
                RollBackTransactionIfAny(objIData);
                throw objEx;
            }
            finally
            {
                this.DisconnectIData(objIData);
            }
        }

        #region Signature
        public bool AddSignature(SignatureBO signature)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.pm_signatures_add");
                objIData.AddParameter("p_id", signature.ID);
                objIData.AddParameter("p_path", signature.PATH);
                objIData.AddParameter("p_createdbyuser", signature.CREATEDBYUSER);
                objIData.AddParameter("p_createdbyip", signature.CREATEDBYIP);
                objIData.AddParameter("p_uuid", signature.UUID);
                objIData.AddParameter("p_isdeleted", signature.ISDELETED);
                var reader = objIData.ExecNonQuery();
                CommitTransactionIfAny(objIData);
                return true;
            }
            catch (Exception objEx)
            {
                RollBackTransactionIfAny(objIData);
                throw objEx;
            }
            finally
            {
                this.DisconnectIData(objIData);
            }
        }

        public List<SignatureBO> GetSignatures(SignatureBO signature)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.pm_signatures_get");
                objIData.AddParameter("p_createdbyuser", signature.CREATEDBYUSER);
                var reader = objIData.ExecStoreToDataReader();
                var list = new List<SignatureBO>();
                ConvertToObject(reader, list);
                reader.Close();
                CommitTransactionIfAny(objIData);
                return list;
            }
            catch (Exception objEx)
            {
                RollBackTransactionIfAny(objIData);
                throw objEx;
            }
            finally
            {
                this.DisconnectIData(objIData);
            }
        }
        #endregion Signature

        #endregion
        /// <summary>
        /// Gửi email
        /// </summary>
        /// <param name="emailData"></param>
        /// <returns></returns>
        public bool AddEmail(EmailDataBO emailData)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.pm_email_add");
                objIData.AddParameter("p_emailtype", emailData.EmailType);
                objIData.AddParameter("p_createdbyemail", emailData.CreatedByEmail);
                objIData.AddParameter("p_createdbyip", emailData.CreatedByIP);
                objIData.AddParameter("p_fromemail", emailData.FromEmail);
                objIData.AddParameter("p_fromname", emailData.FromName);
                objIData.AddParameter("p_mailto", emailData.MailTo);
                objIData.AddParameter("p_mailname", emailData.MailName);
                objIData.AddParameter("p_cc", emailData.ISCC);
                objIData.AddParameter("p_subject", emailData.Subject);
                objIData.AddParameter("p_messages", emailData.Messages);
                objIData.AddParameter("p_content", emailData.Content);
                objIData.AddParameter("p_documentlinkviewer", emailData.DocumentLinkViewer);
                objIData.AddParameter("p_documentlinklogo", emailData.DocumentLinkLogo);
                objIData.AddParameter("p_documentmessage", emailData.DocumentMessage);
                objIData.AddParameter("p_createdbyuser", emailData.CreatedByUser);
                objIData.AddParameter("p_uuid", emailData.UUID);
                objIData.AddParameter("p_isresend", emailData.IsReSend);
                var reader = objIData.ExecNonQuery();
                CommitTransactionIfAny(objIData);
                return true;
            }
            catch (Exception objEx)
            {
                RollBackTransactionIfAny(objIData);
                throw objEx;
            }
            finally
            {
                DisconnectIData(objIData);
            }
        }

        /// <summary>
        /// Cập nhật trạng thái của email
        /// </summary>
        /// <param name="emailData"></param>
        /// <returns></returns>
        public bool UpdateEmail(EmailDataBO emailData)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.pm_email_update");
                objIData.AddParameter("p_id", emailData.ID);
                objIData.AddParameter("p_issent", emailData.IsSent);
                objIData.AddParameter("p_opened", emailData.IsOpened);
                var reader = objIData.ExecNonQuery();
                CommitTransactionIfAny(objIData);
                return true;
            }
            catch (Exception objEx)
            {
                RollBackTransactionIfAny(objIData);
                throw objEx;
            }
            finally
            {
                DisconnectIData(objIData);
            }
        }

        /// <summary>
        /// Thêm thông báo đến người dùng 
        /// </summary>
        /// <param name="notificationData"></param>
        /// <returns></returns>
        public bool AddNotification(NotificationItemBO notificationData)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.pm_notification_add");
                objIData.AddParameter("p_createdbyuser", notificationData.CREATEDBYUSER);
                objIData.AddParameter("p_createdbyip", notificationData.CREATEDBYIP);
                objIData.AddParameter("p_icon", notificationData.ICON);
                objIData.AddParameter("p_title", notificationData.TITLE);
                objIData.AddParameter("p_messages", notificationData.MESSAGES);
                var reader = objIData.ExecNonQuery();
                CommitTransactionIfAny(objIData);
                return true;
            }
            catch (Exception objEx)
            {
                RollBackTransactionIfAny(objIData);
                throw objEx;
            }
            finally
            {
                DisconnectIData(objIData);
            }
        }

        /// <summary>
        /// Lấy thông tin về các thông báo cho người dùng đăng nhập
        /// </summary>
        /// <param name="itemPerPage"></param>
        /// <param name="offset"></param>
        /// <param name="userID"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <returns></returns>
        public List<NTemplateBO> GetNotification(int itemPerPage, int offset, int userID, DateTime fromDate, DateTime toDate)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.pm_notifications_get");
                objIData.AddParameter("p_pagesize", itemPerPage);
                objIData.AddParameter("p_offset", offset);
                objIData.AddParameter("p_createdbyuser", userID);
                objIData.AddParameter("p_fromdate", fromDate);
                objIData.AddParameter("p_todate", toDate);
                var reader = objIData.ExecStoreToDataReader();
                var list = new List<NTemplateBO>();
                ConvertToObject(reader, list);
                reader.Close();
                CommitTransactionIfAny(objIData);
                return list;
            }
            catch (Exception objEx)
            {
                RollBackTransactionIfAny(objIData);
                throw objEx;
            }
            finally
            {
                this.DisconnectIData(objIData);
            }
        }

        /// <summary>
        /// Cập nhật trạng thái thông báo
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="isseen"></param>
        /// <param name="isdeleted"></param>
        /// <returns></returns>
        public bool UpdateNotification(int UserId, bool isseen, bool isdeleted)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.pm_notification_update");
                objIData.AddParameter("p_createdbyuser", UserId);
                objIData.AddParameter("p_isseen", isseen);
                objIData.AddParameter("p_isdeleted", isdeleted);
                var reader = objIData.ExecNonQuery();
                CommitTransactionIfAny(objIData);
                return true;
            }
            catch (Exception objEx)
            {
                RollBackTransactionIfAny(objIData);
                throw objEx;
            }
            finally
            {
                DisconnectIData(objIData);
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
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_user.pm_user_firebase_update");
                objIData.AddParameter("p_user_id", user_id);
                objIData.AddParameter("p_token_firebase", token);
                objIData.AddParameter("p_source", source);
                var reader = objIData.ExecNonQuery();
                CommitTransactionIfAny(objIData);
                return true;
            }
            catch (Exception objEx)
            {
                RollBackTransactionIfAny(objIData);
                throw objEx;
            }
            finally
            {
                DisconnectIData(objIData);
            }
        }

        /// <summary>
        /// Lấy thông tin khách hàng
        /// </summary>
        /// <param name="itemPerPage"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public List<AccountBO> GetCustomerInfo(int itemPerPage, int offset)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.system_user_get");
                objIData.AddParameter("p_pagesize", itemPerPage);
                objIData.AddParameter("p_offset", offset);
                var reader = objIData.ExecStoreToDataReader();
                var list = new List<AccountBO>();
                ConvertToObject(reader, list);
                reader.Close();
                CommitTransactionIfAny(objIData);
                return list;
            }
            catch (Exception objEx)
            {
                RollBackTransactionIfAny(objIData);
                throw objEx;
            }
            finally
            {
                this.DisconnectIData(objIData);
            }
        }


        /// <summary>
        /// Lấy thông tin doanh nghiệp theo thông tin tìm kiếm
        /// </summary>
        /// <param name="itemPerPage"></param>
        /// <param name="offset"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public List<AccountBO> GetCompanyBySearching(int itemPerPage, int offset, string input)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.system_user_get_by_searching");
                objIData.AddParameter("p_pagesize", itemPerPage);
                objIData.AddParameter("p_offset", offset);
                objIData.AddParameter("p_input", input);
                var reader = objIData.ExecStoreToDataReader();
                var list = new List<AccountBO>();
                ConvertToObject(reader, list);
                reader.Close();
                CommitTransactionIfAny(objIData);
                return list;
            }
            catch (Exception objEx)
            {
                RollBackTransactionIfAny(objIData);
                throw objEx;
            }
            finally
            {
                this.DisconnectIData(objIData);
            }
        }

        /// <summary>
        /// Lấy gói sử dụng của người dùng theo id của người dùng
        /// </summary>
        /// <param name="itemPerPage"></param>
        /// <param name="offset"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        public List<PackageBO> GetPackageByUserId(int itemPerPage, int offset, long userid)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.pm_package_get_by_user_id");
                objIData.AddParameter("p_user_id", userid);
                objIData.AddParameter("p_page_size", itemPerPage);
                objIData.AddParameter("p_offset", offset);
                var reader = objIData.ExecStoreToDataReader();
                var list = new List<PackageBO>();
                ConvertToObject(reader, list);
                reader.Close();
                CommitTransactionIfAny(objIData);
                return list;
            }
            catch (Exception objEx)
            {
                RollBackTransactionIfAny(objIData);
                throw objEx;
            }
            finally
            {
                this.DisconnectIData(objIData);
            }
        }

        /// <summary>
        /// Lấy danh sách thành viên
        /// </summary>
        /// <param name="itemPerPage"></param>
        /// <param name="offset"></param>
        /// <param name="owner"></param>
        /// <returns></returns>
        public List<AccountBO> GetUserByOwner(int itemPerPage, int offset, int owner)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.system_user_get_by_owner");
                objIData.AddParameter("p_pagesize", itemPerPage);
                objIData.AddParameter("p_offset", offset);
                objIData.AddParameter("p_owner", owner);
                var reader = objIData.ExecStoreToDataReader();
                var list = new List<AccountBO>();
                ConvertToObject(reader, list);
                reader.Close();
                CommitTransactionIfAny(objIData);
                return list;
            }
            catch (Exception objEx)
            {
                RollBackTransactionIfAny(objIData);
                throw objEx;
            }
            finally
            {
                this.DisconnectIData(objIData);
            }
        }

        public AccountBO GetOwnerByUser(int user_id, string taxcode)
        {
            var list = new List<AccountBO>();
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.system_user_get_owner");
                objIData.AddParameter("p_id", user_id);
                objIData.AddParameter("p_taxcode", taxcode);
                var reader = objIData.ExecStoreToDataReader();
                ConvertToObject(reader, list);
                reader.Close();
                CommitTransactionIfAny(objIData);
                return list?.FirstOrDefault();
            }
            catch (Exception objEx)
            {
                RollBackTransactionIfAny(objIData);
                throw objEx;
            }
            finally
            {
                this.DisconnectIData(objIData);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="comf"></param>
        /// <returns></returns>
        public bool AddingCompany(AccountBO comf)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.system_user_add_company");
                //objIData.AddParameter("p_id", account.ID);
                objIData.AddParameter("p_taxcode", comf.TAXCODE);
                objIData.AddParameter("p_company", comf.COMPANY);
                objIData.AddParameter("p_address", comf.ADDRESS);
                objIData.AddParameter("p_phone", comf.PHONE);
                objIData.AddParameter("p_fullname", comf.FULLNAME);
                objIData.AddParameter("p_isactived", comf.ISACTIVED);
                objIData.AddParameter("p_createdbyuser", comf.CREATEDBYUSER);
                objIData.AddParameter("p_createdbyip", comf.CREATEDBYIP);
                var reader = objIData.ExecNonQuery();
                CommitTransactionIfAny(objIData);
                return true;
            }
            catch (Exception objEx)
            {
                RollBackTransactionIfAny(objIData);
                throw objEx;
            }
            finally
            {
                DisconnectIData(objIData);
            }
        }

        public List<AccountBO> GetUserById(string userID)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.system_user_get_by_id");
                objIData.AddParameter("p_id", userID);
                var reader = objIData.ExecStoreToDataReader();
                var list = new List<AccountBO>();
                ConvertToObject(reader, list);
                reader.Close();
                CommitTransactionIfAny(objIData);
                return list;
            }
            catch (Exception objEx)
            {
                RollBackTransactionIfAny(objIData);
                throw objEx;
            }
            finally
            {
                this.DisconnectIData(objIData);
            }
        }

        /// <summary>
        /// Cập nhật thông tin của thành viên của người dùng đang đăng nhập
        /// </summary>
        /// <param name="company"></param>
        /// <returns></returns>
        public bool UpdateUser(AccountBO company)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.system_user_update");
                objIData.AddParameter("p_id", company.ID);
                objIData.AddParameter("p_fullname", company.FULLNAME);
                objIData.AddParameter("p_username", company.USERNAME);
                objIData.AddParameter("p_email", company.EMAIL);
                objIData.AddParameter("p_phone", company.PHONE);
                objIData.AddParameter("p_taxcode", company.TAXCODE);
                objIData.AddParameter("p_position", company.POSITION);
                objIData.AddParameter("p_address", company.ADDRESS);
                objIData.AddParameter("p_password", company.PASSWORD);
                objIData.AddParameter("p_isadmin", company.ISADMIN);
                objIData.AddParameter("p_isusehsm", company.ISUSEHSM);
                objIData.AddParameter("p_apiurl", company.APIURL);
                objIData.AddParameter("p_apiid", company.APIID);
                objIData.AddParameter("p_secret", company.SECRET);
                objIData.AddParameter("p_isactived", company.ISACTIVED);

                var reader = objIData.ExecNonQuery();
                CommitTransactionIfAny(objIData);

                return true;
            }
            catch (Exception objEx)
            {
                RollBackTransactionIfAny(objIData);
                throw objEx;
            }
            finally
            {
                this.DisconnectIData(objIData);
            }
        }

        public bool UpdateUserInvitaion(AccountBO user)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.system_user_update_invitation");
                objIData.AddParameter("p_id", user.ID);
                objIData.AddParameter("p_email", user.EMAIL);
                objIData.AddParameter("p_taxcode", user.TAXCODE);
                objIData.AddParameter("p_isusehsm", user.ISUSEHSM);
                objIData.AddParameter("p_apiurl", user.APIURL);
                objIData.AddParameter("p_apiid", user.APIID);
                objIData.AddParameter("p_secret", user.SECRET);
                objIData.AddParameter("p_invitaionid", user.INVITATIONID);
                objIData.AddParameter("p_owner", user.OWNER);

                var reader = objIData.ExecNonQuery();
                CommitTransactionIfAny(objIData);

                return true;
            }
            catch (Exception objEx)
            {
                RollBackTransactionIfAny(objIData);
                throw objEx;
            }
            finally
            {
                this.DisconnectIData(objIData);
            }
        }

        public bool UpdateUserActive(AccountBO company)
        {
            IData objIData = this.CreateIData();
            try
            {

                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.system_user_update_active");
                objIData.AddParameter("p_id", company.ID);


                var reader = objIData.ExecNonQuery();
                CommitTransactionIfAny(objIData);
                return true;
            }
            catch (Exception objEx)
            {
                RollBackTransactionIfAny(objIData);
                throw objEx;
            }
            finally
            {
                this.DisconnectIData(objIData);
            }
        }


        #region Owner
        /// <summary>
        /// Gửi lời mời thêm thành viên
        /// </summary>
        /// <param name="reg"></param>
        /// <returns></returns>
        public int InviteUserByOwner(AccountBO reg)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.pm_invitation_add");
                objIData.AddParameter("p_id", reg.ID);
                objIData.AddParameter("p_createdbyuser", reg.CREATEDBYUSER);
                objIData.AddParameter("p_createdbyip", reg.CREATEDBYIP);
                objIData.AddParameter("p_taxcode", reg.TAXCODE);
                objIData.AddParameter("p_fullname", reg.FULLNAME);
                objIData.AddParameter("p_email", reg.EMAIL);
                objIData.AddParameter("p_isusehsm", reg.ISUSEHSM);
                objIData.AddParameter("p_apiurl", reg.APIURL);
                objIData.AddParameter("p_apiid", reg.APIID);
                objIData.AddParameter("p_secret", reg.SECRET);
                var idRequest = objIData.ExecStoreToString();
                CommitTransactionIfAny(objIData);
                return Convert.ToInt32(idRequest);
            }
            catch (Exception objEx)
            {
                RollBackTransactionIfAny(objIData);
                throw objEx;
            }
            finally
            {
                this.DisconnectIData(objIData);
            }
        }

        //public List<AccountBO> CheckExistInviteUserByOwner(AccountBO reg)
        //{
        //    IData objIData = this.CreateIData();
        //    try
        //    {
        //        //NOT isdeleted, NOT isaccepted
        //        BeginTransactionIfAny(objIData);
        //        objIData.CreateNewStoredProcedure("ds_masterdata.pm_invitation_check_exist");
        //        objIData.AddParameter("p_createdbyuser", reg.CREATEDBYUSER);
        //        objIData.AddParameter("p_email", reg.EMAIL);
        //        var reader = objIData.ExecStoreToDataReader();
        //        var list = new List<AccountBO>();
        //        ConvertToObject(reader, list);
        //        reader.Close();
        //        CommitTransactionIfAny(objIData);
        //        return list;
        //    }
        //    catch (Exception objEx)
        //    {
        //        RollBackTransactionIfAny(objIData);
        //        throw objEx;
        //    }
        //    finally
        //    {
        //        this.DisconnectIData(objIData);
        //    }
        //}
        /// <summary>
        /// Tạo hoặc cập nhật người dùng
        /// </summary>
        /// <param name="reg"></param>
        /// <returns></returns>
        //public bool CreateUserByOwner(AccountBO reg)
        //{
        //    IData objIData = this.CreateIData();
        //    try
        //    {
        //        BeginTransactionIfAny(objIData);
        //        objIData.CreateNewStoredProcedure("ds_masterdata.system_user_add_by_owner");
        //        objIData.AddParameter("p_id", reg.ID);
        //        objIData.AddParameter("p_fullname", reg.FULLNAME);
        //        objIData.AddParameter("p_email", reg.EMAIL);
        //        objIData.AddParameter("p_password", reg.PASSWORD);
        //        objIData.AddParameter("p_createdbyuser", reg.CREATEDBYUSER);
        //        objIData.AddParameter("p_createdbyip", reg.CREATEDBYIP);

        //        objIData.AddParameter("p_taxcode", reg.TAXCODE);
        //        objIData.AddParameter("p_isusehsm", reg.ISUSEHSM);
        //        objIData.AddParameter("p_apiurl", reg.APIURL);
        //        objIData.AddParameter("p_apiid", reg.APIID);
        //        objIData.AddParameter("p_secret", reg.SECRET);
        //        objIData.AddParameter("p_isdeleted", reg.ISDELETED);

        //        var reader = objIData.ExecNonQuery();
        //        CommitTransactionIfAny(objIData);

        //        return true;
        //    }
        //    catch (Exception objEx)
        //    {
        //        RollBackTransactionIfAny(objIData);
        //        throw objEx;
        //    }
        //    finally
        //    {
        //        this.DisconnectIData(objIData);
        //    }
        //}
        /// <summary>
        /// Lấy thông tin nhóm quyền người dùng
        /// </summary>
        /// <returns></returns>
        #region Permission
        public List<PermissionGroupBO> GetPermissionGroup()
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.pm_permission_group_get");
                var reader = objIData.ExecStoreToDataReader();
                var list = new List<PermissionGroupBO>();
                ConvertToObject(reader, list);
                reader.Close();
                CommitTransactionIfAny(objIData);
                return list;
            }
            catch (Exception objEx)
            {
                RollBackTransactionIfAny(objIData);
                throw objEx;
            }
            finally
            {
                this.DisconnectIData(objIData);
            }
        }

        /// <summary>
        /// Lấy thông tin quyền người dùng theo id của nhóm quyền người dùng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<PermissionBO> GetPermissionByPermissionGroupID(int id)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.pm_permission_get_by_permission_group_id");
                objIData.AddParameter("p_permission_group_id", id);
                var reader = objIData.ExecStoreToDataReader();
                var list = new List<PermissionBO>();
                ConvertToObject(reader, list);
                reader.Close();
                CommitTransactionIfAny(objIData);
                return list;
            }
            catch (Exception objEx)
            {
                RollBackTransactionIfAny(objIData);
                throw objEx;
            }
            finally
            {
                this.DisconnectIData(objIData);
            }
        }

        /// <summary>
        /// lấy danh sách quyền
        /// </summary>
        /// <param name="permissionGroupID"></param>
        /// <returns></returns>
        public List<PermissionBO> GetAllPermission(int? permissionGroupID = null)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.pm_permission_getall");
                objIData.AddParameter("p_permission_group_id", permissionGroupID);
                var reader = objIData.ExecStoreToDataReader();
                var list = new List<PermissionBO>();
                ConvertToObject(reader, list);
                reader.Close();
                CommitTransactionIfAny(objIData);
                return list;
            }
            catch (Exception objEx)
            {
                RollBackTransactionIfAny(objIData);
                throw objEx;
            }
            finally
            {
                this.DisconnectIData(objIData);
            }
        }

        /// <summary>
        /// lấy danh sách quyền theo lời mời
        /// </summary>
        /// <param name="invitationID"></param>
        /// <param name="createdbyuser"></param>
        /// <returns></returns>
        public List<PermissionUserBO> GetPermissionForUser(int invitationID, int createdbyuser)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.system_user_permission_get");
                objIData.AddParameter("p_invitationid", invitationID);
                objIData.AddParameter("p_createdbyuser", createdbyuser);
                var reader = objIData.ExecStoreToDataReader();
                var list = new List<PermissionUserBO>();
                ConvertToObject(reader, list);
                reader.Close();
                CommitTransactionIfAny(objIData);
                return list;
            }
            catch (Exception objEx)
            {
                RollBackTransactionIfAny(objIData);
                throw objEx;
            }
            finally
            {
                this.DisconnectIData(objIData);
            }
        }

        /// <summary>
        /// Lấy thông tin lời mời
        /// </summary>
        /// <param name="invitationID"></param>
        /// <param name="createdbyuser"></param>
        /// <returns></returns>
        public AccountBO GetInvitaionForUser(int invitationID, string emailTo)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.pm_invitation_get");
                objIData.AddParameter("p_invitationid", invitationID);
                objIData.AddParameter("p_emailto", emailTo);
                var reader = objIData.ExecStoreToDataReader();
                var list = new List<AccountBO>();
                ConvertToObject(reader, list);
                reader.Close();
                CommitTransactionIfAny(objIData);
                if (list.Count > 0)
                    return list.First();
                return new AccountBO();
            }
            catch (Exception objEx)
            {
                RollBackTransactionIfAny(objIData);
                throw objEx;
            }
            finally
            {
                this.DisconnectIData(objIData);
            }
        }

        /// <summary>
        /// Thêm quyền
        /// </summary>
        /// <param name="reg"></param>
        /// <returns></returns>
        public bool AddingPermissionForUser(PermissionUserBO reg)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.system_user_permission_add");
                objIData.AddParameter("p_createdbyuser", reg.CREATEDBYUSER);
                objIData.AddParameter("p_createdbyip", reg.CREATEDBYIP);
                objIData.AddParameter("p_permissionid", reg.PERMISSIONID);
                objIData.AddParameter("p_email", reg.EMAIL);
                objIData.AddParameter("p_invitationid", reg.INVITATIONID);

                var reader = objIData.ExecNonQuery();
                CommitTransactionIfAny(objIData);
                return true;
            }
            catch (Exception objEx)
            {
                RollBackTransactionIfAny(objIData);
                throw objEx;
            }
            finally
            {
                this.DisconnectIData(objIData);
            }
        }

        /// <summary>
        /// Chấp nhận lời mời thành viên -> chấp nhận quyền
        /// </summary>
        /// <param name="reg"></param>
        /// <returns></returns>
        public bool AcceptInvitationForUser(PermissionUserBO reg)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.pm_invitation_update");
                objIData.AddParameter("p_id", reg.INVITATIONID);
                objIData.AddParameter("p_createdbyuser", reg.CREATEDBYUSER);
                objIData.AddParameter("p_emailto", reg.EMAIL);
                var reader = objIData.ExecNonQuery();
                CommitTransactionIfAny(objIData);
                return true;
            }
            catch (Exception objEx)
            {
                RollBackTransactionIfAny(objIData);
                throw objEx;
            }
            finally
            {
                this.DisconnectIData(objIData);
            }
        }

        /// <summary>
        /// Chấp nhận quyền bởi user
        /// </summary>
        /// <param name="reg"></param>
        /// <returns></returns>
        public bool AcceptPermissionForUser(PermissionUserBO reg)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.system_user_permission_update");
                objIData.AddParameter("p_invitationid", reg.INVITATIONID);
                objIData.AddParameter("p_email", reg.EMAIL);

                var reader = objIData.ExecNonQuery();
                CommitTransactionIfAny(objIData);
                return true;
            }
            catch (Exception objEx)
            {
                RollBackTransactionIfAny(objIData);
                throw objEx;
            }
            finally
            {
                this.DisconnectIData(objIData);
            }
        }

        //Cập nhật quyền người dùng khi chấp nhận lời mời tham gia từ email
        public bool UpdatePermission(string email)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.system_user_permission_update");
                objIData.AddParameter("p_email", email);
                var reader = objIData.ExecNonQuery();
                CommitTransactionIfAny(objIData);
                return true;
            }
            catch (Exception objEx)
            {
                RollBackTransactionIfAny(objIData);
                throw objEx;
            }
            finally
            {
                DisconnectIData(objIData);
            }
        }
        #endregion Permission
        #endregion Owner
    }
}
