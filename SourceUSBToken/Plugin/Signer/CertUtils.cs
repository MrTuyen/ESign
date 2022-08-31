using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using WebSocketsCmd;

namespace Plugin.Signer
{
    public class CertUtils
    {
        public static X509Certificate2 GetCertificate()
        {
            X509Store x509Store = new X509Store(StoreName.My, StoreLocation.CurrentUser); // Main
            // X509Store x509Store = new X509Store(StoreName.CertificateAuthority, StoreLocation.CurrentUser); // Custom
            try
            {

                x509Store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
                X509Certificate2Collection numberCerts = x509Store.Certificates;
                X509Certificate2Collection fcollection = numberCerts.Find(X509FindType.FindByTimeValid, DateTime.Now, false);
                X509Certificate2Enumerator certEnumerator;

                //int count = 0;
                //foreach (X509Certificate2 cert in fcollection)
                //{
                //    if (!cert.HasPrivateKey)
                //        continue; // not smartcard for sure
                //    if (!(cert.PrivateKey is RSACryptoServiceProvider rsa))
                //        continue; // not smart card cert again
                //    if (rsa.CspKeyContainerInfo.HardwareDevice) // sure - smartcard
                //    {
                //        newCollection.Add(cert);
                //    }
                //    else
                //    {
                //        if (count < 5)
                //        {
                //            newCollection.Add(cert);
                //            count++;
                //        }
                //    }
                //}

                if (fcollection.Count == 1)
                {
                    certEnumerator = fcollection.GetEnumerator();
                    while (certEnumerator.MoveNext())
                        return certEnumerator.Current;
                    return null;
                }
                else if (fcollection.Count > 1)
                {
                    X509Certificate2Collection chooseCert = X509Certificate2UI.SelectFromCollection(fcollection, "Certificates List (Danh sách chứng thư)", "Choose your certificate (Chọn chứng thư của bạn)", X509SelectionFlag.SingleSelection);
                    if (chooseCert.Count == 1)
                        return chooseCert[0];
                    else
                        return null;
                }
                else
                    return null;
            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                x509Store.Close();
            }
            return null;
        }
        public X509Certificate2 GetCertificate(string serialNumber)
        {
            if (String.IsNullOrEmpty(serialNumber))
            {
                return GetCertificate();
            }

            X509Store x509Store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            x509Store.Open(OpenFlags.ReadOnly);

            X509Certificate2Collection certificateCollection = x509Store.Certificates.Find(X509FindType.FindBySerialNumber, serialNumber.ToLower(), false);
            if (certificateCollection.Count == 0)
            {
                WebSocketLogger.WRITE(System.Reflection.MethodBase.GetCurrentMethod().Name,
                    "Filter Serial: Nothing Found");
                return null;
            }

            return certificateCollection[0];
        }
        public static String GetCertInfo()
        {
            try
            {
                X509Certificate2 cert = GetCertificate();
                if (cert == null)
                    return "";
                String serial = cert.GetSerialNumberString();
                String subject = cert.GetNameInfo(X509NameType.SimpleName, false);
                String issuer = cert.GetNameInfo(X509NameType.SimpleName, true);
                //String timeValidFrom = cert.NotAfter.ToShortDateString().Replace("//", "////");
                //String timeValidTo = cert.NotBefore.ToShortDateString().Replace("//", "////");
                String timeValidFrom = cert.NotAfter.ToString("dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.GetCultureInfo("en-US"));//ToShortDateString().Replace("//", "////");
                String timeValidTo = cert.NotBefore.ToString("dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
                String certBase64 = Convert.ToBase64String(cert.Export(X509ContentType.Cert));
                String publicKeyBase64 = Convert.ToBase64String(cert.GetPublicKey());
                if (serial == null || certBase64 == null)
                    return "";
                String result = String.Format("\"subjectCN\":\"{0}\", \"issuerCN\":\"{1}\", \"serial\":\"{2}\", \"validFrom\":\"{3}\", \"vaildTo\":\"{4}\", \"base64\":\"{5}\" , \"keybase64\":\"{6}\""
                    , subject, issuer, serial, timeValidTo, timeValidFrom, certBase64, publicKeyBase64);
                return "{" + result + "}";
            }
            catch (Exception e)
            {
                WebSocketLogger.WRITE(System.Reflection.MethodBase.GetCurrentMethod().Name, e.Message);
            }
            return "";

        }

        public static String GetCertInfo(X509Certificate2 cert)
        {
            try
            {
                if (cert == null)
                    return "";
                String serial = cert.GetSerialNumberString();
                String subject = cert.GetNameInfo(X509NameType.SimpleName, false);
                String issuer = cert.GetNameInfo(X509NameType.SimpleName, true);
                //String timeValidFrom = cert.NotAfter.ToShortDateString().Replace("//", "////");
                //String timeValidTo = cert.NotBefore.ToShortDateString().Replace("//", "////");
                String timeValidFrom = cert.NotAfter.ToString("dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.GetCultureInfo("en-US"));//ToShortDateString().Replace("//", "////");
                String timeValidTo = cert.NotBefore.ToString("dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
                String certBase64 = Convert.ToBase64String(cert.Export(X509ContentType.Cert));
                String publicKeyBase64 = Convert.ToBase64String(cert.GetPublicKey());
                if (serial == null || certBase64 == null)
                    return "";
                String result = String.Format("\"subjectCN\":\"{0}\", \"issuerCN\":\"{1}\", \"serial\":\"{2}\", \"validFrom\":\"{3}\", \"vaildTo\":\"{4}\", \"base64\":\"{5}\" , \"keybase64\":\"{6}\""
                    , subject, issuer, serial, timeValidTo, timeValidFrom, certBase64, publicKeyBase64);
                return "{" + result + "}";
            }
            catch (Exception e)
            {
                WebSocketLogger.WRITE(System.Reflection.MethodBase.GetCurrentMethod().Name, e.Message);
            }
            return "";
        }

        public static String GetCertInfo(byte[] certBytes)
        {
            try
            {
                if (certBytes == null || certBytes.Length == 0)
                    return "";
                X509Certificate2 cert = new X509Certificate2(certBytes);
                return GetCertInfo(cert);
            }
            catch (Exception e)
            {
                WebSocketLogger.WRITE(System.Reflection.MethodBase.GetCurrentMethod().Name, e.Message);
            }
            return "";
        }
        /// <summary>
        /// lấy thông tin certificate, truyền vào là xâu base64
        /// </summary>
        /// <param name="cert64"></param>
        /// <returns></returns>
        public static String GetCertInfo(string cert64)
        {
            try
            {
                if (String.IsNullOrEmpty(cert64))
                    return "";
                X509Certificate2 cert = new X509Certificate2(Convert.FromBase64String(cert64));
                return GetCertInfo(cert);
            }
            catch (Exception e)
            {
                WebSocketLogger.WRITE(System.Reflection.MethodBase.GetCurrentMethod().Name, e.Message);
            }
            return "";
        }


    }
}
