using Microsoft.AspNetCore.Mvc;
using WhatsAppBot.Service.Services.Ollama;
using WhatsAppBot.Service.Services.PromptContext;

namespace WhatsAppBot.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {


        private readonly IOllamaService _ollamaService;
        private readonly IPromptContextService _promptContextService;

        public WeatherForecastController(IOllamaService ollamaService, IPromptContextService promptContextService)
        {
            _ollamaService = ollamaService;
            _promptContextService = promptContextService;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IActionResult Get()
        {

            return Ok("es");
        }

        [HttpPost("Respuesta de la Ia")]
        public async Task<IActionResult> GetResponse(string message)
        {
            try
            {
                // primero se crea el  prompt que se le enviara a la IA
                var prompt = _promptContextService.CreatePromptContext(message);

                //ahora se envia el contexto a la IA
                var response = await _ollamaService.GetOllamaResponseAsync(prompt, "llama3.2:latest");
                if (response == null)
                {
                    return BadRequest("No se pudo obtener una respuesta de la IA");
                }
                return Ok(response);
            }
            catch (Exception ex)
            {

                return BadRequest("Error getting Ollama response.");
            }
        }

    }

}
