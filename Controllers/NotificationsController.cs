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

        public NotificationsController(IMessagePublisher messagePublisher)
        {
            _messagePublisher = messagePublisher;
        }

        [HttpPost]
        public IActionResult SendNotification([FromBody] RequestModel message)
        {
            _messagePublisher.PublishMessage(message.message);
            return Ok(new { message = "Notification sent" });
        }

        [HttpGet]
        public IActionResult GetNotification()
        {
            return Ok("good");
        }
    }

    public class RequestModel
    {
        public required string message { get; set; }
    }

}
