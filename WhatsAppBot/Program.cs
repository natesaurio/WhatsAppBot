using OllamaSharp;
using RestSharp;
using WhatsAppBot.Data.Models.Config.Models;
using WhatsAppBot.Service.Services.Ollama;
using WhatsAppBot.Service.Services.PromptContext;
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
//crear el objeto Prompt y PrompRules para el bot
var prompRules = new PromptRules
{
    MinLength = 10,
    MaxLength = 500,

    RestrictedWords = new List<string>
    {
        "ilegal", "violencia", "contenido sensible",
        "hackear", "exploit", "malware", "piratería",
        "ataque cibernético", "datos sensibles"
    },

    AllowSpecialCharacters = true,
    AllowedSpecialCharacters = "@#$%^&*()_+-=[]{}|;':,.<>?",

    AllowNumbers = true,
    AllowLetters = true,
    ExpectedFormat = "texto",
    ResponseTone = "educativo",
    ResponseComplexity = "intermedio",

    FullContext = "Soy un asistente especializado en programación, desarrollo de software, " +
                  "normativas técnicas (GDPR, ISO 27001, PCI DSS) y mejores prácticas. " +
                  "Interactúo exclusivamente por WhatsApp. Mis respuestas son en tiempo real " +
                  "y no almaceno conversaciones. Temas permitidos: algoritmos, patrones de diseño, " +
                  "seguridad informática, frameworks y documentación técnica.",

    Language = "es",

    ExamplePrompts = new List<string>
    {
        "¿Qué es el patrón MVC?",
        "Cómo implementar autenticación JWT",
        "Ejemplo de clean architecture en C#",
        "Requisitos de seguridad para APIs REST",
        "Diferencia entre HTTP 1.1 y HTTP/2",
        "Mejores prácticas para manejo de excepciones"
    },

    AllowOpinion = false,
    MaxResponseLength = 500
};

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
//crear el servicio de PromptContext
builder.Services.AddScoped<IPromptContextService>(sp => new PromptContextService(prompRules));
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
