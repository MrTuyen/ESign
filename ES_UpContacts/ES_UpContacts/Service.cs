using System;
using System.Data;
using System.ServiceProcess;
using System.Configuration;
using Npgsql;
using Newtonsoft.Json;
using System.IO;
using Elasticsearch.Net;
using System.Threading;

namespace ES_UpContacts
{
    public partial class Service : ServiceBase
    {
        private readonly string ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionStringDS"].ConnectionString;
        private readonly string LogPath = ConfigurationManager.AppSettings["LogPath"];

        private NpgsqlConnection conn;

        public Service()
        {
            InitializeComponent();
        }

        public void OnDebug()
        {
            OnStart(null);
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                System.Threading.Thread newProcess = new System.Threading.Thread(ListenForNotifications)
                {
                    IsBackground = true
                };
                newProcess.Start();
            }
            catch (Exception objMainEx)
            {
                WriteLog("OnStart", objMainEx.ToString());
            }
        }

        protected override void OnStop()
        {
            WriteLog("OnStop", "OnStop");
        }

        private void ExecuteListenNotify()
        {
            conn.Open();
            var listenCommand = conn.CreateCommand();
            listenCommand.CommandText = $"listen receive_notify;";
            listenCommand.ExecuteNonQuery();
        }

        private void ListenForNotifications()
        {
            conn = new NpgsqlConnection(ConnectionString);
            ExecuteListenNotify();
            //Add event
            conn.Notification += PostgresNotificationReceived;
            while (true)
            {
                try
                {
                    if (!(conn.State == ConnectionState.Open))
                    {
                        ExecuteListenNotify();
                    }
                    conn.Wait();
                }
                catch (Exception ex)
                {
                    WriteLog("ListenForNotifications", ex.ToString());
                    System.Threading.Thread.Sleep(10000);
                }
            }
        }

        private void PostgresNotificationReceived(object sender, NpgsqlNotificationEventArgs e)
        {
            string data = e.AdditionalInformation;
            var emailData = JsonConvert.DeserializeObject<MailToBO>(e.AdditionalInformation);
            try
            {
                if (!string.IsNullOrEmpty(emailData.EMAIL) && emailData.CREATEDBYUSER > 0)
                    new Thread(() =>
                    {
                        ElasticIndexer.Current.IndexClient.Index(emailData, i => i
                            .Index("contacts_list")
                            .Type("contacts")
                            .Id($"{emailData.CREATEDBYUSER}_{emailData.EMAIL}")
                            .Refresh(Refresh.True)
                        );
                    }).Start();
            }
            catch (Exception ex)
            {
                WriteLog("PostgresNotificationReceived", ex.ToString());
            }
        }

        private void WriteLog(string functionName, string strMessage)
        {
            try
            {
                string strErrorLogFile = LogPath + "Elasticsearch-log-" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
                File.AppendAllText(strErrorLogFile, string.Format("{0}{1} {2}: {3}", Environment.NewLine, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), functionName, strMessage));
            }
            catch (Exception ex)
            {
                string strErrorLogFile = LogPath + "Elasticsearch-log-" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
                File.AppendAllText(strErrorLogFile, string.Format("{0}{1} {2}: {3}", Environment.NewLine, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "WriteLog", ex.ToString()));
            }
        }

        public class MailToBO
        {
            public int CREATEDBYUSER { get; set; }
            public string NAME { get; set; }
            public string EMAIL { get; set; }
            public string TAXCODE { get; set; }
            public string ADDRESS { get; set; }
            public string IDNUMBER { get; set; }
            public string PHONENUMBER { get; set; }
        }
    }
}
