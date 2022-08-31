using Plugin.Signer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using WebSocketsCmd;
using System.IO;
using System.Net;
using Plugin.Common;

namespace Plugin.Plugin.Server
{
    public enum FUNCTION_ID
    {
        checkPlugin = 0,
        getCertInfo = 1,
        signPDF = 2,
        chooseFile = 3,
        signXML = 4,
        signPdfAndXml = 5
    }

    public class DataProcessing
    {
        private static String SIGNING_TYPE = "type";
        private static String SIGNING_INPUT = "input";
        private static String SIGNING_OUTPUT = "output";
        private static String SIGNING_INDEX = "index";
        private static string PDF_LIST_SIGN = "pdfListSign";
        //kt559: transaction ID để kiểm tra khi nhận kết quả => check không ký 2 lần
        //private static String TRANSACION_ID = "transacionid";

        private static String SIGNING_SIGREASON = "sigReason";
        private static String SIGNING_SIGLOCATION = "sigLocation";
        private static String SIGNING_SIGCONTACT = "sigContact";
        private static String SIGNING_LAYER2TEXT = "layer2text";
        private static String SIGNING_VISIBLEMODE = "visibleMode";
        private static String SIGNING_RENDERMODE = "rendermode";
        private static String SIGNING_PAGENO = "pageNo";
        private static String SIGNING_IMG = "img";
        //tọa độ vị trí của vùng chữ ký (chữ nhật). cách tính: bên ngoài tính
        private static String SIGNING_LLX = "llX";
        private static String SIGNING_LLY = "llY";
        private static String SIGNING_URX = "urX";
        private static String SIGNING_URY = "urY";

        private static String IMAGE_WIDTH = "imageWidth";
        private static String IMAGE_HEIGHT = "imageHeight";


        private static String SIGNING_SIGFIELD = "sigFieldName";
        //============== một số lựa chọn ký + hash: SHA1RSA hay SHA256RSA ??
        private static String SIGNING_HASHALG = "hashAlg";
        //=============== thông tin time server
        private static String SIGNING_TSA = "withTSA";
        private static String SIGNING_TSAURL = "tsaUrl";
        private static String SIGNING_TSALOGIN = "tsaLogin";
        private static String SIGNING_TSAPASS = "tsaPass";

        //============ thông tin số ký hiệu, ngày tháng khi ký ==================
        private static string SIGNING_SOKYHIEU = "sokyhieu";
        private static string SIGNING_NGAY = "ngay";
        private static string SIGNING_THANG = "thang";

        //============ thông tin chế độ certification level
        private static string SIGNING_CERTIFICATION = "certlevel";

        //hash sign algorithm
        private const string OID_sha1RSA = "1.2.840.113549.1.1.5"; //"sha1RSA"
        private const string OID_sha256RSA = "1.2.840.113549.1.1.11"; //sha256RSA

        //TODO: hàm tạo json trả về
        public static string CreateSigningResult(int code, int index, string erDesc, string type = "", string dataSigned = "")
        {
            String respone = String.Format("\"code\":{0}, \"index\":{1}, \"data\":\"{2}\", \"type\":\"{3}\", \"error\":\"{4}\""
                , code, index, dataSigned, type, erDesc);
            return "{" + respone + "}";
        }

        public static string CreateSigningResult(string transacionID, int code, int index, string erDesc, string type = "", string dataSigned = "", string subject = "", string outputPath = "")
        {
            //String respone = String.Format("\"code\":{0}, \"index\":{1}, \"data\":\"{2}\", \"type\":\"{3}\", \"error\":\"{4},\" \"transactionid\":{5}"
            String respone = String.Format("\"code\":{0}, \"index\":{1}, \"data\":\"{2}\", \"type\":\"{3}\", \"error\":\"{4}\", \"transactionid\":\"{5}\", \"outputpath\":\"{6}\", \"subject\":\"{7}\""
                , code, index, dataSigned, type, erDesc, transacionID, outputPath, subject);
            return "{" + respone + "}";
        }
  
