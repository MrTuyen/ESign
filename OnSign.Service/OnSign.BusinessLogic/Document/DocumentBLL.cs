using OnSign.Common.Helpers;
using OnSign.BusinessObject.Account;
using SAB.Library.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OnSign.BusinessObject.Forms;
using OnSign.DataObject.Document;
using System.Threading;
using iTextSharp.text.pdf;
using OnSign.BusinessObject.Email;
using OnSign.Common;
using OnSign.BusinessLogic.Account;
using OnSign.BusinessObject.Output;
using System.IO;
using OnSign.BusinessLogic.Sign;
using OnSign.BusinessLogic.BusinessObjects;
using System.Web;
using System.Dynamic;
using ICSharpCode.SharpZipLib.Zip;
using SAB.Library.Core.FMSCenterService;
using Newtonsoft.Json;
using OnSign.BusinessObject.Sign;
using OnSign.BusinessLogic.Transaction_Documents;
using OnSign.BusinessObject.Transaction_Documents;
using OnSign.BusinessLogic.Partners;
using OnSign.BusinessObject.Partners;
using OnSign.BusinessObject.Document;

namespace OnSign.BusinessLogic.Document
{
    public class DocumentBLL : BaseBLL
    {
        #region Fields
        private readonly AccountBLL accountBLL = new AccountBLL();
        private readonly SignBLL signBLL = new SignBLL();
        private readonly RequestLogBLL logBLL = new RequestLogBLL();

        protected IData objDataAccess = null;
        private const string CERTIFICATE_NAME = "summary.pdf";
        private const string DOCUMENTS = "Documents";
        #endregion

        #region Properties

        #endregion

        #region Constructor
        public DocumentBLL()
        {
        }

        public DocumentBLL(IData objIData)
        {
            objDataAccess = objIData;
        }
        #endregion

        #region Methods

        [Obsolete]
        public dynamic DownloadZipFile(string ids)
        {
            try
            {
                dynamic result = new ExpandoObject();
                int unixTimestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                var fileName = $"OnSign download files {unixTimestamp}.zip";
                var tempOutPutPath = $"{ConfigHelper.RootFolder}{fileName}";

                using (ZipOutputStream s = new ZipOutputStream(File.Create(tempOutPutPath)))
                {
                    s.SetLevel(9);
                    byte[] buffer = new byte[4096];
                    string summaryFileName = string.Empty;
                    foreach (var id in ids.Split(','))
                    {
                        var resultRequest = GetRequestMasterById(new FormSearch() { ID = long.Parse(id) });
                        int count = 0;
                        foreach (var doc in resultRequest.FILEUPLOADS)
                        {
                            count++;
                            string fullPath = $"{ConfigHelper.FullDocument}{doc.PATH}";
                            if (File.Exists(fullPath + Constants.PDF_SIGNED_EXTENSION) && resultRequest.STATUS != RequestStatus.TU_CHOI && !resultRequest.ISDELETED)
                                fullPath += Constants.PDF_SIGNED_EXTENSION;
                            else fullPath += Constants.PDF_EXTENSION;
                            summaryFileName = $"{new FileInfo(fullPath).Directory}\\{CERTIFICATE_NAME}";
                            string docName = $"{resultRequest.ID}#{doc.NAME}";
                            if (doc.NAME.Length > 200)
                            {
                                docName = doc.NAME.Substring(0, 200) + "...";
                            }
                            ZipEntry entry = new ZipEntry($"{docName}.pdf")
                            {
                                DateTime = DateTime.Now,
                                IsUnicodeText = true
                            };
                            s.PutNextEntry(entry);
                            try
                            {
                                AddToZipFile(s, buffer, fullPath);
                            }
                            catch (Exception ex)
                            {
                                ConfigHelper.Instance.WriteLogException("Lỗi tải xuống file", ex, MethodBase.GetCurrentMethod().Name, "DownloadZipFile");
                            }
                        }
                        if (File.Exists(summaryFileName))
                        {
                            s.PutNextEntry(new ZipEntry($"{resultRequest.ID}#{CERTIFICATE_NAME}"));
                            AddToZipFile(s, buffer, summaryFileName);
                        }
                    }
                    s.Finish();
                    s.Flush();
                    s.Close();
                }
                byte[] finalResult = File.ReadAllBytes(tempOutPutPath);
                System.IO.File.Delete(tempOutPutPath);
                if (finalResult == null || !finalResult.Any())
                {
                    throw new Exception(string.Format("No Files found"));
                }

                result.finalResult = finalResult;
                result.contentType = "application/force-download"; ;
                result.fileName = fileName;
                File.Delete(tempOutPutPath);
                return result;
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, objEx.Message);
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                throw;
            }
        }

