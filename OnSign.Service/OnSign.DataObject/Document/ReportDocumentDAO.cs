using OnSign.BusinessObject.Document;
using OnSign.BusinessObject.Forms;
using OnSign.BusinessObject.Sign;
using SAB.Library.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnSign.DataObject.Document
{
    public class ReportDocumentDAO : BaseDAO
    {
        public ReportDocumentDAO() : base()
        {
        }

        public ReportDocumentDAO(IData objIData)
            : base(objIData)
        {
        }

        public ReportDocumentBO GetReportRequestFinish(FormSearch form)
        {
            var reportDocuments = new List<ReportDocumentBO>();
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.pm_request_get_finish");
                objIData.AddParameter("p_created_by_user", form.CREATED_BY_USER);
                objIData.AddParameter("p_tax_code", form.TAX_CODE);
                var reader = objIData.ExecStoreToDataReader();
                ConvertToObject(reader, reportDocuments);
                reader.Close();
                CommitTransactionIfAny(objIData);
                return reportDocuments?.FirstOrDefault();
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

        public ReportDocumentBO GetReportSessionSigned(FormSearch form)
        {
            var reportDocuments = new List<ReportDocumentBO>();
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.pm_document_sign_get_signed");
                objIData.AddParameter("p_created_by_user", form.CREATED_BY_USER);
                objIData.AddParameter("p_tax_code", form.TAX_CODE);
                var reader = objIData.ExecStoreToDataReader();
                ConvertToObject(reader, reportDocuments);
                reader.Close();
                CommitTransactionIfAny(objIData);
                return reportDocuments?.FirstOrDefault();
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
