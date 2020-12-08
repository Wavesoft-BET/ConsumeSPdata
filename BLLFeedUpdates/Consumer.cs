using System;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace BLLFeedUpdates
{
    public class Consumer
    {
        private ConnectionFactory _factory;
        private IConnection _connection;
        private IModel _model;
        private string _queueName;

        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void CreateConnection(string hostName, string username, string password, int port)
        {
            if (_factory == null)
            {
                _factory = new ConnectionFactory
                {
                    HostName = hostName,
                    UserName = username,
                    Password = password,
                    Port = port
                };
            }

            if (_connection == null)
            {
                _connection = _factory.CreateConnection();
            }

            if (_model == null)
            {
                _model = _connection.CreateModel();
            }
        }

        public void ConsumeData(string queueName, string exchangeName)
        {
            _model.ExchangeDeclare(exchangeName, "topic");
            _model.QueueDeclare(queueName, true, false, false);
            _model.QueueBind(queueName, exchangeName, queueName);

            var consumer = new EventingBasicConsumer(_model);
            _queueName = queueName;

            consumer.Received += Consumer_Received;

            _model.BasicConsume(queueName, false, consumer);
        }

        private void Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            string json = "";
            var consumer = (EventingBasicConsumer)sender;
            try
            {
                json = ObjectSerializer.Deserialize(e.Body);
                var sportId = json.Split(',')[0].Split(':')[1].Trim('"');
                FeedUpdateRepository.SaveToDatabase(json, Convert.ToInt32(sportId));              
                consumer.Model.BasicAck(e.DeliveryTag, false);
                log.Info("Fetched from queue " + _queueName + " and Acknowledged: SportId: " + sportId);
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString() + Environment.NewLine + json);
                consumer.Model.BasicNack(e.DeliveryTag, false, true);
            }
        }

        public void CloseConnection()
        {
            _model.Close();
            _connection.Close();
        }
    }
}
