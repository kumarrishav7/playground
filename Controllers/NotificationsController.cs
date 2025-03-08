using Microsoft.AspNetCore.Mvc;

namespace NotificationService.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly IMessagePublisher _messagePublisher;

        public NotificationsController(IMessagePublisher messagePublisher)
        {
            _messagePublisher = messagePublisher;
        }

        [HttpPost]
        public IActionResult SendNotification([FromBody] string message)
        {
            _messagePublisher.PublishMessage(message);
            return Ok(new { message = "Notification sent" });
        }

        [HttpGet]
        public IActionResult GetNotification()
        {
            return Ok();
        }
    }

}