        private static string GetSigningResult(int ret, int index, string type, byte[] output, string CurTransactionId, string subject, string outputPath = "")
        {
            String result = "";
            switch (ret)
            {
                case (int)SIGNING_RESULT.Success:
                    result = CreateSigningResult(CurTransactionId, ret, index, "", type, Convert.ToBase64String(output), subject, outputPath);
                    break;
                case (int)SIGNING_RESULT.BadKey:
                    result = CreateSigningResult(ret, index, "Not found certificate", type);
                    break;
                case (int)SIGNING_RESULT.BadInput:
                    result = CreateSigningResult(ret, index, "Bad input", type);
                    break;
                case (int)SIGNING_RESULT.NotFoundPrivateKey:
                    result = CreateSigningResult(ret, index, "Private key not exists", type);
                    break;
                case (int)SIGNING_RESULT.SigningFailed:
                    result = CreateSigningResult(ret, index, "Signing failed", type);
                    break;
                case (int)SIGNING_RESULT.Unknow:
                    result = CreateSigningResult(ret, index, "Exception unknow", type);
                    break;
                case (int)SIGNING_RESULT.SigValidateFailed:
                    result = CreateSigningResult(ret, index, "Signature validate failed", type);
                    break;
                case (int)SIGNING_RESULT.NotSupport:
                    result = CreateSigningResult(ret, index, "File not support", type);
                    break;
                default:
                    result = CreateSigningResult(ret, index, "Exception unknow", type);
                    break;
            }
            return result;
        }
        
        public static List<JObject> JArrayToArray(JArray jArr)
        {
            List<JObject> list = new List<JObject>();
            using (IEnumerator<JToken> enumerator = jArr.Children().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    JObject jObject = (JObject)enumerator.Current;
                    list.Add(jObject);
                }
            }
            return list;
        }

