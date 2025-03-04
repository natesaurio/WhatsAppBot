using Microsoft.Extensions.Logging;
using OllamaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhatsAppBot.Service.Services.Ollama
{
    public interface IOllamaService
    {
        Task<string> GetOllamaResponseAsync(string prompt, string model);
    }

    public class OllamaService : IOllamaService
    {
        private readonly IOllamaApiClient _ollamaApiClient;
        private readonly ILogger<OllamaService> _logger;


        public OllamaService(IOllamaApiClient ollamaApiClient, ILogger<OllamaService> logger)
        {
            _logger = logger;
            _ollamaApiClient = ollamaApiClient;
        }

        public async Task<string> GetOllamaResponseAsync(string prompt, string model)
        {
            try
            {
                _ollamaApiClient.SelectedModel = model;
                var responseBuilder = new StringBuilder();
                await foreach (var stream in _ollamaApiClient.GenerateAsync(prompt))
                {
                    responseBuilder.Append(stream.Response);
                }
                return responseBuilder.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Ollama response.");
                throw;
            }
        }
    }
}
