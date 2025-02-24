using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhatsAppBot.Data.Models.Config;
using WhatsAppBot.Data.Models.Config.Models;
namespace WhatsAppBot.Service.Services.WhatsApp;

public interface IWhatsAppHttpClient
{
    Task<RestResponse> SendAsync(object payload);
}

public class WhatsAppHttpClient : IWhatsAppHttpClient
{
    private readonly IRestClient _restClient;
    private readonly WhatsAppConfig _config;

    public WhatsAppHttpClient(IRestClient restClient, WhatsAppConfig config)
    {

        _config = config;
        _restClient = restClient;
    }

    public async Task<RestResponse> SendAsync(object payload)
    {
        var request = new RestRequest($"https://graph.facebook.com/v21.0/{_config.PhoneId}/messages", Method.Post);
        request.AddHeader("Authorization", $"Bearer {_config.AccessToken}");
        request.AddHeader("Content-Type", "application/json");
        request.AddJsonBody(payload);
        var response = await _restClient.ExecuteAsync(request);
        return response;
    }
}
