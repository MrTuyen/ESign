using Newtonsoft.Json;
using OnSign.Common.Helpers;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnSign.BusinessLogic.Partners
{
    public class RabbitMQDLL
    {
        private IConnection connection;
        private IModel channelSend;

        public RabbitMQDLL()
        {
            if (connection == null || !connection.IsOpen)
            {
                var connectionFactory = new ConnectionFactory
                {
                    UserName = ConfigHelper.RabbitMQUserName,
                    Password = ConfigHelper.RabbitMQPassword,
                    VirtualHost = ConfigHelper.RabbitMQVirtualHost,
                    HostName = ConfigHelper.RabbitMQHostName
                };
                connection = connectionFactory.CreateConnection();
                channelSend = connection.CreateModel();
            }
        }

        ~RabbitMQDLL()
        {
            connection.Close();
            connection.Dispose();
            connection = null;
            GC.Collect();
        }

        public void RabbitMQ_SendMessage(string topic, string routingKey, string strMessage)
        {
            byte[] message = Encoding.UTF8.GetBytes(strMessage);
            channelSend.BasicPublish(exchange: topic, routingKey: routingKey, basicProperties: null, body: message);
        }
    }
}
