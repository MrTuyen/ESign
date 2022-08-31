using OnSign.BusinessObject.Email;
using OnSign.BusinessObject.Forms;
using OnSign.BusinessObject.Sign;
using OnSign.Common.Helpers;
using SAB.Library.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OnSign.DataObject.Document
{
    public class DocumentDAO : BaseDAO
    {
        #region Constructor

        public DocumentDAO() : base()
        {
        }

        public DocumentDAO(IData objIData)
            : base(objIData)
        {
        }

        #endregion

        #region Methods
        public int RequestAdd(RequestSignBO requestSign)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.pm_request_add");
                objIData.AddParameter("p_createdbyuser", requestSign.CREATEDBYUSER);
                objIData.AddParameter("p_status", requestSign.STATUS);
                objIData.AddParameter("p_isneedtosign", requestSign.ISONLYMESIGN);
                objIData.AddParameter("p_createdbyip", requestSign.CREATEDBYIP);
                objIData.AddParameter("p_fullname", requestSign.FULLNAME);
                objIData.AddParameter("p_email", requestSign.EMAIL);
                objIData.AddParameter("p_emailsubject", requestSign.EMAILSUBJECT);
                objIData.AddParameter("p_emailmessages", requestSign.EMAILMESSAGES);
                objIData.AddParameter("p_emailto", requestSign.EMAILTO);
                objIData.AddParameter("p_emailname", requestSign.EMAILNAME);
                objIData.AddParameter("p_uuid", requestSign.UUID);
                objIData.AddParameter("p_category", requestSign.CATEGORY);
                objIData.AddParameter("p_repeateverytime", requestSign.REPEATEVERYTIME);
                objIData.AddParameter("p_deadlinetime", requestSign.DEADLINETIME);
                objIData.AddParameter("p_taxcode", requestSign.TAXCODE);
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
                DisconnectIData(objIData);
            }
        }

        public long SaveDraft(RequestSignBO requestSign)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.pm_request_update");
                objIData.AddParameter("p_id", requestSign.ID);
                objIData.AddParameter("p_updatedbyuser", requestSign.CREATEDBYUSER);
                objIData.AddParameter("p_isneedtosign", requestSign.ISONLYMESIGN);
                objIData.AddParameter("p_email", requestSign.EMAIL);
                objIData.AddParameter("p_fullname", requestSign.FULLNAME);
                objIData.AddParameter("p_emailsubject", requestSign.EMAILSUBJECT);
                objIData.AddParameter("p_emailmessages", requestSign.EMAILMESSAGES);
                objIData.AddParameter("p_emailto", requestSign.EMAILTO);
                objIData.AddParameter("p_emailname", requestSign.EMAILNAME);
                objIData.AddParameter("p_status", requestSign.STATUS);
                objIData.AddParameter("p_createdbyip", requestSign.CREATEDBYIP);
                objIData.AddParameter("p_uuid", requestSign.UUID);
                objIData.AddParameter("p_category", requestSign.CATEGORY);
                objIData.AddParameter("p_repeateverytime", requestSign.REPEATEVERYTIME);
                objIData.AddParameter("p_deadlinetime", requestSign.DEADLINETIME);
                var idRequest = objIData.ExecStoreToString();
                CommitTransactionIfAny(objIData);
                return Convert.ToInt64(idRequest);
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

        public bool RequestDelete(RequestSignBO requestSign)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.pm_request_delete");
                objIData.AddParameter("p_id", requestSign.ID);
                objIData.AddParameter("p_createdbyuser", requestSign.CREATEDBYUSER);
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

        public bool CancelRequest(RequestSignBO requestSign)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.pm_request_cancel");
                objIData.AddParameter("p_id", requestSign.ID);
                objIData.AddParameter("p_createdbyuser", requestSign.CREATEDBYUSER);
                objIData.AddParameter("p_iscanceled", requestSign.ISCANCELED);
                objIData.AddParameter("p_cancelreason", requestSign.CANCELREASON);
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

        public bool ReceiveAdd(ReceiverBO mailTo)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.pm_receive_add");
                objIData.AddParameter("p_idrequest", mailTo.IDREQUEST);
                objIData.AddParameter("p_name", mailTo.NAME);
                objIData.AddParameter("p_email", mailTo.EMAIL);
                objIData.AddParameter("p_iscc", mailTo.ISCC);
                objIData.AddParameter("p_uuid", mailTo.UUID);
                objIData.AddParameter("p_createdbyip", mailTo.CREATEDBYIP);
                objIData.AddParameter("p_createdbyuser", mailTo.CREATEDBYUSER);
                objIData.AddParameter("p_isccfinish", mailTo.ISCCFINISH);
                objIData.AddParameter("p_requestsigntype", mailTo.REQUESTSIGNTYPE);

                objIData.AddParameter("p_taxcode", mailTo.TAXCODE);
                objIData.AddParameter("p_address", mailTo.ADDRESS);
                objIData.AddParameter("p_phonenumber", mailTo.PHONENUMBER);
                objIData.AddParameter("p_idnumber", mailTo.IDNUMBER);

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

        public bool ReceiveDelete(RequestSignBO requestSign)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.pm_receive_delete");
                objIData.AddParameter("p_idrequest", requestSign.ID);
                objIData.AddParameter("p_createdbyuser", requestSign.CREATEDBYUSER);
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

        public int DocumentAdd(DocumentBO document)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.pm_document_add");
                objIData.AddParameter("p_idrequest", document.IDREQUEST);
                objIData.AddParameter("p_name", document.NAME);
                objIData.AddParameter("p_pdfpath", document.PATH);
                objIData.AddParameter("p_size", document.SIZE);
                objIData.AddParameter("p_iconpath", document.ICON);
                objIData.AddParameter("p_pages", document.PAGES);
                objIData.AddParameter("p_uuid", document.UUID);
                objIData.AddParameter("p_createdbyip", document.CREATEDBYIP);
                objIData.AddParameter("p_createdbyuser", document.CREATEDBYUSER);
                objIData.AddParameter("p_status", document.STATUS);
                objIData.AddParameter("p_viewer_pages_size", document.VIEWER_PAGES_SIZE);
                int idDoc = int.Parse(objIData.ExecStoreToString());
                CommitTransactionIfAny(objIData);
                return idDoc;
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

        public bool DocumentDelete(RequestSignBO requestSign)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.pm_document_delete");
                objIData.AddParameter("p_idrequest", requestSign.ID);
                objIData.AddParameter("p_createdbyuser", requestSign.CREATEDBYUSER);
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

        public int DocumentSignAdd(DocumentSignBO documentSign)
        {
            IData objIData = this.CreateIData();
            try
            {
                string cer = null;
                if (documentSign.CERINFOBO != null && !string.IsNullOrEmpty(documentSign.CERINFOBO.CERINFO))
                    cer = documentSign.CERINFOBO.CERINFO;
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.pm_document_sign_add");
                objIData.AddParameter("p_iddoc", documentSign.IDDOC);
                objIData.AddParameter("p_typesign", documentSign.TYPESIGN);
                objIData.AddParameter("p_pagesign", documentSign.PAGESIGN);
                objIData.AddParameter("p_xpoint", documentSign.XPOINT);
                objIData.AddParameter("p_ypoint", documentSign.YPOINT);
                objIData.AddParameter("p_signaturewidth", documentSign.SIGNATUREWIDTH);
                objIData.AddParameter("p_signatureheight", documentSign.SIGNATUREHEIGHT);
                objIData.AddParameter("p_signaturepath", documentSign.SIGNATUREPATH);
                objIData.AddParameter("p_signaturetext", cer);
                objIData.AddParameter("p_pdfheight", documentSign.PDFHEIGHT);
                objIData.AddParameter("p_pdfwidth", documentSign.PDFWIDTH);
                objIData.AddParameter("p_uuid", documentSign.UUID);
                objIData.AddParameter("p_emailassignment", documentSign.EMAILASSIGNMENT);
                objIData.AddParameter("p_issigned", documentSign.ISSIGNED);
                objIData.AddParameter("p_signindex", documentSign.SIGNINDEX);
                objIData.AddParameter("p_createdbyip", documentSign.CREATEDBYIP);
                objIData.AddParameter("p_createdbyuser", documentSign.CREATEDBYUSER);
                objIData.AddParameter("p_isinitial", documentSign.ISINITIAL);


                //objIData.AddParameter("p_isinitial", documentSign.ISINITIAL);



                var id = objIData.ExecStoreToString();
                CommitTransactionIfAny(objIData);
                return Convert.ToInt32(id);
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

        public bool DocumentSignDelete(DocumentBO documentSign)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.pm_document_sign_delete");
                objIData.AddParameter("p_iddoc", documentSign.ID);
                objIData.AddParameter("p_createdbyuser", documentSign.CREATEDBYUSER);
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

        public bool UpdateRequestSign(RequestSignBO request)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.pm_request_update_sign");
                objIData.AddParameter("p_id", request.ID);
                objIData.AddParameter("p_status", request.STATUS);
                objIData.AddParameter("p_createdbyuser", request.CREATEDBYUSER);
                objIData.AddParameter("p_declinereason", request.DECLINEREASON);
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

        public bool UpdateDocumentSign(DocumentSignBO documentSign)
        {
            IData objIData = this.CreateIData();
            try
            {
                string signaturePath = "";
                if (!string.IsNullOrEmpty(documentSign.SIGNATUREPATH))
                    signaturePath = documentSign.SIGNATUREPATH;
                else if (!string.IsNullOrEmpty(documentSign.SIGNATUREIMAGE))
                    signaturePath = documentSign.SIGNATUREIMAGE;
                if (signaturePath.Contains("?v="))
                {
                    signaturePath = signaturePath.Replace(ConfigHelper.RootURL, "").Replace(ConfigHelper.RootFolder, "").Replace(ConfigHelper.DocumentRootFolder, "").Replace("//", "/");
                    signaturePath = MethodHelper.BetweenStrings(signaturePath, "", "?v=");
                }
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.pm_document_sign_update_sign");
                objIData.AddParameter("p_id", documentSign.ID);
                objIData.AddParameter("p_issigned", documentSign.ISSIGNED);
                objIData.AddParameter("p_signedbyip", documentSign.SIGNEDBYIP);
                objIData.AddParameter("p_isdeclined", documentSign.ISDECLINED);
                objIData.AddParameter("p_declinebyip", documentSign.DECLINEBYIP);

                objIData.AddParameter("p_cerstartdate", documentSign.CERINFOBO.CERSTARTDATE);
                objIData.AddParameter("p_cerenddate", documentSign.CERINFOBO.CERENDDATE);
                objIData.AddParameter("p_supplier", documentSign.CERINFOBO.SUPPLIER);
                objIData.AddParameter("p_suppliername", documentSign.CERINFOBO.SUPPLIERNAME);
                objIData.AddParameter("p_taxcode", documentSign.CERINFOBO.TAXCODE);
                objIData.AddParameter("p_company", documentSign.CERINFOBO.COMPANY);
                objIData.AddParameter("p_serial", documentSign.CERINFOBO.SERIAL);
                objIData.AddParameter("p_cerinfo", documentSign.CERINFOBO.CERINFO);
                objIData.AddParameter("p_isuseusbtoken", documentSign.CERINFOBO.ISUSEUSBTOKEN);
                objIData.AddParameter("p_signaturepath", signaturePath);

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

        public List<RequestSignBO> GetRequestDashBoard(FormSearch form)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.pm_request_get_dashboard");
                objIData.AddParameter("p_createdbyuser", form.CREATED_BY_USER);
                objIData.AddParameter("p_emailto", form.EMAILTO);
                objIData.AddParameter("p_fromdate", form.FROMDATE);
                objIData.AddParameter("p_todate", form.TODATE);
                var reader = objIData.ExecStoreToDataReader();
                var list = new List<RequestSignBO>();
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

        public List<RequestSignBO> SearchDoccumentByKeywork(int pagesize, int offset, int createdbyUser, string status, string keywork)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.get_by_searching_doccument_manager");
                objIData.AddParameter("p_pagesize", pagesize);
                objIData.AddParameter("p_offset", offset);
                objIData.AddParameter("p_createdbyuser", createdbyUser);
                objIData.AddParameter("p_status", status);
                objIData.AddParameter("p_keywork", keywork);
                var reader = objIData.ExecStoreToDataReader();
                var list = new List<RequestSignBO>();
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

        public List<RequestSignBO> GetRequest(FormSearch form)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.pm_request_get");
                objIData.AddParameter("p_createdbyuser", form.CREATED_BY_USER);
                objIData.AddParameter("p_emailto", form.EMAILTO);
                objIData.AddParameter("p_status", form.STATUS);
                objIData.AddParameter("p_fromdate", form.FROMDATE);
                objIData.AddParameter("p_todate", form.TODATE);
                objIData.AddParameter("p_pagesize", form.ITEMPERPAGE);
                objIData.AddParameter("p_offset", form.OFFSET);
                objIData.AddParameter("p_keyword", form.KEYWORDS ?? "");
                objIData.AddParameter("p_filterstatus", form.FILTERSTATUS);
                objIData.AddParameter("p_filtercategory", form.FILTERCATEGORY);
                var reader = objIData.ExecStoreToDataReader();
                var list = new List<RequestSignBO>();
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

        public List<RequestSignBO> Get_Request_Assign_To_Me(FormSearch form)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.pm_request_get");
                objIData.AddParameter("p_createdbyuser", form.CREATED_BY_USER);
                objIData.AddParameter("p_emailto", form.EMAILTO);
                objIData.AddParameter("p_status", form.STATUS);
                objIData.AddParameter("p_fromdate", form.FROMDATE);
                objIData.AddParameter("p_todate", form.TODATE);
                objIData.AddParameter("p_pagesize", form.ITEMPERPAGE);
                objIData.AddParameter("p_offset", form.OFFSET);
                objIData.AddParameter("p_keyword", form.KEYWORDS ?? "");
                objIData.AddParameter("p_filterstatus", form.FILTERSTATUS);
                objIData.AddParameter("p_filtercategory", form.FILTERCATEGORY);
                var reader = objIData.ExecStoreToDataReader();
                var list = new List<RequestSignBO>();
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



        public RequestSignBO GetRequestById(FormSearch form)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.pm_request_get_by_id");
                objIData.AddParameter("p_id", form.ID);
                objIData.AddParameter("p_createdbyuser", form.CREATED_BY_USER);
                var reader = objIData.ExecStoreToDataReader();
                var list = new List<RequestSignBO>();
                ConvertToObject(reader, list);
                reader.Close();
                CommitTransactionIfAny(objIData);
                var result = new RequestSignBO();
                if (list.Count > 0)
                    result = list.FirstOrDefault();
                return result;
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


        public RequestSignBO GetRequestByUuid(FormSearch form)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.pm_request_get_by_uuid");
                objIData.AddParameter("p_uuid", form.UUID);
                objIData.AddParameter("p_createdbyuser", form.CREATED_BY_USER);
                var reader = objIData.ExecStoreToDataReader();
                var list = new List<RequestSignBO>();
                ConvertToObject(reader, list);
                reader.Close();
                CommitTransactionIfAny(objIData);
                var result = new RequestSignBO();
                if (list.Count > 0)
                    result = list.FirstOrDefault();
                return result;
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

        public List<RequestSignBO> GetRequestByListId(string requestIds)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.pm_request_get_by_list_id");
                objIData.AddParameter("p_ids", requestIds);
                var reader = objIData.ExecStoreToDataReader();
                var list = new List<RequestSignBO>();
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

        public List<DocumentBO> GetDocumentsByIdRequest(FormSearch form)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.pm_document_get_by_idrequest");
                objIData.AddParameter("p_idrequest", form.ID);
                var reader = objIData.ExecStoreToDataReader();
                var list = new List<DocumentBO>();
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

        public List<DocumentSignBO> GetDocumentSignByIdDoc(long idDoc)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.pm_document_sign_get_by_iddoc");
                objIData.AddParameter("p_iddoc", idDoc);
                var reader = objIData.ExecStoreToDataReader();
                var list = new List<DocumentSignBO>();
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

        public List<ReceiverBO> GetReceiveByIdRequest(FormSearch form)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.pm_receive_get");
                objIData.AddParameter("p_idrequest", form.ID);
                var reader = objIData.ExecStoreToDataReader();
                var list = new List<ReceiverBO>();
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


        public List<RequestSignBO> GetRequestReSend(FormSearch form)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.pm_request_get_resend");
                objIData.AddParameter("p_pagesize", form.ITEMPERPAGE);
                objIData.AddParameter("p_offset", form.OFFSET);
                var reader = objIData.ExecStoreToDataReader();
                var list = new List<RequestSignBO>();
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

        public List<EmailDataBO> GetSendFailed(FormSearch form)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.pm_request_get_sendfailed");
                objIData.AddParameter("p_pagesize", form.ITEMPERPAGE);
                objIData.AddParameter("p_offset", form.OFFSET);
                var reader = objIData.ExecStoreToDataReader();
                var list = new List<EmailDataBO>();
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

        public bool UpdateStatusDocument(DocumentBO document)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.pm_document_update_status");
                objIData.AddParameter("p_id", document.ID);
                objIData.AddParameter("p_uuid", document.UUID);
                objIData.AddParameter("p_status", document.STATUS);
                objIData.AddParameter("p_pages", document.PAGES);
                objIData.AddParameter("p_viewer_pages_size", document.VIEWER_PAGES_SIZE);
                var reader = objIData.ExecNonQuery();
                CommitTransactionIfAny(objIData);
                return true;
            }
            catch (Exception objEx)
            {
                string msg = "Lỗi chuyển đổi định dạng file tải lên sang file PDF";
                ConfigHelper.Instance.WriteLogException(msg, objEx, MethodBase.GetCurrentMethod().Name, "UploadFiles");
                RollBackTransactionIfAny(objIData);
                throw objEx;
            }
            finally
            {
                DisconnectIData(objIData);
            }
        }

        #endregion

        #region New Schema
        public RequestSignBO GetDetailRequest_New(FormSearch form)
        {
            var listRequest = new List<RequestSignBO>();
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_request.pm_request_get_by_id");
                objIData.AddParameter("p_id", form.ID);
                var reader = objIData.ExecStoreToDataReader();
                //var s = objIData.ExecStoreToString();
                ConvertToObject(reader, listRequest);
                reader.Close();
                CommitTransactionIfAny(objIData);
                return listRequest?.FirstOrDefault();
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

        #endregion


    }
}
