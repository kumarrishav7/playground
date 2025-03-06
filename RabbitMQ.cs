using RabbitMQ.Client;
using System.Text;

namespace NotificationService
{
    public class RabbitMqChannel : IQueueChannel
    {
        private readonly IModel _channel;

        public RabbitMqChannel(IConnection connection)
        {
            _channel = connection.CreateModel();
        }

        public void DeclareQueue(string queueName)
        {
            _channel.QueueDeclare(queue: queueName,
                                  durable: true,
                                  exclusive: false,
                                  autoDelete: false,
                                  arguments: null);
        }

        public void Publish(string exchange, string routingKey, byte[] message)
        {
            _channel.BasicPublish(exchange: exchange,
                                  routingKey: routingKey,
                                  basicProperties: null,
                                  body: message);
        }

        public void Dispose()
        {
            _channel?.Dispose();
        }
    }

    public class RabbitMqMessagePublisher : IMessagePublisher
    {
        private readonly IQueueChannel _queueChannel;

        public RabbitMqMessagePublisher(IQueueChannel queueChannel)
        {
            _queueChannel = queueChannel;
        }

        public void PublishMessage(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            _queueChannel.Publish("logs", "notifications4", body);
        }
    }

}
