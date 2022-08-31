using System.Text;
using WebSockets.Client;
using WebSockets.Common;

namespace WebSocketsCmd.Client
{
    class PluginWebSocketClient : WebSocketClient
    {
        public PluginWebSocketClient(bool noDelay, IWebSocketLogger logger) : base(noDelay, logger)
        {

        }
    }
}
