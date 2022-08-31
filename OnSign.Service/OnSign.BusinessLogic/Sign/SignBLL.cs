using CyberClientLib;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.security;
using Newtonsoft.Json;
using OnSign.BusinessLogic.Partners;
using OnSign.BusinessObject.Sign;
using OnSign.Common;
using OnSign.Common.Helpers;
using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;

namespace OnSign.BusinessLogic.Sign
{

    public class SignBLL : BaseBLL
    {
        private class StarBuildParams
        {
            //Status
            public string CN { get; set; }
            //Địa chỉ email
            public string O { get; set; }
            //ID rq
            public string MST { get; set; }
            //Nhận bản sao hay k (CC)
            public string C { get; set; }
            //Người ký thứ mấy
            public string P { get; set; }
        }

        public SignBLL()
        {
        }

        public static byte[] ConvertImageToByteArray(string imagePath)
        {
            byte[] imageByteArray = null;
            FileStream fileStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
            using (BinaryReader reader = new BinaryReader(fileStream))
            {
                imageByteArray = new byte[reader.BaseStream.Length];
                for (int i = 0; i < reader.BaseStream.Length; i++)
                    imageByteArray[i] = reader.ReadByte();
            }
            return imageByteArray;
        }

        public bool SignInitialPdf(PdfSignBO pdfSign)
        {
            try
            {
                pdfSign.ypoint = pdfSign.pdfSize.Height - pdfSign.ypoint - pdfSign.height;
                //Nếu tọa độ x của chữ ký + chiều rộng ảnh chữ ký lớn hơn chiều rộng biên phải của file thì được gán bằng chiều rộng file - chiều rộng ảnh chữ ký
                if (pdfSign.pdfSize.Width < pdfSign.xpoint + pdfSign.width)
                    pdfSign.xpoint = (int)(pdfSign.pdfSize.Width - pdfSign.width);

                //Nếu tọa độ x của chữ ký nhỏ hơn 0 biên trái của file thì được gán bằng 0
                if (pdfSign.xpoint < 0) pdfSign.xpoint = 0;

                //Tọa độ biên trên của chữ ký < vị trí y + chiều cao của ảnh thì được gán bằng chiều cao của file - chiều cao của ảnh
                if (pdfSign.pdfSize.Top < pdfSign.ypoint + pdfSign.height)
                    pdfSign.ypoint = (int)(pdfSign.pdfSize.Top - pdfSign.height);

                //Nếu tọa độ y biên dưới < 0 thì gán = 0
                if (pdfSign.ypoint < 0) pdfSign.ypoint = 0;
                PdfStamper pdfStamper = null;

                // Open the PDF file to be signed
                PdfReader pdfReader = new PdfReader(pdfSign.base64pdf);
                PdfReader.unethicalreading = true;

                var outputTemp = $"{pdfSign.pathOutputPdf_Temp}_temp.pdf";

                //Output stream to write the stamped PDF to
                using (FileStream outStream = new FileStream(outputTemp, FileMode.Create))
                {
                    try
                    {
                        // Stamper to stamp the PDF with a signature
                        pdfStamper = new PdfStamper(pdfReader, outStream, '\0', true);
                        // Load signature image
                        Image sigImg = Image.GetInstance(pdfSign.base64image);
                        // Scale image to fit
                        sigImg.ScaleToFit(pdfSign.width, pdfSign.height);
                        // Set signature position on page
                        sigImg.SetAbsolutePosition(pdfSign.xpoint, pdfSign.ypoint);
                        // Add signatures to desired page
                        PdfContentByte over = pdfStamper.GetOverContent(pdfSign.pagesign);
                        over.AddImage(sigImg);
                    }
                    finally
                    {
                        if (pdfStamper != null)
                            pdfStamper.Close();
                        if (pdfReader != null)
                            pdfReader.Close();
                    }
                }

                File.Copy(outputTemp, pdfSign.pathOutputPdf_Temp, true);
                File.Delete(outputTemp);
                return true;
            }
            catch (Exception objEx)
            {
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, "Lỗi ký nháy tài liệu");
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                throw;
            }
        }

