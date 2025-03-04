using Microsoft.Extensions.Logging;
using Newtonsoft;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WhatsappBusiness.CloudApi.Interfaces;
using WhatsappBusiness.CloudApi.Messages.ReplyRequests;
using WhatsappBusiness.CloudApi.Messages.Requests;
using WhatsappBusiness.CloudApi.Webhook;

namespace WhatsAppBot.Service.Services.WhatsApp;

public interface IWhatsAppBusinessService
{
    Task<RestResponse> SendMessageAsync(string message, string phoneNumber);
}
public class WhatsAppBusinessService : IWhatsAppBusinessService
{
    private readonly IWhatsAppHttpClient _whatsAppHttpClient;
    private readonly ITextMessageBuilder _textMessageBuilder;
    private readonly ILogger<WhatsAppBusinessService> _logger;

    public WhatsAppBusinessService(
        IWhatsAppHttpClient whatsAppHttpClient,
        ITextMessageBuilder textMessageBuilder,
        ILogger<WhatsAppBusinessService> logger)
    {
        _textMessageBuilder = textMessageBuilder;
        _whatsAppHttpClient = whatsAppHttpClient;
        _logger = logger;
    }



    public async Task<RestResponse> SendMessageAsync(string message, string phoneNumber)
    {
        try
        {
            var payload = _textMessageBuilder.BuildPayload(message, phoneNumber);
            var response = await _whatsAppHttpClient.SendAsync(payload);

            if (response.IsSuccessful)
                _logger.LogInformation("Message sent successfully!");
            else
                _logger.LogError("Error: {StatusCode}, {Content}", response.StatusCode, response.Content);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending message.");
            throw;
        }
    }
}
