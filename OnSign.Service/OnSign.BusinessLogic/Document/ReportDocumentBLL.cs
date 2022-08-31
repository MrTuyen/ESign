using OnSign.BusinessObject.Document;
using OnSign.BusinessObject.Forms;
using OnSign.BusinessObject.Sign;
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
    public class ReportDocumentBLL : BaseBLL
    {
        protected IData objDataAccess = null;

        public ReportDocumentBLL()
        {
        }

        public ReportDocumentBLL(IData objIData)
        {
            objDataAccess = objIData;
        }


        public ReportDocumentBO GetReportRequestFinish(FormSearch form)
        {
            try
            {
                ReportDocumentDAO documentDAO = new ReportDocumentDAO();
                var request_finish = documentDAO.GetReportRequestFinish(form);
                var session_signed = documentDAO.GetReportSessionSigned(form);
                ReportDocumentBO reportDocument = new ReportDocumentBO
                {
                    COMPANY_REQUEST_FINISH = request_finish.COMPANY_REQUEST_FINISH,
                    USER_REQUEST_FINISH = request_finish.USER_REQUEST_FINISH,

                    COMPANY_SESSION_SIGNED = session_signed.COMPANY_SESSION_SIGNED,
                    USER_SESSION_SIGNED = session_signed.USER_SESSION_SIGNED
                };
                return reportDocument;
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi lấy thông tin báo cáo số lượng hợp đồng Hoàn thành");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                throw objEx;
            }
        }

    }
}
