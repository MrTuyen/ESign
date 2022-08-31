using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using WebSocketsCmd;

namespace Plugin.Signer
{
    public class XmlSigner : BaseSigner
    {

        //https://www.w3.org/TR/2013/NOTE-xmlsec-algorithms-20130124/
        const string SIGN_METHOD_SHA1 = "http://www.w3.org/2000/09/xmldsig#rsa-sha1";
        const string SIGN_METHOD_SHA256 = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";

        const string DIGEST_METHOD_SHA1 = "http://www.w3.org/2000/09/xmldsig#sha1";
        const string DIGEST_METHOD_SHA256 = "http://www.w3.org/2001/04/xmlenc#sha256";

        //hash sign algorithm
        const string OID_sha1RSA = "1.2.840.113549.1.1.5"; //"sha1RSA"
        const string OID_sha256RSA = "1.2.840.113549.1.1.11"; //sha256RSA

        #region Sign

        public int SignUSBXML(byte[] input, string uri, out byte[] output, out string subject, X509Certificate2 cert)
        {
            output = null;
            subject = string.Empty;
            try
            {
                //Add SHA256 info
                CryptoConfig.AddAlgorithm(typeof(RSAPKCS1SHA256SignatureDescription), SIGN_METHOD_SHA256);//"http://www.w3.org/2001/04/xmldsig-more#rsa-sha256"

                if (input == null || input.Length == 0)
                {
                    WebSocketLogger.WRITE(System.Reflection.MethodBase.GetCurrentMethod().Name, "input not set!");
                    return (int)SIGNING_RESULT.BadInput;
                }
                // Load data into XML
                XmlDocument xmlDocument = null;
                using (MemoryStream oStream = new MemoryStream(input))
                {
                    xmlDocument = new XmlDocument();
                    xmlDocument.PreserveWhitespace = false;
                    xmlDocument.Load(oStream);
                }
                if (xmlDocument == null)
                {
                    WebSocketLogger.WRITE(System.Reflection.MethodBase.GetCurrentMethod().Name,
                        "can not load xml document!");
                    return (int)SIGNING_RESULT.BadInput;
                }

                // load cert
                if (cert == null)
                {
                    cert = CertUtils.GetCertificate();
                }
                if (cert == null)
                {
                    WebSocketLogger.WRITE(System.Reflection.MethodBase.GetCurrentMethod().Name,
                        "Not found certificate");
                    return (int)SIGNING_RESULT.BadKey;
                }
                if (cert.PrivateKey == null)
                {
                    WebSocketLogger.WRITE(System.Reflection.MethodBase.GetCurrentMethod().Name,
                        "Not found private key");
                    return (int)SIGNING_RESULT.NotFoundPrivateKey;
                }

                //subject = cert.SubjectName.Name;
                subject = "";
                SignedXml signedXml = new SignedXml(xmlDocument);
                //vtn: Set SigningKey
                //signedXml.SigningKey = (RSACryptoServiceProvider)cert.PrivateKey; => khong dung duoc cho SHA256, thay bang doan sau
                var privKey = (RSACryptoServiceProvider)cert.PrivateKey;
                var enhCsp = new RSACryptoServiceProvider().CspKeyContainerInfo;
                var cspparams = new CspParameters(enhCsp.ProviderType, enhCsp.ProviderName, privKey.CspKeyContainerInfo.KeyContainerName);
                //cspparams.Flags = CspProviderFlags.UseDefaultKeyContainer // Khách hàng bình thường
                //cspparams.Flags = CspProviderFlags.UseMachineKeyStore; // Khách hàng Cer lưu trên máy

                cspparams.Flags = CspProviderFlags.UseDefaultKeyContainer;

                // tuyennv - fix bug cer lưu trên máy sẽ gán cspparams.Flags = CspProviderFlags.UseMachineKeyStore;
                try
                {
                    privKey = new RSACryptoServiceProvider(cspparams);
                }
                catch (Exception)
                {
                    cspparams.Flags = CspProviderFlags.UseMachineKeyStore;
                    privKey = new RSACryptoServiceProvider(cspparams);
                }

                signedXml.SigningKey = privKey;

                //---- Set property
                WebSocketLogger.WRITE(System.Reflection.MethodBase.GetCurrentMethod().Name, "after privKey");
                //check cert support SHA256 or not ?? CryptoConfig.MapNameToOID("sha256RSA")
                string signhashAlg = cert.SignatureAlgorithm.Value; //OID, cert.SignatureAlgorithm.FriendlyName - "sha256RSA";
                if (signhashAlg == OID_sha256RSA)
                {
                    signedXml.SignedInfo.SignatureMethod = SIGN_METHOD_SHA256; //chuẩn ký:"http://www.w3.org/2000/09/xmldsig#rsa-sha1" "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256"; 
                }
                else
                {
                    signedXml.SignedInfo.SignatureMethod = SIGN_METHOD_SHA1;
                }
                //nếu bắt signedXml.SignedInfo.SignatureMethod = SIGN_METHOD_SHA256 - thì lỗi với cert chỉ có SHA1RSA
                //signedXml.SignedInfo.SignatureMethod = SIGN_METHOD_SHA256;               
                //vtn: set reference, with URI
                Reference reference = new Reference();
                XmlDsigEnvelopedSignatureTransform envenloped = new XmlDsigEnvelopedSignatureTransform();
                reference.AddTransform(envenloped);
                reference.Uri = uri; //chỉ ký trên 1 vùng - ví dụ nếu node nào đó đặt ID là NTT <data ID=NTT> thì khi đó chỉ ra node cần ký là "#NTT"; //https://coding.abel.nu/2015/12/xml-signatures-and-references/ 
                if (signhashAlg == OID_sha256RSA)
                {
                    reference.DigestMethod = DIGEST_METHOD_SHA256; // chuẩn hash "http://www.w3.org/2000/09/xmldsig#sha1" "http://www.w3.org/2001/04/xmlenc#sha256"
                }
                else
                {
                    reference.DigestMethod = DIGEST_METHOD_SHA1;
                }
                WebSocketLogger.WRITE(System.Reflection.MethodBase.GetCurrentMethod().Name, "reference");
                //vtnam: thử băt dùng SHA256 với các cert mà kh lỗi
                //reference.DigestMethod = DIGEST_METHOD_SHA256;

                signedXml.AddReference(reference);

                //vtn: add desiged key info
                KeyInfo keyInfo = new KeyInfo();
                KeyInfoX509Data keyData = new KeyInfoX509Data(cert);
                keyData.AddSubjectName(cert.SubjectName.Name);
                //keyData.AddSubjectKeyId()
                //X509IssuerSerial x509Serial;

                ////keyInfo.AddClause(new RSAKeyValue((RSA)cert.PublicKey.Key));
                ////keyInfo.AddClause(new KeyInfoX509Data(cert));
                //x509Serial.IssuerName = cert.IssuerName.Name;
                //x509Serial.SerialNumber = cert.SerialNumber;
                //keyData.AddIssuerSerial(x509Serial.IssuerName, x509Serial.SerialNumber);

                WebSocketLogger.WRITE(System.Reflection.MethodBase.GetCurrentMethod().Name, "AddClause");

                keyInfo.AddClause(keyData);

                signedXml.KeyInfo = keyInfo;
                //Compute
                signedXml.ComputeSignature();
                var isGay = signedXml.CheckSignature();
                //add to XML
                XmlElement xmlDigitalSignature = signedXml.GetXml();
                xmlDocument.DocumentElement.AppendChild(xmlDocument.ImportNode(xmlDigitalSignature, true));
                WebSocketLogger.WRITE(System.Reflection.MethodBase.GetCurrentMethod().Name, "Signing success!");
                output = Encoding.UTF8.GetBytes(xmlDocument.OuterXml);

                //vtnam: tự sinh chữ ký rồi, không kiểm tra lại nữa
                return (int)SIGNING_RESULT.Success;
                /*if (Verify(output))
                {
                    return (int)SIGNING_RESULT.Success;
                }
                else
                {
                    WebSocketLogger.WRITE(System.Reflection.MethodBase.GetCurrentMethod().Name, "Validate signature failed!");
                    output = null;
                    return (int)SIGNING_RESULT.SigValidateFailed;
                };*/
            }
            catch (Exception e)
            {
                cert = null;
                Console.WriteLine(e.Message);
                WebSocketLogger.WRITE(System.Reflection.MethodBase.GetCurrentMethod().Name, e.Message + " - " + e.InnerException.ToString() + "\n" + e.ToString());
            }
            return (int)SIGNING_RESULT.SigningFailed;
        }
        public int SignXML(byte[] input, string uri, out byte[] output, out string subject)
        {
            output = null;
            subject = string.Empty;
            try
            {
                //Add SHA256 info
                CryptoConfig.AddAlgorithm(typeof(RSAPKCS1SHA256SignatureDescription), SIGN_METHOD_SHA256);//"http://www.w3.org/2001/04/xmldsig-more#rsa-sha256"

                if (input == null || input.Length == 0)
                {
                    WebSocketLogger.WRITE(System.Reflection.MethodBase.GetCurrentMethod().Name, "input not set!");
                    return (int)SIGNING_RESULT.BadInput;
                }
                // Load data into XML
                XmlDocument xmlDocument = null;
                using (MemoryStream oStream = new MemoryStream(input))
                {
                    xmlDocument = new XmlDocument();
                    xmlDocument.PreserveWhitespace = false;
                    xmlDocument.Load(oStream);
                }
                if (xmlDocument == null)
                {
                    WebSocketLogger.WRITE(System.Reflection.MethodBase.GetCurrentMethod().Name,
                        "can not load xml document!");
                    return (int)SIGNING_RESULT.BadInput;
                }
                // load cert
                X509Certificate2 cert = CertUtils.GetCertificate();
                if (cert == null)
                {
                    WebSocketLogger.WRITE(System.Reflection.MethodBase.GetCurrentMethod().Name,
                        "Not found certificate");
                    return (int)SIGNING_RESULT.BadKey;
                }
                if (cert.PrivateKey == null)
                {
                    WebSocketLogger.WRITE(System.Reflection.MethodBase.GetCurrentMethod().Name,
                        "Not found private key");
                    return (int)SIGNING_RESULT.NotFoundPrivateKey;
                }

                subject = cert.SubjectName.Name;
                SignedXml signedXml = new SignedXml(xmlDocument);

                //vtn: Set SigningKey
                //signedXml.SigningKey = (RSACryptoServiceProvider)cert.PrivateKey; => khong dung duoc cho SHA256, thay bang doan sau
                var privKey = (RSACryptoServiceProvider)cert.PrivateKey;
                var enhCsp = new RSACryptoServiceProvider().CspKeyContainerInfo;
                var cspparams = new CspParameters(enhCsp.ProviderType, enhCsp.ProviderName, privKey.CspKeyContainerInfo.KeyContainerName);
                try
                {
                    cspparams.Flags = CspProviderFlags.UseDefaultKeyContainer;
                }
                catch (Exception ex)
                {
                    cspparams.Flags = CspProviderFlags.UseMachineKeyStore;
                }

                privKey = new RSACryptoServiceProvider(cspparams);

                signedXml.SigningKey = privKey;

                //---- Set property

                //check cert support SHA256 or not ?? CryptoConfig.MapNameToOID("sha256RSA")
                string signhashAlg = cert.SignatureAlgorithm.Value; //OID, cert.SignatureAlgorithm.FriendlyName - "sha256RSA";
                if (signhashAlg == OID_sha256RSA)
                {
                    signedXml.SignedInfo.SignatureMethod = SIGN_METHOD_SHA256; //chuẩn ký:"http://www.w3.org/2000/09/xmldsig#rsa-sha1" "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256"; 
                }
                else
                {
                    signedXml.SignedInfo.SignatureMethod = SIGN_METHOD_SHA1;
                }
                //nếu bắt signedXml.SignedInfo.SignatureMethod = SIGN_METHOD_SHA256 - thì lỗi với cert chỉ có SHA1RSA
                //signedXml.SignedInfo.SignatureMethod = SIGN_METHOD_SHA256;

                //vtn: set reference, with URI
                Reference reference = new Reference();
                XmlDsigEnvelopedSignatureTransform envenloped = new XmlDsigEnvelopedSignatureTransform();
                reference.AddTransform(envenloped);
                reference.Uri = uri; //chỉ ký trên 1 vùng - ví dụ nếu node nào đó đặt ID là NTT <data ID=NTT> thì khi đó chỉ ra node cần ký là "#NTT"; //https://coding.abel.nu/2015/12/xml-signatures-and-references/ 
                if (signhashAlg == OID_sha256RSA)
                {
                    reference.DigestMethod = DIGEST_METHOD_SHA256; // chuẩn hash "http://www.w3.org/2000/09/xmldsig#sha1" "http://www.w3.org/2001/04/xmlenc#sha256"
                }
                else
                {
                    reference.DigestMethod = DIGEST_METHOD_SHA1;
                }
                //vtnam: thử băt dùng SHA256 với các cert mà kh lỗi
                //reference.DigestMethod = DIGEST_METHOD_SHA256;

                signedXml.AddReference(reference);

                //vtn: add desiged key info
                KeyInfo keyInfo = new KeyInfo();
                KeyInfoX509Data keyData = new KeyInfoX509Data(cert);
                keyData.AddSubjectName(cert.SubjectName.Name);
                //keyData.AddSubjectKeyId()
                //X509IssuerSerial x509Serial;

                ////keyInfo.AddClause(new RSAKeyValue((RSA)cert.PublicKey.Key));
                ////keyInfo.AddClause(new KeyInfoX509Data(cert));
                //x509Serial.IssuerName = cert.IssuerName.Name;
                //x509Serial.SerialNumber = cert.SerialNumber;
                //keyData.AddIssuerSerial(x509Serial.IssuerName, x509Serial.SerialNumber);
                keyInfo.AddClause(keyData);

                signedXml.KeyInfo = keyInfo;
                //Compute
                signedXml.ComputeSignature();
                //add to XML
                XmlElement xmlDigitalSignature = signedXml.GetXml();
                xmlDocument.DocumentElement.AppendChild(xmlDocument.ImportNode(xmlDigitalSignature, true));
                WebSocketLogger.WRITE(System.Reflection.MethodBase.GetCurrentMethod().Name, "Signing success!");
                output = Encoding.UTF8.GetBytes(xmlDocument.OuterXml);

                //vtnam: tự sinh chữ ký rồi, không kiểm tra lại nữa
                return (int)SIGNING_RESULT.Success;
                /*if (Verify(output))
                {
                    return (int)SIGNING_RESULT.Success;
                }
                else
                {
                    WebSocketLogger.WRITE(System.Reflection.MethodBase.GetCurrentMethod().Name, "Validate signature failed!");
                    output = null;
                    return (int)SIGNING_RESULT.SigValidateFailed;
                };*/
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.Message);
                WebSocketLogger.WRITE(System.Reflection.MethodBase.GetCurrentMethod().Name, e.Message);
            }
            return (int)SIGNING_RESULT.SigningFailed;
        }
        #endregion
        #region Verify
        //Hàm kiểm tra trước khi trả lại kết quả cho client.
        //hoặc update hỗ trợ SHA256 hoặc bỏ qua, giả định ký là OK
        public override bool Verify(byte[] signedData)
        {

            if (signedData == null)
            {
                WebSocketLogger.WRITE(System.Reflection.MethodBase.GetCurrentMethod().Name,
                                 "Verify failed!");
                return false;
            }
            XmlDocument xmlDoc = new XmlDocument();
            SignedXml signedXml;
            XmlNodeList nodeList;
            CryptoConfig.AddAlgorithm(typeof(RSAPKCS1SHA256SignatureDescription), SIGN_METHOD_SHA256);//"http://www.w3.org/2001/04/xmldsig-more#rsa-sha256"
            try
            {
                xmlDoc.LoadXml(Encoding.UTF8.GetString(signedData));
                signedXml = new SignedXml(xmlDoc);
                nodeList = xmlDoc.GetElementsByTagName("Signature");
            }
            catch (Exception ex)
            {
                WebSocketLogger.WRITE(System.Reflection.MethodBase.GetCurrentMethod().Name,
                                 "Unable load XML for signature verify!");
                return false;
            };
            //-------- đoạn này chỉ cho SHA1 và 1 chữ ký đầu tiên ---------------
            //signedXml.LoadXml((XmlElement)nodeList[0]);
            //return signedXml.CheckSignature();

            //-----------------------------------------------------
            //------------- VTNAM: check hỗ trợ SHA256 và nhiều chữ ký, --------------------
            bool isOK = true;
            int indx = 0;
            try
            {
                XmlNodeList certificateNode = xmlDoc.GetElementsByTagName("X509Certificate");
                if (certificateNode.Count == nodeList.Count)
                {
                    for (indx = 0; indx < nodeList.Count; indx++)
                    {
                        signedXml.LoadXml((XmlElement)nodeList[indx]);
                        isOK = signedXml.CheckSignature(new X509Certificate2(Convert.FromBase64String(certificateNode[indx].InnerText)), true);
                        if (isOK == false)
                        {
                            return isOK;
                        }
                    }
                }
                else //check kiểu SHA1, chỉ chữ ký đầu tiên
                {
                    signedXml.LoadXml((XmlElement)nodeList[0]);
                    return signedXml.CheckSignature();
                }
            }
            catch (Exception ex)
            {
                WebSocketLogger.WRITE(System.Reflection.MethodBase.GetCurrentMethod().Name,
                                 "Verify signature failed:" + ex.Message);
                return false;
            };
            return isOK;

        }
        #endregion
    }
}