        public int CreateRequestSign(RequestSignBO requestSign, IData objIData, AccountBO objUser)
        {
            try
            {
                var lstSign = new List<DocumentSignBO>();
                requestSign.FILEUPLOADS.ForEach(doc => doc.SIGN.Where(sign => !sign.ISSIGNED).ToList().ForEach(z => lstSign.Add(z)));

                DocumentDAO documentDAO = new DocumentDAO(objIData);
                //Nếu tồn tại request Nháp trước đó thì xóa đi
                if (requestSign.STATUS == RequestStatus.NHAP)
                {
                    //Tạo ra UUID mới cho request
                    var newUUID = MethodHelper.GenerateUUID();
                    //Xóa request Nháp
                    documentDAO.RequestDelete(new RequestSignBO() { ID = requestSign.ID, CREATEDBYUSER = objUser.ID });

                    //Copy file từ folder Request Draft -> Request Waiting
                    string source = $"{ConfigHelper.FullDocument}\\{requestSign.CREATEDBYUSER}\\{requestSign.UUID}";
                    string new_source = $"{ConfigHelper.FullDocument}\\{requestSign.CREATEDBYUSER}\\{newUUID}";
                    if (!Directory.Exists(new_source))
                        Directory.CreateDirectory(new_source);
                    MethodHelper.CopyFolder(source, new_source);
                    //Gán uuid request = new uuid
                    requestSign.UUID = newUUID;
                    //Đổi đường dẫn file doc.PATH thành đường dẫn mới
                    foreach (var file in requestSign.FILEUPLOADS)
                    {
                        if (!string.IsNullOrEmpty(file.UUID))
                            file.PATH = file.PATH.Replace(file.UUID, newUUID);
                    }
                }

                //Khởi tạo request
                requestSign.STATUS = requestSign.ISONLYMESIGN ? RequestStatus.HOAN_THANH : RequestStatus.CHO_KY;

                //Add vào master (pm_request)
                var idRequest = documentDAO.RequestAdd(requestSign);
                if (requestSign.FILEUPLOADS != null)
                {
                    var emailDatas = new List<EmailDataBO>();
                    var firstSign = lstSign.OrderBy(x => x.SIGNINDEX).FirstOrDefault();

                    //Add vào Tài liệu (pm_document)
                    foreach (var doc in requestSign.FILEUPLOADS)
                    {
                        //Lấy thông tin page viewer size
                        string documentPath = $"{ConfigHelper.FullDocument}{doc.PATH}{Constants.PDF_EXTENSION}";
                        var listViewerSize = new List<ViewerPageSize>();
                        using (PdfReader pdfReader = new PdfReader(documentPath))
                        {
                            PdfReader.unethicalreading = true;
                            doc.PAGES = pdfReader.NumberOfPages;
                            for (int page = 1; page <= doc.PAGES; page++)
                            {
                                var realSize = pdfReader.GetPageSizeWithRotation(page);
                                var viewerPageSize = new ViewerPageSize
                                {
                                    PAGE = page,
                                    SIZE = $"{(int)realSize.Width}x{(int)realSize.Height}"
                                };
                                listViewerSize.Add(viewerPageSize);
                            }
                            doc.VIEWER_PAGES_SIZE = JsonConvert.SerializeObject(listViewerSize, new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore,
                                DefaultValueHandling = DefaultValueHandling.Ignore
                            });
                            pdfReader.Close();
                            pdfReader.Dispose();
                        }
                        doc.IDREQUEST = idRequest;
                        doc.UUID = requestSign.UUID;
                        doc.CREATEDBYUSER = objUser.ID;
                        doc.CREATEDBYIP = MethodHelper.GetIPClient();

                        var idDoc = documentDAO.DocumentAdd(doc);
                        if (doc.SIGN != null)
                        {
                            foreach (var sign in doc.SIGN)
                            {
                                sign.IDDOC = idDoc;
                                sign.IDREQUEST = idRequest;
                                sign.UUID = requestSign.UUID;
                                sign.CREATEDBYUSER = objUser.ID;
                                sign.CREATEDBYIP = MethodHelper.GetIPClient();
                                sign.TYPESIGN = sign.TYPESIGN.Replace(ConfigHelper.RootURL, "/");
                                var result = documentDAO.DocumentSignAdd(sign);
                            }
                        }
                    }

                    if (requestSign.LISTMAILTO != null)
                    {
                        foreach (var p in requestSign.LISTMAILTO)
                        {
                            p.IDREQUEST = idRequest;
                            p.UUID = requestSign.UUID;
                            p.CREATEDBYUSER = objUser.ID;
                            p.CREATEDBYIP = MethodHelper.GetIPClient();
                            var resultReceiveAdd = documentDAO.ReceiveAdd(p);

                            string Code = string.Format("{0}.{1}", requestSign.CREATEDBYUSER, idRequest);
                            string Subject = string.Format(Constants.EMAIL_SUBJECT_SIGN, Code, requestSign.STATUS, requestSign.EMAILSUBJECT);
                            string DocumentLinkViewer = GenerateLinkViewer(requestSign.STATUS, p.EMAIL.ToLower(), idRequest, p.ISCC, firstSign.SIGNINDEX);

                            EmailDataBO emailData = new EmailDataBO()
                            {
                                EmailType = Constants.TEMPLATE_MAILTYPE_DOCUMENT,
                                CreatedByEmail = requestSign.EMAIL,
                                CreatedByUser = requestSign.CREATEDBYUSER,
                                CreatedByIP = requestSign.CREATEDBYIP,
                                FromEmail = requestSign.EMAIL.ToLower(),
                                FromName = requestSign.FULLNAME,
                                MailTo = p.EMAIL.ToLower(),
                                MailName = p.NAME,
                                Subject = Subject,
                                Messages = requestSign.EMAILMESSAGES,
                                ISCC = p.ISCC,
                                DocumentLinkLogo = DocumentLogo.REQUEST,
                                DocumentLinkViewer = DocumentLinkViewer,
                                UUID = requestSign.UUID
                            };

                            //Add Email gửi đi
                            if (p.ISCC && !p.ISCCFINISH)
                            {
                                //Logo tài liệu bản sao
                                emailData.DocumentLinkLogo = DocumentLogo.GENERIC;

                                //Nếu bản thân nhận bản sao
                                if (requestSign.EMAIL.ToLower() == p.EMAIL.ToLower())
                                    emailData.DocumentMessage = Constants.DOCUMENT_MESSAGE_ISCC_SELF;
                                else
                                    //Nếu là người khác nhận bản sao
                                    emailData.DocumentMessage = string.Format(Constants.DOCUMENT_MESSAGE_ISCC_ONE, requestSign.FULLNAME, requestSign.EMAIL.ToLower());

                                if (requestSign.ISONLYMESIGN)
                                {
                                    //Nếu chỉ mình tôi ký thfi Logo hoàn thành và tin nhắn hoàn thành;
                                    emailData.DocumentLinkLogo = DocumentLogo.COMPLETE;
                                    emailData.DocumentMessage = Constants.DOCUMENT_MESSAGE_SIGN_ONLY_ME;
                                }
                                emailDatas.Add(emailData);
                            }
                            //Nếu là người khác ký
                            else if (p.EMAIL.ToLower() == firstSign.EMAILASSIGNMENT.ToLower())
                            {
                                emailData.DocumentMessage = string.Format(Constants.DOCUMENT_MESSAGE_SIGN_ONE_SENT, requestSign.FULLNAME, requestSign.EMAIL.ToLower());
                                emailDatas.Add(emailData);
                            }
                        }

                        AccountBLL objAccountBLL = new AccountBLL(objIData);
                        var result = objAccountBLL.AddEmail(emailDatas);
                    }

                }
                return idRequest;
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi khi tạo luồng trình ký");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                return 0;
            }
        }

        /// <summary>
        /// Lưu nháp request
        /// </summary>
        /// <param name="requestSign">Đối tượng request</param>
        /// <returns></returns>
        public bool SaveDraft(RequestSignBO requestSign)
        {
            //Bắt đầu transaction
            IData objIData;
            if (objDataAccess == null)
                objIData = Data.CreateData(ConfigHelper.Instance.GetConnectionStringDS(), false);
            else
                objIData = objDataAccess;
            try
            {
                if (objDataAccess == null)
                    objIData.BeginTransaction();

                DocumentDAO documentDAO = new DocumentDAO(objIData);
                //Lưu nháp thằng cha pm_request
                requestSign.ID = documentDAO.SaveDraft(requestSign);

                //Nếu tồn tại danh sách người nhận
                if (requestSign.LISTMAILTO != null)
                {
                    //Xóa tất cả người nhận
                    var resultReceiveDelete = documentDAO.ReceiveDelete(requestSign);
                    //Duyệt danh sách người nhận bổ sung thông tin thêm UUID, ID Request, IP khởi tạo
                    foreach (var p in requestSign.LISTMAILTO)
                    {
                        p.UUID = requestSign.UUID;
                        p.IDREQUEST = requestSign.ID;
                        p.CREATEDBYIP = requestSign.CREATEDBYIP;
                        p.CREATEDBYUSER = requestSign.CREATEDBYUSER;
                        documentDAO.ReceiveAdd(p);
                    }
                }
                //Nếu tồn tại danh sách tải 
                if (requestSign.FILEUPLOADS != null)
                {
                    //Xóa tất cả các dòng có id = idrequest
                    var resultDocumentDelete = documentDAO.DocumentDelete(requestSign);
                    //Nếu tồn file tải lên thì gán thông tin bổ sung vào
                    foreach (var doc in requestSign.FILEUPLOADS)
                    {
                        doc.UUID = requestSign.UUID;
                        doc.IDREQUEST = requestSign.ID;
                        doc.CREATEDBYIP = requestSign.CREATEDBYIP;
                        doc.CREATEDBYUSER = requestSign.CREATEDBYUSER;
                        var idDoc = documentDAO.DocumentAdd(doc);
                        //Không lưu vị trí ký ở nháp
                        doc.SIGN = new List<DocumentSignBO>();
                    }
                }
                if (objDataAccess == null)
                    objIData.CommitTransaction();
                return true;
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi thêm thêm tài liệu");
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

        public bool UpdateRequestSign(RequestSignBO requestSign)
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
                DocumentDAO documentDAO = new DocumentDAO(objIData);
                var result = documentDAO.UpdateRequestSign(requestSign);
                foreach (var item in requestSign.FILEUPLOADS)
                {
                    foreach (var sign in item.SIGN)
                    {
                        if (!sign.ISSIGNED)
                        {
                            if (sign.CERINFOBO == null)
                                sign.CERINFOBO = new CERTINFOBO();
                            documentDAO.UpdateDocumentSign(sign);
                        }
                    }
                }

                if (objDataAccess == null)
                    objIData.CommitTransaction();
                return true;
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi cập nhật trạng thái ký");
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

        public bool UpdateDocumentSign(DocumentSignBO documentSign)
        {
            try
            {
                DocumentDAO documentDAO = new DocumentDAO();
                var result = documentDAO.UpdateDocumentSign(documentSign);
                return true;
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi cập nhật trạng thái ký");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                return false;
            }
            finally
            {
            }
        }

        public bool UpdateDocumentSign(DocumentSignBO documentSign, IData objIData)
        {
            try
            {
                DocumentDAO documentDAO = new DocumentDAO(objIData);
                var result = documentDAO.UpdateDocumentSign(documentSign);
                return true;
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi cập nhật trạng thái ký");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                return false;
            }
            finally
            {
            }
        }

        public List<RequestSignBO> GetMasterRequest(FormSearch form)
        {
            try
            {
                DocumentDAO documentDAO = new DocumentDAO();
                var result = documentDAO.GetRequest(form);
                return result;
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi lấy danh sách tài liệu");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                return new List<RequestSignBO>();
            }
        }

        //public List<RequestSignBO> GetRequestLastActivity(FormSearch form)
        //{
        //    try
        //    {
        //        DocumentDAO documentDAO = new DocumentDAO();
        //        var result = documentDAO.GetRequestLastActivity(form);
        //        foreach (var item in result)
        //        {
        //            var formx = new FormSearch() { ID = item.ID };
        //            item.FILEUPLOADS = documentDAO.GetDocumentsByIdRequest(formx);
        //            item.LISTMAILTO = documentDAO.GetReceiveByIdRequest(formx);
        //        }
        //        return result;
        //    }
        //    catch (Exception objEx)
        //    {
        //        this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi lấy danh sách tài liệu");
        //        objResultMessageBO = ConfigHelper.Instance.WriteLog(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
        //        return new List<RequestSignBO>();
        //    }
        //}

        public RequestSignBO GetRequestDashBoard(FormSearch form)
        {
            try
            {
                DocumentDAO documentDAO = new DocumentDAO();
                var result = documentDAO.GetRequestDashBoard(form);
                if (result.Count > 0)
                    return result.First();
                return new RequestSignBO();
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi lấy thông tin dashboard");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                return new RequestSignBO();
            }
        }

        public bool CheckEmailExist(RequestSignBO request, string email)
        {
            var result = false;
            var hasEmail = request.LISTMAILTO.Any(x => x.EMAIL.ToLower() == email.ToLower());
            if (request.EMAIL == email || hasEmail)
            {
                result = true;
            }
            return result;
        }

        public RequestSignBO GetRequestById(FormSearch form)
        {
            try
            {
                DocumentDAO documentDAO = new DocumentDAO();

                //Lấy request master
                var request = documentDAO.GetRequestById(form);

                //Lấy danh sách tài liệu được tải lên
                request.FILEUPLOADS = documentDAO.GetDocumentsByIdRequest(form);
                foreach (var doc in request.FILEUPLOADS)
                {
                    //Lấy chữ ký trong mỗi tài liệu
                    doc.SIGN = documentDAO.GetDocumentSignByIdDoc(doc.ID);
                    doc.SIGN.ForEach(x =>
                    {
                        if (!string.IsNullOrEmpty(x.TYPESIGN))
                            x.TYPESIGN = ConfigHelper.RootURL + x.TYPESIGN.Replace(ConfigHelper.RootURL, "").Replace("..", "");
                    });
                }

                //Lấy danh sách người nhận
                request.LISTMAILTO = documentDAO.GetReceiveByIdRequest(form);

                var ssss = new List<DocumentSignBO>();
                foreach (var re in request.LISTMAILTO)
                {
                    foreach (var doc in request.FILEUPLOADS)
                    {
                        foreach (var sign in doc.SIGN)
                        {
                            if (sign.EMAILASSIGNMENT == re.EMAIL)
                            {
                                sign.TAXCODE = re.TAXCODE;
                                sign.ADDRESS = re.ADDRESS;
                                sign.IDNUMBER = re.IDNUMBER;
                                sign.PHONENUMBER = re.PHONENUMBER;
                                sign.EMAILASSIGNMENTNAME = re.NAME;
                                sign.REQUESTSIGNTYPE = re.REQUESTSIGNTYPE;
                                ssss.Add(sign);
                            }
                        }
                    }
                }

                request.LISTSIGN = ssss.Distinct().ToList().OrderBy(x => x.SIGNINDEX).ToList();

                return request;
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi lấy chi tiết tài liệu");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                return new RequestSignBO();
            }
            finally
            {
            }
        }

        public RequestSignBO GetRequestMasterById(FormSearch form)
        {
            try
            {
                DocumentDAO documentDAO = new DocumentDAO();

                //Lấy request master
                var request = documentDAO.GetRequestById(form);

                //Lấy danh sách tài liệu được tải lên
                request.FILEUPLOADS = documentDAO.GetDocumentsByIdRequest(form);
                return request;
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi lấy chi tiết tài liệu");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                return new RequestSignBO();
            }
        }


        public RequestSignBO GetRequestMasterByUuid(FormSearch form)
        {
            try
            {
                DocumentDAO documentDAO = new DocumentDAO();
                var request = documentDAO.GetRequestByUuid(form);
                return request;
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi lấy chi tiết tài liệu");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                return new RequestSignBO();
            }
        }

        public void ConvertToPng(string pdfDocName)
        {
            var rootFileName = Path.GetFileName(pdfDocName);
            var DirectoryName = Path.GetDirectoryName(pdfDocName);

            try
            {
                string fullPath = $"{DirectoryName}/{rootFileName}-{Constants.SMALL_THUMB}_{Constants.THUMB_EXTENSION}";
                int countPage = 0;
                using (PdfReader pdfReader = new PdfReader(pdfDocName))
                {
                    PdfReader.unethicalreading = true;
                    countPage = pdfReader.NumberOfPages;
                    pdfReader.Close();
                    pdfReader.Dispose();
                }
                new Thread(() =>
                {
                    pdfToImage.ConvertToPNG(pdfDocName, fullPath, 1, countPage, 10);
                }).Start();
                string big = fullPath.Replace(Constants.SMALL_THUMB, Constants.BIG_THUMB);
                for (int i = 1; i <= countPage; i++)
                {
                    pdfToImage.ConvertToPNG(pdfDocName, big, i, i, 200);
                }
            }
            catch (Exception ex)
            {
                ConfigHelper.Instance.WriteLogException("Lỗi chuyển đổi định dạng pdf sang image", ex, MethodBase.GetCurrentMethod().Name, "ConvertImageToPdf");
                throw;
            }
        }

        public ObjectResult GetDetailRequest(LinkViewerBO linkViewer, AccountBO objUser, HttpSessionStateBase sesionUser)
        {
            try
            {
                //Lấy thông tin chi tiết của request theo id
                var form = new FormSearch() { ID = linkViewer.id };
                var result = GetRequestById(form);

                //Nếu tài liệu đã bị hủy bỏ
                if (result.ISCANCELED)
                {
                    return new ObjectResult
                    {
                        rs = false,
                        isReload = true,
                        view = "HasCanceled"
                    };
                }

                //Nếu tài liệu đã bị từ chối ký
                if (result.STATUS == RequestStatus.TU_CHOI)
                {
                    return new ObjectResult
                    {
                        rs = false,
                        isReload = true,
                        view = "HasDeclined"
                    };
                }

                //Nếu tài liệu đã bị xóa
                if (result.ISDELETED)
                {
                    return new ObjectResult
                    {
                        rs = false,
                        isReload = true,
                        view = "HasDeleted"
                    };
                }

                //Nếu trạng thái khác trạng thái trong link
                if (linkViewer.status != result.STATUS)
                {
                    return new ObjectResult
                    {
                        rs = false,
                        isReload = true,
                        view = "Document"
                    };
                }

                //Nếu đang chờ ký thì tính toán thời gian hết hạn
                if (result.STATUS == RequestStatus.CHO_KY)
                {
                    var expriedAtTime = result.DEADLINETIME > 0 ?
                    result.CREATEDATTIME.AddDays(result.DEADLINETIME) :
                    result.CREATEDATTIME.AddDays(1000);
                    //Nếu thời gian hết hạn < thời gian hiện tại
                    if (expriedAtTime < DateTime.Now)
                        return new ObjectResult
                        {
                            rs = false,
                            isReload = true,
                            view = "HasExpried"
                        };
                }

                //Lấy thông tin của người ký xem có là thành viên của Onsign hay không?
                objUser = accountBLL.UserLogin(new AccountBO() { USERNAME = linkViewer.email });
                //Nếu chưa tồn tại thì gán đối tượng mới hoặc là tài khoản demo => coi như chưa có
                if (objUser == null || objUser.TAXCODE == "0106579683-999" || objUser.TAXCODE == "0101990346-999")

                {
                    objUser = new AccountBO
                    {
                        EMAIL = linkViewer.email,
                    };
                }
                objUser.ISCC = linkViewer.iscc;
                objUser.SIGNINDEX = linkViewer.signIndex;

                if ((result.STATUS == RequestStatus.CHO_KY || result.STATUS == RequestStatus.HOAN_THANH || result.STATUS == RequestStatus.TU_CHOI) && !result.ISDELETED && !result.ISCANCELED)
                {
                    //Lấy ra danh sách chữ ký của người ký hiện tại
                    MakeSignatureForSign(result, objUser);

                    //Lấy ra danh sách hình ảnh tài liệu
                    GetDocForViewer(result);

                    //Lấy ra thông tin người tạo phiên trình ký
                    //var createdByUser = accountBLL.UserLogin(new AccountBO() { USERNAME = result.EMAIL });
                    foreach (var recieve in result.LISTMAILTO)
                    {
                        //Nếu người nhận  = người nhận trong link email
                        if (recieve.EMAIL == linkViewer.email)
                        {
                            //Nếu người ký là cá nhân => OnSign sẽ là người đại diện không quan tâm người đó có dùng HSM hay không
                            if (recieve.REQUESTSIGNTYPE == Constants.REQUESTSIGNTYPE_ONSIGN)
                            {
                                objUser.ISUSEHSM = true;
                            }
                            //nếu đây là công ty thì 
                            if (recieve.REQUESTSIGNTYPE == Constants.REQUESTSIGNTYPE_HSM)
                            {
                                objUser.COMPANY = objUser.FULLNAME = recieve.NAME;
                            }
                            else objUser.COMPANY = null;
                            objUser.TAXCODE = recieve.TAXCODE;
                            objUser.PHONE = recieve.PHONENUMBER;
                            break;
                        }
                    }
                }
                sesionUser[ConfigHelper.User] = objUser;
                return new ObjectResult
                {
                    rs = true,
                    dataResponse = result,
                    iscc = linkViewer.iscc,
                    email = linkViewer.email,
                    signIndex = linkViewer.signIndex,
                    objUser = objUser
                };
            }
            catch (Exception ex)
            {
                logBLL.Request_Log_Add(new RequestLogBO()
                {
                    ID_REQUEST = linkViewer.id,
                    CREATED_BY_USER = -1,
                    ACTION = MethodBase.GetCurrentMethod().Name,
                    CREATED_BY_IP = MethodHelper.GetIPClient(),
                    TYPE = LOG_TYPE.ERROR,
                    MESSAGES = ErrorMsg
                });
                string msg = "Lỗi lấy thông tin yêu cầu ký file!";
                ConfigHelper.Instance.WriteLogException(msg, ex, MethodBase.GetCurrentMethod().Name, objUser.USERNAME);
                return new ObjectResult { rs = false, msg = msg };
            }
        }


        /// <summary>
        /// Ký vào tài liệu được trình ký (Người khác ký)
        /// </summary>
        /// <param name="currentRequest"></param>
        /// <param name="objUser"></param>
        /// <param name="outPath"></param>
        /// <returns></returns>
        public ObjectResult FinishRequest(RequestSignBO currentRequest, AccountBO objUser, out string outPath)
        {
            try
            {
                outPath = string.Empty;
                var lstSign = new List<DocumentSignBO>();
                List<PdfSignUSB> pdfSignUSBs = new List<PdfSignUSB>();
                List<EmailDataBO> emailDatas = new List<EmailDataBO>();

                //Hậu kiểm lấy lại thông tin của phiên tài liệu này xem có bị thay đổi trạng thái hay không?
                var resultRawRequest = GetRequestById(new FormSearch { ID = currentRequest.ID });

                currentRequest.FILEUPLOADS.ForEach(x => x.SIGN.Where(y => !y.ISSIGNED).ToList().ForEach(z => lstSign.Add(z)));
                var nextSignDefault = lstSign.OrderBy(x => x.SIGNINDEX).FirstOrDefault();

                //Nếu trạng thái khác chờ ký, hoặc bị hủy bỏ, hoặc không phải là người ký hiện tại=> file đã bị thay đổi, trả thông báo cho người dùng
                if (resultRawRequest.ISCANCELED || resultRawRequest.STATUS != RequestStatus.CHO_KY || nextSignDefault.EMAILASSIGNMENT != objUser.EMAIL)
                {
                    return new ObjectResult
                    {
                        rs = false,
                        isReload = true,
                        msg = $"Nội dung phiên làm việc đã bị thay đổi bởi ai đó. Bạn có thể tải lại trang " +
                        $"<strong><a href=\"javascript:window.location.reload(true)\">tại đây</a></strong> " +
                        $"để cập nhật phiên làm việc mới nhất!"
                    };
                }

                //Kiểm còn chữ ký nào chưa ký không
                var isImageNull = currentRequest.LISTSIGN.Any(x => string.IsNullOrEmpty(x.SIGNATUREIMAGE));
                if (isImageNull)
                {
                    return new ObjectResult
                    {
                        rs = false,
                        msg = "Bạn buộc phải ký vào tất cả chữ ký hiện có"
                    };
                }

                //Tiến hành ký vào file
                var _flagStatus = SignFileProgress(currentRequest, pdfSignUSBs, objUser, ref outPath);
                if (!_flagStatus.rs)
                {
                    return _flagStatus;
                }

                //kiểm tra đã ký hết chữ ký chưa
                if (pdfSignUSBs.Where(x => x.pdfListSign.Count > 0).Count() > 0 && !objUser.ISUSEHSM)
                {
                    return new ObjectResult
                    {
                        rs = true,
                        signUSB = true,
                        idrequest = currentRequest.ID,
                        pdfSignUSBs = pdfSignUSBs
                    };
                }

                //tìm người ký kế tiếp và tiến hành gởi mail
                MakeListForNextStep(currentRequest, lstSign);
                var nextSign = lstSign?.OrderBy(x => x.SIGNINDEX)?.FirstOrDefault();

                //tồn tại người ký kế tiếp tiến hành ký
                if (nextSign != null)
                {
                    var linkViewer = BaseBLL.GenerateLinkViewer(RequestStatus.CHO_KY, nextSign.EMAILASSIGNMENT, currentRequest.ID, false, nextSign.SIGNINDEX);
                    var e = currentRequest.LISTMAILTO.Where(x => x.EMAIL == nextSign.EMAILASSIGNMENT).FirstOrDefault();
                    string Code = string.Format("{0}.{1}", currentRequest.CREATEDBYUSER, currentRequest.ID);
                    e = e ?? new ReceiverBO();
                    var subject = $"{string.Format(Constants.EMAIL_SUBJECT_SIGN, Code, RequestStatus.CHO_KY, currentRequest.EMAILSUBJECT)}";
                    var msg = string.Format(Constants.DOCUMENT_MESSAGE_SIGN_ONE_SENT, objUser.FULLNAME, objUser.EMAIL).Replace("@", "&#64;").Replace(".", "&#46;");
                    var logo = e.ISCC ? DocumentLogo.GENERIC : DocumentLogo.REQUEST;
                    MakeListSendMail(currentRequest, emailDatas, linkViewer, e, subject, msg, logo, objUser);
                }
                //Nếu tất cả các file đã ký 
                else
                {
                    currentRequest.STATUS = RequestStatus.HOAN_THANH;

                    //Cập nhật vào requset
                    var result = UpdateRequestSign(currentRequest);
                    if (ResultMessageBO.IsError)
                    {
                        logBLL.Request_Log_Add(new RequestLogBO()
                        {
                            ID_REQUEST = currentRequest.ID,
                            CREATED_BY_USER = -1,
                            ACTION = MethodBase.GetCurrentMethod().Name,
                            CREATED_BY_IP = MethodHelper.GetIPClient(),
                            TYPE = LOG_TYPE.ERROR,
                            MESSAGES = ResultMessageBO.Message
                        });

                        ConfigHelper.Instance.WriteLogString(ResultMessageBO.Message, ResultMessageBO.MessageDetail, MethodBase.GetCurrentMethod().Name, "FinishRequest");
                        return new ObjectResult { rs = false, msg = ResultMessageBO.Message };
                    }

                    //Gửi mail khi hoàn thành tất cả
                    foreach (var receive in currentRequest.LISTMAILTO)
                    {
                        string linkViewer = GenerateLinkViewer(RequestStatus.HOAN_THANH, receive.EMAIL, currentRequest.ID, receive.ISCC);
                        string Code = $"{currentRequest.CREATEDBYUSER}.{currentRequest.ID}";
                        var subject = $"{string.Format(Constants.EMAIL_SUBJECT_SIGN, Code, RequestStatus.HOAN_THANH, currentRequest.EMAILSUBJECT)}";
                        var msg = Constants.DOCUMENT_MESSAGE_FINISH;
                        var logo = DocumentLogo.COMPLETE;
                        MakeListSendMail(currentRequest, emailDatas, linkViewer, receive, subject, msg, logo, objUser);
                    }
                }

                AccountBLL accountBLL = new AccountBLL();
                var resultAddEmail = accountBLL.AddEmail(emailDatas);
                if (accountBLL.ResultMessageBO.IsError)
                {
                    logBLL.Request_Log_Add(new RequestLogBO()
                    {
                        ID_REQUEST = currentRequest.ID,
                        CREATED_BY_USER = -1,
                        ACTION = MethodBase.GetCurrentMethod().Name,
                        CREATED_BY_IP = MethodHelper.GetIPClient(),
                        TYPE = LOG_TYPE.ERROR,
                        MESSAGES = ResultMessageBO.Message
                    });
                    ConfigHelper.Instance.WriteLogString(accountBLL.ResultMessageBO.Message, accountBLL.ResultMessageBO.MessageDetail, MethodBase.GetCurrentMethod().Name, "Login");
                    return new ObjectResult { rs = false, msg = accountBLL.ResultMessageBO.Message };
                }
                return new ObjectResult { rs = true };
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, objEx.Message);
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                throw objEx;
            }
        }

        public ObjectResult RequestSign(RequestSignBO request, AccountBO objUser, out string outPath, out long requestId)
        {
            requestId = 0; outPath = string.Empty;
            List<PdfSignBO> listPdfSignPath = new List<PdfSignBO>();

            //Khởi tạo transaction 
            IData objIData;
            if (objDataAccess == null)
                objIData = Data.CreateData(ConfigHelper.Instance.GetConnectionStringDS(), false);
            else
                objIData = objDataAccess;
            try
            {
                //Bắt đầu transaction
                if (objDataAccess == null)
                    objIData.BeginTransaction();

                #region Kiểm tra ràng buộc
                //Kiểm tra xem Đã có Email subject chưa, nếu chưa có thì ghép tên của các file tải lên
                if (string.IsNullOrEmpty(request.EMAILSUBJECT)) request.EMAILSUBJECT = string.Join(", ", request.FILEUPLOADS.Select(x => x.NAME).ToList());

                //Nếu người ký là người khác thì kiểm tra mail
                if (request.ISONLYMESIGN)
                    request.LISTMAILTO = new List<ReceiverBO>();
                else
                {
                    //Nếu danh sách người nhận null or count = 0 
                    if (request.LISTMAILTO?.Any() != true)
                        return new ObjectResult { rs = false, msg = "Vui lòng nhập thông tin người nhận ở bước 2" };

                    //Lọc trùng người nhận
                    request.LISTMAILTO = request.LISTMAILTO.Distinct().ToList();

                    int countReceiver = 1;
                    foreach (var rec in request.LISTMAILTO)
                    {
                        //Kiểm tra email người nhận
                        if (string.IsNullOrEmpty(rec.EMAIL) || !MethodHelper.IsValidEmail(rec.EMAIL))
                            return new ObjectResult { rs = false, msg = $"Vui lòng kiểm tra email người nhận thứ {countReceiver}" };
                        //Kiểm tra tên người nhận
                        if (string.IsNullOrEmpty(rec.NAME))
                            return new ObjectResult { rs = false, msg = $"Tên người nhận thứ {countReceiver} không được để trống!" };

                        rec.ISCC = false;
                        //Viết thường email, viết hoa tên
                        rec.EMAIL = rec.EMAIL.ToLower();
                        rec.NAME = rec.NAME.ToUpper();

                        //Nếu đối tượng ký kết là Doanh Nghiêp
                        if (rec.REQUESTSIGNTYPE == Constants.REQUESTSIGNTYPE_HSM)
                        {
                            //Kiểm tra mã số thuế
                            if (string.IsNullOrEmpty(rec.TAXCODE))
                                return new ObjectResult { rs = false, msg = $"Mã số thuế của người thứ {countReceiver} không được để trống!" };
                            rec.ADDRESS = null; //Địa chỉ
                            rec.IDNUMBER = null; //Số chứng minh/thẻ căn cước
                            rec.PHONENUMBER = null; //Số điện thoại
                        }
                        else
                        //Nếu đối tượng ký kết là Cá nhân
                        if (rec.REQUESTSIGNTYPE == Constants.REQUESTSIGNTYPE_ONSIGN)
                        {
                            //Kiểm tra địa chỉ
                            if (string.IsNullOrEmpty(rec.ADDRESS))
                                return new ObjectResult { rs = false, msg = $"Địa chỉ của người thứ {countReceiver} không được để trống!" };
                            //Kiểm tra số điện thoại
                            if (string.IsNullOrEmpty(rec.PHONENUMBER))
                                return new ObjectResult { rs = false, msg = $"Số điện thoại của người thứ {countReceiver} không được để trống!" };
                            //Kiểm tra số chứng minh nhân/thẻ căn cước
                            if (string.IsNullOrEmpty(rec.IDNUMBER))
                                return new ObjectResult { rs = false, msg = $"Số chứng minh nhân dân/thẻ căn cước của người thứ {countReceiver} không được để trống!" };
                            rec.TAXCODE = null;
                        }
                        else
                        //Nếu đối tượng chỉ nhận bản sao
                        if (rec.REQUESTSIGNTYPE == Constants.REQUESTSIGNTYPE_ISCC)
                        {
                            rec.ISCC = true;
                            rec.TAXCODE = null;
                            rec.ADDRESS = null;
                            rec.IDNUMBER = null;
                            rec.PHONENUMBER = null;
                        }
                        countReceiver++;
                    }
                    //Lấy ra danh sách những người tham gia ký kết
                    var ListEmailReceiver = request.LISTMAILTO.Where(z => !z.ISCC).Select(x => x.EMAIL.ToLower()).ToList();
                    //Nối chuỗi danh sách người nhận 
                    request.EMAILTO = string.Join(", ", ListEmailReceiver);
                }


                if (request.FILEUPLOADS?.Any() != true)
                    return new ObjectResult { rs = false, msg = "Vui lòng tải lên tài liệu (file) cần ký kết." };

                foreach (var doc in request.FILEUPLOADS)
                {
                    //Kiểm tra danh sách vị trí ký kết
                    if (doc.SIGN?.Any() != true)
                    {
                        return new ObjectResult { rs = false, msg = $"Vui lòng gắn chữ ký cho tài liệu {doc.NAME} ở bước 3." };
                    }

                    foreach (var sign in doc.SIGN)
                    {
                        //Viết thường email được gán vào khu vực ký kết
                        sign.EMAILASSIGNMENT = sign.EMAILASSIGNMENT.ToLower();
                    }
                }
                #endregion

                //Thêm bản thân người tạo vào là người nhận thư
                ReceiverBO mailTo = new ReceiverBO
                {
                    ISCC = !request.ISONLYMESIGN,
                    EMAIL = request.EMAIL,
                    NAME = request.FULLNAME,
                    REQUESTSIGNTYPE = Constants.REQUESTSIGNTYPE_ISCC //Bản thân luôn là người nhận bản sao
                };
                request.LISTMAILTO.Add(mailTo);

                //Thêm mã số thuế, chỉ IP, User ID vào request
                request.CREATEDBYIP = objUser.IP;
                request.TAXCODE = objUser.TAXCODE;
                request.CREATEDBYUSER = objUser.ID;

                //Lấy ra danh sách các chữ ký được sắp xếp theo thứ tự ký
                var ListSign = request.FILEUPLOADS.SelectMany(x => x.SIGN).OrderBy(z => z.SIGNINDEX).ToList();
                //Sắp xếp lại thứ tự từ 1
                int signIndex = 0;
                ListSign.ForEach(sign => sign.SIGNINDEX = ++signIndex);

                //Đếm số người được trình ký (gắn chữ ký)
                var countSignAssignment = ListSign.GroupBy(sign => sign.EMAILASSIGNMENT).Count();

                CERTINFOBO cert_info = new CERTINFOBO();

                //Đếm số người tham gia ký kết
                var countReceive = request.LISTMAILTO.Where(x => !x.ISCC).Count();
                if (request.ISONLYMESIGN)
                {
                    //kiểm tra xem người đó có được ký hsm hay không
                    if (!objUser.ISUSEHSM)
                    {
                        return new ObjectResult { rs = false, msg = "Hệ thống không tìm thấy chữ ký số cloud (HSM) của bạn" };
                    }
                    else
                    {
                        //Lấy thông tin HSM của user và gán vào user
                        AccountBLL accountBLL = new AccountBLL();
                        var hsmInfo = accountBLL.Get_HSM_Info(new AccountBO() { USERNAME = request.EMAIL });
                        CyberLotusBO cyberLotusBO = new CyberLotusBO
                        {
                            ISCOMPANY = true,
                            TAXCODE = objUser.TAXCODE,
                            APIID = objUser.APIID = hsmInfo.APIID,
                            APIURL = objUser.APIURL = hsmInfo.APIURL,
                            SECRET = objUser.SECRET = hsmInfo.SECRET
                        };

                        cert_info = CyberLotusHSM.Get_Cert_Info(cyberLotusBO);
                        //Nếu status # null => có lỗi
                        if (!string.IsNullOrEmpty(cert_info.STATUS))
                        {
                            return new ObjectResult { rs = false, msg = cert_info.STATUS };
                        }
                    }
                    countReceive = 1;
                }
                //Kiểm tra số người tham gia ký kết và số người gán ký 
                if (countReceive != countSignAssignment)
                {
                    return new ObjectResult { rs = false, msg = "Vui lòng gán chữ ký đầy đủ cho các bên ký!" };
                }

                //Xử lý file tải lên, nếu người ký là chỉ mình tôi thì ký 
                foreach (var doc in request.FILEUPLOADS)
                {
                    //Lấy ra các đường đãn liên quan đến file gồm: file gốc, file gốc.pdf, file đã được ký (áp dụng cho người ký là mình tôi)
                    string docPath = $"{ConfigHelper.FullDocument}/{doc.PATH}";
                    string inputPdfPath = $"{docPath}{Constants.PDF_EXTENSION}";
                    string outputPdfSignedPath = $"{docPath}{Constants.PDF_SIGNED_EXTENSION}";
                    outPath = inputPdfPath;

                    foreach (var sign in doc.SIGN)
                    {
                        //Lấy ra size thực tế của page ký pdf
                        iTextSharp.text.Rectangle realSize;
                        using (PdfReader pdfReader = new PdfReader(inputPdfPath))
                        {
                            PdfReader.unethicalreading = true;
                            doc.PAGES = pdfReader.NumberOfPages;
                            realSize = pdfReader.GetPageSizeWithRotation(sign.PAGESIGN == 0 ? 1 : sign.PAGESIGN);
                            sign.PDFWIDTH = (int)realSize.Width; //Chiều rộng thực tế
                            sign.PDFHEIGHT = (int)realSize.Height; //Chiều cao thực tế
                            pdfReader.Close();
                            pdfReader.Dispose();
                        }

                        //Nếu là chỉ mình tôi ký thì thực hiện ký file
                        if (request.ISONLYMESIGN)
                        {
                            sign.SIGNATUREPATH = $"{ConfigHelper.RootFolder}{objUser.SIGNATUREIMAGE}".Replace(ConfigHelper.RootURL, "");
                            PdfSignBO pdfSign = SetPdfSignBO(realSize, inputPdfPath, outputPdfSignedPath, sign, objUser);
                            pdfSign.certificate = cert_info.certificate;
                            //Ký file sử dụng HSM
                            var resultSign = signBLL.SignDocumentViaHSM(pdfSign);
                            //Nếu ký không thành công add Log
                            if (!resultSign.SIGNED)
                            {
                                logBLL.Request_Log_Add(new RequestLogBO()
                                {
                                    ID_REQUEST = -1,
                                    ACTION = "Ký bằng HSM",
                                    CREATED_BY_IP = MethodHelper.GetIPClient(),
                                    CREATED_BY_USER = objUser.ID,
                                    TYPE = LOG_TYPE.ERROR,
                                    MESSAGES = resultSign.STATUS
                                });
                                ConfigHelper.Instance.WriteLogString("Sự kiện ký", resultSign.STATUS, MethodBase.GetCurrentMethod().Name, null, objUser.EMAIL, objUser.ID);
                                return new ObjectResult { rs = false, msg = resultSign.STATUS };
                            }
                            sign.CERINFOBO = resultSign;
                            sign.ISSIGNED = true; //Thành công thì set đã ký = true

                            sign.TYPESIGN = sign.SIGNATUREPATH = sign.SIGNATUREPATH.Replace(ConfigHelper.FullDocument, "");

                            //Nếu tồn tại file tạm đã ký => file đầu vào = file tạm
                            if (File.Exists(pdfSign.pathOutputPdf_Temp))
                                inputPdfPath = pdfSign.pathOutputPdf_Temp;
                        }
                    }
                    //lấy đường dẫn file temp và file chính
                    listPdfSignPath.Add(new PdfSignBO { pathOutputPdf = outputPdfSignedPath, pathOutputPdf_Temp = $"{outputPdfSignedPath}_temp.pdf" });
                }

                //Sau khi ký xong file hiện tại thì tạo ra các file ảnh hiển thị của file đã ký
                if (request.ISONLYMESIGN)
                {
                    signBLL.CreateFileSign(listPdfSignPath);
                    foreach (var file in listPdfSignPath)
                    {
                        new Thread(() =>
                        {
                            ConvertToPng(file.pathOutputPdf);
                        }).Start();
                    }
                }

                var resultRequestSign = CreateRequestSign(request, objIData, objUser);
                if (ResultMessageBO.IsError || resultRequestSign == 0)
                {
                    logBLL.Request_Log_Add(new RequestLogBO()
                    {
                        ID_REQUEST = -1,
                        ACTION = "CreateRequestSign",
                        CREATED_BY_IP = MethodHelper.GetIPClient(),
                        CREATED_BY_USER = objUser.ID,
                        TYPE = LOG_TYPE.ERROR,
                        MESSAGES = ResultMessageBO.Message
                    });
                    ConfigHelper.Instance.WriteLogString(ResultMessageBO.Message, ResultMessageBO.MessageDetail, MethodBase.GetCurrentMethod().Name, "RequestSign");
                    return new ObjectResult { rs = false, msg = ResultMessageBO.Message };
                }

                requestId = resultRequestSign;

                if (objDataAccess == null)
                    objIData.CommitTransaction();

                return new ObjectResult { rs = true, msg = "Thành công!" };
            }
            catch (Exception objEx)
            {
                signBLL.DeleteFileTemp(listPdfSignPath);
                ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, objEx.Message);

                logBLL.Request_Log_Add(new RequestLogBO()
                {
                    ID_REQUEST = -1,
                    ACTION = "CreateRequestSign",
                    CREATED_BY_IP = MethodHelper.GetIPClient(),
                    CREATED_BY_USER = objUser.ID,
                    TYPE = LOG_TYPE.ERROR,
                    MESSAGES = ErrorMsg
                });

                objResultMessageBO = ConfigHelper.Instance.WriteLogException(ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), NameSpace);
                if (objDataAccess == null)
                    objIData.RollBackTransaction();
                throw;
            }
            finally
            {
                if (objDataAccess == null)
                    objIData.Disconnect();
            }
        }

        public List<DocumentBO> GetDocumentsByIdRequest(FormSearch form)
        {
            try
            {
                DocumentDAO documentDAO = new DocumentDAO();
                var docs = documentDAO.GetDocumentsByIdRequest(form);
                return docs;
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, objEx.Message);
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                throw;
            }
        }

        public List<ReceiverBO> GetReceiveByIdRequest(FormSearch form)
        {
            try
            {
                DocumentDAO documentDAO = new DocumentDAO();
                return documentDAO.GetReceiveByIdRequest(form);
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, objEx.Message);
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                throw;
            }
        }

        public List<RequestSignBO> GetRequestReSend(FormSearch form)
        {
            try
            {
                DocumentDAO documentDAO = new DocumentDAO();
                return documentDAO.GetRequestReSend(form);
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, objEx.Message);
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                throw;
            }
        }

        public bool RequestDelete(RequestSignBO requestSign)
        {
            try
            {
                DocumentDAO documentDAO = new DocumentDAO();
                documentDAO.RequestDelete(requestSign);
                return true;
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi xóa tài liệu");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                throw;
            }
        }

        public bool CancelRequest(RequestSignBO requestSign)
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

                var emailDatas = new List<EmailDataBO>();
                DocumentDAO documentDAO = new DocumentDAO(objIData);
                var result = documentDAO.CancelRequest(requestSign);

                var requestResult = this.GetRequestById(new FormSearch() { ID = requestSign.ID, CREATED_BY_USER = requestSign.CREATEDBYUSER });


                //Danh sách chữ ký sắp xếp theo thứ tự ký, lọc trùng
                var list_Signs = requestResult.FILEUPLOADS.SelectMany(doc => doc.SIGN).OrderBy(od => od.SIGNINDEX).Distinct().ToList();
                //Danh sách các chữ ký đã hoàn thành
                var list_Signed = list_Signs.Where(sign => sign.ISSIGNED).ToList();
                //Người ký tiếp theo
                var next_Sign = list_Signs.Where(sign => !sign.ISSIGNED).FirstOrDefault();

                var list_Send_Email = new List<ClientInfo>();

                //Thêm người đã ký vào list tạm
                foreach (var sign in list_Signed)
                {
                    list_Send_Email.Add(new ClientInfo()
                    {
                        EMAIL = sign.EMAILASSIGNMENT
                    });
                }

                //Nếu có người ký tiếp theo thì thêm vào list tạm
                if (next_Sign != null)
                {
                    list_Send_Email.Add(new ClientInfo()
                    {
                        EMAIL = next_Sign.EMAILASSIGNMENT
                    });
                }

                foreach (var receiver in requestResult.LISTMAILTO)
                {
                    var mailTo = new ClientInfo
                    {
                        EMAIL = receiver.EMAIL,
                        NAME = receiver.NAME
                    };

                    //Nếu hoàn thành thì lấy tất cả người nhận bản sao
                    var is_finish = requestResult.STATUS == RequestStatus.HOAN_THANH && receiver.ISCC;
                    //Nếu đang chờ ký thì chỉ lấy người được nhận CC, bỏ người nhận CC khi hoàn thành
                    var is_wating = requestResult.STATUS == RequestStatus.CHO_KY && receiver.ISCC && !receiver.ISCCFINISH;
                    if (is_finish || is_wating)
                        list_Send_Email.Add(mailTo);
                    else
                    {
                        foreach (var mail in list_Send_Email)
                        {
                            if (mail.EMAIL == mailTo.EMAIL)
                                mail.NAME = mailTo.NAME;
                        }
                    }
                }

                list_Send_Email = list_Send_Email.GroupBy(x => x.EMAIL).Select(y => y.FirstOrDefault()).ToList();

                ////Lấy danh sách người đã ký và người nhận bản sao
                //var lstSigns = new List<DocumentSignBO>();
                //foreach (var doc in requestResult.FILEUPLOADS)
                //{
                //    foreach (var sign in doc.SIGN)
                //    {
                //        lstSigns.Add(sign);
                //    }
                //}
                ////sắp xếp danh sách chữ ký theo thứ tự tăng dần
                //lstSigns = lstSigns.OrderBy(x => x.SIGNINDEX).Distinct().ToList();

                //var listSendEmail = new List<ClientInfo>();
                ////Lấy ra danh sách đã ký
                //var listSigned = lstSigns.Where(x => x.ISSIGNED).ToList();
                //foreach (var signed in listSigned)
                //{
                //    listSendEmail.Add(new ClientInfo() { EMAIL = signed.EMAILASSIGNMENT });
                //}
                ////Lấy ra người ký tiếp theo
                //var nextSign = lstSigns.Where(x => !x.ISSIGNED).FirstOrDefault();
                //if (nextSign != null && !string.IsNullOrEmpty(nextSign.EMAILASSIGNMENT))
                //{
                //    listSendEmail.Add(new ClientInfo() { EMAIL = nextSign.EMAILASSIGNMENT });
                //}

                ////Thêm người nhận bản sao vào list
                //foreach (var receiver in requestResult.LISTMAILTO)
                //{
                //    var mailTo = new ClientInfo
                //    {
                //        EMAIL = receiver.EMAIL,
                //        NAME = receiver.NAME
                //    };
                //    //Nếu chưa hoàn thành thì chỉ gửi cho người nhận bản sao
                //    if ((requestResult.STATUS == RequestStatus.CHO_KY && receiver.ISCC && !receiver.ISCCFINISH) ||
                //        (requestResult.STATUS == RequestStatus.HOAN_THANH && receiver.ISCC))
                //        listSendEmail.Add(mailTo);
                //    foreach (var rc in listSendEmail)
                //    {
                //        if (receiver.EMAIL == rc.EMAIL)
                //        {
                //            rc.NAME = receiver.NAME;
                //        }
                //    }
                //}

                //listSendEmail = listSendEmail.Distinct().ToList();
                foreach (var rc in list_Send_Email)
                {
                    var linkViewer = BaseBLL.GenerateLinkViewer(RequestStatus.TU_CHOI, rc.EMAIL, requestResult.ID, false, 0);
                    string Code = string.Format("{0}.{1}", requestResult.CREATEDBYUSER, requestResult.ID);
                    EmailDataBO emailData = new EmailDataBO()
                    {
                        EmailType = Constants.TEMPLATE_MAILTYPE_DOCUMENT,
                        CreatedByEmail = requestResult.EMAIL,
                        CreatedByUser = requestResult.CREATEDBYUSER,
                        CreatedByIP = MethodHelper.GetIPClient(),
                        MailTo = rc.EMAIL,
                        MailName = rc.NAME,
                        FromEmail = requestResult.EMAIL,
                        FromName = requestResult.FULLNAME,
                        Subject = string.Format(Constants.EMAIL_SUBJECT_SIGN, Code, RequestStatus.CANCEL, requestResult.EMAILSUBJECT),
                        ISCC = false,
                        DocumentLinkViewer = linkViewer,
                        DocumentLinkLogo = DocumentLogo.REQUEST,
                        DocumentMessage = string.Format(Constants.DOCUMENT_MESSAGE_CANCEL_SIGN, requestResult.EMAIL == rc.EMAIL ? "Bạn" : requestResult.FULLNAME, requestSign.CANCELREASON)
                    };
                    emailDatas.Add(emailData);
                }

                AccountBLL accountBLL = new AccountBLL(objIData);
                var resultAddEmail = accountBLL.AddEmail(emailDatas);

                if (objDataAccess == null)
                    objIData.CommitTransaction();
                return true;
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi hủy bỏ tài liệu");
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

        public void MakeListForNextStep(RequestSignBO request, List<DocumentSignBO> lstSign)
        {
            try
            {
                request.FILEUPLOADS.ForEach((doc) =>
                {
                    doc.SIGN.OrderBy(x => x.SIGNINDEX).ToList().ForEach((s) =>
                    {
                        doc.SIGNFORDISPLAY.OrderBy(x => x.SIGNINDEX).ToList().ForEach((sfd) =>
                        {
                            if (s.ID == sfd.ID && sfd.ISSIGNED)
                            {
                                lstSign.Remove(s);
                            }
                        });
                    });
                });
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, objEx.Message);
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                throw;
            }

        }

        public void MakeListSendMail(RequestSignBO request,
            List<EmailDataBO> emailDatas,
            string linkViewer,
            ReceiverBO mailToBO,
            string subject,
            string massege,
            string logo,
            AccountBO objUser,
            bool isReSend = false
        )
        {
            try
            {
                EmailDataBO emailData = new EmailDataBO()
                {
                    EmailType = Constants.TEMPLATE_MAILTYPE_DOCUMENT,
                    CreatedByEmail = objUser.EMAIL,
                    CreatedByUser = objUser.ID,
                    CreatedByIP = objUser.IP,

                    MailTo = mailToBO.EMAIL,
                    MailName = mailToBO.NAME,
                    FromEmail = request.EMAIL,
                    FromName = request.FULLNAME,
                    Subject = subject,
                    Messages = request.EMAILMESSAGES,
                    ISCC = mailToBO.ISCC,
                    DocumentLinkViewer = linkViewer,
                    DocumentLinkLogo = logo,
                    DocumentMessage = massege,
                    UUID = request.UUID,
                    IsReSend = isReSend
                };
                bool has = emailDatas.Any(item => item.Link == emailData.Link && item.MailTo == emailData.MailTo);
                if (!has)
                    emailDatas.Add(emailData);
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, objEx.Message);
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                throw;
            }
        }

        public List<RequestSignBO> SearchDoccument(int pageSize, int offset, int createdbyUser, string status, string keywork)
        {
            var documentDAO = new DocumentDAO();
            return documentDAO.SearchDoccumentByKeywork(pageSize, offset, createdbyUser, status, keywork);
        }

        public void MakeSignatureForSign(RequestSignBO request, AccountBO objUser)
        {
            try
            {
                request.LISTSIGN = new List<DocumentSignBO>();
                if (request.STATUS == RequestStatus.CHO_KY && !objUser.ISCC)
                {
                    var lsSignatureWaitSign = new List<DocumentSignBO>();
                    foreach (var doc in request.FILEUPLOADS)
                    {
                        foreach (var sign in doc.SIGN)
                        {
                            if (!sign.ISSIGNED)
                            {
                                lsSignatureWaitSign.Add(sign);
                            }
                        }
                    }
                    lsSignatureWaitSign = lsSignatureWaitSign.OrderBy(x => x.SIGNINDEX).ToList();

                    var lsSignatureForIndex = new List<DocumentSignBO>();
                    for (int i = 0; i < lsSignatureWaitSign.Count; i++)
                    {
                        if (lsSignatureWaitSign[i].SIGNINDEX == objUser.SIGNINDEX && lsSignatureWaitSign[i].EMAILASSIGNMENT.Contains(objUser.EMAIL))
                        {
                            lsSignatureForIndex.Add(lsSignatureWaitSign[i]);
                        }
                        else if (lsSignatureWaitSign[i].SIGNINDEX != objUser.SIGNINDEX)
                        {
                            if (lsSignatureWaitSign[i].EMAILASSIGNMENT == objUser.EMAIL)
                                lsSignatureForIndex.Add(lsSignatureWaitSign[i]);
                            else break;
                        }
                    }

                    foreach (var doc in request.FILEUPLOADS)
                    {
                        if (doc.SIGNFORDISPLAY == null)
                        {
                            doc.SIGNFORDISPLAY = new List<DocumentSignBO>();
                        }

                        foreach (var sign in lsSignatureForIndex)
                        {
                            var hasSign = doc.SIGNFORDISPLAY.SingleOrDefault(x => x.ID == sign.ID);
                            if (sign.IDDOC == doc.ID && hasSign == null)
                            {
                                doc.SIGNFORDISPLAY.Add(sign);
                            }
                        }
                    }
                    request.LISTSIGN.AddRange(lsSignatureForIndex);
                }
            }
            catch (Exception objEx)
            {
                var ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, objEx.Message);
                logBLL.Request_Log_Add(new RequestLogBO()
                {
                    ID_REQUEST = request.ID,
                    CREATED_BY_USER = request.CREATEDBYUSER,
                    ACTION = MethodBase.GetCurrentMethod().Name,
                    CREATED_BY_IP = MethodHelper.GetIPClient(),
                    TYPE = LOG_TYPE.ERROR,
                    MESSAGES = ErrorMsg
                });
            }
        }

        public ObjectResult SignUSB(List<PdfSignUSB> arrSign, List<string> data, AccountBO objUser)
        {
            var lstSign = new List<DocumentSignBO>();

            //Hậu kiểm trc khi ký
            DocumentBLL documentBLL = new DocumentBLL();
            //Hậu kiểm lấy lại thông tin của phiên tài liệu này
            var resultRequest = documentBLL.GetRequestById(new FormSearch { ID = arrSign.First().id });

            resultRequest.FILEUPLOADS.ForEach(x => x.SIGN.Where(y => !y.ISSIGNED).ToList().ForEach(z => lstSign.Add(z)));
            var nextSignDefault = lstSign.OrderBy(x => x.SIGNINDEX).First();

            //Nếu trạng thái khác chờ ký => file đã bị thay đổi, trả thông báo cho người dùng
            if (resultRequest.STATUS != RequestStatus.CHO_KY || nextSignDefault.EMAILASSIGNMENT != objUser.EMAIL)
            {
                return new ObjectResult()
                {
                    rs = true,
                    msg = "Nội dung phiên làm việc đã bị thay đổi bởi ai đó. Bạn có thể tải lại trang <strong><a href=\"javascript:window.location.reload(true)\">tại đây</a></strong> để cập nhật phiên làm việc mới nhất!"
                };
            }

            var subject = new CERTINFOBO();

            //Đổi thành file t
            foreach (var item in data)
            {
                var pdfSigned = JsonConvert.DeserializeObject<PdfSignedUSB>(item, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore
                });
                MixSpecialChar(subject, pdfSigned);
                subject.CERINFO = pdfSigned.subject;
                subject.ISUSEUSBTOKEN = false;
            }

            var cerLower = subject.CERINFO.ToLower();
            if (cerLower.Contains("mst"))
            {
                string mst;
                try
                {
                    mst = MethodHelper.BetweenStrings(cerLower, "mst", ",");
                }
                catch (Exception)
                {
                    mst = MethodHelper.BetweenStrings(cerLower, "mst", "");
                }
                if (!mst.Contains(nextSignDefault.TAXCODE))
                {
                    subject.SIGNED = false;
                    subject.STATUS = "Bạn đang dùng chứng thư số khác với Mã số thuế của bạn để ký tài liệu. Vui lòng kiểm tra lại!";
                    return new ObjectResult
                    {
                        rs = false,
                        msg = "Bạn đang dùng chứng thư số khác với Mã số thuế của bạn để ký tài liệu. Vui lòng kiểm tra lại"
                    };
                }
            }

            // int docSigned = 0; //Đếm số lượng chữ ký
            SignBLL signBLL = new SignBLL();
            string rootDocumentPath = ConfigHelper.RootFolder + ConfigHelper.DocumentRootFolder;

            documentBLL.MakeSignatureForSign(resultRequest, objUser);

            foreach (var doc in resultRequest.FILEUPLOADS)
            {
                // bool checkDocComplete = false;//Kiểm tra xem file tài liệu này đã hoàn thành chưa? 1 request có 1 hoặc nhiều file
                string pdfPath = rootDocumentPath + doc.PATH;
                string inputPath = pdfPath + Constants.PDF_EXTENSION;
                string outputSignedPath = pdfPath + Constants.PDF_SIGNED_EXTENSION;
                string imageSignaturePath = string.Empty;
                //ultRequest.LISTSIGN

                //duyệt các chữ ký trong file
                foreach (var sign in doc.SIGNFORDISPLAY)
                {
                    sign.CERINFOBO = subject;

                    //NẾU CHƯA KÝ VÀ LÀ NGƯỜI KÝ ĐƯỢC GÁN
                    //if (!sign.ISSIGNED && sign.EMAILASSIGNMENT == objUser.EMAIL && (sign.SIGNINDEX == 0 || sign.SIGNINDEX == objUser.SIGNINDEX))
                    //{
                    //Gán đã ký và lấy địa chỉ ip ký
                    sign.ISSIGNED = true;
                    sign.SIGNEDBYIP = objUser.IP;
                    sign.CERINFOBO.ISUSEUSBTOKEN = true;
                    sign.TYPESIGN = MethodHelper.BetweenStrings(sign.TYPESIGN.Replace(ConfigHelper.RootURL, ""), "", "?v=");
                    foreach (var ar in arrSign)
                    {
                        if (ar.input.Contains(doc.PATH))
                            foreach (var asi in ar.pdfListSign)
                            {
                                if (asi.img.Contains($"_signature_{sign.SIGNINDEX}.png"))
                                {
                                    //"http://localhost:5555//Documents/1/48ceb3c5-426a-4d4a-b7d3-d44cd4b0dd08-1607961476/3351b358-be6a-4cef-908c-4f5682623cc4.pdf_signature_1.png?v=526"
                                    string imgPath = asi.img.Replace(ConfigHelper.RootURL, "");
                                    imgPath = imgPath.Replace(ConfigHelper.DocumentRootFolder, "");
                                    imgPath = MethodHelper.BetweenStrings(imgPath, "", "?v=").Replace("//", "/");
                                    sign.SIGNATUREPATH = imgPath;
                                }
                            }
                    }

                    //Cập nhật vào document sign 
                    var result = documentBLL.UpdateDocumentSign(sign);
                    if (documentBLL.ResultMessageBO.IsError)
                    {
                        ConfigHelper.Instance.WriteLogString(documentBLL.ResultMessageBO.Message, documentBLL.ResultMessageBO.MessageDetail, MethodBase.GetCurrentMethod().Name, "FinishRequest");
                        return new ObjectResult
                        {
                            rs = true,
                            msg = documentBLL.ResultMessageBO.Message
                        };
                    }
                }

                ////Kiểm tra xem trong file này đã được ký hết chưa, nếu ký hết rồi thì tăng số lượng file đã ký lên 1 và generate file đã ký thành hình ảnh
                //checkDocComplete = doc.SIGNFORDISPLAY.Where(x => !x.ISSIGNED).Count() == 0;
                //if (checkDocComplete && !objUser.ISUSEHSM) //Nếu file tài liệu đã ký xong hết
                //{
                //    docSigned++; //Tăng số lượng đồng thời tạo ra hình ảnh của file đã ký
                //}
            }

            List<EmailDataBO> emailDatas = new List<EmailDataBO>();

            ////Nếu chưa chưa hoàn thành thì tiến hành gửi cho người ký tiếp theo
            //if (docSigned != resultRequest.FILEUPLOADS.Count)
            //{
            //lstSign = new List<DocumentSignBO>();

            documentBLL.MakeListForNextStep(resultRequest, lstSign);

            //resultRequest.FILEUPLOADS.ForEach(x => x.SIGN.Where(y => !y.ISSIGNED).ToList().ForEach(z => lstSign.Add(z)));

            var nextSign = lstSign.OrderBy(x => x.SIGNINDEX).FirstOrDefault();
            if (nextSign != null)
            {
                var linkViewer = BaseBLL.GenerateLinkViewer(RequestStatus.CHO_KY, nextSign.EMAILASSIGNMENT, resultRequest.ID, false, nextSign.SIGNINDEX);
                var e = resultRequest.LISTMAILTO.Where(x => x.EMAIL == nextSign.EMAILASSIGNMENT).FirstOrDefault();
                string Code = string.Format("{0}.{1}", resultRequest.CREATEDBYUSER, resultRequest.ID);
                EmailDataBO emailData = new EmailDataBO()
                {
                    EmailType = Constants.TEMPLATE_MAILTYPE_DOCUMENT,
                    CreatedByEmail = objUser.EMAIL,
                    CreatedByUser = objUser.ID,
                    CreatedByIP = objUser.IP,

                    MailTo = e.EMAIL,
                    MailName = e.NAME,
                    FromEmail = resultRequest.EMAIL,
                    FromName = resultRequest.FULLNAME,
                    Subject = string.Format(Constants.EMAIL_SUBJECT_SIGN, Code, RequestStatus.CHO_KY, resultRequest.EMAILSUBJECT),
                    Messages = resultRequest.EMAILMESSAGES,
                    ISCC = e.ISCC,
                    DocumentLinkViewer = linkViewer,
                    DocumentLinkLogo = DocumentLogo.REQUEST,
                    DocumentMessage = string.Format(Constants.DOCUMENT_MESSAGE_SIGN_ONE_SENT, objUser.FULLNAME, objUser.EMAIL).Replace("@", "&#64;").Replace(".", "&#46;")
                };
                emailDatas.Add(emailData);
            }
            //Nếu tất cả các file đã ký 
            else
            {
                resultRequest.STATUS = RequestStatus.HOAN_THANH;
                //Cập nhật vào requset
                var result = documentBLL.UpdateRequestSign(resultRequest);
                if (documentBLL.ResultMessageBO.IsError)
                {
                    ConfigHelper.Instance.WriteLogString(documentBLL.ResultMessageBO.Message, documentBLL.ResultMessageBO.MessageDetail, MethodBase.GetCurrentMethod().Name, "FinishRequest");
                    return new ObjectResult
                    {
                        rs = true,
                        msg = documentBLL.ResultMessageBO.Message
                    };
                }
                //Gửi mail khi hoàn thành tất cả
                foreach (var receive in resultRequest.LISTMAILTO)
                {
                    string linkViewer = BaseBLL.GenerateLinkViewer(RequestStatus.HOAN_THANH, receive.EMAIL, resultRequest.ID, receive.ISCC);
                    string Code = string.Format("{0}.{1}", resultRequest.CREATEDBYUSER, resultRequest.ID);
                    EmailDataBO emailData = new EmailDataBO()
                    {
                        EmailType = Constants.TEMPLATE_MAILTYPE_DOCUMENT,
                        CreatedByEmail = objUser.EMAIL,
                        CreatedByUser = objUser.ID,
                        CreatedByIP = objUser.IP,

                        MailTo = receive.EMAIL,
                        MailName = receive.NAME,
                        FromEmail = resultRequest.EMAIL,
                        FromName = resultRequest.FULLNAME,
                        Subject = string.Format(Constants.EMAIL_SUBJECT_SIGN, Code, RequestStatus.HOAN_THANH, resultRequest.EMAILSUBJECT),
                        Messages = resultRequest.EMAILMESSAGES,
                        ISCC = receive.ISCC,
                        DocumentLinkViewer = linkViewer,
                        DocumentLinkLogo = DocumentLogo.COMPLETE,
                        DocumentMessage = Constants.DOCUMENT_MESSAGE_FINISH,
                        UUID = resultRequest.UUID
                    };
                    emailDatas.Add(emailData);
                }
            }

            AccountBLL accountBLL = new AccountBLL();
            var resultAddEmail = accountBLL.AddEmail(emailDatas);
            if (accountBLL.ResultMessageBO.IsError)
            {
                ConfigHelper.Instance.WriteLogString(accountBLL.ResultMessageBO.Message, accountBLL.ResultMessageBO.MessageDetail, MethodBase.GetCurrentMethod().Name, "Login");
                return new ObjectResult
                {
                    rs = true,
                    msg = accountBLL.ResultMessageBO.Message
                };
            }

            foreach (var item in data)
            {
                new Thread(() =>
                {
                    var pdfSigned = JsonConvert.DeserializeObject<PdfSignedUSB>(item, new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Ignore
                    });
                    var msg = SaveFile2Disk(pdfSigned.outputpath, pdfSigned.data, objUser);
                    documentBLL.ConvertToPng(pdfSigned.outputpath);
                }).Start();
            }

            return new ObjectResult { rs = true, msg = "Ký thành công" };
        }

        public ObjectResult DeclineRequest(RequestSignBO request, AccountBO objUser)
        {
            //Hậu kiểm lấy lại thông tin của phiên tài liệu này
            var resultRequest = GetRequestById(new FormSearch { ID = request.ID });
            //Nếu mà trạng thái là chờ ký (chưa hoàn thành) thì cho phép
            if (resultRequest.STATUS != RequestStatus.CHO_KY)
                return new ObjectResult { rs = false, isReload = true, msg = "Đường dẫn hết hạn, vui lòng tải lại trang!" };

            MakeSignatureForSign(resultRequest, objUser);

            if (resultRequest.LISTSIGN.Count == 0)
                return new ObjectResult { rs = false, isReload = false, msg = "Bạn không có quyền từ chối tài liệu mà bạn đã ký!" };

            if (objUser.ISCC)
                return new ObjectResult { rs = false, isReload = false, msg = "Bạn (người nhận bản sao) không có quyền từ chối tài liệu này!" };

            //Cập nhật trạng thái chờ ký => từ chối
            request.STATUS = RequestStatus.TU_CHOI;

            foreach (var doc in request.FILEUPLOADS)
            {
                foreach (var sign in doc.SIGN)
                {
                    if (sign.SIGNINDEX == objUser.SIGNINDEX)
                    {
                        sign.ISDECLINED = true;
                        sign.DECLINEBYIP = MethodHelper.GetIPClient();
                        break;
                    }
                }
            }

            var resultUpdate = UpdateRequestSign(request);

            if (ResultMessageBO.IsError)
            {
                ConfigHelper.Instance.WriteLogString(ResultMessageBO.Message, ResultMessageBO.MessageDetail, MethodBase.GetCurrentMethod().Name, "DeclineRequest");
                return new ObjectResult { rs = false, msg = ResultMessageBO.Message };
            }

            //Lấy ra danh sách những người đã ký
            var lstSigneder = new List<DocumentSignBO>();
            request.FILEUPLOADS.ForEach(x => x.SIGN.Where(y => y.ISSIGNED || y.ISDECLINED).ToList().ForEach(z => lstSigneder.Add(z)));
            var nextSign = lstSigneder.GroupBy(x => x.EMAILASSIGNMENT).ToList();

            List<EmailDataBO> emailDatas = new List<EmailDataBO>();

            if (request.LISTMAILTO != null)
            {
                foreach (var re in request.LISTMAILTO)
                {
                    foreach (var x in nextSign)
                    {
                        //Nếu là người nhận CC
                        if (re.ISCC && !re.ISCCFINISH || re.EMAIL == x.Key)
                        {
                            string Code = string.Format("{0}.{1}", request.CREATEDBYUSER, request.ID);
                            var _subject = string.Format(Constants.EMAIL_SUBJECT_SIGN, Code, RequestStatus.TU_CHOI, request.EMAILSUBJECT);
                            var _linkViewer = GenerateLinkViewer(RequestStatus.TU_CHOI, objUser.EMAIL, request.ID, false);
                            var _message = string.Format(Constants.DOCUMENT_MESSAGE_DECLINE_SIGN, re.EMAIL == objUser.EMAIL ? "Bạn" : objUser.FULLNAME, objUser.EMAIL, request.DECLINEREASON).Replace("@", "&#64;").Replace(".", "&#46;");
                            MakeListSendMail(request, emailDatas, _linkViewer, re, _subject, _message, DocumentLogo.DECLINE, objUser);
                        }
                    }
                }
            }
            var resultAddEmail = accountBLL.AddEmail(emailDatas);
            if (accountBLL.ResultMessageBO.IsError)
            {
                ConfigHelper.Instance.WriteLogString(accountBLL.ResultMessageBO.Message, accountBLL.ResultMessageBO.MessageDetail, MethodBase.GetCurrentMethod().Name, "Login");
                return new ObjectResult { rs = false, msg = accountBLL.ResultMessageBO.Message };
            }

            return new ObjectResult { rs = true };
        }

        private string SaveFile2Disk(string fileName, string base64String, AccountBO objUser)
        {
            var msg = "";
            try
            {
                //string dstFilePath = Server.MapPath(fileName);
                byte[] dataBuffer = Convert.FromBase64String(base64String);
                using (FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    if (dataBuffer.Length > 0)
                    {
                        fileStream.Write(dataBuffer, 0, dataBuffer.Length);
                    }
                    msg = "Convert File thành công";
                }
            }
            catch (Exception ex)
            {
                msg = "Lỗi lưu file đã ký USB";
                ConfigHelper.Instance.WriteLogException(msg, ex, MethodBase.GetCurrentMethod().Name, objUser.USERNAME);
            }
            return msg;
        }

        private void MixSpecialChar(CERTINFOBO subject, PdfSignedUSB pdfSigned)
        {
            try
            {
                subject.TAXCODE = MethodHelper.BetweenStrings(pdfSigned.subject.Replace("=", @"=").Replace(",", @","), "MST:", ",");
                subject.COMPANY = MethodHelper.BetweenStrings(pdfSigned.subject.Replace("=", @"=").Replace(", ", @","), "CN=", ",");
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// lấy ra danh sách hình ảnh tài liệu
        /// </summary>
        /// <param name="result"></param>
        private void GetDocForViewer(RequestSignBO result)
        {
            foreach (var doc in result.FILEUPLOADS)
            {
                string path = doc.PATH + Constants.PDF_EXTENSION;

                if (File.Exists($"{ConfigHelper.RootFolder}{ConfigHelper.DocumentRootFolder}{doc.PATH}{Constants.PDF_SIGNED_EXTENSION}"))
                    path = doc.PATH + Constants.PDF_SIGNED_EXTENSION;

                doc.thumbs = new List<Thumb>();
                for (int i = 1; i <= doc.PAGES; i++)
                {
                    string temp = $"{ConfigHelper.DocumentRootFolder}{path}-{Constants.SMALL_THUMB}_{i}{Constants.THUMB_EXTENSION}";
                    var t = new Thumb
                    {
                        name = doc.NAME,
                        page = i,
                        small = temp + "?v=" + new Random().Next(100000, 999999)
                    };
                    var big = temp.Replace(Constants.SMALL_THUMB, Constants.BIG_THUMB) + "?v=" + new Random().Next(100000, 999999);
                    t.big = $"{ConfigHelper.RootURL}{big}";
                    doc.thumbs.Add(t);
                }
            }
        }

        //private string GetFileTypeImg(DocumentSignBO sign)
        //{
        //    string msg = "Lỗi lưu hình ảnh chữ ký";
        //    string imageSignaturePath = $"{ConfigHelper.FullDocument}{sign.SIGNATUREIMAGE}";

        //    try
        //    {
        //        //Lưu hình ảnh chữ ký vào server
        //        if (!string.IsNullOrEmpty(imageSignaturePath))
        //        {
        //            byte[] imageBytes = File.ReadAllBytes(imageSignaturePath);
        //            using (Image image = Image.FromStream(new MemoryStream(imageBytes)))
        //            {
        //                var ratio = (float)500 / (float)image.Width;
        //                var newImage = new Bitmap((int)Math.Round(image.Width * ratio), (int)Math.Round(image.Height * ratio));
        //                Graphics.FromImage(newImage).DrawImage(image, 0, 0, (int)Math.Round(image.Width * ratio), (int)Math.Round(image.Height * ratio));
        //                using (Bitmap bmp = new Bitmap(newImage))
        //                {
        //                    bmp.Save(imageSignaturePath, ImageFormat.Png);
        //                }
        //            }
        //            //Đường dẫn chữ ký bằng /user/...
        //            sign.SIGNATUREPATH = imageSignaturePath.Replace(ConfigHelper.FullDocument, "");
        //        }
        //        return string.Empty;
        //    }
        //    catch (Exception ex)
        //    {
        //        ConfigHelper.Instance.WriteLog(msg, ex, MethodBase.GetCurrentMethod().Name, "FinishRequest");
        //        return msg;
        //    }
        //}

        private PdfSignBO SetPdfSignBO(iTextSharp.text.Rectangle realSize, string inputPath, string outputSignedPath, DocumentSignBO sign, AccountBO objUser)
        {
            return new PdfSignBO()
            {
                base64pdf = inputPath,
                pdfSize = realSize,
                pathOutputPdf = outputSignedPath,
                pathOutputPdf_Temp = $"{outputSignedPath}_temp.pdf",
                xpoint = sign.XPOINT,
                ypoint = sign.YPOINT,
                pagesign = sign.PAGESIGN,
                base64image = sign.SIGNATUREPATH.Contains("?v=") ? MethodHelper.BetweenStrings(sign.SIGNATUREPATH, "", "?") : sign.SIGNATUREPATH,
                objUser = objUser,
                signaturename = $"Chữ ký của {objUser.FULLNAME} tại OnSign lúc {DateTime.Now:dd/MM/yyyy HH:mm:ss:ffffff}",
                height = sign.SIGNATUREHEIGHT,
                width = sign.SIGNATUREWIDTH
            };
        }

        private ObjectResult ProgressSign(DocumentSignBO sign, PdfSignBO pdfSign, SignBLL signBLL, PdfSignUSB pdfSignUSB, AccountBO objUser, IData objIData)
        {
            try
            {
                //Ký bằng USB Token
                if (!objUser.ISUSEHSM)
                {
                    //Chiều rộng
                    var imgWidth = sign.SIGNATUREWIDTH;
                    //Chiều cao
                    var imgHeight = sign.SIGNATUREHEIGHT;

                    //Đảo tọa độ của Y bên trên xuống dưới
                    //Tọa độ thực tế = chiều cao thực tế của file pdf - tọa độ trên view - chiều cao của ảnh
                    sign.YPOINT = sign.PDFHEIGHT - sign.YPOINT - imgHeight;

                    //Gốc tọa độ được tính từ góc dưới - trái
                    if (sign.XPOINT < 0) sign.XPOINT = 0;
                    if (sign.XPOINT + imgWidth > sign.PDFWIDTH)
                        sign.XPOINT = sign.PDFWIDTH - imgWidth;
                    if (sign.YPOINT < 0) sign.YPOINT = 0;
                    if (sign.YPOINT + imgHeight > sign.PDFHEIGHT)
                        sign.YPOINT = sign.PDFHEIGHT - imgHeight;

                    var pdfListSignUSB = new PdfListSignUSB()
                    {
                        visibleMode = 3,
                        rendermode = 0,
                        llX = sign.XPOINT,
                        llY = sign.YPOINT,
                        urX = sign.XPOINT + imgWidth,
                        urY = sign.YPOINT + imgHeight,
                        pageNo = sign.PAGESIGN,
                        imageWidth = imgWidth,
                        imageHeight = imgHeight,
                        img = sign.SIGNATUREIMAGE
                    };
                    pdfSignUSB.pdfListSign.Add(pdfListSignUSB);

                }
                else //nếu ký bằng HSM
                {
                    pdfSign.base64image = pdfSign.base64image.Replace(ConfigHelper.RootURL, ConfigHelper.RootFolder);
                    //Ký file qua HSM
                    var resultSign = signBLL.SignDocumentViaHSM(pdfSign);
                    //Nếu ký không thành công
                    if (!resultSign.SIGNED)
                    {
                        var msg = resultSign.STATUS;
                        ConfigHelper.Instance.WriteLogString("Lỗi khi ký file bằng HSM", resultSign.STATUS, MethodBase.GetCurrentMethod().Name, "SignDocument", objUser.EMAIL);
                        return new ObjectResult { rs = false, msg = msg };
                    }
                    sign.CERINFOBO = resultSign;
                    //Cập nhật vào document sign 
                    sign.ISSIGNED = true;
                    sign.SIGNEDBYIP = objUser.IP;
                    sign.TYPESIGN = sign.SIGNATUREPATH = sign.SIGNATUREPATH.Replace(ConfigHelper.FullDocument, "");
                    var result = UpdateDocumentSign(sign, objIData);
                    if (ResultMessageBO.IsError || !result)
                    {
                        ConfigHelper.Instance.WriteLogString(ResultMessageBO.Message, ResultMessageBO.MessageDetail, MethodBase.GetCurrentMethod().Name, "FinishRequest");
                        return new ObjectResult { rs = true, msg = ResultMessageBO.Message };
                    }
                }
                return new ObjectResult { rs = true };
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, objEx.Message);
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                throw;
            }
        }

        /// <summary>
        /// Ký vào file 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="pdfSignUSBs"></param>
        /// <param name="objUser"></param>
        /// <param name="outPdfSignedPath"></param>
        /// <returns></returns>
        private ObjectResult SignFileProgress(RequestSignBO request, List<PdfSignUSB> pdfSignUSBs, AccountBO objUser, ref string outPdfSignedPath)
        {

            List<PdfSignBO> lsPdfSign = new List<PdfSignBO>();

            IData objIData;
            if (objDataAccess == null)
                objIData = Data.CreateData(ConfigHelper.Instance.GetConnectionStringDS(), false);
            else
                objIData = objDataAccess;
            try
            {
                if (objDataAccess == null)
                    objIData.BeginTransaction();
                CERTINFOBO certInfo = new CERTINFOBO();
                //nếu company IsNullOrEmpty thì đây là cá nhân
                if (string.IsNullOrEmpty(objUser.COMPANY))
                {
                    //Nếu là cá nhân thì check xem có được sử dụng HSM của chủ sở hữu hay không
                    //Lấy thông tin đăng nhập HSM từ người khởi tạo
                    AccountBLL accountBLL = new AccountBLL(objIData);
                    var creadted_by_user = accountBLL.Get_HSM_Info(new AccountBO() { USERNAME = request.EMAIL });
                    if (creadted_by_user != null)
                    {
                        //Nếu được sử dụng chính HSM của họ
                        if (creadted_by_user.IS_USE_HSM_OF_OWNER)
                        {
                            objUser.APIID = creadted_by_user.APIID;
                            objUser.APIURL = creadted_by_user.APIURL;
                            objUser.SECRET = creadted_by_user.SECRET;
                        }
                        else
                        {
                            //Nếu không thì dùng HSM của OnSign
                            objUser.APIURL = ConfigHelper.APIURL_ONSIGN;
                            objUser.APIID = ConfigHelper.APIID_ONSIGN;
                            objUser.SECRET = ConfigHelper.SECRET_ONSIGN;
                        }
                    }
                    else
                    {
                        return new ObjectResult { rs = false, msg = "Hệ thống không tìm thấy thông tin người khởi tạo, vui lòng liên hệ lại người khởi tạo để kiểm tra thông tin" };
                    }
                }
                else //Nếu người ký là doanh nghiệp (tên công ty # null)
                {
                    //Nếu người ký được sử dụng HSM
                    if (objUser.ISUSEHSM)
                    {
                        //Lấy thông tin đăng nhập HSM của người ký
                        var current_user = accountBLL.Get_HSM_Info(new AccountBO() { USERNAME = objUser.EMAIL });
                        if (current_user != null)
                        {
                            //Nếu người dùng có HSM
                            objUser.APIID = current_user.APIID;
                            objUser.APIURL = current_user.APIURL;
                            objUser.SECRET = current_user.SECRET;
                        }
                        if (request.TAXCODE == "0106579683-999" || request.TAXCODE == "0101990346-999")
                        {
                            objUser.APIID = ConfigHelper.APIID;
                            objUser.APIURL = ConfigHelper.APIURL;
                            objUser.SECRET = ConfigHelper.SECRET;
                        }
                    }
                }

                //Lấy thông tin chi tiết chữ ký số HSM 
                if (!string.IsNullOrEmpty(objUser.APIID))
                {
                    CyberLotusBO cyberLotusBO = new CyberLotusBO
                    {
                        APIID = objUser.APIID,
                        APIURL = objUser.APIURL,
                        SECRET = objUser.SECRET,
                        TAXCODE = objUser.TAXCODE,
                        ISCOMPANY = !string.IsNullOrEmpty(objUser.COMPANY),
                    };

                    certInfo = CyberLotusHSM.Get_Cert_Info(cyberLotusBO);
                    //Nếu status # null => có lỗi
                    if (!string.IsNullOrEmpty(certInfo.STATUS))
                    {
                        return new ObjectResult { rs = false, msg = certInfo.STATUS };
                    }
                }

                iTextSharp.text.Rectangle realSize;

                MakeSignatureForSign(request, objUser);

                foreach (var doc in request.FILEUPLOADS)
                {
                    //Đường dẫn file Raw
                    string pdfPath = $"{ConfigHelper.FullDocument}{doc.PATH}";
                    //Đường dẫn file pdf 
                    string inputPath = $"{pdfPath}{Constants.PDF_EXTENSION}";
                    //Đường dẫn file pdf đã được ký
                    string outputSignedPath = outPdfSignedPath = $"{pdfPath}{Constants.PDF_SIGNED_EXTENSION}";

                    //khai báo sử dụng trong trường hợp ký bằng USB Token
                    PdfSignUSB pdfSignUSB = new PdfSignUSB
                    {
                        pdfListSign = new List<PdfListSignUSB>()
                    };

                    //tiến hành ký list chữ ký vào file
                    foreach (var sign in doc.SIGNFORDISPLAY)
                    {
                        //Khởi tạo mới thông tin của chứng chỉ
                        sign.CERINFOBO = new CERTINFOBO();

                        //Nếu tồn tại file đã ký được ký bởi người trước đó thì gán file đầu vào là file đã ký
                        if (File.Exists(outputSignedPath + "_temp.pdf"))
                            inputPath = outputSignedPath + "_temp.pdf";
                        else if (File.Exists(outputSignedPath))
                            inputPath = outputSignedPath;

                        pdfSignUSB.id = request.ID;
                        pdfSignUSB.type = "pdf";
                        pdfSignUSB.index = 0;
                        pdfSignUSB.output = outputSignedPath;
                        pdfSignUSB.input = inputPath.Replace(ConfigHelper.RootFolder, ConfigHelper.RootURL); //Convert.ToBase64String(System.IO.File.ReadAllBytes(inputPath));

                        //Lấy ra size thực tế của page ký
                        using (PdfReader pdfReader = new PdfReader(inputPath))
                        {
                            PdfReader.unethicalreading = true;
                            doc.PAGES = pdfReader.NumberOfPages;
                            realSize = pdfReader.GetPageSizeWithRotation(sign.PAGESIGN);
                            pdfReader.Close();
                            pdfReader.Dispose();
                        }

                        sign.SIGNATUREPATH = string.IsNullOrEmpty(sign.SIGNATUREPATH) ? sign.SIGNATUREIMAGE : sign.SIGNATUREPATH;
                        //Tạo đối tượng pdf chứa những thông tin cần ký
                        PdfSignBO pdfSign = SetPdfSignBO(realSize, inputPath, outputSignedPath, sign, objUser);
                        pdfSign.certificate = certInfo.certificate;
                        //kiểm tra và thêm chữ ký vào file
                        var signStatus = ProgressSign(sign, pdfSign, signBLL, pdfSignUSB, objUser, objIData);
                        if (!signStatus.rs)
                        {
                            return signStatus;
                        }
                    }

                    //lấy đường dẫn file temp và file chính
                    lsPdfSign.Add(new PdfSignBO { pathOutputPdf = outputSignedPath, pathOutputPdf_Temp = $"{outputSignedPath}_temp.pdf" });

                    //thêm dữ liệu cho ký bằng USB token
                    if (!string.IsNullOrEmpty(pdfSignUSB.output))
                    {
                        pdfSignUSBs.Add(pdfSignUSB);
                    }

                    //Copy file tạm sang file output, xóa file tamj
                    signBLL.CreateFileSign(lsPdfSign);

                    //Tạo luồng render cho file đã ký
                    if (File.Exists(outputSignedPath))
                    {
                        //Kiểm tra xem đã tồn tại file output chưa rồi mới render
                        new Thread(() =>
                        {
                            ConvertToPng(outputSignedPath);
                        }).Start();
                    }
                }

                if (objDataAccess == null)
                    objIData.CommitTransaction();

                //tạo lại file và xóa file temp
                return new ObjectResult { rs = true };
            }
            catch (Exception objEx)
            {
                signBLL.DeleteFileTemp(lsPdfSign);
                ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, objEx.Message);
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), NameSpace);
                if (objDataAccess == null)
                    objIData.RollBackTransaction();
                throw objEx;
            }
            finally
            {
                if (objDataAccess == null)
                    objIData.Disconnect();
            }
        }

        public void GenerateCertificate(string htmlContent, string path)
        {
            new Thread(() =>
            {
                var htmlToPdf = new NReco.PdfGenerator.HtmlToPdfConverter();
                var pdfBytes = htmlToPdf.GeneratePdf(htmlContent);

                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                File.WriteAllBytes(path, pdfBytes);
            }).Start();
        }

        public void AddToZipFile(ZipOutputStream s, byte[] buffer, string fullPath)
        {
            using (FileStream fs = System.IO.File.OpenRead(fullPath))
            {
                int sourceBytes;
                do
                {
                    sourceBytes = fs.Read(buffer, 0, buffer.Length);
                    s.Write(buffer, 0, sourceBytes);
                } while (sourceBytes > 0);
            }
        }

        private void AddToZipFile(ZipOutputStream s, byte[] buffer, int sourceBytes)
        {
            do
            {
                s.Write(buffer, 0, sourceBytes);
            } while (sourceBytes > 0);
        }

        public void ConvertFileToPdf(DocumentBO doc)
        {
            DocumentDAO documentDAO = new DocumentDAO();
            new Thread(() =>
            {
                try
                {
                    string documentPath = $"{ConfigHelper.RootFolder}{ConfigHelper.DocumentRootFolder}{doc.PATH}";
                    string newDocumentPath = $"{documentPath}{Constants.PDF_EXTENSION}";
                    //Kiểm tra xem file tài liệu này có tồn tại hay không?
                    if (File.Exists(documentPath))
                    {
                        //Kiểm tra xem file pdf mới này tồn tại hay không?
                        if (!File.Exists(newDocumentPath))
                        {
                            //Nếu file gốc là file PDF thì copy sang file PDF mới
                            if (documentPath.ToLower().EndsWith("pdf"))
                            {
                                File.Copy(documentPath, newDocumentPath, true);
                            }
                            //Nếu file gốc là file Word(doc/docx) thì save as PDF
                            else if (documentPath.ToLower().EndsWith("doc") || documentPath.ToLower().EndsWith("docx"))
                            {
                                try
                                {
                                    Type wordType = Type.GetTypeFromProgID("Word.Application");
                                    dynamic app = Activator.CreateInstance(wordType);
                                    if (app.Documents != null)
                                    {
                                        var document = app.Documents.Open(documentPath);
                                        if (document != null)
                                        {
                                            document.ExportAsFixedFormat(newDocumentPath, Microsoft.Office.Interop.Word.WdExportFormat.wdExportFormatPDF);
                                            document.Close();
                                        }
                                        app.Quit();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    string msg = "Lỗi convert WORD file to PDF file";
                                    ConfigHelper.Instance.WriteLogException(msg, ex, MethodBase.GetCurrentMethod().Name, "ConvertFiles");
                                }
                            }
                            //Nếu file gốc là file Excel(xls/xlsx) thì save as PDF
                            else if (documentPath.ToLower().EndsWith("xls") || documentPath.ToLower().EndsWith("xlsx"))
                            {
                                try
                                {
                                    Type wordType = Type.GetTypeFromProgID("Excel.Application");
                                    dynamic app = Activator.CreateInstance(wordType);
                                    var document = app.Workbooks.Open(documentPath);
                                    if (document != null)
                                    {
                                        document.ExportAsFixedFormat(Microsoft.Office.Interop.Excel.XlFixedFormatType.xlTypePDF, newDocumentPath);
                                        document.Close();
                                    }
                                    app.Quit();
                                }
                                catch (Exception ex)
                                {
                                    string msg = "Lỗi chuyển định dạng từ file excel!";
                                    ConfigHelper.Instance.WriteLogException(msg, ex, MethodBase.GetCurrentMethod().Name, "ConvertFiles");
                                }
                            }
                            //Nếu file gốc là file Ảnh(jpg/jpeg/png/webp) thì Convert to PDF
                            else if (documentPath.ToLower().EndsWith("jpg") || documentPath.ToLower().EndsWith("jpeg") || documentPath.ToLower().EndsWith("png") || documentPath.ToLower().EndsWith("webp"))
                            {
                                if (documentPath.ToLower().EndsWith("webp"))
                                {
                                    documentPath += ".png";
                                }
                                try
                                {
                                    bool result = MethodHelper.ConvertImageToPdf(documentPath, newDocumentPath);
                                }
                                catch (Exception ex)
                                {
                                    string msg = "Lỗi chuyển định dạng từ file ảnh!";
                                    ConfigHelper.Instance.WriteLogException(msg, ex, MethodBase.GetCurrentMethod().Name, "ConvertFiles");
                                }
                            }

                            //Nếu là tài khoản demo thì gắn thêm logo Demo
                            if (doc.TAXCODE == "0106579683-999" || doc.TAXCODE == "0101990346-999")
                            {
                                using (Stream inputPdfStream = new FileStream(newDocumentPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                                using (Stream inputImageStream = new FileStream($"{ConfigHelper.RootFolder}/Images/demo.png", FileMode.Open, FileAccess.Read, FileShare.Read))
                                using (Stream outputPdfStream = new FileStream(newDocumentPath + "_temp.pdf", FileMode.Create, FileAccess.Write, FileShare.None))
                                {
                                    var reader = new PdfReader(inputPdfStream);
                                    PdfReader.unethicalreading = true;
                                    var stamper = new PdfStamper(reader, outputPdfStream);
                                    doc.PAGES = reader.NumberOfPages;
                                    iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(inputImageStream);
                                    image.ScaleAbsolute(200, 200);
                                    image.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                                    var listViewerSize = new List<ViewerPageSize>();
                                    for (int i = 1; i <= reader.NumberOfPages; i++)
                                    {
                                        var pdfContentByte = stamper.GetOverContent(i);
                                        image.SetAbsolutePosition((reader.GetPageSize(i).Width - image.ScaledWidth) / 2, (reader.GetPageSize(i).Height - image.ScaledHeight) / 2);
                                        pdfContentByte.AddImage(image);

                                        var realSize = reader.GetPageSizeWithRotation(i);
                                        var viewerPageSize = new ViewerPageSize
                                        {
                                            PAGE = i,
                                            SIZE = $"{(int)realSize.Width}x{(int)realSize.Height}"
                                        };
                                        listViewerSize.Add(viewerPageSize);

                                    }
                                    doc.VIEWER_PAGES_SIZE = JsonConvert.SerializeObject(listViewerSize, new JsonSerializerSettings
                                    {
                                        NullValueHandling = NullValueHandling.Ignore,
                                        DefaultValueHandling = DefaultValueHandling.Ignore
                                    });
                                    stamper.Close();
                                    reader.Close();
                                    reader.Dispose();
                                }
                                File.Copy(newDocumentPath + "_temp.pdf", newDocumentPath, true);
                                File.Delete(newDocumentPath + "_temp.pdf");
                            }
                            else
                            {
                                using (PdfReader pdfReader = new PdfReader(newDocumentPath))
                                {
                                    PdfReader.unethicalreading = true;
                                    doc.PAGES = pdfReader.NumberOfPages;
                                    var listViewerSize = new List<ViewerPageSize>();
                                    for (int page = 1; page <= doc.PAGES; page++)
                                    {
                                        var realSize = pdfReader.GetPageSizeWithRotation(page);
                                        var viewerPageSize = new ViewerPageSize
                                        {
                                            PAGE = page,
                                            SIZE = $"{(int)realSize.Width}x{(int)realSize.Height}"
                                        };
                                        listViewerSize.Add(viewerPageSize);
                                    }
                                    doc.VIEWER_PAGES_SIZE = JsonConvert.SerializeObject(listViewerSize, new JsonSerializerSettings
                                    {
                                        NullValueHandling = NullValueHandling.Ignore,
                                        DefaultValueHandling = DefaultValueHandling.Ignore
                                    });
                                    pdfReader.Close();
                                    pdfReader.Dispose();
                                }
                            }
                        }
                        //Chuyển pdf sang image để view
                        new Thread(() =>
                        {
                            pdfToImage.ConvertPdfToPNG(newDocumentPath);
                        }).Start();
                        doc.STATUS = Constants.DOCUMENT_COMPLETED;

                        documentDAO.UpdateStatusDocument(doc);
                    }
                }
                catch (Exception ex)
                {
                    doc.STATUS = Constants.DOCUMENT_FAILED;
                    documentDAO.UpdateStatusDocument(doc);
                    string msg = "Lỗi chuyển đổi định dạng file tải lên sang file PDF";
                    ConfigHelper.Instance.WriteLogException(msg, ex, MethodBase.GetCurrentMethod().Name, "ConvertFiles");
                }
            }).Start();
        }

        /// <summary>
        /// render theo rabbitmq
        /// </summary>
        /// <param name="doc"></param>
        public void ConvertPdfToImage(DocumentBO doc)
        {
            DocumentDAO documentDAO = new DocumentDAO();
            new Thread(() =>
            {
                try
                {
                    string documentPath = $"{ConfigHelper.RootFolder}{ConfigHelper.DocumentRootFolder}{doc.PATH}";
                    string signedDocumentPath = $"{documentPath}{Constants.PDF_SIGNED_EXTENSION}";
                    //Kiểm tra xem file tài liệu này có tồn tại hay không?
                    if (File.Exists(documentPath))
                    {
                        //Kiểm tra xem file pdf mới này tồn tại hay không?
                        if (File.Exists(signedDocumentPath))
                        {
                            //Chuyển pdf sang image để view
                            pdfToImage.ConvertPdfToPNG(signedDocumentPath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    doc.STATUS = Constants.DOCUMENT_FAILED;
                    documentDAO.UpdateStatusDocument(doc);
                    string msg = "Lỗi chuyển đổi định dạng file tải lên sang file PDF";
                    ConfigHelper.Instance.WriteLogException(msg, ex, MethodBase.GetCurrentMethod().Name, "ConvertFiles");
                }
            }).Start();
        }

        #endregion

        protected class ViewerPageSize
        {
            public int PAGE { get; set; }
            public string SIZE { get; set; }
        }
    }
}
