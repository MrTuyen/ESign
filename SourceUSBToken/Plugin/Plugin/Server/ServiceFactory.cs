using System.IO;
using System.Reflection;
using WebSockets.Server;
using WebSockets.Server.Http;
using WebSockets.Common;

namespace WebSocketsCmd.Server
{
    internal class ServiceFactory : IServiceFactory
    {
        private readonly IWebSocketLogger _logger;
        private readonly string _webRoot;
        private readonly string subName = "/plugin";

        private string GetWebRoot()
        {
            if (!string.IsNullOrEmpty(_webRoot) && Directory.Exists(_webRoot))
            {
                return _webRoot;
            }

            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase).Replace(@"file:\", string.Empty);
        }

        public ServiceFactory(string webRoot, IWebSocketLogger logger)
        {
            _logger = logger;
            _webRoot = string.IsNullOrEmpty(webRoot) ? GetWebRoot() : webRoot;
            if (!Directory.Exists(_webRoot))
            {
                _logger.Warning(this.GetType(), "Web root not found: {0}", _webRoot);
            }
            else
            {
                _logger.Information(this.GetType(), "Web root: {0}", _webRoot);
            }
        }

        public IService CreateInstance(ConnectionDetails connectionDetails)
        {
            switch (connectionDetails.ConnectionType)
            {
                case ConnectionType.WebSocket:
                    // you can support different kinds of web socket connections using a different path
                    if (connectionDetails.Path == subName)
                    {
                        return new PluginService(connectionDetails.Stream, connectionDetails.TcpClient, connectionDetails.Header, _logger);
                    }
                    break;
                case ConnectionType.Http:
                    // this path actually refers to the reletive location of some html file or image
                    return new HttpService(connectionDetails.Stream, connectionDetails.Path, _webRoot, _logger);
            }

            return new BadRequestService(connectionDetails.Stream, connectionDetails.Header, _logger);
        }
    }
}
