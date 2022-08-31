using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using WebSockets;
using WebSockets.Common;
using WebSockets.Events;
using WebSocketsCmd;
using WebSocketsCmd.Client;
using WebSocketsCmd.Server;

namespace Plugin
{
    class Program
    {
        public static void AddApplicationToStartup()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                key.SetValue("Plugin", "\"" + Application.ExecutablePath + "\"");
            }
            _logger.Information(typeof(Program), "Plugin start up with windows");
        }

        private static IWebSocketLogger _logger;
        private static readonly int[] ports = { 4000, 4001, 4002, 4003 };
        private static string DecodeString(string source, Int16 space)
        {
            var maxChar = Convert.ToInt32(char.MaxValue);
            var minChar = Convert.ToInt32(char.MinValue);

            var buffer = source.ToCharArray();

            for (var i = 0; i < buffer.Length; i++)
            {
                var spaced = Convert.ToInt32(buffer[i]) + space;

                if (spaced > maxChar)
                {
                    spaced -= maxChar;
                }
                else if (spaced < minChar)
                {
                    spaced += maxChar;
                }

                buffer[i] = Convert.ToChar(spaced);
            }

            return new string(buffer);
        }
        public static string ConvertToUnsecureString(SecureString securePassword)
        {
            if (securePassword == null)
                throw new ArgumentNullException("securePassword");

            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(securePassword);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }
        private static X509Certificate2 GetSSL()
        {
            SecureString certBase = new SecureString();
            Array.ForEach("UExMUlxETEVEfUZGR2tySkZWdEpWTGU2R1RIS0RkRkZHanZIamo3S1BMTFJEfUZGRVtESkZWdEpWTGU2R1RIS0RkRkZFWkhIampZZ1BMTElaV0ZGRVlYSkZ8dEpWTGU2R1RIUEZqSEZyTExILm1GRkVTXHpORFxOTnJdTGt5ZlFEVHpFRH1EZEVFVEQuUExFcWZuMjJ7fXUzT3o5NTd2eWddbTU1RExGRURESGpqV0w0VTdFa3t8RXJKUGtrNmUyLjNlbU5oOjJLSlNVZjlvLlQyZFV5Tn1sdHw3Vzx4XTM8e29RWnQ1czN5ZGhneEV7N05Fbkt0NjI4Om1SeXE2R2xOSEd3d0hYN3k7eWhLeldmbjN1R3xPbUg7dztHW3VnXEZYO1xybmtlS01VcTY4a0tZZDdPOEZTRE97TG43ckt9UmlrRVd9R2Y5elB0dHVQZXdwdmxVM1FMVnhWTWhaaE9ET0dYanpIMzZLanM4ZG54b0tzeG0zPDN8UGd5dzddaFVFZm5tdWZbWHEuTFhvS0RscVJGUTVkODp6RlxkOVlsbnE5aHV7Nnh4bl0zdnxQTzkyMzRXd2VILlQ4a0dtZ29YTEpFO205XVxIOVFzMkdwdXB6elNFbTNMem1dZDQ3TVY0fG5dWntcUi5zSHF2WkdkSU5ZdXpQUWRZc1tVOlkzUFFzTVQuNFhLZTd9UXVXV29uenhpU1ZPaTc0VGV7ZXk0W3s4Vl1VOzVFci5ud20zS3VVZ3FHSnMzO2Y5ekpHSERWRktqbmZVVlJ5T3NsNFxQeG5NZlZIWExkenJ0R3o8NztwLll6Rzd6djhmbkk5WGZ5OHlVNXI2RWVnezR3RzRUSTNMTlFsO0VIdGh2TDRmOlJROEV9TGY7TlhYaERlcGxJXDppTVJOTmldV3dQVjhOfGpNTi43OHhqXW02eFJZfUc3cUlaUkQ5WV16OVd5TnV1OjlweG1FWkRQM1lzdDZMUS48cHkuUmRIdnZYcVouNVdrSFZMW3l5cFA8cDpNcGxwWXJmeGxlODkudE1vcW00WXFUcU1Tc1trVERGPE4uSk42aVBHeUY3TTk2TzxkNWRmRnZtUnZYdDhJbztZci5YR11kbXVbVVJbTFJUUW9KWFlLa1VYWDlwSVZ4Z21bNTk8dns2cH1WW0hKc3FbV2pKWVFlZW98U3N8dkk2dEZ8WzdqeU5LWGdySWx0U0c7NDkzdGU0XW57eGU7bkRoTFZwNE1TRXNwWmVLZ2xnRTc1bXR8Z2p2V1o4fU1RZzJ6TjxTOGw0fDJkcWVlaEhFVWkudFBNbWVnaldIbVd9b2hFZFxQcE9mRDNTV1s2b1MueVkuR3BLTi5ON1pENntaemh7UWZNaXJHcm4uTzxkW1RdSjl9ezNORkxQO3dva1JackxUMzIuRFNSezduUGRXa3A3T11xV0w5TFR1W2dJOkZSO3NxV2x8cDZ6WnA4Ojc7SUluPHlybFo1fWRsbXZXSmhbM110XWdESVRHfHt6cHhFTkc1TFBPMlE2Omo7XFJlRzRsVWlRTzQ3Mm5vNk1yNnlQWy5MTTN3PGtqSVxucnpVVTlWOUhbcF1KbWg2cVg0dm16dTJoT1s1NFBsbzRTU3BwW1dnclpJT1A0XWR7R0Q8dW1VWjhsVmc4O1Q3SXJ1O0pnRHw3ZmddS09nRVtbLmRpenBvVX01R2dtNHl1OTR0dEVGR3U4VU03NHRoZVNONTtJbjVRcktNT3Jlb11EZXV9W283ZVNsOThnbzVZXXxMcm53clFoV216aFgyZDZaR296d0ptT1FHc1E7bmc3TlxtMmhkXVFFeFVoclpGfXlYM0U1LnBcU2tyWFI6eklEWTpcM09wMntVXEY5ZDhPN3BWNW9HUkZVdkldUWxaM3MuXEt4VmpoMzJkemZRM2RGSWV9NUd4bWk6b1REa3ZVaElGcGZsXXk4NzxPdTZRM0s7SkhzUG53cWtmaVlEeWRmZHNofHozXWp7Ti5ITFF4ZDZKNWtob3NobWZ9XExmTS44R3ZGWTw8cDNSb3N9enhIdEhmS0pwVWw0S25EcG5kUGc3MzZabjlWWl02dE1nSXBWV3xNNFBsb2s1Tlk8eHhmVnk5RHBvaFhwd0k0cjNHVndzcl04TWl4dEZqXVJ5dThINztZPHo0fVl6bW1IOjVEOWdXcVZYWWQ7Llk5aHc4NjRwbHVVfHs4Nzl1aXlyWm9rR1NMTzJQUzU8THNJUktpdzVyaklWTFk0TlNRZ0hnfC5OSFg2V007eWZPamRQdU90V3pHb2YzRWg0ZU1aPGR7ZGxzTlx0MnxFOlczN0k7SWZWRGY6WjJbazdzUWdxUVo5b1Q6Z3g4M2hVOTR6Nk47WE08WTtkVkR8WXpUTmVIelhuVDtqUVlbejhmN0s7U3lQWGp6TFRcTU5yXUxreWZRRFRuWFBVVGhIakV2REo7RFx6RWtESnpEZERFeURLUERnR0RtRWpudGtubEo8ejNFRlVYe0lqVFhQdXFUTnB4W0hceGxPSXlxVzlddVg6NXt4V3p6ampsT0VqbnRrbmxKPHozRUV6ZGpqams7UExMTGhETEVER0ZGRktISkZWdEpWTGU2R1RIS0RXRHJFanJ0a25sSjx6M0VHREhKUEVySElEbXh0RjJkNFY6ZnU1ejw2PHs6dGVPWGl4O3NEakxIRExGRkZHbWx4SXROPHtsbGZ3b0pGa2c4ZDdOUWtqRVN2ZTRtcDZEOlF6SFttSlw5SDkzLmppRk87TlZpSmxLW3hSbU9LdGxZaGQyWE5uZkU1U2xtWVBOaVJ1PExlN3ZbfXJ4cXBNRzh7b0Q4TlZsTG1LMk1TfGl1Tm1SW2VKVnp6am5xUlF1dXtoUVl4UEQzNXtHO2ouazNbSFJNVEhWXTJ6OUp9eVp8MnN3em9Qd1BOdGozcS5bU0hlfW1aTW1lVzhFW0dTXGpOdHNQOWdIVnM1bXBGa3h3R3FbfF16WHVmUUczPEpmc2t7eXE1bjRca11PTVtcM29vXF0yRVhFTG9QaXQ2M1p2ZEkyRnZMOHR6amxGdzdGOlt1R3ZlOXg8Zm5wU2lQVzdHVHk0dEdEd05WXHtbN25UcWZ9WHRIXVNqaTlZUTs7PE1zNVtXbGlyTE9kdnEzdlF8SFJPUWpwPEtFdEQ1W2Vxc0kuZjZHd2o7cS5RO3tdM0hRSlNOTnJNUElOTVNIdGV9T1kzWFs4cWw4alk6e0t8bzRTXWhsTWdLZXlkVzJ8dVJ1Z1tmOUhmZ3dddXBaSzNGRUVdNm82fG9KeFUzb3NzNVRbNXQ0UGpXVntlb0VNWUlmR0pmXUo6dm92M2pFfFNJSGhTZGtzc29dWzM5NVtuVXRrRjcucXZPTVJxcTxlN1Q2WVhXRHcyWXtxe1RMUTZFcDtxRkRHbzN9c3lVW2c1XXdkeG9ocmk5SDZJNk9qNnI2NnJMW1E0ZHM0dHNEZH02e0U4Onk0SDtpWmh2SUp3TX1LUGVZektIb259Tll7RG41MmRrS3MzblVJSFB0PHJoNlFqSVNNeHZQeG90RU19TFBRelVTV25ONUhQa2plODN3WVk6b09HM3dFTGtEV1Nyc3BlRFVLPFp9XWhnb1hMdWp8Tk5wRU1sUmpYaml0Nk9dLnNxbnc5cEpaNUhqd3VaLndkRDxbbjprM0VodU5cbEhPNHdKV2xJREpqdzd9NGtPXFB4V2hUbjZxVGw6bjNlSFdZVDp8ajY3eE1oVloyfVtVOjdlSG0udFg5ZkZVazpsUmhNZ25PSjp6XFs0dHY0OjRocVJ5aGxOWWVEfEUyRHN5MlA3XUhJWjQ5cjd8O3dlcHBzcnB4bUZFRmlyTGtRWWQ1UEVGUzI0WnNQbkY8aXYyeFBRckpPdDZNVTZGWnRFfDlMXHdGfDZ6WHh3SFRXfWVFVS5xZH1kbktyfHE4cDouWnw0UGxdbTpTVW9NeFxwXUt1c2RXMjg6Z2VReUhbeFlMbmtvNzdHeTJwWWU4UTZHcDNpNnhXSjYuM1RRU1E2U217M2VSdElcXVBxbkg1SXB4RjRPV0dlTE05Z0s1d0w3XXxvSXJ7bG5KcVZcT3xPWk9XT1B1SjZoOzhmU3VWN31KV3tEVzIuPG4ufEtHTld2al16OzN0Zml2S3RpdlFaRm5LUFFqdGxWR1w6WnQ2amtYZnFRNm5YUkgyM0ZpdW07UV13Z0Z5OmdMel1lOFBJS2pRNllPRC53eTY1MmdGR25Lck85UntuT1N3VlQ1TTx5cEtkZlh1c3M4cltuNTZ3OWZramlXZkl2MkVdZ1A5cjRrTnx4UWVLcDxqZWVSV1dbOlxkXXBlcmllN2puRzhwa0VwUFNqbXtXeDZKeFI8fFJLXGg7ZXpoalZ4ezd8aWlHWnBvSEU1V05zNDw5ZFdyeFlIVnA0UjJ8OEtzTXZaanZqaDRtUztHOUV6V0x9XVNtfFZldDpPUi5yezQ8M3dUUHdwOEZWckRsUm40cHlFXV15aTw2N31nZ2lOZkVQZGppOWlQWHRHZkwzfWt2OngzXHs1fXVUWmh8bDtUTVB2enNwUElXMjt9TlJsdldSMnFkUl1Jd0lKd2U1c1tnb2ZwcG9zNkhbaTV0cUlpW2s5dW5YaW1RSGw8ZEk8eE17WUtEcDp3UXtkV0Y2S1hVb3ZnVXVHZnY4UnFHUkRJaDZcWWQ3dE1mc09VT2d8eW9YM3JdMlZqVDlTcjNEN1JKczVrSGZvODJ5ZnBbVGdpR3NRO11oRTZ2WEs0RDtaeEdvLmd4NEV1OW5wbjJacGUzSV1PXXVaOHY5fHp1Rkp3VTxbbXxXeklrV1MuRjJHXVJ0bzttRVFoUUl8NTxUbExzbWxlTmxqRHo0WXZ6cnsySWRMSDlqSFFre29le3ZuVmRQN0RmSjN5SndHRjhTUkhuLlpReWdbfTNZWmd7WmZZelIuTFY3PGxQOjJISTxoRGkzeUZyNEo1V0xRSVRLUXZ0amwuTzpXekd2OnNUM1ZWUEtUaXhEcltXfFpbeHNca0txekdyeFxSe3tQXW41b211MjY2TlxZOHRHXElaNnxycXJIUy46UUxJcllEazNHV2Y4VkhKNDdQW1ozRDVYajhvN303TmpTc1I2Nm13N3VPdFlNT0s4PHVIS3pNU2VFRHVQSDNXe1FZXWVOWVNNT0R4a2lFaTR4SnZFfEhxeWZJWXxxRVR4bFcufVdNclVlTl1KcDNHeVE7TElEaGsyTTVYUVJcPGg1TGdROVddRE9zWlpKdmRsNGdYd0p9cnlRb1k1Wi5aNmd3T04zZFh2UFdRcmlten19ZFZ8MmluPDp8ejJmW3JxZDxbWVluSTU6TzdJOFNofVE8W0xaOVhlSUpRRzdYfGZsaVZHUGQ0N1VJS1s5d2k7W21bRE18aHllcXRRc1F0R0p5aWRzM21dejhTOG5uemUyRG98aU40RHxNOzR7UzxxVlBaal1afXE1c3pXWFQyalFvdGtFODN3Vnd4OlFaSTc5dFBUcDlPRn1celBrZ0hZN0U8ZlxOVkg0WkxVRVdlNzddN1lqPGtWNHlbOkRNRnRbU0hJWDRFOG1VakUzOXBXbU15bVtXR0V1bkVGV1d4eXFaSF1ITTxLcHJMUDxNcXV3UlVcXTdyeVEuNm1uRlhbV1o5bHV7RXNYWk5MTVdpODh1RG5qMm94a1Q0UERqUHpbRWw8fVQ2XExcSVBoRTtteTp9ZU18T1RbR3R0blxxcUtOdkw8bmhGM0ZncWZrSG1NSkpUblBLOnJlZkg4OWd3ZnBVejxRSE1RUFhlR2k5PHJaPHpqOjJFcHN8aC52SXBJT1N1NHp3NUpLcGxVcVtmOFFQWTZoTFdoWTY7RkR2cVg7SDtJXElPamtVZGt5fXdkaDp0PHtvfFBKSWhzfUs2c1N0eU11R1RLUHdGWlJUTGRwek1QdXpTaFpdN2lwaGttXXl0dWg8REdYSlRrOFRcXGkzWVA6M2VrNi5Sc3xRejpGbFA6dWhzOXQ4XH1oMzpFNXxvV2paPDlzcTdLOGhmTn1WWEd4eG5GPGdZO3FoMzhrdkhoeTtoW1VUbTg2cGl3RWs0N1prXUZUckRPXFh6ZGUucVtqM1dFVl1QXXBLOkx7ekxxbVlcT0toSHBccGR8NH1EU0hmOjRsZlhzc0Z9PHM4TTZWNms3Z21pTTpKezM5TVpuWk0uTVo6MkYud1FdWXtGN3lmV1FqMnRvXFpYNDw5US57TWdqZXp7ezpUdVBscVFXUDJzNzk0NTN7W3lwTXFvNDNIblI2eW00N1pIUVRROUZodUp9SGxXV1c8RTVyXFlLZHtMM2hSOEtEeGUuZUpYbTV5ZG13U007ZEpvcko0aDZXcXsuaVNEbUhYLkc4cm4zcmtESWtLSy48RDJdUjtEOzI1NWlmbW9dUTl9aH1MbjJZLnBvcVg5bzRVWVBHM3pMV0RNRWpYdUdqUEZKalhERUVWaXJJbWhHUDRWZEhTN2x7fHlNTjVaM1N2Sm5EVFhKXGtXSmtEUHRRbHQ5NHV7cFh6RkxPXGRETjNGRGpURA==".ToCharArray(), certBase.AppendChar);
            certBase.MakeReadOnly();
            SecureString certPass = new SecureString();
            Array.ForEach("cXN4bGVtZmU1NjdlRA==".ToCharArray(), certPass.AppendChar);
            certPass.MakeReadOnly();
            var decodeCert = Convert.FromBase64String(ConvertToUnsecureString(certBase));
            var decodePass = Convert.FromBase64String(ConvertToUnsecureString(certPass));
            var cert2 = DecodeString(Encoding.UTF8.GetString(decodeCert), -3);
            var pass = DecodeString(Encoding.UTF8.GetString(decodePass), -4);
            X509Certificate2 cert;

            try
            {
                cert = new X509Certificate2(Convert.FromBase64String(cert2), pass); //Bình thường
                if (cert.PrivateKey == null)
                {
                    cert = new X509Certificate2(Convert.FromBase64String(cert2), pass, X509KeyStorageFlags.MachineKeySet); // Cer lưu trên máy
                }
            }
            catch (Exception ex)
            {
                cert = new X509Certificate2(Convert.FromBase64String(cert2), pass, X509KeyStorageFlags.MachineKeySet); // Cer lưu trên máy
                if (cert.PrivateKey == null)
                {
                    cert = new X509Certificate2(Convert.FromBase64String(cert2), pass); //Bình thường
                }
                _logger.Error(typeof(Program), ex);
            }
            //var cert = new X509Certificate2(@"test.p12", "1");
            return cert;
        }
        private static void KillOtherProcess()
        {
            try
            {
                string ProjectName = Assembly.GetEntryAssembly().GetName().Name;
                Process[] pname = Process.GetProcessesByName(ProjectName);
                var currentProcess = Process.GetCurrentProcess().Id;
                foreach (var p in pname)
                {
                    if (!p.Id.Equals(currentProcess))
                    {
                        p.Kill();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(typeof(Program), ex);
            }
        }

        [STAThread]
        private static void Main(string[] args)
        {
            _logger = new WebSocketLogger();
            // Kiem tra tien trinh neu da co thi kill 
            AddApplicationToStartup();
            KillOtherProcess();
            //Cấu hình
            Configuration.Configuration.Start();
            int i = 0;
            do
            {
                try
                {
                    string webRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Plugin Log");
                    if (!Directory.Exists(webRoot))
                    {
                        string baseFolder = AppDomain.CurrentDomain.BaseDirectory;
                        _logger.Warning(typeof(Program), "Webroot folder {0} not found. Using application base directory: {1}", webRoot, baseFolder);
                        webRoot = baseFolder;
                    }

                    // used to decide what to do with incoming connections
                    ServiceFactory serviceFactory = new ServiceFactory(webRoot, _logger);

                    using (WebServer server = new WebServer(serviceFactory, _logger))
                    {
                        X509Certificate2 cert = GetSSL();
                        server.Listen(ports[i], cert);
                        _logger.Information(typeof(Program), "Plugin setup on: " + ports[i]);
                        Thread clientThread = new Thread(new ParameterizedThreadStart(TestClient))
                        {
                            IsBackground = false
                        };
                        clientThread.Start("wss://localhost:" + ports[i] + "/plugin");
                        new ManualResetEvent(false).WaitOne();
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(typeof(Program), ex);
                    i++;
                }
            }
            while (i < ports.Length);
            Environment.Exit(-1);
            return;
        }

        // Test
        public static void Client_TextFrame(object sender, TextFrameEventArgs e)
        {
            _logger.Information(typeof(Program), "Client: {0}", e.Text);
            var client = (PluginWebSocketClient)sender;

            // lets test the close handshake
            client.Dispose();
        }
        public static void Client_ConnectionOpened(object sender, EventArgs e)
        {
            _logger.Information(typeof(Program), "Client: Connection Opened");
            var client = (PluginWebSocketClient)sender;
        }
        public static void TestClient(object state)
        {
            try
            {
                string url = (string)state;
                using (var client = new PluginWebSocketClient(true, _logger))
                {
                    Uri uri = new Uri(url);
                    client.TextFrame += Client_TextFrame;
                    client.ConnectionOpened += Client_ConnectionOpened;

                    // test the open handshake
                    client.OpenBlocking(uri);
                }

                _logger.Information(typeof(Program), "Plugin running");
            }
            catch (Exception ex)
            {
                _logger.Error(typeof(Program), ex);
            }
        }
    }
}
