using Microsoft.AspNetCore.Mvc;
using System.Net;
using WhatsAppBot.Data.Models;
using System.Net.Http.Headers;
using WhatsAppBot.Service.Services.WhatsApp;
using WhatsAppBot.Service.Services.Ollama;
using WhatsAppBot.Service.Services.PromptContext;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WhatsAppBot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecieveMessageController : ControllerBase
    {


        private readonly IWhatsAppBusinessService _whatsAppBusinessService;
        private readonly IPromptContextService _promptContextService;
        private readonly IOllamaService _ollamaService;
        private readonly string _secretToken;

        public RecieveMessageController(IWhatsAppBusinessService whatsAppBusinessService, IOllamaService ollamaService, IPromptContextService promptContextService, IConfiguration configuration)
        {
            _whatsAppBusinessService = whatsAppBusinessService;
            _ollamaService = ollamaService;
            _promptContextService = promptContextService;
            _secretToken = configuration["secretToken"];
        }

        //recibimos los datos de validacion via get
        [HttpGet]
        [Route("webhook")]
        public string Webhook(
            [FromQuery(Name = "hub.mode")] string mode,
            [FromQuery(Name = "hub.challenge")] string challenge,
            [FromQuery(Name = "hub.verify_token")] string verifyToken
            )
        {
            if (verifyToken == _secretToken)
            {
                return challenge;
            }
            else
            {
                return "Error, bad token";
            }
        }

        [HttpPost]
        [Route("webhook")]
        public async Task<HttpResponseMessage> Data([FromBody] WebHookResponseModel entry)
        {
            try
            {
                // Mensaje recibido
                string message = entry.entry[0].changes[0].value.messages[0].text.body;
                // Extraemos el id único del mensaje
                string wa_id = entry.entry[0].changes[0].value.messages[0].id;
                // Extraemos el número de teléfono del remitente
                string phone = entry.entry[0].changes[0].value.messages[0].from;


                //pasamos el contexto a la IA para que limite las respuestas
                string promp = _promptContextService.CreatePromptContext(message);
                //configuramos a Ollama para que responda el mensaje
                string responseMessage = await _ollamaService.GetOllamaResponseAsync(promp, "llama3.2:latest");
                await _whatsAppBusinessService.SendMessageAsync(responseMessage, phone);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent("Mensaje procesado correctamente");
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
                return response;
            }
            catch (Exception ex)
            {
                var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                response.Content = new StringContent($"Error interno del servidor: {ex.Message}");
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
                return response;
            }
        }


    }
}
