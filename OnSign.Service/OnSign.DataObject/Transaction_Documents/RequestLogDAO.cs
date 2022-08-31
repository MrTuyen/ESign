using OnSign.BusinessObject.Notifications;
using OnSign.BusinessObject.Transaction_Documents;
using SAB.Library.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnSign.DataObject.Transaction_Documents
{
    public class RequestLogDAO : BaseDAO
    {
        public RequestLogDAO() : base()
        {
        }

        public RequestLogDAO(IData objIData)
            : base(objIData)
        {
        }

        public bool Request_Log_Add(RequestLogBO requestLog)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_transaction_request.pm_log_add");
                objIData.AddParameter("p_id_request", requestLog.ID_REQUEST);
                objIData.AddParameter("p_uuid", requestLog.UUID);
                objIData.AddParameter("p_created_by_ip", requestLog.CREATED_BY_IP);
                objIData.AddParameter("p_created_by_user", requestLog.CREATED_BY_USER);
                objIData.AddParameter("p_action", requestLog.ACTION);
                objIData.AddParameter("p_messages", requestLog.MESSAGES);
                objIData.AddParameter("p_type", requestLog.TYPE);
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
    }
}
