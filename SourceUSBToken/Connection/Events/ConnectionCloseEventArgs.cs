using System;
using WebSockets.Common;

namespace WebSockets.Events
{
    public class ConnectionCloseEventArgs : EventArgs
    {
        public WebSocketCloseCode Code { get; private set; }
        public string Reason { get; private set; }

        public ConnectionCloseEventArgs(WebSocketCloseCode code, string reason)
        {
            Code = code;
            Reason = reason;
        }
    }
}