        public static byte[] ReadToEnd(System.IO.Stream stream)
        {
            long originalPosition = 0;
            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }
            try
            {
                byte[] readBuffer = new byte[4096];
                int totalBytesRead = 0;
                int bytesRead;
                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;
                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }
                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
            }
        }

        public bool ByteArrayToFile(string fileName, byte[] byteArray)
        {
            try
            {
                using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(byteArray, 0, byteArray.Length);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught in process: {0}", ex);
                return false;
            }
        }

        public CERTINFOBO SignDocumentViaHSM(PdfSignBO pdfSign)
        {
            CERTINFOBO signResult = new CERTINFOBO();
            PdfSignatureAppearance sap = null;
            MemoryStream baos = null;
            try
            {
                //Tính lại tọa độ của y (gốc tọa độ là dưới cùng trái) = chiều cao thực tế của file - cao hiện tại - chiều cao của ảnh hiển thị
                //Gốc tọa độ (0,0) được tính từ vị trí dưới cùng bên trái
                pdfSign.ypoint = pdfSign.pdfSize.Height - pdfSign.ypoint - pdfSign.height;

                //Nếu tọa độ x của chữ ký + chiều rộng ảnh chữ ký lớn hơn chiều rộng biên phải của file thì được gán bằng chiều rộng file - chiều rộng ảnh chữ ký
                if (pdfSign.pdfSize.Width < pdfSign.xpoint + pdfSign.width)
                    pdfSign.xpoint = (int)(pdfSign.pdfSize.Width - pdfSign.width);

                //Nếu tọa độ x của chữ ký nhỏ hơn 0 biên trái của file thì được gán bằng 0
                if (pdfSign.xpoint < 0) pdfSign.xpoint = 0;

                //Tọa độ biên trên của chữ ký < vị trí y + chiều cao của ảnh thì được gán bằng chiều cao của file - chiều cao của ảnh
                if (pdfSign.pdfSize.Top < pdfSign.ypoint + pdfSign.height)
                    pdfSign.ypoint = (int)(pdfSign.pdfSize.Top - pdfSign.height);

                //Nếu tọa độ y biên dưới < 0 thì gán = 0
                if (pdfSign.ypoint < 0) pdfSign.ypoint = 0;

                //Chuyển file pdf sang byte array
                var pdfContent = File.ReadAllBytes(pdfSign.base64pdf);

                //hash file pdf
                byte[] bhashvalue = this.HashPdfFile(
                    pdfContent, pdfSign.certificate, out baos, out sap,
                    pdfSign.pagesign, pdfSign.xpoint, pdfSign.ypoint,
                    pdfSign.width, pdfSign.height,
                    (int)TYPESIGNATURE.IMAGE,
                    Convert.ToBase64String(File.ReadAllBytes(pdfSign.base64image)),
                    pdfSign.textout, pdfSign.signaturename);

                //Ký hash file pdf
                var taskHashData = Task.Run(() => CyberLotusHSM.SignPdfHashData(new PdfSignHashDataCyberBO()
                {
                    base64hash = Convert.ToBase64String(bhashvalue),
                    hashalg = HASHALG.SHA1.ToString()
                }, pdfSign.objUser));
                taskHashData.Wait();
                signResult.SIGNED = false;
                signResult.STATUS = taskHashData.Result.description;

                if (taskHashData.Result.status == 0) // Thành công
                {
                    //Convert file đã ký
                    byte[] bSignature = Convert.FromBase64String(taskHashData.Result.obj);
                    PdfLib.addExternalSignature(bSignature, sap);
                    File.WriteAllBytes(pdfSign.pathOutputPdf_Temp, baos.ToArray());
                    signResult.SIGNED = true;
                    GetCerInfo(signResult, pdfSign.certificate);
                    //signResult.CERINFO = pdfSign.certificate.;

                }
                return signResult;
            }
            catch (Exception objEx)
            {
                string msg = "Lỗi ký tài liệu bằng HSM";
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, msg);
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                throw objEx;
            }
        }

        public void CreateFileSign(List<PdfSignBO> pdfSignBOs)
        {
            try
            {
                pdfSignBOs.ForEach((file) =>
                {
                    CreateFileSign(file.pathOutputPdf_Temp, file.pathOutputPdf);
                });
            }
            catch (Exception objEx)
            {
                ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, objEx.Message);
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                throw;
            }
        }

        public void DeleteFileTemp(List<PdfSignBO> pdfSignBOs)
        {
            try
            {
                pdfSignBOs.ForEach((file) =>
                {
                    DeleteFileTemp(file.pathOutputPdf_Temp);
                });
            }
            catch (Exception objEx)
            {
                ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, objEx.Message);
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                // return false;
                throw;
            }

        }

        public void CreateFileSign(string pathtemp, string pathFinish)
        {
            try
            {
                if (File.Exists(pathtemp))
                {
                    File.Copy(pathtemp, pathFinish, true);
                    File.Delete(pathtemp);
                }
            }
            catch (Exception objEx)
            {
                ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, objEx.Message);
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                // return false;
                throw;
            }
        }

        private void DeleteFileTemp(string pathtemp)
        {
            try
            {
                if (File.Exists(pathtemp))
                {
                    File.Delete(pathtemp);
                }
            }
            catch (Exception objEx)
            {
                ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, objEx.Message);
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                // return false;
                throw;
            }
        }

        private void GetCerInfo(CERTINFOBO signResult, X509Certificate cer)
        {
            try
            {

                var supplier = cer.IssuerDN.ToString();
                var company = cer.SubjectDN.ToString();
                signResult.CERINFO = $"{supplier}, {company},NotBefore={cer.NotBefore:dd/MM/yyyy HH:mm:ss},NotAfter={cer.NotAfter:dd/MM/yyyy HH:mm:ss},SERIAL={cer.SerialNumber}";

                signResult.CERSTARTDATE = cer.NotBefore;
                signResult.CERENDDATE = cer.NotAfter;
                signResult.SERIAL = cer.SerialNumber.ToString();

                var dictSupplier = HttpUtility.ParseQueryString(supplier.Replace(",", "&"));
                string jsonSupplier = JsonConvert.SerializeObject(dictSupplier.Cast<string>().ToDictionary(k => k, v => dictSupplier[v]));
                var respObj = JsonConvert.DeserializeObject<StarBuildParams>(jsonSupplier);
                signResult.TAXCODE = MethodHelper.BetweenStrings(company, "MST:", ",");
                signResult.COMPANY = MethodHelper.BetweenStrings(company, "CN=", ",");
            }
            catch (Exception objEx)
            {
                string msg = "Lỗi parse thông tin Cert";
                this.ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, msg);
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(this.ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                throw;
            }
        }

        public byte[] HashPdfFile(
               byte[] pfdContent,
               X509Certificate endCert,
               out MemoryStream baos,
               out PdfSignatureAppearance sap,
               int page,
               float x,
               float y,
               float width,
               float height,
               int typeSig,
               string base64Image,
               string text,
               string signature_name)
        {
            try
            {
                PdfReader reader = new PdfReader(pfdContent);
                PdfReader.unethicalreading = true;

                baos = new MemoryStream();

                PdfStamper stp = PdfStamper.CreateSignature(reader, baos, '\0', null, true);
                sap = stp.SignatureAppearance;

                Rectangle pageRect = new Rectangle(x, y, x + width, y + height);

                //Signature name : ngẫu nhiên không trùng trong 1 file ký
                sap.SetVisibleSignature(pageRect, page, signature_name);
                sap.Certificate = endCert;

                switch (typeSig)
                {
                    // hình ảnh
                    case 1:
                        {
                            Image instance = Image.GetInstance(Convert.FromBase64String(base64Image));
                            sap.Image = instance;
                            sap.Acro6Layers = true;
                            sap.Layer2Text = string.Empty;
                            break;
                        }
                    // text
                    case 2:
                        {

                            BaseColor color = new BaseColor(0, 128, 0);
                            Font layer2Font = new Font(BaseFont.CreateFont("times.ttf", BaseFont.IDENTITY_H, true), 9f, 0, color);
                            sap.Layer2Font = layer2Font;

                            string noidung = "Ký bởi: " + text + "\n";
                            noidung += "Ký ngày: " + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");

                            sap.Layer2Text = noidung;
                            sap.Layer2Text.PadLeft(100);
                            break;
                        }
                    //text + images
                    case 3:
                        {
                            BaseColor color = new BaseColor(0, 128, 0);
                            Font layer2Font = new Font(BaseFont.CreateFont("times.ttf", BaseFont.IDENTITY_H, true), 9f, 0, color);
                            sap.Layer2Font = layer2Font;

                            string noidung = "Ký bởi: " + text + "\n";
                            noidung += "Ký ngày: " + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");

                            Image instance2 = Image.GetInstance(Convert.FromBase64String(base64Image));
                            //instance2.ScalePercent(50);
                            instance2.SetAbsolutePosition(x, y);

                            sap.SignatureGraphic = instance2;
                            /* DESCRIPTION = 0,
                            NAME_AND_DESCRIPTION = 1,
                            GRAPHIC_AND_DESCRIPTION = 2,
                            GRAPHIC = 3
                            */
                            sap.SignatureRenderingMode = PdfSignatureAppearance.RenderingMode.GRAPHIC_AND_DESCRIPTION;
                            //sap.Image = instance2;
                            //sap.Image.Alignment = 0;
                            sap.ImageScale = 0.3f;
                            //sap.Image.ScaleAbsoluteHeight(height);
                            new Rectangle((float)x, (float)y, x + width, y + height);
                            sap.Acro6Layers = false;
                            sap.Layer2Text = noidung;
                            break;
                        }
                }


                PdfSignature dic = new PdfSignature(PdfName.ADOBE_PPKLITE, PdfName.ADBE_PKCS7_DETACHED)
                {
                    Reason = sap.Reason,
                    Location = sap.Location,
                    Contact = sap.Contact,
                    Date = new PdfDate(sap.SignDate)
                };
                sap.CryptoDictionary = dic;

                Dictionary<PdfName, int> exc = new Dictionary<PdfName, int>
                {
                    { PdfName.CONTENTS, (int)(8192 * 2 + 2) }
                };
                sap.PreClose(exc);
                Stream data = sap.GetRangeStream();
                byte[] hash = DigestAlgorithms.Digest(data, "SHA1");

                return hash;
            }
            catch (Exception objEx)
            {
                ErrorMsg = MethodHelper.Instance.GetErrorMessage(objEx, objEx.Message);
                objResultMessageBO = ConfigHelper.Instance.WriteLogException(ErrorMsg, objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), this.NameSpace);
                // return false;
                throw;
            }
        }
    }
}
