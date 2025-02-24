using OllamaSharp;
using RestSharp;
using WhatsAppBot.Data.Models.Config.Models;
using WhatsAppBot.Service.Services.Ollama;
using WhatsAppBot.Service.Services.WhatsApp;
using WhatsappBusiness.CloudApi.Configurations;
using WhatsappBusiness.CloudApi.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration
      .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();
var whatsAppConfig = builder.Configuration.GetSection("WhatsAppConfig").Get<WhatsAppConfig>();
var ollamaApiUri = new Uri(builder.Configuration.GetValue<string>("OllamaApiUri"));
//build the WhatsApp Config
//LOGGIN
builder.Services.AddSingleton(whatsAppConfig);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IWhatsAppHttpClient>(sp => new WhatsAppHttpClient(new RestClient(), whatsAppConfig));
builder.Services.AddScoped<ITextMessageBuilder>(message => new TextMessageBuilder(whatsAppConfig));
builder.Services.AddScoped<IWhatsAppBusinessService, WhatsAppBusinessService>();
builder.Services.AddScoped<IOllamaApiClient>(sp => new OllamaApiClient(ollamaApiUri));
builder.Services.AddScoped<IOllamaService, OllamaService>();
var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