        public static string SignPDF(List<JObject> listData)
        {
            PdfSigner pdfSigner = new PdfSigner();
            List<string> listStr = new List<string>();


            foreach (var objData in listData)
            {
                try
                {
                    byte[] output = null;
                    byte[] input = null;

                    var type = objData[SIGNING_TYPE].Value<string>();
                    var pdfInput = objData[SIGNING_INPUT].Value<string>(); // base64
                    var pdfOutput = objData[SIGNING_OUTPUT].Value<string>(); // base64
                    var index = objData[SIGNING_INDEX].Value<int>();
                    //var CurTransactionId = objData[TRANSACION_ID].Value<string>();

                    try
                    {
                        if (FileUtils.IsUrlValid(pdfInput))
                        {
                            using (WebClient client = new WebClient())
                            {
                                input = client.DownloadData(pdfInput);
                                //
                            }
                        }
                        else input = Convert.FromBase64String(pdfInput);
                    }
                    catch (Exception e)
                    {
                        input = Convert.FromBase64String(pdfInput);
                    }

                    //input = Convert.FromBase64String(pdfInput);

                    var listSign = objData[PDF_LIST_SIGN].Value<JArray>(); // 

                    for (int i = 0; i < listSign.Count; i++)
                    {
                        var item = listSign[i];
                        if (output != null)
                        {
                            input = output;
                        }

                        signatureConfig objConf = new signatureConfig();

                        if (item[SIGNING_SIGREASON] != null) objConf.sigReason = item[SIGNING_SIGREASON].Value<string>();
                        else objConf.sigReason = "";

                        if (item[SIGNING_SIGLOCATION] != null) objConf.sigLocation = item[SIGNING_SIGLOCATION].Value<string>();
                        if (item[SIGNING_SIGCONTACT] != null) objConf.sigContact = item[SIGNING_SIGCONTACT].Value<string>();
                        if (item[SIGNING_LAYER2TEXT] != null) objConf.layer2text = item[SIGNING_LAYER2TEXT].Value<string>();
                        if (item[SIGNING_SIGFIELD] != null) objConf.sigFieldName = item[SIGNING_SIGFIELD].Value<string>();
                        if (item[SIGNING_VISIBLEMODE] != null) objConf.visibleMode = item[SIGNING_VISIBLEMODE].Value<int>();
                        else objConf.visibleMode = 0;
                        if (item[SIGNING_PAGENO] != null) objConf.pageNo = item[SIGNING_PAGENO].Value<int>();
                        if (item[SIGNING_RENDERMODE] != null) objConf.rendermode = item[SIGNING_RENDERMODE].Value<int>();
                        if (item[SIGNING_LLX] != null) objConf.llX = item[SIGNING_LLX].Value<float>();
                        if (item[SIGNING_LLY] != null) objConf.llY = item[SIGNING_LLY].Value<float>();
                        if (item[SIGNING_URX] != null) objConf.urX = item[SIGNING_URX].Value<float>();
                        if (item[SIGNING_URY] != null) objConf.urY = item[SIGNING_URY].Value<float>();
                        if (item[IMAGE_WIDTH] != null) objConf.imageWidth = item[IMAGE_WIDTH].Value<int>();
                        if (item[IMAGE_HEIGHT] != null) objConf.imageHeight = item[IMAGE_HEIGHT].Value<int>();
                        if (item[SIGNING_IMG] != null && item[SIGNING_IMG].ToString().Length > 0)
                        {
                            string imgPath = item[SIGNING_IMG].Value<string>();
                            try
                            {
                                if (FileUtils.IsUrlValid(imgPath))
                                {
                                    using (WebClient client = new WebClient())
                                    {
                                        objConf.img = client.DownloadData(imgPath);
                                    }
                                }
                                else objConf.img = Convert.FromBase64String(imgPath);
                            }
                            catch (Exception e)
                            {
                                objConf.img = Convert.FromBase64String(imgPath);
                            }
                        }
                        if (item[SIGNING_HASHALG] != null) objConf.hashAlg = item[SIGNING_HASHALG].Value<string>(); else objConf.hashAlg = "";
                        if (item[SIGNING_TSA] != null) objConf.withTSA = item[SIGNING_TSA].Value<bool>();
                        if (item[SIGNING_TSAURL] != null) objConf.tsaUrl = item[SIGNING_TSAURL].Value<string>();
                        if (item[SIGNING_TSALOGIN] != null) objConf.tsaLogin = item[SIGNING_TSALOGIN].Value<string>();
                        if (item[SIGNING_TSAPASS] != null) objConf.tsaPass = item[SIGNING_TSAPASS].Value<string>();

                        if (item[SIGNING_CERTIFICATION] != null) objConf.CertificationLevel = item[SIGNING_CERTIFICATION].Value<int>();
                        string subject = string.Empty;
                        int ret = (int)SIGNING_RESULT.Unknow;
                        if (type.ToLower().Equals("pdf"))
                        {
                            ret = pdfSigner.Sign(input, objConf, out output, out subject);
                        }
                        else
                        {
                            ret = (int)SIGNING_RESULT.NotSupport;
                        }
                        if (i + 1 == listSign.Count)
                        {
                            var respone = GetSigningResult(ret, index, type, output, "", subject.Replace("\"", "'"), pdfOutput.Replace("\\", "/"));
                            listStr.Add(respone);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw (ex);
                }
            }
            string json = JsonConvert.SerializeObject(listStr);
            return json;
        }

        public static string SignXML(List<JObject> listData, string uri)
        {
            XmlSigner xmlSigner = new XmlSigner();
            List<string> listStr = new List<string>();
            X509Certificate2 cert = null;
            string json = "";
            var response = "";
            string exportCerFile = string.Empty;
            if (cert == null)
            {
                cert = CertUtils.GetCertificate();
                exportCerFile = Convert.ToBase64String(cert.Export(X509ContentType.Cert));
                try
                {
                    if (cert.PrivateKey == null) //if (certificate.PrivateKey == null) // certificate.HasPrivateKey == false
                    {
                        WebSocketLogger.WRITE(System.Reflection.MethodBase.GetCurrentMethod().Name, "Error on check certificate.PrivateKey - invalid state");
                        response = CreateSigningResult(1, 1, "Chứng thư số không có PrivateKey", "xml", "xml");
                        listStr.Add(response);
                        json = JsonConvert.SerializeObject(listStr);
                        return json;
                    }
                }
                catch (Exception ex)
                {
                    WebSocketLogger.WRITE(System.Reflection.MethodBase.GetCurrentMethod().Name, "Error on check certificate.PrivateKey - invalid state " + ex);
                    response = CreateSigningResult(1, 1, "Chứng thư số không có PrivateKey", "xml", "xml");
                    listStr.Add(response);
                    json = JsonConvert.SerializeObject(listStr);
                    return json;
                }
                try
                {
                    if (cert.PrivateKey is RSACryptoServiceProvider rsa)
                    {
                        string signhashAlg = cert.SignatureAlgorithm.Value; //OID, cert.SignatureAlgorithm.FriendlyName - "sha256RSA";
                        if (signhashAlg == OID_sha256RSA)
                        {
                            string SignatureString = "Data that is to be signed";
                            byte[] plainTextBytes = Encoding.ASCII.GetBytes(SignatureString);
                            bool Verified = false;
                            using (SHA256CryptoServiceProvider shaM = new SHA256CryptoServiceProvider())
                            {
                                byte[] hash = shaM.ComputeHash(plainTextBytes);
                                byte[] digitalSignature = rsa.SignHash(hash, CryptoConfig.MapNameToOID("SHA256"));
                                RSACryptoServiceProvider rsaPublicKey = (RSACryptoServiceProvider)cert.PublicKey.Key;
                                Verified = rsaPublicKey.VerifyHash(hash, CryptoConfig.MapNameToOID("SHA256"), digitalSignature);
                                if (!Verified)
                                {
                                    response = CreateSigningResult(1, 1, "Lỗi lấy mã pin USB Token.", "xml", "xml");
                                    listStr.Add(response);
                                    json = JsonConvert.SerializeObject(listStr);
                                    return json;
                                }
                            }
                        }
                        else
                        {
                            string SignatureString = "Data that is to be signed";
                            byte[] plainTextBytes = Encoding.ASCII.GetBytes(SignatureString);
                            bool Verified = false;
                            using (SHA1Managed shaM = new SHA1Managed())
                            {
                                byte[] hash = shaM.ComputeHash(plainTextBytes);
                                byte[] digitalSignature = rsa.SignHash(hash, CryptoConfig.MapNameToOID("SHA1"));
                                RSACryptoServiceProvider rsaPublicKey = (RSACryptoServiceProvider)cert.PublicKey.Key;
                                Verified = rsaPublicKey.VerifyHash(hash, CryptoConfig.MapNameToOID("SHA1"), digitalSignature);
                                if (!Verified)
                                {
                                    response = CreateSigningResult(1, 1, "Lỗi lấy mã pin USB Token.", "xml", "xml");
                                    listStr.Add(response);
                                    json = JsonConvert.SerializeObject(listStr);
                                    return json;
                                }
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    WebSocketLogger.WRITE(System.Reflection.MethodBase.GetCurrentMethod().Name, "Error on check certificate.PrivateKey - invalid state" + ex);
                    response = CreateSigningResult(1, 1, "Chứng thư số không có PrivateKey", "xml", "xml");
                    listStr.Add(response);
                    json = JsonConvert.SerializeObject(listStr);
                    return json;
                }
            }
            foreach (var objData in listData)
            {
                try
                {
                    var type = objData[SIGNING_TYPE].Value<string>();
                    var input = objData[SIGNING_INPUT].Value<string>(); // base64
                    var index = objData[SIGNING_INDEX].Value<int>(); // 
                    byte[] output = null;
                    string subject = string.Empty;
                    int ret = (int)SIGNING_RESULT.Unknow;
                    if (type.ToLower().Equals("xml"))
                    {
                        ret = xmlSigner.SignUSBXML(Convert.FromBase64String(input), uri, out output, out subject, cert);
                    }
                    else
                    {
                        ret = (int)SIGNING_RESULT.NotSupport;
                    }
                    //var respone = GetSigningResult(ret, index, type, output, "", subject);
                    subject = exportCerFile;
                    var respone = GetSigningResult(ret, index, type, output, "", subject);
                    listStr.Add(respone);
                }
                catch (Exception ex)
                {
                    cert = null;
                }
            }
            cert = null;
            json = JsonConvert.SerializeObject(listStr);
            return json;
        }

        public static string SignPDFANDXML(List<JObject> listData)
        {
            PdfSigner pdfSigner = new PdfSigner();
            XmlSigner xmlSigner = new XmlSigner();
            List<string> listStr = new List<string>();
            foreach (var objData in listData)
            {
                try
                {
                    var type = objData[SIGNING_TYPE].Value<string>();
                    var input = objData[SIGNING_INPUT].Value<string>(); // base64
                    var index = objData[SIGNING_INDEX].Value<int>(); // 
                    //var CurTransactionId = objData[TRANSACION_ID].Value<string>();
                    byte[] output = null;
                    string subject = string.Empty;
                    int ret = (int)SIGNING_RESULT.Unknow;
                    if (type.ToLower().Equals("pdf"))
                    {
                        signatureConfig objConf = new signatureConfig();

                        if (objData[SIGNING_SIGREASON] != null) objConf.sigReason = objData[SIGNING_SIGREASON].Value<string>();
                        else objConf.sigReason = "";

                        if (objData[SIGNING_SIGLOCATION] != null) objConf.sigLocation = objData[SIGNING_SIGLOCATION].Value<string>();
                        if (objData[SIGNING_SIGCONTACT] != null) objConf.sigContact = objData[SIGNING_SIGCONTACT].Value<string>();
                        if (objData[SIGNING_LAYER2TEXT] != null) objConf.layer2text = objData[SIGNING_LAYER2TEXT].Value<string>();
                        if (objData[SIGNING_SIGFIELD] != null) objConf.sigFieldName = objData[SIGNING_SIGFIELD].Value<string>();
                        if (objData[SIGNING_VISIBLEMODE] != null) objConf.visibleMode = objData[SIGNING_VISIBLEMODE].Value<int>();
                        else objConf.visibleMode = 0;
                        if (objData[SIGNING_PAGENO] != null) objConf.pageNo = objData[SIGNING_PAGENO].Value<int>();
                        if (objData[SIGNING_RENDERMODE] != null) objConf.rendermode = objData[SIGNING_RENDERMODE].Value<int>();
                        if (objData[SIGNING_LLX] != null) objConf.llX = objData[SIGNING_LLX].Value<float>();
                        if (objData[SIGNING_LLY] != null) objConf.llY = objData[SIGNING_LLY].Value<float>();
                        if (objData[SIGNING_URX] != null) objConf.urX = objData[SIGNING_URX].Value<float>();
                        if (objData[SIGNING_URY] != null) objConf.urY = objData[SIGNING_URY].Value<float>();

                        if (objData[IMAGE_WIDTH] != null) objConf.imageWidth = objData[IMAGE_WIDTH].Value<int>();
                        if (objData[IMAGE_HEIGHT] != null) objConf.imageHeight = objData[IMAGE_HEIGHT].Value<int>();

                        if (objData[SIGNING_IMG] != null && objData[SIGNING_IMG].ToString().Length > 0)
                            objConf.img = Convert.FromBase64String(objData[SIGNING_IMG].Value<string>());
                        //objConf.withTSA ;  objConf.tsaUrl; objConf.tsaLogin ; objConf.tsaPass
                        if (objData[SIGNING_HASHALG] != null) objConf.hashAlg = objData[SIGNING_HASHALG].Value<string>(); else objConf.hashAlg = "";

                        if (objData[SIGNING_TSA] != null) objConf.withTSA = objData[SIGNING_TSA].Value<bool>();
                        if (objData[SIGNING_TSAURL] != null) objConf.tsaUrl = objData[SIGNING_TSAURL].Value<string>();
                        if (objData[SIGNING_TSALOGIN] != null) objConf.tsaLogin = objData[SIGNING_TSALOGIN].Value<string>();
                        if (objData[SIGNING_TSAPASS] != null) objConf.tsaPass = objData[SIGNING_TSAPASS].Value<string>();

                        if (objData[SIGNING_CERTIFICATION] != null) objConf.CertificationLevel = objData[SIGNING_CERTIFICATION].Value<int>();
                        ret = pdfSigner.Sign(Convert.FromBase64String(input), objConf, out output, out subject);
                    }
                    else if (type.ToLower().Equals("xml"))
                    {
                        string FUNC_URI = "uri";
                        string uri = (objData[FUNC_URI] != null) ? objData[FUNC_URI].Value<string>() : "";
                        ret = xmlSigner.SignXML(Convert.FromBase64String(input), uri, out output, out subject);
                    }
                    else
                    {
                        ret = (int)SIGNING_RESULT.NotSupport;
                    }
                    subject = string.Empty;
                    var respone = GetSigningResult(ret, index, type, output, "", subject);
                    listStr.Add(respone);
                }
                catch (Exception ex)
                {
                    throw (ex);
                }
            }
            string json = JsonConvert.SerializeObject(listStr);
            return json;
        }
    }
}
