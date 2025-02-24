using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WhatsAppBot.Service.Services.WhatsApp;

namespace WhatsAppBot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SendMessageController : ControllerBase
    {
        private readonly IWhatsAppBusinessService _whatsAppBusinessService;

        public SendMessageController(IWhatsAppBusinessService whatsAppBusinessService)
        {
            _whatsAppBusinessService = whatsAppBusinessService;
        }

        [HttpGet("SendMessage")]
        public async Task<IActionResult> Get()
        {
            var messaje = "Este es el mensaje de prueba actual";
            var response = await _whatsAppBusinessService.SendMessageAsync(messaje);
            return Ok(response);
        }
    }
}
