using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace NotificationService
{
    public class Consumer2Service : IHostedService
    {
        private readonly ILogger<Consumer2Service> _logger;
        private IConnection _connection;
        private IModel _channel;
        private const string QueueName = "notifications4";
        private MessageRepository _messageRepository;

        public Consumer2Service(ILogger<Consumer2Service> logger, MessageRepository messageRepository)
        {
            _logger = logger;
            _messageRepository = messageRepository;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(exchange: "logs", type: ExchangeType.Fanout);
            _channel.QueueDeclare(queue: QueueName,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            _channel.QueueBind(queue: QueueName, exchange: "logs", routingKey: "");

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (sender, args) =>
            {
                try
                {
                    var messageContent = Encoding.UTF8.GetString(args.Body.ToArray());
                    _logger.LogInformation($"Received notification: {messageContent}");

                    var message = new Message
                    {
                        SenderId = "SenderId",  
                        ReceiverId = "ReceiverId2",
                        Content = messageContent,
                        Timestamp = DateTime.UtcNow
                    };

                    await _messageRepository.AddMessageAsync(message);
                    _channel.BasicAck(args.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error processing message: {ex.Message}");
                    _channel.BasicNack(args.DeliveryTag, false, true);
                }
            };

            _channel.BasicConsume(queue: QueueName,
                                 autoAck: false,
                                 consumer: consumer);
            await Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _channel?.Close();
            _connection?.Close();
            return Task.CompletedTask;
        }
    }

}
