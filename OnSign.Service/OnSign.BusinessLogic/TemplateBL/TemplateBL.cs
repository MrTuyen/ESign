using OnSign.BusinessObject.Account;
using OnSign.BusinessObject.TemplateBO;
using OnSign.Common.Helpers;
using OnSign.DataObject.ImportDAO;
using SAB.Library.Data;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.IO;
using FileIO = System.IO.File;
using OnSign.BusinessObject.Forms;
using OnSign.BusinessLogic.CommonBL;
using System.Threading;
using OnSign.BusinessObject.Sign;
using System.Runtime.InteropServices;
using Microsoft.Office.Interop.Word;
using iTextSharp.text.pdf;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Diagnostics;

namespace OnSign.BusinessLogic.TemplateBL
{
    public class TemplateBL : BaseBLL
    {
        #region Fields
        private readonly TemplateDAO templateDAO = new TemplateDAO();
        protected IData objDataAccess = null;
        private const string TEXT_FLAG_SIGN = "ky_chinh";
        private const string TEXT_FLAG_INITIAL = "ky_nhay";
        private const string ICON_PATH = "/Images/pdf.svg";
        private const string UUROW_ID = "uurowid";
        private const string DOC_2003_EXTENSION = ".doc";
        private const int ROW_HEADER_BOOKMARK = 1;
        private const int RROW_START_BOOKMARK = 2;
        private const int ROW_START_RECEIVE = 3;
        private const int COLUMN_START = 1;
        private const string FOLDER_NAME_DOCUMENT_SIGN = "DocumentSign";
        #endregion

        #region Properties

        #endregion

        #region Constructor
        public TemplateBL()
        {
        }

        public TemplateBL(IData objIData)
        {
            objDataAccess = objIData;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Lưu file word template
        /// </summary>
        /// <param name="doctemplate"></param>
        /// <param name="objUser"></param>
        /// <param name="directory"></param>
        public bool SaveTemplateToDB(ref DocumentTemplate doctemplate, AccountBO objUser)
        {
            try
            {
                var result = false;

                //Tạo đường dẫn file template
                var filePath = $"{ConfigHelper.FullDocument}{doctemplate.PATH}";

                object _pathFile = filePath;
                Type wordType = Type.GetTypeFromProgID("Word.Application");
                dynamic _applicationclass = Activator.CreateInstance(wordType);

                object readOnly = true;
                object missing = Type.Missing;
                dynamic _document = _applicationclass.Documents.Open(ref _pathFile, missing, readOnly);
                _applicationclass.Visible = false;


                if (CheckExistSignInFile(_document, filePath))
                {
                    doctemplate.CREATEDBYIP = objUser.IP;
                    doctemplate.CREATEDBYUSER = objUser.ID;
                    doctemplate.PAGES = GetPagesDocument(filePath);

                    //Lưu file word vào cơ sở dữ liệu
                    var docid = templateDAO.DocumentTemplateAdd(doctemplate);

                    //Lưu bookmark vào cơ sở dữ liệu
                    SaveBookmarkWord(_document, docid);

                    //Lấy lại file word từ DB
                    doctemplate = templateDAO.DocumentTemplateGetByID(docid);

                    result = true;
                }

                /*Getting an error in WdSaveOptions */
                object saveChanges = WdSaveOptions.wdDoNotSaveChanges;
                _document.Close(ref saveChanges);
                _document = null;
                _applicationclass.Quit();
                return result;
            }
            catch (Exception objEx)
            {
                ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi SaveTemplateToDB");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(
                    ErrorMsg,
                    objEx,
                    MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()),
                    NameSpace
                );

                return false;
            }
        }

