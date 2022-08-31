using Newtonsoft.Json;
using OnSign.BusinessObject.Account;
using OnSign.BusinessObject.Forms;
using OnSign.BusinessObject.Output;
using OnSign.BusinessObject.Partners;
using OnSign.BusinessObject.Sign;
using OnSign.Common.Helpers;
using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace OnSign.BusinessLogic.Partners
{
    public class CyberLotusHSM
    {
        /// <summary>
        /// Ký số xml dùng api Cyber
        /// phungpd 20200407
        /// </summary>
        /// <param name="pdfBase64">chuỗi file pdf dạng base64</param>
        public static async Task<PdfSignedBO> SignPdfFile(PdfSignCyberBO ob, AccountBO userModel)
        {
            HttpResponseMessage result;
            PdfSignedBO dataSigned = new PdfSignedBO();
            try
            {
                string url = userModel.APIURL + ConfigHelper.HSMApiSignPdf;
                //Gọi sang Cyber ký
                using (var client = MethodHelper.CreateHttpClient(url, userModel.APIID, userModel.SECRET))
                {
                    var stringPayload = JsonConvert.SerializeObject(ob);
                    var content = new StringContent(stringPayload, Encoding.UTF8, "application/json");
                    result = await client.PostAsync(url, content);
                    if (result.IsSuccessStatusCode)
                    {
                        var oResult = await result.Content.ReadAsStringAsync();
                        dataSigned = JsonConvert.DeserializeObject<PdfSignedBO>(oResult);
                    }
                }
            }
            catch (Exception)
            {
                dataSigned.status = 0;
            }
            return dataSigned;
        }

        public static async Task<PdfSignedBO> SignPdfHashData(PdfSignHashDataCyberBO ob, AccountBO userModel)
        {
            HttpResponseMessage result;
            PdfSignedBO dataSigned = new PdfSignedBO();
            try
            {
                string url = userModel.APIURL + ConfigHelper.HSMApiSignPdfHashData;
                //Gọi sang Cyber ký
                using (var client = MethodHelper.CreateHttpClient(url, userModel.APIID, userModel.SECRET))
                {
                    var stringPayload = JsonConvert.SerializeObject(ob);
                    var content = new StringContent(stringPayload, Encoding.UTF8, "application/json");
                    result = await client.PostAsync(url, content);
                    if (result.IsSuccessStatusCode)
                    {
                        var oResult = await result.Content.ReadAsStringAsync();
                        dataSigned = JsonConvert.DeserializeObject<PdfSignedBO>(oResult);
                    }
                }
            }
            catch (Exception)
            {
                dataSigned.status = 0;
            }
            return dataSigned;
        }

        public static CERTINFOBO Get_Cert_Info(CyberLotusBO userModel)
        {
            CERTINFOBO signResult = new CERTINFOBO();
            //Lấy thông tin cer từ server HSM
            var taskGetCer = Task.Run(() => GetCertificate(userModel));
            taskGetCer.Wait();
            X509Certificate cer = taskGetCer.Result;
            if (cer == null)
            {
                signResult.SIGNED = false;
                signResult.STATUS = "Lỗi không lấy được thông tin chữ ký số HSM của bạn!";
                return signResult;
            }
            Parse_Certificate_Information(signResult, cer);
            //Nếu Tên doanh nghiệp # null => doanh nghiệp
            if (userModel.ISCOMPANY)
            {
                //Nếu chứng thư dùng để ký không phải demo
                if (!signResult.CERINFO.Contains("0123456789") &&
                    !signResult.CERINFO.Contains(userModel.TAXCODE))
                {
                    signResult.SIGNED = false;
                    signResult.STATUS = "Bạn đang dùng chữ ký số khác với Mã số thuế của bạn để ký tài liệu. Vui lòng kiểm tra lại!";
                    return signResult;
                }
            }
            return signResult;
        }

        private static async Task<X509Certificate> GetCertificate(CyberLotusBO userModel)
        {
            try
            {
                string url = userModel.APIURL + ConfigHelper.HSMApiSignGetCertificate;
                using (var client = MethodHelper.CreateHttpClient(url, userModel.APIID, userModel.SECRET))
                {
                    var response_x = await client.GetAsync(url);
                    var result = await response_x.Content.ReadAsStringAsync();
                    var certByte = Convert.FromBase64String(result);
                    var cer = new X509CertificateParser().ReadCertificate(certByte);
                    return cer;
                }
            }
            catch (Exception)
            {

            }
            return null;
        }

        private static void Parse_Certificate_Information(CERTINFOBO signResult, X509Certificate cer)
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
                signResult.certificate = cer;
            }
            catch (Exception objEx)
            {
                string msg = "Lỗi parse thông tin Cert";
                ConfigHelper.Instance.WriteLogException(MethodHelper.Instance.GetErrorMessage(objEx, msg),
                   objEx, MethodHelper.Instance.MergeEventStr(MethodBase.GetCurrentMethod()), "GetCerInfo");
                throw;
            }
        }

    }
}
