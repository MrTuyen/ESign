using OnSign.BusinessObject.Document;
using SAB.Library.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnSign.DataObject.Document
{
    public class VerifyCodeDAO : BaseDAO
    {
        public VerifyCodeDAO() : base()
        {
        }

        public VerifyCodeDAO(IData objIData)
            : base(objIData)
        {
        }

        /// <summary>
        /// Add OTP vào hệ thống
        /// </summary>
        /// <param name="verifyCode"></param>
        /// <returns></returns>
        public bool AddCodeOTP(VerifyCodeBO verifyCode)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_notification.pm_otp_sign_add");
                objIData.AddParameter("p_id_request", verifyCode.ID_REQUEST);
                objIData.AddParameter("p_uuid", verifyCode.UUID);
                objIData.AddParameter("p_phone_number", verifyCode.PHONE_NUMBER);
                objIData.AddParameter("p_code", verifyCode.CODE);
                objIData.AddParameter("p_created_by_ip", verifyCode.CREATED_BY_IP);
                objIData.AddParameter("p_created_by_user", verifyCode.CREATED_BY_USER);
                objIData.AddParameter("p_created_by_email", verifyCode.CREATED_BY_EMAIL);
                objIData.AddParameter("p_type", verifyCode.TYPE);
                objIData.AddParameter("p_is_called", verifyCode.IS_CALLED);
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
        /// Lấy danh sách code OTP đã gửi
        /// </summary>
        /// <param name="verifyCode"></param>
        /// <returns></returns>
        public List<VerifyCodeBO> GetCodeOTP(VerifyCodeBO verifyCode)
        {
            var _VerifyCodes = new List<VerifyCodeBO>();
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_notification.pm_otp_sign_get");
                objIData.AddParameter("p_id_request", verifyCode.ID_REQUEST);
                objIData.AddParameter("p_uuid", verifyCode.UUID);
                objIData.AddParameter("p_created_by_user", verifyCode.CREATED_BY_USER);
                objIData.AddParameter("p_created_by_email", verifyCode.CREATED_BY_EMAIL);
                var reader = objIData.ExecStoreToDataReader();
                ConvertToObject(reader, _VerifyCodes);
                reader.Close();
                CommitTransactionIfAny(objIData);
                return _VerifyCodes;
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
        /// Cập nhật cuộc gọi từ crm caller
        /// </summary>
        /// <param name="verifyCode"></param>
        /// <returns></returns>
        public bool UpdateCallerActived(VerifyCodeBO verifyCode)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_notification.pm_otp_sign_update_actived");
                objIData.AddParameter("p_code", verifyCode.CODE);
                objIData.AddParameter("p_id_request", verifyCode.ID_REQUEST);
                objIData.AddParameter("p_uuid", verifyCode.UUID);
                objIData.AddParameter("p_created_by_user", verifyCode.CREATED_BY_USER);
                objIData.AddParameter("p_created_by_email", verifyCode.CREATED_BY_EMAIL);
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
        /// Cập nhật cuộc gọi từ crm caller
        /// </summary>
        /// <param name="verifyCode"></param>
        /// <returns></returns>
        public bool UpdateCallerCalled(VerifyCodeBO verifyCode)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_notification.pm_otp_sign_update_called");
                objIData.AddParameter("p_id", verifyCode.ID);
                objIData.AddParameter("p_code", verifyCode.CODE);
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
    }
}