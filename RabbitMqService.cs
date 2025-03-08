using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace NotificationService
{
    public class RabbitMqService : IHostedService, IDisposable
    {
        private readonly ILogger<RabbitMqService> _logger;
        private readonly IConnection _connection;
        private IModel _channel;
        private const string QueueName = "notifications4";
        private readonly MessageRepository _messageRepository;
        private readonly string _exchangeName = "logs";

        public RabbitMqService(ILogger<RabbitMqService> logger,
                               IConnection connection,
                               MessageRepository messageRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _channel = _connection.CreateModel();
                SetupRabbitMq();

                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += async (sender, args) => await HandleReceivedMessage(args);

                _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);
                _logger.LogInformation("RabbitMQ service started and consuming messages.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error starting RabbitMQ service: {ex.Message}");
                throw;
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                _channel?.Close();
                _logger.LogInformation("RabbitMQ service stopped.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error stopping RabbitMQ service: {ex.Message}");
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _channel?.Dispose();
        }

        private void SetupRabbitMq()
        {
            _channel.ExchangeDeclare(exchange: _exchangeName, type: ExchangeType.Fanout);

            _channel.QueueDeclare(queue: QueueName,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            _channel.QueueBind(queue: QueueName, exchange: _exchangeName, routingKey: "");
        }

        private async Task HandleReceivedMessage(BasicDeliverEventArgs args)
        {
            try
            {
                var messageContent = Encoding.UTF8.GetString(args.Body.ToArray());
                _logger.LogInformation($"Received notification: {messageContent}");

                var message = new Message
                {
                    SenderId = "SenderId",
                    ReceiverId = "ReceiverId",
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
        }
    }
}
