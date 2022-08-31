using OnSign.BusinessLogic.BusinessObjects;
using OnSign.BusinessObject.Document;
using OnSign.Common;
using OnSign.Common.Helpers;
using OnSign.DataObject.Document;
using SAB.Library.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OnSign.BusinessLogic.Document
{
    public class VerifyCodeBLL : BaseBLL
    {
        protected IData objDataAccess = null;

        public VerifyCodeBLL()
        {
        }

        public VerifyCodeBLL(IData objIData)
        {
            objDataAccess = objIData;
        }

        /// <summary>
        /// Thêm OTP
        /// </summary>
        /// <param name="verifyCode"></param>
        /// <returns></returns>
        public ObjectResult AddCodeOTP(VerifyCodeBO verifyCode)
        {
            string msg = "Thêm mới thành công";
            ObjectResult _ObjectResult = new ObjectResult
            {
                rs = true,
                msg = msg
            };
            try
            {

                //Lấy ra OPT gần nhất gọi thành công
                var _LastCode = this.GetCodeOTP(verifyCode)?.OrderByDescending(x => x.ID).Where(x => x.IS_CALLED).FirstOrDefault();

                if (_LastCode != null)
                {
                    //Kiểm tra lời gửi gần nhất được gửi lúc nào (mắc định là khóa trong vòng 2 phút)
                    var deadline = _LastCode.CREATED_AT_TIME.AddMinutes(2);
                    if (deadline > DateTime.Now)
                    {
                        TimeSpan value = deadline.Subtract(DateTime.Now);
                        msg = $"Vui lòng thử lại sau {(int)value.TotalSeconds} giây";
                        return _ObjectResult = new ObjectResult
                        {
                            rs = false,
                            msg = msg,
                            idrequest = (int)value.TotalSeconds
                        };
                    }
                }

                if (verifyCode.TYPE == 1)
                {
                    return _ObjectResult = new ObjectResult
                    {
                        rs = true,
                        type = verifyCode.TYPE,
                        msg = $"+{verifyCode.PHONE_NUMBER}"
                    };
                }
                verifyCode.CODE = MethodHelper.RandomNumber(6);
                VerifyCodeDAO verifyCodeDAO = new VerifyCodeDAO();
                var result = verifyCodeDAO.AddCodeOTP(verifyCode);
                return _ObjectResult;
            }
            catch (Exception objEx)
            {
                msg = "Lỗi thêm Code OTP";
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, msg);
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                _ObjectResult = new ObjectResult
                {
                    rs = false,
                    msg = msg
                };
                return _ObjectResult;
            }
        }

        /// <summary>
        /// Xác thực với mã OTP
        /// </summary>
        /// <param name="verifyCode"></param>
        /// <returns></returns>
        public ObjectResult AuthenticateCodeOTP(VerifyCodeBO verifyCode)
        {
            var _objectResult = new ObjectResult()
            {
                rs = true,
                msg = "Thành công"
            };
            try
            {
                VerifyCodeDAO verifyCodeDAO = new VerifyCodeDAO();

                if (verifyCode.TYPE == 1)
                {
                    verifyCode.IS_CALLED = true;
                    verifyCode.IS_ACTIVED = true;
                    bool resultAddCodeSMS = verifyCodeDAO.AddCodeOTP(verifyCode);
                    return _objectResult;
                }
                var listOtp = verifyCodeDAO.GetCodeOTP(verifyCode)?.Where(x => !x.IS_ACTIVED && x.TYPE == 0).Select(y => y.CODE);
                string strListOTP = string.Join(", ", listOtp);
                if (!strListOTP.Contains(verifyCode.CODE))
                {
                    return new ObjectResult()
                    {
                        rs = false,
                        msg = "Mã xác thực không đúng, vui lòng thử lại"
                    };
                }

                var result = verifyCodeDAO.UpdateCallerActived(verifyCode);
                return _objectResult;
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi lấy Code OTP");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                throw objEx;
            }
        }

        /// <summary>
        /// Lấy danh sách các mã OTP được tạo cho user trong 1 ngày và gửi đi cuộc gọi thành công
        /// </summary>
        /// <param name="verifyCode"></param>
        /// <returns></returns>
        public List<VerifyCodeBO> GetCodeOTP(VerifyCodeBO verifyCode)
        {
            try
            {
                VerifyCodeDAO verifyCodeDAO = new VerifyCodeDAO();
                return verifyCodeDAO.GetCodeOTP(verifyCode);
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi lấy Code OTP");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                throw objEx;
            }
        }

        public bool UpdateCallerActived(VerifyCodeBO verifyCode)
        {
            try
            {
                VerifyCodeDAO verifyCodeDAO = new VerifyCodeDAO();
                return verifyCodeDAO.UpdateCallerActived(verifyCode);
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi cập nhật OTP Caller");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                throw objEx;
            }
        }

        public bool UpdateCallerCalled(VerifyCodeBO verifyCode)
        {
            try
            {
                VerifyCodeDAO verifyCodeDAO = new VerifyCodeDAO();
                return verifyCodeDAO.UpdateCallerCalled(verifyCode);
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi cập nhật OTP Caller");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                throw objEx;
            }
        }

    }
}