        /// <summary>
        /// Lấy danh sách bookmarks của tệp word mẫu
        /// </summary>
        /// <param name="docid">ID của file word mẫu</param>
        /// <returns></returns>
        public List<DocumentTemplateBookmark> DocumentTemplateBookmarkGetByDocID(long docid)
        {
            try
            {
                TemplateDAO templateDAO = new TemplateDAO();
                var result = templateDAO.DocumentTemplateBookmarkGetByDocID(docid);
                return result;
            }
            catch (Exception objEx)
            {
                ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi ReadDataBookmark");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(
                    ErrorMsg,
                    objEx,
                    MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()),
                    NameSpace
                );
                throw objEx;
            }
        }

        /// <summary>
        /// Dọc file excel data import và save data vào cơ sở dữ liệu
        /// </summary>
        /// <param name="docid"></param>
        /// <param name="pages"></param>
        /// <param name="excelPath"></param>
        /// <param name="objUser"></param>
        /// <returns></returns>
        public List<DocumentTemplatePdf> ReadDataBookmark(long docid, int pages, string excelPath, AccountBO objUser, out bool excelNotMapColumn, out List<DocumentTemplateBookmark> bookmarks)
        {
            try
            {
                var filePdfs = new List<DocumentTemplatePdf>();
                var document = templateDAO.DocumentTemplateGetByID(docid);
                document.DocumentTemplateBookmarks = bookmarks = templateDAO.DocumentTemplateBookmarkGetByDocID(docid);
                excelNotMapColumn = true;

                //đọc file excel
                var dataExcels = ReadExcelDataBookmark(excelPath);

                foreach (IDictionary<string, object> excel in dataExcels)
                {
                    if (!bookmarks.Any(x => excel.ContainsKey(x.NAME)))
                    {
                        excelNotMapColumn = false;
                        break;
                    }
                }

                //số thứ tự khi hiển thị trên web
                var i = 1;

                //duyệt qua tường dòng trong data
                foreach (var row in dataExcels)
                {
                    //tạo tên file pdf
                    int unixTimestamp = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

                    //tạo đối tượng pdf để lưu và cơ sở dữ liệu
                    var pdf = new DocumentTemplatePdf
                    {
                        CREATEDBYUSER = objUser.ID,
                        CREATEDBYIP = objUser.IP,
                        PAGES = pages,
                        NAME = $"{Guid.NewGuid()}-{unixTimestamp}.pdf",
                        DOCTEMPLATEID = docid,
                        ICON = ICON_PATH,
                        DocumentDatas = CovertToDocumentTemplateData(row, docid),
                        TempID = Guid.NewGuid()
                    };

                    filePdfs.Add(pdf);
                    i++;
                }

                //trả về danh sách file pdf
                return filePdfs;
            }
            catch (Exception objEx)
            {
                ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi ReadDataBookmark");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(
                    ErrorMsg,
                    objEx,
                    MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()),
                    NameSpace
                );
                excelNotMapColumn = false;
                bookmarks = new List<DocumentTemplateBookmark>();
                return new List<DocumentTemplatePdf>();
            }
        }

        /// <summary>
        /// Lưu dữ liệu bookmark
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        public List<DocumentTemplatePdf> SaveImportDataBookmarkToDB(DocumentTemplate document, AccountBO currentUser)
        {
            try
            {
                var result = new List<DocumentTemplatePdf>();
                foreach (var pdf in document.DocumentTemplatePdfs)
                {
                    pdf.PAGES = document.PAGES;
                    pdf.CREATEDBYUSER = currentUser.ID;
                    pdf.CREATEDBYIP = MethodHelper.GetIPClient();

                    //Nếu đã tồn tại records thì cập nhật 
                    if (pdf?.ID > 0)
                    {
                        //lưu data file pdf vào cơ sở dữ liệu
                        templateDAO.DocumentTemplatePdfUpdate(pdf);
                        templateDAO.DocumentTemplateDataDelete(pdf.ID);
                    }
                    else
                    {
                        //Lưu data file pdf vào cơ sở dữ liệu
                        pdf.ID = templateDAO.DocumentTemplatePdfAdd(pdf);
                    }

                    // duyệt từng phần tử data trong excel và lưu data vào cơ sở dữ liệu
                    foreach (var data in pdf.DocumentDatas)
                    {
                        data.PDFID = pdf.ID;
                        data.DOCID = pdf.DOCTEMPLATEID;
                        templateDAO.DocumentTemplateDataAdd(data);
                    }

                    result.Add(templateDAO.DocumentTemplatePdfGetByID(pdf.ID));
                }
                return result;
            }
            catch (Exception objEx)
            {
                ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi SaveImportDataBookmarkToDB");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(
                    ErrorMsg,
                    objEx,
                    MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()),
                    NameSpace
                );
                return new List<DocumentTemplatePdf>();
            }
        }

        /// <summary>
        /// Tạo lô file pdf
        /// </summary>
        /// <param name="document"></param>
        /// <param name="objUser"></param>
        /// <param name="directory"></param>
        public void GeneratePDFs(
          DocumentTemplate document,
          string directory,
          AccountBO objUser
        )
        {
            try
            {
                //Duyệt danh sách file cần tạo
                foreach (var pdfFile in document.DocumentTemplatePdfs)
                {
                    //Tiến hành tạo từng file
                    GeneratePDF(directory, document.PATH, pdfFile);
                }
            }
            catch (Exception objEx)
            {
                ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi CreatePDFs");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(
                    ErrorMsg,
                    objEx,
                    MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()),
                    NameSpace
                );
            }
        }

        /// <summary>
        /// Tạo đơn lẻ file pdf
        /// </summary>
        /// <param name="docPath"></param>
        /// <param name="pdfFile"></param>
        /// <param name="objUser"></param>
        public void GeneratePDF(
           string directory,
           string docPath,
           DocumentTemplatePdf pdfFile
        )
        {
            try
            {
                AutoResetEvent mainEvent = new AutoResetEvent(false);

                if (!pdfFile.STATUS)
                {
                    //lấy thư mục chứa file
                    var dir = Path.GetDirectoryName(docPath);
                    docPath = $"{directory}{docPath}";
                    var tempPath = $"{directory}{dir}\\temp";

                    //tạo thư mục tạm
                    if (!Directory.Exists(tempPath))
                    {
                        Directory.CreateDirectory(tempPath);
                    }

                    new Thread(() =>
                    {
                        try
                        {
                            //Lấy thông tin file 
                            var docFileInfo = new FileInfo(docPath);

                            //Khởi tạo tên file tạm
                            object _pathFile = $"{tempPath}\\{Guid.NewGuid()}{docFileInfo.Extension}";

                            //Copy file template vào thư mục tạm
                            FileIO.Copy(docPath, _pathFile.ToString(), true);

                            //Đọc file word   
                            Type wordType = Type.GetTypeFromProgID("Word.Application");
                            dynamic _applicationclass = Activator.CreateInstance(wordType);
                            var _document = _applicationclass.Documents.Open(ref _pathFile);
                            _applicationclass.Visible = false;

                            //var _document = _applicationclass.ActiveDocument;

                            //Get page của file word
                            var _page = _document.PageSetup;

                            //Tìm và add dữ liệu cho bookmark 
                            ProcessBookmark(pdfFile, _document);

                            //lấy thông file 
                            FileInfo fileInfo = new FileInfo(_pathFile.ToString());

                            //Get sign trong file word và lưu sign vào cơ sở dữ liệu
                            ProcessSign(pdfFile, _applicationclass, _document, _page, fileInfo.Extension == DOC_2003_EXTENSION);

                            var pdfFileUUID = $"{dir}\\{Guid.NewGuid()}{OnSign.Common.Constants.PDF_EXTENSION}";

                            //Khởi tạo path cho file pdf
                            var _pdfPath = $"{directory}{pdfFileUUID}";

                            //Xuất ra file pdf
                            _document.ExportAsFixedFormat(_pdfPath, WdExportFormat.wdExportFormatPDF);

                            //Lấy thông tin file 
                            var pdfFileInfo = new FileInfo(_pdfPath);

                            //Set lại trạng thái và update trạng thái trong cơ sở dữ liệu
                            pdfFile.STATUS = true;
                            pdfFile.SIZE = pdfFileInfo.Length;
                            pdfFile.NAME = pdfFile.NAME;
                            pdfFile.PATH = pdfFileUUID;
                            pdfFile.COMPLETEPERCENT = 100;
                            templateDAO.DocumentTemplatePdfUpdate(pdfFile);

                            /*Getting an error in WdSaveOptions */
                            object saveChanges = WdSaveOptions.wdDoNotSaveChanges;
                            _document.Close(ref saveChanges);
                            _applicationclass.Quit();

                            //Xóa file word tạm
                            if (FileIO.Exists(_pathFile.ToString()))
                                FileIO.Delete(_pathFile.ToString());
                        }
                        catch (Exception objEx)
                        {
                            ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi CreatePDF in new theard");
                            objResultMessageBO = ConfigHelper.Instance.WriteLogException(
                                ErrorMsg,
                                objEx,
                                MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()),
                                NameSpace
                            );
                        }

                    }).Start();
                }
            }
            catch (Exception objEx)
            {
                ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi CreatePDF");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(
                    ErrorMsg,
                    objEx,
                    MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()),
                    NameSpace
                );
            }
        }

        /// <summary>
        /// Lấy dữ liệu quản lý file đã tạo
        /// </summary>
        /// <param name="template"></param>
        public DocumentTemplate DocumentTemplatePdfGetView(DocumentTemplate template)
        {
            try
            {
                var result = new DocumentTemplate();
                result = templateDAO.DocumentTemplateGetByID(template.ID);
                result.DocumentTemplatePdfs = new List<DocumentTemplatePdf>();
                result.DocumentTemplatePdfs = templateDAO.DocumentTemplatePdfGetByDoctemplateID(template.ID);
                return result;
            }
            catch (Exception objEx)
            {
                ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi DocumentTemplatePdfGetView");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(
                    ErrorMsg,
                    objEx,
                    MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()),
                    NameSpace
                );
                return new DocumentTemplate();
            }

        }

        /// <summary>
        /// Lấy danh sách mẫu tài liệu theo người đăng nhập
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public List<DocumentTemplate> DocumentTemplateGetByUser(FormSearch form)
        {
            try
            {
                var documentTemplates = templateDAO.DocumentTemplateGetByUser(form);
                return documentTemplates;
            }
            catch (Exception objEx)
            {
                ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi lấy danh sách mẫu tài liệu");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(ErrorMsg, objEx, null, null);
                throw objEx;
            }
        }

        /// <summary>
        /// Lấy dữ liệu file đã tạo theo ID của mẫu
        /// </summary>
        /// <param name="docId"></param>
        /// <returns></returns>
        public List<DocumentTemplatePdf> DocumentTemplatePdfGetByDoctemplateID(long docId)
        {
            try
            {
                var rs = templateDAO.DocumentTemplatePdfGetByDoctemplateID(docId);
                return rs;
            }
            catch (Exception objEx)
            {
                ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi DocumentTemplatePdfGetByDoctemplateID");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(
                    ErrorMsg,
                    objEx,
                    MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()),
                    NameSpace
                );

                return new List<DocumentTemplatePdf>();
            }

        }

        /// <summary>
        /// Lấy dữ lệu file đã tạo bằng ID
        /// </summary>
        /// <param name="pdfId"></param>
        /// <returns></returns>
        public DocumentTemplatePdf DocumentTemplatePdfID(long pdfId)
        {
            try
            {
                var rs = templateDAO.DocumentTemplatePdfGetByID(pdfId);
                return rs;
            }
            catch (Exception objEx)
            {
                ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi DocumentTemplatePdfID");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(
                    ErrorMsg,
                    objEx,
                    MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()),
                    NameSpace
                );

                return new DocumentTemplatePdf();
            }

        }

        /// <summary>
        /// Tạo yêu cầu ký
        /// </summary>
        /// <param name="filePdf"></param>
        /// <param name="objUser"></param>
        /// <returns></returns>
        public RequestSignBO CreateRequestSign(
            DocumentTemplatePdf filePdf,
            AccountBO objUser,
            string subject,
            string contnet,
            ref List<DocumentTemplateReceive> receives,
            string requestUuid,
            string rootFolder
        )
        {
            try
            {
                //lấy dữ liệu pdf từ DB
                var _pdfDB = templateDAO.DocumentTemplatePdfGetByID(filePdf.ID);
                var uuid = requestUuid ?? MethodHelper.GenerateUUID();
                string pathDocSign = CopyNewFileForSign(rootFolder, _pdfDB, uuid, objUser);

                //xóa danh sách người gởi thất bại
                var _receives = templateDAO.DocumentTemplateReceiveGetByRequestUuid(requestUuid);
                templateDAO.DocumentTemplateReceiveDeleteByRequestUuid(requestUuid);

                if (_receives.Count > 0)
                    filePdf.Receives.ForEach(x => x.ID = 0);

                //lấy dữ liệu chữ ký template
                var _signs = templateDAO.DocumentTemplatePdfSignGetByPdfID(filePdf.ID);

                //tạo đối tượng requestsignbo
                var request = new RequestSignBO(true)
                {
                    UUID = filePdf.REQUEST_UUID = uuid,
                    EMAIL = objUser.EMAIL,
                    FULLNAME = objUser.FULLNAME,
                    EMAILSUBJECT = subject,
                    EMAILMESSAGES = contnet
                };

                //tạo đôi tượng documentbo
                var doc = new DocumentBO
                {
                    CREATEDBYIP = _pdfDB.CREATEDBYIP,
                    CREATEDBYUSER = _pdfDB.CREATEDBYUSER,
                    PATH = pathDocSign,
                    PAGES = _pdfDB.PAGES,
                    SIZE = _pdfDB.SIZE,
                    PDFPATH = pathDocSign,
                    ICON = _pdfDB.ICON,
                    NUMBEROFPAGES = _pdfDB.PAGES,
                    NAME = _pdfDB.NAME,
                    UUID = request.UUID
                };

                //duyệt thông người nhận
                foreach (var _rec in filePdf.Receives)
                {
                    //insert vào cơ sở dữ liệu
                    UpdateOrInsertReceive(subject, contnet, request, _rec);

                    //tạo đối tượng documentsignBo
                    InitDocumentSignBO(_pdfDB, _signs, doc, _rec, request.UUID);

                    //tạo đối tượng mailtobo
                    InitMailToBO(_pdfDB, request, _rec);
                }

                //thêm documentbo vào request
                request.FILEUPLOADS.Add(doc);

                receives = templateDAO.DocumentTemplateReceiveGetByRequestUuid(request.UUID);
                return request;
            }
            catch (Exception objEx)
            {
                ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi CreateRequest");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(
                    ErrorMsg,
                    objEx,
                    MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()),
                    NameSpace
                );
                return new RequestSignBO();
            }
        }

        /// <summary>
        /// Xóa file đã tạo
        /// </summary>
        /// <param name="pdf"></param>
        /// <returns></returns>
        public bool DocumentTemplatePdfDelete(DocumentTemplatePdf pdf, string directory)
        {
            try
            {
                var pathFile = $"{directory}{pdf.PATH}{OnSign.Common.Constants.PDF_EXTENSION}";

                if (FileIO.Exists(pathFile))
                {
                    using (var _pdfReader = new PdfReader(pathFile))
                    {
                        PdfReader.unethicalreading = true;
                        for (int i = 1; i <= _pdfReader.NumberOfPages; i++)
                        {
                            var bigThumbPath = $"{pathFile}-big_thumb_{i}.png";
                            var smallThumbPath = $"{pathFile}-small_thumb_{i}.png";
                            DeleteFile(bigThumbPath);
                            DeleteFile(smallThumbPath);
                        }
                    }
                }

                DeleteFile(pathFile);

                return templateDAO.DocumentTemplateDataDelete(pdf.ID) &&
                    templateDAO.DocumentTemplatePdfDelete(pdf);
            }
            catch (Exception objEx)
            {
                ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi DocumentTemplatePdfDelete");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(
                    ErrorMsg,
                    objEx,
                    MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()),
                    NameSpace
                );
                return false;
            }
        }

        /// <summary>
        /// Xóa template
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public bool DocumentTemplateDelete(DocumentTemplate doc, string directory)
        {
            try
            {
                var filePath = $"{directory}{doc.PATH}";
                DeleteFile(filePath);

                var pdfs = templateDAO.DocumentTemplatePdfGetByDoctemplateID(doc.ID);
                foreach (var pdf in pdfs)
                {
                    templateDAO.DocumentTemplateDataDelete(pdf.ID);
                }

                return templateDAO.DocumentTemplatePdfDeleteByDocId(doc.ID) &&
                     templateDAO.DocumentTemplateBookmarkDeleteByDocID(doc.ID) &&
                     templateDAO.DocumentTemplateDelete(doc);
            }
            catch (Exception objEx)
            {
                ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi DocumentTemplatePdfDelete");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(
                    ErrorMsg,
                    objEx,
                    MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()),
                    NameSpace
                );
                return false;
            }
        }

        /// <summary>
        /// Đọc file excel dữ người nhận
        /// </summary>
        /// <param name="excelPath"></param>
        /// <param name="pdfId"></param>
        /// <param name="objUser"></param>
        /// <returns></returns>
        //public List<DocumentTemplateReceive> ReadExcelDataReceive(string excelPath, long pdfId, AccountBO objUser)
        //{
        //    Type wordType = Type.GetTypeFromProgID("Excel.Application");
        //    dynamic application = Activator.CreateInstance(wordType);
        //    try
        //    {
        //        //Khởi tạo list data chứa data đọc từ excel.
        //        var receives = new List<DocumentTemplateReceive>();
        //        if (FileIO.Exists(excelPath))
        //        {
        //            //Đọc file excel                  
        //            var xwb = application.Workbooks.Open(excelPath, ReadOnly: true);
        //            application.Visible = false;

        //            //Get sheet ragne, row, col.
        //            var xlwsheet = xwb.Worksheets[1];
        //            var xlRange = xlwsheet.UsedRange;

        //            var rowCount = xlRange.Rows.Count;

        //            //Duyệt qua từng row có dữ liệu của excel.
        //            for (var i = ROW_START_RECEIVE; i <= rowCount; i++)
        //            {
        //                //Kiểm tra email người gởi có trùng với email người nhận không
        //                if (xlRange.Cells[i, 2].Value == objUser.EMAIL)
        //                {
        //                    return new List<DocumentTemplateReceive>();
        //                }

        //                //Tiến hành đọc file excel
        //                ProcessExcel(pdfId, objUser, receives, xlRange, i);
        //            }

        //            xwb.Close();
        //            application.Quit();
        //            if (FileIO.Exists(excelPath))
        //            {
        //                FileIO.Delete(excelPath);
        //            }
        //        }

        //        return receives;
        //    }
        //    catch (Exception objEx)
        //    {
        //        ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi ReadExcelDataReceive");
        //        objResultMessageBO = ConfigHelper.Instance.WriteLogException(
        //            ErrorMsg,
        //            objEx,
        //            MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()),
        //            NameSpace
        //        );
        //        return new List<DocumentTemplateReceive>();
        //    }
        //    //finally
        //    //{
        //    //    application.Quit();
        //    //    Marshal.ReleaseComObject(application);
        //    //}
        //}

        private void ReadExcel(string fileName)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (SpreadsheetDocument doc = SpreadsheetDocument.Open(fs, false))
                {
                    WorkbookPart workbookPart = doc.WorkbookPart;
                    SharedStringTablePart sstpart = workbookPart.GetPartsOfType<SharedStringTablePart>().First();
                    SharedStringTable sst = sstpart.SharedStringTable;

                    WorksheetPart worksheetPart = workbookPart.WorksheetParts.First();
                    Worksheet sheet = worksheetPart.Worksheet;

                    var cells = sheet.Descendants<DocumentFormat.OpenXml.Spreadsheet.Cell>();
                    var rows = sheet.Descendants<DocumentFormat.OpenXml.Spreadsheet.Row>();

                    Debug.WriteLine("Row count = {0}", rows.LongCount());
                    Debug.WriteLine("Cell count = {0}", cells.LongCount());

                    // One way: go through each cell in the sheet
                    foreach (DocumentFormat.OpenXml.Spreadsheet.Cell cell in cells)
                    {
                        if ((cell.DataType != null) && (cell.DataType == CellValues.SharedString))
                        {
                            int ssid = int.Parse(cell.CellValue.Text);
                            string str = sst.ChildElements[ssid].InnerText;
                            Debug.WriteLine("Shared string {0}: {1}", ssid, str);
                        }
                        else if (cell.CellValue != null)
                        {
                            Debug.WriteLine("Cell contents: {0}", cell.CellValue.Text);
                        }
                    }

                    // Or... via each row
                    //foreach (DocumentFormat.OpenXml.Spreadsheet.Row row in rows)
                    //{
                    //    foreach (DocumentFormat.OpenXml.Spreadsheet.Cell c in row.Elements<DocumentFormat.OpenXml.Spreadsheet.Cell>())
                    //    {
                    //        if ((c.DataType != null) && (c.DataType == CellValues.SharedString))
                    //        {
                    //            int ssid = int.Parse(c.CellValue.Text);
                    //            string str = sst.ChildElements[ssid].InnerText;
                    //            Debug.WriteLine("each row Shared string {0}: {1}", ssid, str);
                    //        }
                    //        else if (c.CellValue != null)
                    //        {
                    //            Debug.WriteLine("each row Cell contents: {0}", c.CellValue.Text);
                    //        }
                    //    }
                    //}
                }
            }
        }


        /// <summary>
        /// Đọc dữ liệu người nhận
        /// </summary>
        /// <param name="excelPath"></param>
        /// <param name="objUser"></param>
        /// <returns></returns>
        public List<DocumentTemplatePdf> ReadExcelDataReceive(string excelPath, long pdf_id, AccountBO objUser)
        {
            var result = new List<DocumentTemplatePdf>();
            try
            {
                //Khởi tạo list data chứa data đọc từ excel.
                var receives = new List<DocumentTemplateReceive>();
                if (FileIO.Exists(excelPath))
                {
                    //Đọc file excel
                    Type wordType = Type.GetTypeFromProgID("Excel.Application");
                    dynamic application = Activator.CreateInstance(wordType);
                    var xwb = application.Workbooks.Open(excelPath, ReadOnly: true);
                    application.Visible = false;

                    //Get sheet ragne, row, col.
                    var xlRange = xwb.Worksheets[1].UsedRange;
                    var rowCount = xlRange.Rows.Count;

                    for (var i = ROW_START_RECEIVE; i <= rowCount; i++)
                    {
                        //Kiểm tra email người gởi có trùng với email người nhận không
                        if (xlRange.Cells[i, 2].Value == objUser.EMAIL)
                        {
                            return result;
                        }
                        //Tiến hành xử lý file excel
                        this.ProcessExcel(pdf_id, objUser, receives, xlRange, i);
                    }

                    if (pdf_id > 0)
                    {
                        var currentContract = templateDAO.DocumentTemplatePdfGetByID(pdf_id);
                        if (currentContract != null)
                        {
                            string subject = receives.FirstOrDefault().SUBJECT;
                            if (string.IsNullOrEmpty(subject))
                                subject = currentContract.NAME;
                            string content = receives.FirstOrDefault().CONTENT;

                            currentContract.EMAIL_SUBJECT = subject;
                            currentContract.EMAIL_CONTENT = content;
                            currentContract.Receives = receives;
                            result.Add(currentContract);
                        }
                    }
                    else
                    {
                        var IDs = receives.GroupBy(x => x.IDCONTRACT, (key, group) => new
                        {
                            ID_CONTRACT = key,
                            ID_GROUP = group.ToList()
                        }).ToList();

                        foreach (var id_contract in IDs)
                        {
                            string idContract = id_contract.ID_CONTRACT;
                            if (string.IsNullOrEmpty(idContract))
                            {
                                return result;
                            }
                            var currentContract = templateDAO.DocumentTemplatePdfGetByIdContract(idContract);
                            if (currentContract != null)
                            {
                                string subject = id_contract.ID_GROUP.FirstOrDefault().SUBJECT;
                                if (string.IsNullOrEmpty(subject))
                                    subject = currentContract.NAME;
                                string content = id_contract.ID_GROUP.FirstOrDefault().CONTENT;
                                currentContract.EMAIL_SUBJECT = subject;
                                currentContract.EMAIL_CONTENT = content;
                                currentContract.Receives = id_contract.ID_GROUP;
                                result.Add(currentContract);
                            }
                        }
                    }
                    xwb.Close();
                    application.Quit();

                    if (FileIO.Exists(excelPath))
                    {
                        FileIO.Delete(excelPath);
                    }
                }

                return result;
            }
            catch (Exception objEx)
            {
                ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi ReadExcelDataReceive");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(
                    ErrorMsg,
                    objEx,
                    MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()),
                    NameSpace
                );
                return result;
            }

        }

        /// <summary>
        /// Lấy danh sách pdfs
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public DocumentTemplate DocumentTemplatePdfForView(FormSearch form)
        {
            try
            {
                var doc_tem_id = form.DOCID.Value;

                var result = templateDAO.DocumentTemplateGetByID(doc_tem_id);
                result.DocumentTemplatePdfs = templateDAO.DocumentTemplatePdfGetByDoctemplateIDPagging(form);
                result.DocumentTemplateBookmarks = templateDAO.DocumentTemplateBookmarkGetByDocID(doc_tem_id);
                return result;
            }
            catch (Exception objEx)
            {
                ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi DocumentTemplatePdfForView");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(
                    ErrorMsg,
                    objEx,
                    MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()),
                    NameSpace
                );
                return new DocumentTemplate();
            }
        }

        /// <summary>
        /// lấy dữ liệu theo Mã định danh
        /// </summary>
        /// <param name="idContract"></param>
        /// <returns></returns>
        public DocumentTemplatePdf DocumentTemplatePdfGetByIdContract(string idContract)
        {
            try
            {
                return templateDAO.DocumentTemplatePdfGetByIdContract(idContract);
            }
            catch (Exception objEx)
            {
                ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi DocumentTemplatePdfGetByIdContract");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(
                    ErrorMsg,
                    objEx,
                    MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()),
                    NameSpace
                );
                return new DocumentTemplatePdf();
            }
        }

        /// <summary>
        /// lấy danh sách file pdfs chưa được tạo
        /// </summary>
        /// <param name="docid"></param>
        /// <returns></returns>
        public List<DocumentTemplatePdf> DocumentTemplatePdfsGetStatusFalse(long docid)
        {
            try
            {
                return templateDAO.DocumentTemplatePdfGetStatusFalse(docid);
            }
            catch (Exception objEx)
            {
                ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi DocumentTemplatePdfGetByIdContract");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(
                    ErrorMsg,
                    objEx,
                    MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()),
                    NameSpace
                );
                return new List<DocumentTemplatePdf>();
            }
        }

        /// <summary>
        /// Cập lại trạng thai khi gởi lỗi
        /// </summary>
        /// <param name="receives"></param>
        /// <returns></returns>
        public bool UpdateStatusReceives(
            List<DocumentTemplateReceive> receives,
            bool sent
        )
        {
            try
            {
                foreach (var _receive in receives)
                {
                    _receive.SENT = sent;
                    templateDAO.DocumentTemplateReceiveUpdate(_receive);
                }
                return true;
            }
            catch (Exception objEx)
            {
                ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi DocumentTemplatePdfForView");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(
                    ErrorMsg,
                    objEx,
                    MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()),
                    NameSpace
                );
                return false;
            }
        }

        /// <summary>
        /// Xử lý file excel lấy thông tin người nhận, thứ tự ký kết
        /// </summary>
        /// <param name="pdfId">ID của file PDF</param>
        /// <param name="objUser">Current User</param>
        /// <param name="receives">Danh sách người nhận</param>
        /// <param name="xlRange">Thông tin Excel</param>
        /// <param name="rowIndex">Dòng hiện tại trong excel</param>
        private void ProcessExcel(long pdfId, AccountBO objUser, List<DocumentTemplateReceive> receives, dynamic xlRange, int rowIndex)
        {
            try
            {
                //Kiểm tra xem Tên và địa chỉ Email có rỗng hay không
                if (string.IsNullOrEmpty(xlRange.Cells[rowIndex, 4].Value) || string.IsNullOrEmpty(xlRange.Cells[rowIndex, 5].Value))
                    return;

                //Nếu là doanh nghiệp => mã số thuế không được rỗng (doanh nghiệp: value = 1)
                if (xlRange.Cells[rowIndex, 7].Value + string.Empty == "1" &&
                    string.IsNullOrEmpty(xlRange.Cells[rowIndex, 8].Value + string.Empty))
                    return;

                //Nếu là cá nhân => Địa chỉ, số điện thoại, email không được rỗng (cá nhân value = 0)
                if (xlRange.Cells[rowIndex, 7].Value + string.Empty == "0" &&
                    string.IsNullOrEmpty(xlRange.Cells[rowIndex, 9].Value + string.Empty) &&
                    string.IsNullOrEmpty(xlRange.Cells[rowIndex, 10].Value + string.Empty) &&
                    string.IsNullOrEmpty(xlRange.Cells[rowIndex, 11].Value + string.Empty))
                    return;



                //Tạo đối tượng chứ data.
                var receive = new DocumentTemplateReceive
                {
                    PDFID = pdfId,
                    TempID = Guid.NewGuid(),
                    EMAILFROM = objUser.EMAIL,
                    NAMEFROM = objUser.FULLNAME,
                    //Tệp tài liệu
                    IDCONTRACT = xlRange.Cells[rowIndex, 1].Value + string.Empty,
                    //Tiêu đề 
                    SUBJECT = xlRange.Cells[3, 2].Value + string.Empty,
                    //Tin nhắn
                    CONTENT = xlRange.Cells[3, 3].Value + string.Empty,
                    //Tên người nhận
                    NAMETO = (xlRange.Cells[rowIndex, 4].Value + string.Empty).ToUpper(),
                    //Email người nhận
                    EMAILTO = (xlRange.Cells[rowIndex, 5].Value + string.Empty).ToLower(),
                    //Thứ tự ký kết
                    SIGNINDEX = xlRange.Cells[rowIndex, 6].Value == null ? null : int.Parse(xlRange.Cells[rowIndex, 6].Value + string.Empty),
                    //Đối tượng tham gia ký kết: Doanh nghiệp - cá nhân
                    REQUESTSIGNTYPE = xlRange.Cells[rowIndex, 7].Value == null ? Common.Constants.REQUESTSIGNTYPE_ISCC :
                                       (xlRange.Cells[rowIndex, 7].Value + string.Empty == "1" ? Common.Constants.REQUESTSIGNTYPE_HSM : Common.Constants.REQUESTSIGNTYPE_ONSIGN),
                    //Mã số thuế của doanh nghiệp
                    TAXCODE = xlRange.Cells[rowIndex, 8].Value + string.Empty,
                    //Địa chỉ cá nhân
                    ADDRESS = xlRange.Cells[rowIndex, 9].Value + string.Empty,
                    //Số CMND/Thẻ căn cước  cá nhân
                    IDNUMBER = xlRange.Cells[rowIndex, 10].Value + string.Empty,
                    //Số điện thoại cá nhân
                    PHONENUMBER = xlRange.Cells[rowIndex, 11].Value + string.Empty,
                };

                if (xlRange.Cells[rowIndex, 7].Value == null || (xlRange.Cells[rowIndex, 7].Value + string.Empty != "0" && xlRange.Cells[rowIndex, 7].Value + string.Empty != "1"))
                    receive.REQUESTSIGNTYPE = Common.Constants.REQUESTSIGNTYPE_ISCC;

                //Nếu là doanh nghiệp
                if (xlRange.Cells[rowIndex, 7].Value + string.Empty == "1")
                    receive.REQUESTSIGNTYPE = Common.Constants.REQUESTSIGNTYPE_HSM;
                //Nếu là cá nhân
                else if (xlRange.Cells[rowIndex, 7].Value + string.Empty == "0")
                    receive.REQUESTSIGNTYPE = Common.Constants.REQUESTSIGNTYPE_ONSIGN;
                receives.Add(receive);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Dọc excel dữ liệu bookmark
        /// </summary>
        /// <param name="excelPath"></param>
        /// <param name="docid"></param>
        /// <returns></returns>
        private List<ExpandoObject> ReadExcelDataBookmark(string excelPath)
        {
            //Khởi tạo list data chứa data đọc từ excel.
            var bookmarkExcels = new List<ExpandoObject>();
            if (FileIO.Exists(excelPath))
            {
                //Đọc file excel
                Type wordType = Type.GetTypeFromProgID("Excel.Application");
                dynamic application = Activator.CreateInstance(wordType);
                var xwb = application.Workbooks.Open(excelPath, ReadOnly: true);
                application.Visible = false;

                //Get sheet ragne, row, col.
                var xlwsheet = xwb.Worksheets[1];
                var xlRange = xlwsheet.UsedRange;
                var rowCount = xlRange.Rows.Count;
                var colCount = xlRange.Columns.Count;

                //Duyệt qua từng row có dữ liệu của excel.
                for (var i = RROW_START_BOOKMARK; i <= rowCount; i++)
                {
                    //Tạo đối tượng chứ data.
                    IDictionary<string, object> obj = new ExpandoObject();
                    for (var j = COLUMN_START; j <= colCount; j++)
                    {
                        var _value = xlRange.Cells[ROW_HEADER_BOOKMARK, j].Value + string.Empty;
                        if (!string.IsNullOrEmpty(_value))
                        {
                            obj.Add(_value, string.Empty);
                        }
                    }

                    //flag kiểm row rỗng.
                    var isNull = true;
                    for (var j = 0; j < obj.Keys.Count; j++)
                    {
                        var _value = xlRange.Cells[i, j + COLUMN_START].Value + string.Empty;
                        if (!string.IsNullOrEmpty(_value))
                        {
                            isNull = false;
                        };
                    }

                    //Nếu row ko rỗng.
                    if (!isNull)
                    {
                        //Duyệt tứng col add và obj.
                        for (var j = 0; j < obj.Keys.Count; j++)
                        {
                            var _value = xlRange.Cells[i, j + COLUMN_START].Value + string.Empty;
                            obj[obj.ElementAt(j).Key] = _value;
                        }

                        obj["uurowid"] = Guid.NewGuid();
                        bookmarkExcels.Add((ExpandoObject)obj);
                    }
                }

                xwb.Close();
                application.Quit();
                if (FileIO.Exists(excelPath))
                {
                    FileIO.Delete(excelPath);
                }
            }

            return bookmarkExcels;
        }

        /// <summary>
        /// Lưu người gởi vào cơ sở dữ liệu
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="contnet"></param>
        /// <param name="request"></param>
        /// <param name="_rec"></param>
        private void UpdateOrInsertReceive(
            string subject,
            string contnet,
            RequestSignBO request,
            DocumentTemplateReceive _rec
        )
        {
            _rec.SUBJECT = subject;
            _rec.CONTENT = contnet;
            _rec.SENT = false;
            _rec.REQUESTUUID = request.UUID;

            if (_rec.ID > 0)
            {
                templateDAO.DocumentTemplateReceiveUpdate(_rec);
            }
            else
            {
                templateDAO.DocumentTemplateReceiveAdd(_rec);
            }
        }

        /// <summary>
        /// Tìm và set dữ liệu bookmark
        /// </summary>
        /// <param name="pdfFile"></param>
        /// <param name="_document"></param>
        private void ProcessBookmark(DocumentTemplatePdf pdfFile, dynamic _document)
        {
            //Tìm và set data cho bookmarks.
            pdfFile.DocumentDatas = templateDAO.Get_DocumentTemplateDataBookmarks_By_PdfId(pdfFile.ID);
            pdfFile.IDCONTRACT = MethodHelper.GenerateReferenceCode();
            foreach (var obj in pdfFile.DocumentDatas)
            {
                foreach (var _bm in _document.Bookmarks)
                {
                    if (_bm.Name.ToLower().Trim() == obj.NAME.ToLower().Trim())
                    {
                        _bm.Range.Text = obj.VALUE;
                    }
                }
            }
        }

        public List<DocumentTemplateData> Get_DocumentTemplateDataBookmarks_By_PdfId(long pdf_id)
        {
            try
            {
                var result = templateDAO.Get_DocumentTemplateDataBookmarks_By_PdfId(pdf_id);
                return result;
            }
            catch (Exception objEx)
            {
                ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi lấy danh sách data bookmarks");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(
                    ErrorMsg,
                    objEx,
                    MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()),
                    NameSpace
                );
                throw objEx;
            }
        }


        /// <summary>
        /// Lưu dữ liệu bookmard vào cơ sở dữ liệu
        /// </summary>
        /// <param name="_document"></param>
        /// <param name="docid"></param>
        private void SaveBookmarkWord(dynamic _document, long docid)
        {
            foreach (var _bm in _document.Bookmarks)
            {
                var bookmark = new DocumentTemplateBookmark()
                {
                    DOCTEMPLATEID = docid,
                    NAME = _bm.Name
                };

                templateDAO.DocumentTemplateBookmarkAdd(bookmark);
            }
        }

        /// <summary>
        /// Kiểm tra file đã bookmarks chữ ký hay chưa?
        /// </summary>
        /// <param name="_document"></param>
        /// <returns></returns>
        private bool CheckExistSignInFile(dynamic _document, string path)
        {
            //Chữ ký
            var shapes = _document.Shapes;
            FileInfo fileInfo = new FileInfo(path);
            if (fileInfo.Extension.ToLower() == DOC_2003_EXTENSION.ToLower())
            {
                for (int i = 1; i <= shapes.Count; i++)
                {
                    string title;
                    var shape = shapes[i];
                    title = GetSign(shape, true);
                    if (!string.IsNullOrEmpty(title) &&
                        (title.Trim().ToLower() == TEXT_FLAG_INITIAL || title.Trim().ToLower() == TEXT_FLAG_SIGN))
                    {
                        return true;
                    }
                }
            }
            else
            {
                for (int i = 1; i <= shapes.Count; i++)
                {
                    var shape = shapes[i];
                    string title = GetTitle(shape);
                    if (title.Trim().ToLower() == TEXT_FLAG_INITIAL || title.Trim().ToLower() == TEXT_FLAG_SIGN)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Lấy chữ ký word 2003
        /// </summary>
        /// <param name="shape"></param>
        /// <param name="getName">if false will get index else get name</param>
        /// <returns></returns>
        private static string GetSign(dynamic shape, bool getName)
        {
            var altText = shape.AlternativeText;
            return getName ? altText?.Split(':').Length > 1 ?

                    //lấy chữ ký
                    altText?.Split(':')[1].Split('-').Length > 0 ?
                    altText?.Split(':')[1].Split('-')[0].Trim() :
                    altText?.Split(':')[1].Trim() :

                    //lấy thứ tự ký
                    altText.Trim() : altText?.Split(':').Length > 2 ?
                    altText?.Split(':')[2].Trim() :
                    altText?.Split(':')[1].Trim();
        }

        /// <summary>
        /// Copy file để ký kết
        /// </summary>
        /// <param name="rootFolder"></param>
        /// <param name="_pdfDB"></param>
        /// <returns></returns>
        private string CopyNewFileForSign(
            string rootFolder,
            DocumentTemplatePdf _pdfDB,
            string uuid,
            AccountBO objUser
        )
        {
            var pathOrigin = $"{rootFolder}{_pdfDB.PATH}";
            var directory = $@"{rootFolder}\{objUser.ID}\{uuid}";

            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

            var fileName = $@"{MethodHelper.GenerateUUID()}{OnSign.Common.Constants.PDF_EXTENSION}";
            var pathDocSign = $@"\{objUser.ID}\{uuid}\{fileName}";
            var pathSave = ($@"{directory}\{fileName}{OnSign.Common.Constants.PDF_EXTENSION}");
            if (FileIO.Exists(pathOrigin))
            {
                //FileIO.Copy(pathOrigin, $@"{directory}\{fileName}");
                FileIO.Copy(pathOrigin, pathSave);
            }

            new Thread(() =>
            {
                //Chuyển file pdf thành hình ảnh
                pdfToImage.ConvertPdfToPNG(pathSave);
            }).Start();
            return pathDocSign;
        }

        /// <summary>
        /// Tìm chữ ký và save chứ ký vào csdl
        /// </summary>
        /// <param name="pdfFile"></param>
        /// <param name="_applicationclass"></param>
        /// <param name="_document"></param>
        /// <param name="_page"></param>
        /// <param name="pdfTempPath"></param>
        private void ProcessSign(
            DocumentTemplatePdf pdfFile,
            dynamic _applicationclass,
            dynamic _document,
            dynamic _page,
            bool isW2003
        )
        {
            var shapes = _document.Shapes;
            for (int i = 1; i <= shapes.Count; i++)
            {
                var shape = shapes[i];
                string title = isW2003 ? GetSign(shape, true) : GetTitle(shape);

                //Kiểm tra xác định đây có phải là chữ ký không.
                if (title.Trim().ToLower() == TEXT_FLAG_SIGN.ToLower() ||
                    title.Trim().ToLower() == TEXT_FLAG_INITIAL.ToLower())
                {
                    //lấy thông tin chứ ký
                    shape.Select();
                    int currentPage = Convert.ToInt32(_applicationclass.Selection.Information(WdInformation.wdActiveEndPageNumber));

                    //var Top = _applicationclass.Selection.get_Information(WdInformation.wdVerticalPositionRelativeToPage);
                    //var Left = _applicationclass.Selection.get_Information(WdInformation.wdHorizontalPositionRelativeToPage);

                    //Lấy thứ tự ký
                    int.TryParse(isW2003 ? GetSign(shape, false) : shape.AlternativeText, out int signIndex);

                    //Tạo đối tượng chữ ký.
                    var sign = new DocumentTemplatePdfSign
                    {
                        SIGNATUREHEIGHT = shape.Height,
                        SIGNATUREWIDTH = shape.Width,
                        YPOINT = isW2003 ? shape.Top + shape.Height / 2 : shape.Top,
                        XPOINT = shape.Left,
                        PAGESIGN = currentPage,
                        PDFID = pdfFile.ID,
                        ISINITIAL = (title.Trim().ToLower() == TEXT_FLAG_INITIAL.ToLower()),
                        PDFHEIGHT = (int)_page.PageHeight,
                        PDFWIDTH = (int)_page.PageWidth,
                        SIGNINDEX = signIndex
                    };
                    sign.TYPESIGN = sign.ISINITIAL ? Common.Constants.INITIAL_ICON : Common.Constants.SIGN_ICON;

                    //Lưu chữ ký vào cơ sở dữ liệu.
                    templateDAO.DocumentTemplatePdfSignAdd(sign);
                    shape.Delete();
                    i--;
                }
            }
        }

        /// <summary>
        /// Khởi tạo đối tượng MailToBo
        /// </summary>
        /// <param name="filePdf"></param>
        /// <param name="request"></param>
        /// <param name="_rec"></param>
        private static void InitMailToBO(
            DocumentTemplatePdf filePdf,
            RequestSignBO request,
            DocumentTemplateReceive _rec
        )
        {
            var mailTo = new ReceiverBO
            {
                ADDRESS = _rec.ADDRESS,
                CREATEDBYIP = filePdf.CREATEDBYIP,
                CREATEDBYUSER = filePdf.CREATEDBYUSER,
                NAME = _rec.NAMETO,
                EMAIL = _rec.EMAILTO,
                IDNUMBER = _rec.IDNUMBER,
                PHONENUMBER = _rec.PHONENUMBER,
                REQUESTSIGNTYPE = _rec.REQUESTSIGNTYPE,
                TAXCODE = _rec.TAXCODE,
                UUID = request.UUID
            };

            if (request.LISTMAILTO.Count == 0 || request.LISTMAILTO.Any(x => x.EMAIL != mailTo.EMAIL))
            {
                request.LISTMAILTO.Add(mailTo);
            }
        }

        /// <summary>
        /// Khởi tạo đối tượng documentsignBO
        /// </summary>
        /// <param name="filePdf"></param>
        /// <param name="_signs"></param>
        /// <param name="doc"></param>
        /// <param name="_rec"></param>
        private static void InitDocumentSignBO(
            DocumentTemplatePdf filePdf,
            List<DocumentTemplatePdfSign> _signs,
            DocumentBO doc,
            DocumentTemplateReceive _rec,
            string UUID
        )
        {
            var sign = new DocumentSignBO();

            var _sign = _signs.FirstOrDefault(x => x.SIGNINDEX == _rec.SIGNINDEX);
            if (_sign != null)
            {
                sign.PDFHEIGHT = _sign.PDFHEIGHT;
                sign.PDFWIDTH = _sign.PDFWIDTH;
                sign.PAGESIGN = _sign.PAGESIGN;
                sign.PHONENUMBER = _rec.PHONENUMBER;
                sign.EMAILASSIGNMENT = _rec.EMAILTO;
                sign.EMAILASSIGNMENTNAME = _rec.NAMETO;
                sign.DOCPATH = filePdf.PATH;
                sign.SIGNATUREHEIGHT = _sign.SIGNATUREHEIGHT;
                sign.SIGNATUREWIDTH = _sign.SIGNATUREWIDTH;
                sign.YPOINT = _sign.YPOINT;
                sign.XPOINT = _sign.XPOINT;
                sign.TYPESIGN = _sign.TYPESIGN;
                sign.ISINITIAL = _sign.ISINITIAL;
                sign.TYPESIGN = _sign.TYPESIGN;
                sign.UUID = UUID;
                doc.SIGN.Add(sign);
            }
        }

        /// <summary>
        /// Lấy title của chữ ký
        /// </summary>
        /// <param name="shape"></param>
        /// <returns></returns>
        private string GetTitle(dynamic shape)
        {
            try
            {
                return shape.Title;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Convert từ ExpandoObject sang List<DocumentTemplateData>
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="docid"></param>
        /// <returns></returns>
        private List<DocumentTemplateData> CovertToDocumentTemplateData(ExpandoObject obj, long docid)
        {
            IDictionary<string, object> _obj = obj;
            var rs = new List<DocumentTemplateData>();
            foreach (var key in _obj.Keys)
            {
                if (!key.Contains(UUROW_ID))
                {
                    var dynamicDataImport = new DocumentTemplateData
                    {
                        DOCID = docid,
                        NAME = key,
                        VALUE = _obj[key] + string.Empty,
                        UUROWID = _obj[UUROW_ID] + string.Empty
                    };
                    rs.Add(dynamicDataImport);
                }
            }

            return rs;
        }

        /// <summary>
        /// Xóa file
        /// </summary>
        /// <param name="filePath"></param>
        private void DeleteFile(string filePath)
        {
            if (FileIO.Exists(filePath))
            {
                FileIO.Delete(filePath);
            }
        }

        /// <summary>
        /// Xóa folder
        /// </summary>
        /// <param name="directory"></param>
        private void DeleteDirectory(string directory)
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }

        /// <summary>
        /// Lấy tổng số trang của file word
        /// </summary>
        /// <param name="_document"></param>
        /// <returns></returns>
        private int GetPagesDocument(string _document)
        {
            int pageCount;
            using (WordprocessingDocument document = WordprocessingDocument.Open(_document, false))
            {
                pageCount = int.Parse(document.ExtendedFilePropertiesPart.Properties.Pages.Text);
            }
            return pageCount;
        }
        #endregion
    }
}
