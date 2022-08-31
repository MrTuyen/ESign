using Microsoft.Win32;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using WebSockets.Common;
using WebSocketsCmd;

namespace Plugin.Configuration
{
    public class Configuration
    {
        private static readonly string FIREFOX_FROFILES_PATH = @"\Mozilla\Firefox\Profiles";
        private const string userKey = "HKEY_CURRENT_USER";
        private const string subkey = @"Software\Microsoft\Windows\CurrentVersion\Internet Settings";
        // Chrome
        private static void InstallRootCertificate(string cerFileName, StoreLocation storeLocation)
        {
            try
            {
                X509Certificate2 certificate = new X509Certificate2();
                certificate.Import(cerFileName);
                X509Store store = new X509Store(StoreName.Root, storeLocation);
                store.Open(OpenFlags.ReadWrite);
                store.Add(certificate);
                store.Close();
            }
            catch (Exception ex)
            {
                //throw new Exception("InstallRootCertificate: error install cert" + ex.Message);
                IWebSocketLogger _logger = new WebSocketLogger();
                _logger.Information(typeof(Program), "InstallRootCertificate error certificate" + ex.Message);
            }
        }
        public static void ConfigIE()
        {

            //string keyName = userKey + "\\" + subkey;
            //string currentFolder = AppDomain.CurrentDomain.BaseDirectory;
            //Registry.SetValue(keyName, "CertificateRevocation", 0, RegistryValueKind.DWord);
            //int counter = 0;
            //string line;
            //try
            //{
            //    Microsoft.Win32.RegistryKey rkey;
            //    System.IO.StreamReader file =
            //       new System.IO.StreamReader(currentFolder + "config.txt");
            //    while ((line = file.ReadLine()) != null)
            //    {
            //        Console.WriteLine(line);
            //        int find = line.IndexOf("//");
            //        if (find == -1)
            //        {
            //            continue;
            //        }
            //        String domain = line.Substring(find + 2, line.Length - find - 2);
            //        String frefix = line.Substring(0, find - 1);
            //        String[] subStr = domain.Split('.');
            //        if (subStr.Length < 2)
            //        {
            //            continue;
            //        }
            //        else
            //        {
            //            int number;
            //            bool isNumeric = int.TryParse(subStr[subStr.Length - 1], out number);
            //            if (isNumeric)
            //            {
            //                String pathReg = String.Format(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Internet Settings\ZoneMap\Ranges\Range{0}"
            //                              , counter + 100);
            //                rkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(pathReg);
            //                rkey.SetValue(":Range", domain, RegistryValueKind.String);
            //                if (frefix.Equals("http"))
            //                    rkey.SetValue("http", "1", RegistryValueKind.DWord);
            //                else
            //                    rkey.SetValue("https", "2", RegistryValueKind.DWord);
            //            }
            //            else
            //            {
            //                String pathReg = String.Format(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Internet Settings\ZoneMap\Domains\{0}.{1}"
            //                               , subStr[subStr.Length - 2], subStr[subStr.Length - 1]);
            //                rkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(pathReg);
            //                if (frefix.Equals("http"))
            //                    rkey.SetValue("http", "1", RegistryValueKind.DWord);
            //                else
            //                    rkey.SetValue("https", "2", RegistryValueKind.DWord);
            //            }
            //        }
            //        counter++;
            //    }
            //    file.Close();
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e.Message);
            //}
        }
        public static void Start()
        {
            // Chrome config
            string appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Plugin Log");
            InstallRootCertificate(appData + "\\AdminCA.cer", StoreLocation.LocalMachine);
            InstallRootCertificate(appData + "\\AdminCA.cer", StoreLocation.CurrentUser);

            // Firefox
            String ffFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + FIREFOX_FROFILES_PATH;
            if (Directory.Exists(ffFolder))
            {
                var folderList = Directory.GetDirectories(ffFolder);
                foreach (String folderName in folderList)
                {
                    if (folderName.Contains("default"))
                    {
                        String soureFile = appData + "\\cert8.db";
                        String targetFile = folderName + "\\cert8.db";
                        if (File.Exists(soureFile))
                        {
                            File.Copy(soureFile, targetFile, true);
                            break;
                        }
                    }
                }
            }
            // Edge + IE
            ConfigIE();

            // Khoi dong cung windows
            string path = Directory.GetCurrentDirectory();
            WebSocketLogger.WRITE("Test", path);
            if (!Environment.OSVersion.ToString().Contains("5.1"))
            {
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                    Arguments = "/C CheckNetIsolation LoopbackExempt -a -n=Microsoft.MicrosoftEdge_8wekyb3d8bbwe"

                };
                process.StartInfo = startInfo;
                process.Start();
            }
            else // win xp
            {
                //String pathReg = String.Format(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
                //RegistryKey rkey = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(pathReg);
            }
        }
    }
}
