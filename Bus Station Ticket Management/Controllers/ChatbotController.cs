using Microsoft.AspNetCore.Mvc;
using Bus_Station_Ticket_Management.Services;

namespace Bus_Station_Ticket_Management.Controllers
{
    public class ChatbotController : Controller
    {
        private readonly IChatbotService _chatbotService;

        public ChatbotController(IChatbotService chatbotService)
        {
            _chatbotService = chatbotService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatMessage message)
        {
            if (string.IsNullOrWhiteSpace(message?.Content))
            {
                return BadRequest("Message cannot be empty");
            }

            var response = await _chatbotService.ProcessMessage(message.Content);
            return Json(new { response });
        }
    }

    public class ChatMessage
    {
        public string Content { get; set; }
    }
} 