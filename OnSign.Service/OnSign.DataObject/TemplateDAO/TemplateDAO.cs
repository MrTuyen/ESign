using OnSign.BusinessObject.Forms;
using OnSign.BusinessObject.TemplateBO;
using SAB.Library.Data;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnSign.DataObject.ImportDAO
{
    public class TemplateDAO : BaseDAO
    {
        #region Constructor

        public TemplateDAO() : base()
        {
        }

        public TemplateDAO(IData objIData)
            : base(objIData)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Thêm mới tài liệu mẫu
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        public long DocumentTemplateAdd(DocumentTemplate document)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_document_template.pm_document_template_add");
                objIData.AddParameter("p_name", document.NAME);
                objIData.AddParameter("p_pdfpath", document.PATH);
                objIData.AddParameter("p_size", document.SIZE);
                objIData.AddParameter("p_iconpath", document.ICON);
                objIData.AddParameter("p_pages", document.PAGES);
                objIData.AddParameter("p_createdbyip", document.CREATEDBYIP);
                objIData.AddParameter("p_createdbyuser", document.CREATEDBYUSER);
                var docid = objIData.ExecStoreToString();
                CommitTransactionIfAny(objIData);
                return Convert.ToInt64(docid);
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

        public long DocumentTemplateDataAdd(DocumentTemplateData data)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_document_template.pm_document_template_data_add");
                objIData.AddParameter("p_docid", data.DOCID);
                objIData.AddParameter("p_name", data.NAME);
                objIData.AddParameter("p_value", data.VALUE);
                objIData.AddParameter("p_pdfid", data.PDFID);
                var dataid = objIData.ExecStoreToString();
                CommitTransactionIfAny(objIData);
                return Convert.ToInt64(dataid);
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

        public bool DocumentTemplateDataDelete(long pdfId)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_document_template.pm_document_template_data_delete");
                objIData.AddParameter("p_pdfid", pdfId);
                objIData.ExecStoreToString();
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

        public bool DocumentTemplateDelete(DocumentTemplate document)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_document_template.pm_document_template_delete");
                objIData.AddParameter("p_id", document.ID);
                objIData.ExecStoreToString();
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

        public List<DocumentTemplateData> Get_DocumentTemplateDataBookmarks_By_PdfId(long pdfid)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_document_template.pm_document_template_data_get_by_pdfid");
                objIData.AddParameter("p_pdfid", pdfid);
                var reader = objIData.ExecStoreToDataReader();
                var list = new List<DocumentTemplateData>();
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
                DisconnectIData(objIData);
            }
        }

        public long DocumentTemplatePdfSignAdd(DocumentTemplatePdfSign sign)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_document_template.pm_document_template_pdf_sign_add");
                objIData.AddParameter("p_pdfid", sign.PDFID);
                objIData.AddParameter("p_typesign", sign.TYPESIGN + string.Empty);
                objIData.AddParameter("p_pagesign", sign.PAGESIGN);
                objIData.AddParameter("p_xpoint", sign.XPOINT);
                objIData.AddParameter("p_ypoint", sign.YPOINT);
                objIData.AddParameter("p_signatureheight", sign.SIGNATUREHEIGHT);
                objIData.AddParameter("p_signaturewidth", sign.SIGNATUREWIDTH);
                objIData.AddParameter("p_pdfheight", sign.PDFHEIGHT);
                objIData.AddParameter("p_pdfwidth", sign.PDFWIDTH);
                objIData.AddParameter("p_signindex", sign.SIGNINDEX);
                objIData.AddParameter("p_isinitial", sign.ISINITIAL);
                var dataid = objIData.ExecStoreToString();
                CommitTransactionIfAny(objIData);
                return Convert.ToInt64(dataid);
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

        public bool DocumentTemplateReceiveAdd(DocumentTemplateReceive receive)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_document_template.pm_document_template_receive_add");
                objIData.AddParameter("p_namefrom", receive.NAMEFROM);
                objIData.AddParameter("p_emailfrom", receive.EMAILFROM);
                objIData.AddParameter("p_requestsigntype", receive.REQUESTSIGNTYPE + string.Empty);
                objIData.AddParameter("p_emailto", receive.EMAILTO);
                objIData.AddParameter("p_nameto", receive.NAMETO);
                objIData.AddParameter("p_taxcode", receive.TAXCODE);
                objIData.AddParameter("p_address", receive.ADDRESS);
                objIData.AddParameter("p_idnumber", receive.IDNUMBER);
                objIData.AddParameter("p_phonenumber", receive.PHONENUMBER);
                objIData.AddParameter("p_subject", receive.SUBJECT);
                objIData.AddParameter("p_content", receive.CONTENT);
                objIData.AddParameter("p_pdfid", receive.PDFID);
                objIData.AddParameter("p_iscompany", receive.ISCOMPANY);
                objIData.AddParameter("p_sent", receive.SENT);
                objIData.AddParameter("p_signindex", receive.SIGNINDEX);
                objIData.AddParameter("p_requestuuid", receive.REQUESTUUID);
                objIData.ExecStoreToString();
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

        public bool DocumentTemplateReceiveUpdate(DocumentTemplateReceive receive)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_document_template.pm_document_template_receive_update");
                objIData.AddParameter("p_id", receive.ID);
                objIData.AddParameter("p_namefrom", receive.NAMEFROM);
                objIData.AddParameter("p_emailfrom", receive.EMAILFROM);
                objIData.AddParameter("p_requestsigntype", receive.REQUESTSIGNTYPE + string.Empty);
                objIData.AddParameter("p_emailto", receive.EMAILTO);
                objIData.AddParameter("p_nameto", receive.NAMETO);
                objIData.AddParameter("p_taxcode", receive.TAXCODE);
                objIData.AddParameter("p_address", receive.ADDRESS);
                objIData.AddParameter("p_idnumber", receive.IDNUMBER);
                objIData.AddParameter("p_phonenumber", receive.PHONENUMBER);
                objIData.AddParameter("p_subject", receive.SUBJECT);
                objIData.AddParameter("p_content", receive.CONTENT);
                objIData.AddParameter("p_pdfid", receive.PDFID);
                objIData.AddParameter("p_iscompany", receive.ISCOMPANY);
                objIData.AddParameter("p_sent", receive.SENT);
                objIData.AddParameter("p_signindex", receive.SIGNINDEX);
                objIData.AddParameter("p_requestuuid", receive.REQUESTUUID);
                objIData.ExecStoreToString();
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

        public bool DocumentTemplateReceiveDelete(DocumentTemplateReceive receive)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_document_template.pm_document_template_receive_delete");
                objIData.AddParameter("p_id", receive.ID);
                objIData.ExecStoreToString();
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

        public bool DocumentTemplateReceiveDeleteByRequestUuid(string requestUuid)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_document_template.pm_document_template_receive_delete_by_requestuuid");
                objIData.AddParameter("p_requestuuid", requestUuid);
                objIData.ExecStoreToString();
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

        public List<DocumentTemplateReceive> DocumentTemplateReceiveGetByPdfID(long pdfId)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_document_template.pm_document_template_receive_get_by_pdfid");
                objIData.AddParameter("p_pdfid", pdfId);
                var reader = objIData.ExecStoreToDataReader();
                var list = new List<DocumentTemplateReceive>();
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
                DisconnectIData(objIData);
            }
        }

        public List<DocumentTemplateReceive> DocumentTemplateReceiveGetByRequestUuid(string requestUuid)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_document_template.pm_document_template_receive_get_by_requestuuid");
                objIData.AddParameter("p_requestuuid", requestUuid);
                var reader = objIData.ExecStoreToDataReader();
                var list = new List<DocumentTemplateReceive>();
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
                DisconnectIData(objIData);
            }
        }

        public long DocumentTemplatePdfAdd(DocumentTemplatePdf pdf)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_document_template.pm_document_template_pdf_add");
                objIData.AddParameter("p_createdbyuser", pdf.CREATEDBYUSER);
                objIData.AddParameter("p_createdbyip", pdf.CREATEDBYIP);
                objIData.AddParameter("p_idcontract", pdf.IDCONTRACT);
                objIData.AddParameter("p_name", pdf.NAME);
                objIData.AddParameter("p_icon", pdf.ICON);
                objIData.AddParameter("p_doctemplateid", pdf.DOCTEMPLATEID);
                objIData.AddParameter("p_path", pdf.PATH);
                objIData.AddParameter("p_size", pdf.SIZE);
                objIData.AddParameter("p_pages", pdf.PAGES);
                objIData.AddParameter("p_status", pdf.STATUS);
                var pdfid = objIData.ExecStoreToString();
                CommitTransactionIfAny(objIData);
                return Convert.ToInt64(pdfid);
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

        public void DocumentTemplatePdfUpdate(DocumentTemplatePdf pdf)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_document_template.pm_document_template_pdf_update");
                objIData.AddParameter("p_id", pdf.ID);
                objIData.AddParameter("p_name", pdf.NAME);
                objIData.AddParameter("p_icon", pdf.ICON);
                objIData.AddParameter("p_path", pdf.PATH);
                objIData.AddParameter("p_completepercent", pdf.COMPLETEPERCENT);
                objIData.AddParameter("p_idcontract", pdf.IDCONTRACT);
                objIData.AddParameter("p_size", pdf.SIZE);
                objIData.AddParameter("p_pages", pdf.PAGES);
                objIData.AddParameter("p_status", pdf.STATUS);
                objIData.ExecStoreToString();
                CommitTransactionIfAny(objIData);
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

        public bool DocumentTemplatePdfDeleteByDocId(long docid)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_document_template.pm_document_template_pdf_delete_by_docid");
                objIData.AddParameter("p_docid", docid);
                objIData.ExecStoreToString();
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

        public bool DocumentTemplatePdfDelete(DocumentTemplatePdf pdf)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_document_template.pm_document_template_pdf_delete");
                objIData.AddParameter("p_id", pdf.ID);
                objIData.ExecStoreToString();
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

        public DocumentTemplate DocumentTemplateGetByID(long id)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_document_template.pm_document_template_get_by_id");
                objIData.AddParameter("p_id", id);
                var reader = objIData.ExecStoreToDataReader();
                var list = new List<DocumentTemplate>();
                ConvertToObject(reader, list);
                reader.Close();
                CommitTransactionIfAny(objIData);
                return list.FirstOrDefault();
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

        public DocumentTemplatePdf DocumentTemplatePdfGetByID(long pdfid)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_document_template.pm_document_template_pdf_get_by_id");
                objIData.AddParameter("p_id", pdfid);
                var reader = objIData.ExecStoreToDataReader();
                var list = new List<DocumentTemplatePdf>();
                ConvertToObject(reader, list);
                reader.Close();
                CommitTransactionIfAny(objIData);
                return list.FirstOrDefault();
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

        public List<DocumentTemplatePdfSign> DocumentTemplatePdfSignGetByPdfID(long pdfid)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_document_template.pm_document_template_pdf_sign_get_by_pdfid");
                objIData.AddParameter("p_pdfid", pdfid);
                var reader = objIData.ExecStoreToDataReader();
                var list = new List<DocumentTemplatePdfSign>();
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
                DisconnectIData(objIData);
            }
        }

        public List<DocumentTemplatePdf> DocumentTemplatePdfGetByDoctemplateID(long docId)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_document_template.pm_document_template_pdf_get_by_doctemplateid");
                objIData.AddParameter("p_doctemplateid", docId);
                var reader = objIData.ExecStoreToDataReader();
                var list = new List<DocumentTemplatePdf>();
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
                DisconnectIData(objIData);
            }
        }

        public List<DocumentTemplatePdf> DocumentTemplatePdfGetStatusFalse(long docId)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_document_template.pm_document_template_pdf_get_status_false");
                objIData.AddParameter("p_doctemplateid", docId);
                var reader = objIData.ExecStoreToDataReader();
                var list = new List<DocumentTemplatePdf>();
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
                DisconnectIData(objIData);
            }
        }

        public DocumentTemplatePdf DocumentTemplatePdfGetByIdContract(string idcontract)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_document_template.pm_document_template_pdf_get_by_idcontract");
                objIData.AddParameter("p_idcontract", idcontract);
                var reader = objIData.ExecStoreToDataReader();
                var list = new List<DocumentTemplatePdf>();
                ConvertToObject(reader, list);
                reader.Close();
                CommitTransactionIfAny(objIData);
                return list.FirstOrDefault();
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

        public List<DocumentTemplatePdf> DocumentTemplatePdfGetByDoctemplateIDPagging(FormSearch form)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_document_template.pm_document_template_pdf_get_by_docid_pagging");
                objIData.AddParameter("p_pagesize", form.ITEMPERPAGE);
                objIData.AddParameter("p_offset", form.OFFSET);
                objIData.AddParameter("p_doctemplateid", form.DOCID);
                var reader = objIData.ExecStoreToDataReader();
                var list = new List<DocumentTemplatePdf>();
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
                DisconnectIData(objIData);
            }
        }

        /// <summary>
        /// Lấy danh sách mẫu tài liệu theo người đăng nhập
        /// </summary>
        /// <param name="formSearch"></param>
        /// <returns></returns>
        public List<DocumentTemplate> DocumentTemplateGetByUser(FormSearch formSearch)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_document_template.pm_document_template_get_pagging");
                objIData.AddParameter("p_pagesize", formSearch.ITEMPERPAGE);
                objIData.AddParameter("p_offset", formSearch.OFFSET);
                objIData.AddParameter("p_userid", formSearch.CREATED_BY_USER);
                var reader = objIData.ExecStoreToDataReader();
                var list = new List<DocumentTemplate>();
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
                DisconnectIData(objIData);
            }
        }

        public long DocumentTemplateBookmarkAdd(DocumentTemplateBookmark bookmark)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_document_template.pm_document_template_bookmard_add");
                objIData.AddParameter("p_doctemplateid", bookmark.DOCTEMPLATEID);
                objIData.AddParameter("p_bookname", bookmark.NAME);
                var docid = objIData.ExecStoreToString();
                CommitTransactionIfAny(objIData);
                return Convert.ToInt64(docid);
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

        public bool DocumentTemplateBookmarkDeleteByDocID(long docid)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_document_template.pm_document_template_bookmark_delete_by_docid");
                objIData.AddParameter("p_doctemplateid", docid);
                objIData.ExecStoreToString();
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
        /// Lấy thông tin bookmarks của file word mẫu
        /// </summary>
        /// <param name="docid"></param>
        /// <returns></returns>
        public List<DocumentTemplateBookmark> DocumentTemplateBookmarkGetByDocID(long docid)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_document_template.pm_document_template_bookmark_get_by_docid");
                objIData.AddParameter("p_doctemplateid", docid);
                var reader = objIData.ExecStoreToDataReader();
                var list = new List<DocumentTemplateBookmark>();
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
                DisconnectIData(objIData);
            }
        }

        #endregion
    }
}
