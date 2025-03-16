using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace NotificationService.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("sliding")]
    public class NotificationsController : ControllerBase
    {
        private readonly IMessagePublisher _messagePublisher;
        private readonly MessageRepository _messageRepository;

        public NotificationsController(IMessagePublisher messagePublisher, MessageRepository messageRepository)
        {
            _messagePublisher = messagePublisher;
            _messageRepository = messageRepository;
        }

        [HttpPost]
        public IActionResult SendNotification([FromBody] RequestModel message)
        {
            _messagePublisher.PublishMessage(message.message);
            return Ok(new { message = "Notification sent" });
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Message>>> GetNotification()
        {
            List<Message> _m = await _messageRepository.GetMessagesAsync("SenderId", "ReceiverId");
            return Ok(_m);
        }
    }

    public class RequestModel
    {
        public required string message { get; set; }
    }

}
