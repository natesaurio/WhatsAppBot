using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhatsAppBot.Data.Models.Prompts;

namespace WhatsAppBot.Service.Services.PromptContext
{
    public interface IPromptContextService
    {
        string CreatePromptContext(string Message);
    }

    public class PromptContextService : IPromptContextService
    {
        private readonly Prompt _prompt;
        private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        };

        public PromptContextService(PromptRules promptRules)
        {
            if (promptRules == null)
                throw new ArgumentNullException(nameof(promptRules));

            _prompt = new Prompt
            {
                Rules = promptRules
            };
        }

        public string CreatePromptContext(string message)
        {
            if (_prompt.Rules == null)
                throw new InvalidOperationException("No se han definido reglas para este prompt.");

            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("El mensaje no puede estar vacío.");

            // Construir el prompt para la IA
            var prompt = $@"
        Eres un asistente especializado en programación y desarrollo de software.
        Responde de manera clara y concisa, siguiendo estas reglas:
        - Tono: {_prompt.Rules.ResponseTone}
        - Complejidad: {_prompt.Rules.ResponseComplexity}
        - Longitud máxima: {_prompt.Rules.MaxResponseLength} caracteres
        - No uses palabras ilegales, violentas o sensibles
        - No uses las siguientes palabras: {string.Join(", ", _prompt.Rules.RestrictedWords)}
        - Formato de respuesta: {_prompt.Rules.ExpectedFormat}
        -Cualquier pregunta que no cumpla con estas reglas será rechazada. Y se enviara el siguiente mensaje: Disculpa no pude procesar la petición solo se acepto preguntas relacionadas con programación y desarrollo de software.
        Pregunta del usuario: {message}
    ";
            return prompt;
        }
    }
}
