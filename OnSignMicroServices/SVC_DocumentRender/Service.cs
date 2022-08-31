using System;
using System.ServiceProcess;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Newtonsoft.Json;
using System.Reflection;
using OnSign.BusinessLogic.Document;
using OnSign.Common.Helpers;
using OnSign.BusinessObject.Sign;
using System.Threading;
//using Newtonsoft.Json;

namespace SVC_DocumentRender
{
    public partial class Service : ServiceBase
    {
        private IConnection connection = null;
        private readonly DocumentBLL documentBLL = new DocumentBLL();
        private readonly string exchangeName = ConfigHelper.RabbitMQRenderTopic;

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
            ListRabbitMQ();
        }

        protected override void OnStop()
        {
        }


        private void ListRabbitMQ()
        {
            var connectionFactory = new ConnectionFactory
            {
                UserName = ConfigHelper.RabbitMQUserName,
                Password = ConfigHelper.RabbitMQPassword,
                VirtualHost = ConfigHelper.RabbitMQVirtualHost,
                HostName = ConfigHelper.RabbitMQHostName
            };

            connection = connectionFactory.CreateConnection();

            IModel channelSend = connection.CreateModel();
            channelSend.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Topic, durable: false, autoDelete: false, arguments: null);

            this.ReceiverConverPdfToImage();
            this.ReceiverConverAndRenderRawToPdf();
        }

        private void ReceiverConverAndRenderRawToPdf()
        {
            var clientID = Guid.NewGuid().ToString();
            IModel channelReceive = connection.CreateModel();
            channelReceive.QueueDeclare(queue: clientID, durable: true, exclusive: false, autoDelete: false, arguments: null);
            channelReceive.QueueBind(queue: clientID, exchange: exchangeName, routingKey: "document_render_raw");

            var consumer = new EventingBasicConsumer(channelReceive);
            channelReceive.BasicConsume(queue: clientID, autoAck: true, consumer: consumer);
            consumer.Received += (s, ee) =>
            {
                try
                {
                    byte[] body = ee.Body.ToArray();
                    string message = Encoding.UTF8.GetString(body);
                    if (!string.IsNullOrEmpty(message))
                    {
                        DocumentBO document = JsonConvert.DeserializeObject<DocumentBO>(message, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                        documentBLL.ConvertFileToPdf(document);
                    }
                }
                catch (Exception ex)
                {
                    string msg = "Lỗi chuyển đổi định dạng file tải lên sang file PDF";
                    ConfigHelper.Instance.WriteLogException(msg, ex, MethodBase.GetCurrentMethod().Name, "ListRabbitMQ - Received");
                }
            };
        }

        private void ReceiverConverPdfToImage()
        {
            var clientID = Guid.NewGuid().ToString();
            IModel channelReceive = connection.CreateModel();
            channelReceive.QueueDeclare(queue: clientID, durable: true, exclusive: false, autoDelete: false, arguments: null);
            channelReceive.QueueBind(queue: clientID, exchange: exchangeName, routingKey: "document_render_signed");

            var consumer = new EventingBasicConsumer(channelReceive);
            channelReceive.BasicConsume(queue: clientID, autoAck: true, consumer: consumer);
            consumer.Received += (s, ee) =>
            {
                try
                {
                    byte[] body = ee.Body.ToArray();
                    string message = Encoding.UTF8.GetString(body);
                    if (!string.IsNullOrEmpty(message))
                    {
                        DocumentBO document = JsonConvert.DeserializeObject<DocumentBO>(message, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                        documentBLL.ConvertPdfToImage(document);
                    }
                }
                catch (Exception ex)
                {
                    string msg = "Lỗi chuyển đổi định dạng file tải lên sang file PDF";
                    ConfigHelper.Instance.WriteLogException(msg, ex, MethodBase.GetCurrentMethod().Name, "ListRabbitMQ - Received");
                }
            };
        }

    }
}
