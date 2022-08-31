using System;
using WebSockets.Common;
using System.IO;

namespace WebSocketsCmd
{
    internal class WebSocketLogger : IWebSocketLogger
    {
        const String LOG_FOLDER = "\\Plugin Log";
        private static String pathLog = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + LOG_FOLDER,
            DateTime.Now.ToString("dd-MM-yyyy") + ".txt");
        public static void WRITE(String funcName, String log)
        {
            try
            {
                String logLine = DateTime.Now.ToShortTimeString() + "\t[" + funcName + "]\t" + log;
                File.AppendAllText(pathLog, logLine + Environment.NewLine);
            }
            catch (Exception ex)
            {
            }
            return;
        }
        public void Information(Type type, string format, params object[] args)
        {
            WRITE(type.ToString(), String.Format(format, args));
        }
        public void Information(string func, string format, params object[] args)
        {
            WRITE(func, String.Format(format, args));
        }
        public void Warning(Type type, string format, params object[] args)
        {
            WRITE(type.ToString(), String.Format(format, args));
        }

        public void Error(Type type, string format, params object[] args)
        {
            WRITE(type.ToString(), String.Format(format, args));
        }

        public void Error(Type type, Exception exception)
        {
            WRITE(type.ToString(), String.Format("{0}", exception));
        }
    }
}
