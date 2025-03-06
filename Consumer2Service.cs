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

            // Declare the queue in RabbitMQ
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
                    // Convert the byte array to a message string
                    var messageContent = Encoding.UTF8.GetString(args.Body.ToArray());
                    _logger.LogInformation($"Received notification: {messageContent}");

                    // Create a Message object to save in MongoDB
                    var message = new Message
                    {
                        SenderId = "SenderId",  // This should be dynamically set in your real implementation
                        ReceiverId = "ReceiverId2", // Similarly, dynamically set this
                        Content = messageContent,
                        Timestamp = DateTime.UtcNow
                    };

                    // Use the MessageRepository to save the message in MongoDB
                    await _messageRepository.AddMessageAsync(message);

                    // Acknowledge the message after saving it to MongoDB
                    _channel.BasicAck(args.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    // Log any error that occurs during the message processing
                    _logger.LogError($"Error processing message: {ex.Message}");

                    // Optionally, requeue the message if there is an error
                    _channel.BasicNack(args.DeliveryTag, false, true); // Requeue the message for retry
                }
            };

            // Start consuming messages from RabbitMQ
            _channel.BasicConsume(queue: QueueName,
                                 autoAck: false,  // Turn off auto-ack to manually acknowledge after processing
                                 consumer: consumer);

            // Wait until cancellation is requested or the task is complete
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
