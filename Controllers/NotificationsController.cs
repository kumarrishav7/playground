using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.Text;
using System.Threading.Channels;

namespace NotificationService.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public NotificationsController()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(exchange: "logs", type: ExchangeType.Fanout);
        }

        [HttpPost]
        public IActionResult SendNotification([FromBody] string message)
        {
            _channel.QueueDeclare(queue: "notifications4",
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(exchange: "logs",
                                 routingKey: "notifications4",
                                 basicProperties: null,
                                 body: body);

            return Ok(new { message = "Notification sent" });
        }
    }

}
