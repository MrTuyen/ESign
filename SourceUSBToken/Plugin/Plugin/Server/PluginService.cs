using System.Net.Sockets;
using WebSockets.Server.WebSocket;
using WebSockets.Common;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using Plugin.Plugin.Server;
using Plugin.Signer;
using Plugin.Common;

namespace WebSocketsCmd.Server
{
    internal class PluginService : WebSocketService
    {
        private readonly IWebSocketLogger _logger;
        private readonly String FUNC_ID = "id";
        private readonly String FUNC_CALLBACK = "callback";
        private readonly String FUNC_DATA = "data";
        private readonly String FUNC_SERIAL = "serial";
        private readonly String FUNC_URI = "uri";

        public PluginService(Stream stream, TcpClient tcpClient, string header, IWebSocketLogger logger)
            : base(stream, tcpClient, header, true, logger)
        {
            _logger = logger;
        }
        // Xu ly cac su kien 
        protected override void OnTextFrame(string text)
        {
            try
            {
                if (text.Equals("Hi"))
                {
                    _logger.Information(this.GetType(), "Plugin already");
                    return;
                }
                JObject jObject = JsonConvert.DeserializeObject<JObject>(text);
                if (jObject == null)
                {
                    _logger.Error(this.GetType(), "JSON Object null");
                    return;
                }
                var id = jObject[FUNC_ID].Value<int>();
                var callback = jObject[FUNC_CALLBACK].Value<string>();
                string result = "";
                switch (id)
                {
                    case (int)FUNCTION_ID.getCertInfo:
                        result = CertUtils.GetCertInfo();
                        break;
                    case (int)FUNCTION_ID.signPDF:
                        var data3 = jObject[FUNC_DATA].Value<JArray>();
                        var listData3 = DataProcessing.JArrayToArray(data3);
                        result = DataProcessing.SignPDF(listData3);
                        break;
                    case (int)FUNCTION_ID.signXML:
                        var data1 = jObject[FUNC_DATA].Value<JArray>();
                        var listData1 = DataProcessing.JArrayToArray(data1);
                        var uri = jObject[FUNC_URI].Value<String>();
                        result = DataProcessing.SignXML(listData1, uri);
                        break;
                    case (int)FUNCTION_ID.signPdfAndXml:
                        var data5 = jObject[FUNC_DATA].Value<JArray>();
                        var listData5 = DataProcessing.JArrayToArray(data5);
                        result = DataProcessing.SignPDFANDXML(listData5);
                        break;
                    case (int)FUNCTION_ID.chooseFile:
                        result = FileUtils.ChooseFile();
                        break;
                    case (int)FUNCTION_ID.checkPlugin:
                        _logger.Information(this.GetType(), "Plugin already");
                        result = "1"; //1 : OK, -1: error
                        break;
                    default:
                        break;
                }

                string response = result + "*" + callback;
                base.Send(response);
            }
            catch (Exception ex)
            {
                _logger.Error(this.GetType(), ex);
            }
        }
    }
}

