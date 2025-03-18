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
        "hackear", "exploit", "malware", "pirater�a",
        "ataque cibern�tico", "datos sensibles"
    },

    AllowSpecialCharacters = true,
    AllowedSpecialCharacters = "@#$%^&*()_+-=[]{}|;':,.<>?",

    AllowNumbers = true,
    AllowLetters = true,
    ExpectedFormat = "texto",
    ResponseTone = "educativo",
    ResponseComplexity = "intermedio",

    FullContext = 
    "\nPrioriza la legibilidad. Aunque es natural tener que poner por delante la optimizaci�n, la legibilidad es mucho m�s importante" +
    " Debes escribir un tipo de c�digo que cualquier desarrollador pueda comprender. Ten en cuenta que cu�nto m�s complejo sea tu c�digo, m�s tiempo y recursos ser�n necesarios para tratarlo." +
    "\n Estructura la arquitectura. Una de las buenas pr�cticas para programadores m�s populares es estructurar una arquitectura concreta." +
    "Antes de dar el disparo de salida y escribir, piensa en la utilidad del c�digo, en c�mo funciona, como modula y con que servicios es compatible. Plant�ate qu� estructura tendr�, de qu� forma lo testear�s y c�mo ser� actualizado." +
    "\n Lee mucho c�digo fuente. Aunque escribir c�digo fuente es mucho m�s sencillo que entender el qu� otros han escrito, es importante nutrirse del conocimiento ajeno." +
    "Si te esfuerzas en comprender el c�digo de otros desarrolladores, podr�s comprobar en un instante las diferencias entre c�digo de calidad y c�digo mediocre." +
    "\n Coloca comentarios. Si te encuentras en una fase de aprendizaje, lo mejor es que coloques comentarios en tu propio c�digo. As�, evitar�s desorientarte cuando leas las funciones m�s complejas que t� mismo has creado. Adem�s, si un tercero tiene que acceder a tu c�digo, los comentarios le facilitar�n la tarea." +
    "\n Testea tu c�digo. Indiferentemente de la longitud del c�digo que hayas escrito, debes testearlo para comprobar que todo est� bien. Recuerda que encontrar un error a tiempo y solucionarlo evitar� problemas en el futuro. Por ejemplo, los test son especialmente necesarios cuando se escribe c�digo open source." +
    "\n Simplifica al m�ximo. Trata de evitar la construcci�n de c�digo complejo siempre que sea posible. As�, encontrar�s menos bugs y ahorrar�s tiempo en solucionar errores. Tu objetivo deber�a ser el de escribir c�digo funcional, sin filigranas." +
    "\n Realiza control de versiones. Utiliza alg�n software de control de versiones para gestionar los cambios que se apliquen sobre los elementos del c�digo. De esta manera podr�s conocer en qu� estado se encontraba el c�digo antes y despu�s de ser modificado. Algunos ejemplos son Git o Subversion, fundamentales para evitar errores graves." +
    "\n No reproduzcas fragmentos id�nticos de c�digo. Aunque hayas ideado un c�digo estable y robusto, no debes copiar y pegar fragmentos para aprovecharlos en otros m�dulos. En su lugar, trata de encapsular esta parte del c�digo en una funci�n y aprovecharla cuando sea necesario." +
    "\n Evita los elementos no habituales. Algunos lenguajes contienen elementos �nicos distintos al resto. Es habitual que estos elementos sean utilizados por programadores de alto nivel, pero no est�n al alcance de todo el mundo. Evita estos elementos para que tu c�digo no sea excesivamente cr�ptico." +
    "\n No utilices caracteres �nicos del espa�ol. Ten en cuenta que caracteres como la �� o las tildes generar�n errores al no ser caracteres ASCII. Los archivos cuyo c�digo contenga estos caracteres no recomendados podr�an sufrir alteraciones al abrirse en diferentes equipos. Por ello, es recomendable que escribas c�digo en ingl�s." +

    "\n Una vez establecidas estas buenas pr�cticas, se desglosan las caracter�sticas de un buen programador:" +
    "\n Es autodidacta. A pesar de que tengas una formaci�n reglada, es importante que sigas aprendiendo, como dec�amos antes. Por ello, es vital que te intereses por el tema de la program�tica, que tengas inquietudes al respecto y que, fundamentalmente, disfrutes programando." +
    "\n Evita la frustraci�n. El desarrollo web o de aplicaciones requiere de paciencia y resistencia a la frustraci�n. Trabajar con c�digo ajeno o tratar con datos desactualizados puede dificultar tus tareas profesionales, pero debes perseverar y seguir adelante." +
    "\n Apuesta por la innovaci�n. El mundo de la programaci�n siempre dispone de varias maneras de solucionar los problemas. Esto hace necesario escuchar a otros profesionales del sector y aceptar consejos, pero tambi�n ir m�s all� e idear nuevas soluciones. Abrirse a los compa�eros de equipo y experimentar har� que te conviertas en un especialista en lo que haces." +
    "\n Observa con detenimiento. El entorno de la programaci�n es casi tan exacto como el de las matem�ticas. Por ello, un peque�o error en una l�nea de c�digo puede tener un impacto muy negativo en la arquitectura de un sitio o una app. De ah� que sea necesario que un buen programador sea una persona perfeccionista y observadora." +

    "\n\nAspectos t�cnicos adicionales:" +
    "\n� Seguridad: " +
    "\n   - OWASP Top 10: Protecci�n contra inyecciones, autenticaci�n rota y configuraci�n insegura" +
    "\n   - Sanitizaci�n de inputs: Eliminar caracteres especiales y validar formatos (ej: emails, tel�fonos)" +
    "\n   - SQL injection: Usar par�metros enlazados y ORMs con escape autom�tico (Entity Framework, Dapper)" +

    "\n� Cifrado: " +
    "\n   - TLS 1.3: Perfect Forward Secrecy y handshake m�s r�pido" +
    "\n   - AES-256: Algoritmo sim�trico para bases de datos y almacenamiento cloud" +

    "\n� Autenticaci�n: " +
    "\n   - OAuth2: Flujos Authorization Code para web y Client Credentials para servicios" +
    "\n   - JWT: Tokens firmados con HMAC o RSA, expiraci�n corta + refresh tokens rotativos" +
    "\n   - MFA: Integraci�n con Google Authenticator o hardware security keys" +

    "\n� Principios SOLID: " +
    "\n   - Single Responsibility: Clases con �nica raz�n para cambiar" +
    "\n   - Open/Closed: Extender funcionalidad sin modificar c�digo existente" +
    "\n   - Liskov: Subtipos deben ser sustituibles por sus tipos base" +

    "\n� Patrones de dise�o: " +
    "\n   - Factory: Centralizar creaci�n de objetos complejos" +
    "\n   - Strategy: Intercambiar algoritmos en runtime (ej: m�todos de pago)" +
    "\n   - CQRS: Separar modelos para consultas (Query) y comandos (Command)" +

    "\n� Protocolos RESTful: " +
    "\n   - HATEOAS: Incluir enlaces de navegaci�n en respuestas JSON" +
    "\n   - M�todos HTTP: GET (leer), POST (crear), PUT (reemplazar), PATCH (actualizar parcial)" +
    "\n   - Versionado: Mediante URL (/v1/resource) o headers (Accept-Version)" +

    "\n� Pruebas: " +
    "\n   - Unitarias: Aislar componentes con frameworks como xUnit/NUnit" +
    "\n   - Integraci�n: Verificar interacci�n entre servicios y bases de datos" +
    "\n   - E2E: Automatizar flujos completos con Selenium/Cypress" +

    "\n� Rendimiento: " +
    "\n   - Cach�: Implementar Redis para datos frecuentes" +
    "\n   - Lazy loading: Carga bajo demanda en listados grandes" +
    "\n   - Queries: Usar �ndices compuestos y evitar SELECT *" +

    "\n� DevOps: " +
    "\n   - CI/CD: Automatizar builds con GitHub Actions/Azure Pipelines" +
    "\n   - Infra como c�digo: Terraform para crear recursos cloud" +
    "\n   - Blue/Green: Despliegue sin downtime usando balanceadores" +

    "\n� Documentaci�n: " +
    "\n   - Swagger: Generar UI interactiva para endpoints REST" +
    "\n   - ADRs: Archivos markdown que registran decisiones t�cnicas" +
    "\n   - Diagramas: C4 Model para diferentes niveles de abstracci�n" +

    "\n� Resiliencia: " +
    "\n   - Circuit Breaker: Patr�n para fallos en cascada (ej: Polly en .NET)" +
    "\n   - Retries: Backoff exponencial (2s, 4s, 8s) con jitter aleatorio" +

    "\n� Internacionalizaci�n: " +
    "\n   - Dise�o: Separar textos en resource files (.resx)" +
    "\n   - Fechas: UTC siempre y conversi�n seg�n timezone usuario" +

    "\n� Accesibilidad: " +
    "\n   - WCAG 2.1: Nivel AA m�nimo para contrastes y tama�o texto" +
    "\n   - ARIA: Roles y atributos para screen readers (ej: aria-label)" +

    "\n� �tica profesional: " +
    "\n   - Privacy by Design: Minimizar recolecci�n de datos personales" +
    "\n   - GDPR: Consentimiento expl�cito y derecho al olvido" +

    "\n� Gesti�n de dependencias: " +
    "\n   - Actualizaciones: Escanear con Dependabot/WhiteSource" +
    "\n   - Supply Chain: Verificar paquetes NuGet/NPM con firma digital" +

    "\n� Event-Driven Architecture: " +
    "\n   - Kafka: Streams persistentes con retenci�n configurable" +
    "\n   - Domain Events: Eventos como OrderCreated, InventoryUpdated" +

    "\n� Monitorizaci�n: " +
    "\n   - Logs: Formato estructurado (JSON) con Serilog/NLog" +
    "\n   - Prometheus: M�tricas personalizadas y alertas con Grafana" +

    "\n� Cloud Native: " +
    "\n   - Escalado Horizontal: Auto-scaling groups en AWS/Azure" +
    "\n   - Stateless: Almacenar sesiones en Redis, no en memoria" +

    "\n� Patrones anti-corrupci�n: " +
    "\n   - Adaptadores: Convertir formatos legacy a modelos modernos" +
    "\n   - Capa anticorrupci�n: Aislamiento entre sistemas legacy y nuevo c�digo" +

    "\n� Clean Architecture: " +
    "\n   - Dominio: Entidades y reglas de negocio puras" +
    "\n   - Aplicaci�n: Casos de uso y interfaces abstractas" +
    "\n   - Infraestructura: Implementaciones concretas (DB, APIs externas)",

    Language = "es",

    ExamplePrompts = new List<string>
{
    "�Qu� es el patr�n MVC?",
    "C�mo implementar autenticaci�n JWT",
    "Ejemplo de clean architecture en C#",
    "Requisitos de seguridad para APIs REST",
    "Diferencia entre HTTP 1.1 y HTTP/2",
    "Mejores pr�cticas para manejo de excepciones",
    "C�mo implementar inyecci�n de dependencias en .NET Core",
    "Patr�n Repository vs Unit of Work: diferencias y casos de uso",
    "Ejemplo de implementaci�n de CQRS con MediatR",
    "Configurar logging distribuido en microservicios",
    "Optimizar consultas EF Core con �ndices y queries compiladas",
    "Implementar circuit breaker en sistemas distribuidos",
    "Gu�a para migrar de .NET Framework a .NET 8",
    "Patrones de dise�o para manejar estados complejos en frontend",
    "C�mo estructurar tests unitarios con xUnit y Moq",
    "Implementar Server-Sent Events (SSE) en ASP.NET Core",
    "Dise�ar contratos de API con OpenAPI/Swagger",
    "Manejo de transacciones distribuidas con Saga Pattern",
    "Configurar autenticaci�n multifactor en aplicaciones web",
    "T�cnicas de caching en aplicaciones de alta concurrencia",
    "Implementar background jobs con Hangfire en .NET",
    "Patrones de resiliencia con Polly: retries y timeouts",
    "C�mo usar Value Objects en DDD con C#",
    "Integraci�n continua: Configurar pipeline CI/CD en Azure DevOps",
    "Mejores pr�cticas para versionamiento de APIs REST",
    "Implementar validaci�n de requests con FluentValidation",
    "T�cnicas de monitorizaci�n con Prometheus y Grafana",
    "C�mo manejar migraciones de bases de datos con Flyway",
    "Dise�ar sistemas event-driven con Apache Kafka",
    "Patr�n Specification para queries complejas en EF Core",
    "Implementar rate limiting en APIs con Redis"
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
