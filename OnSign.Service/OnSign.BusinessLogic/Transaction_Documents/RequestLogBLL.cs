using OnSign.BusinessObject.Transaction_Documents;
using OnSign.Common.Helpers;
using OnSign.DataObject.Transaction_Documents;
using SAB.Library.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OnSign.BusinessLogic.Transaction_Documents
{
    public class RequestLogBLL : BaseBLL
    {
        protected IData objDataAccess = null;
        public RequestLogBLL()
        {
        }

        public RequestLogBLL(IData objIData)
        {
            objDataAccess = objIData;
        }

        public bool Request_Log_Add(RequestLogBO requestLog)
        {
            try
            {
                RequestLogDAO documentDAO = new RequestLogDAO();
                var result = documentDAO.Request_Log_Add(requestLog);
                return true;
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi ghi log request");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                return false;
            }
        }
    }
}
