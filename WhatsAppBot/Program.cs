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

    FullContext = 
    "\nPrioriza la legibilidad. Aunque es natural tener que poner por delante la optimización, la legibilidad es mucho más importante" +
    " Debes escribir un tipo de código que cualquier desarrollador pueda comprender. Ten en cuenta que cuánto más complejo sea tu código, más tiempo y recursos serán necesarios para tratarlo." +
    "\n Estructura la arquitectura. Una de las buenas prácticas para programadores más populares es estructurar una arquitectura concreta." +
    "Antes de dar el disparo de salida y escribir, piensa en la utilidad del código, en cómo funciona, como modula y con que servicios es compatible. Plantéate qué estructura tendrá, de qué forma lo testearás y cómo será actualizado." +
    "\n Lee mucho código fuente. Aunque escribir código fuente es mucho más sencillo que entender el qué otros han escrito, es importante nutrirse del conocimiento ajeno." +
    "Si te esfuerzas en comprender el código de otros desarrolladores, podrás comprobar en un instante las diferencias entre código de calidad y código mediocre." +
    "\n Coloca comentarios. Si te encuentras en una fase de aprendizaje, lo mejor es que coloques comentarios en tu propio código. Así, evitarás desorientarte cuando leas las funciones más complejas que tú mismo has creado. Además, si un tercero tiene que acceder a tu código, los comentarios le facilitarán la tarea." +
    "\n Testea tu código. Indiferentemente de la longitud del código que hayas escrito, debes testearlo para comprobar que todo esté bien. Recuerda que encontrar un error a tiempo y solucionarlo evitará problemas en el futuro. Por ejemplo, los test son especialmente necesarios cuando se escribe código open source." +
    "\n Simplifica al máximo. Trata de evitar la construcción de código complejo siempre que sea posible. Así, encontrarás menos bugs y ahorrarás tiempo en solucionar errores. Tu objetivo debería ser el de escribir código funcional, sin filigranas." +
    "\n Realiza control de versiones. Utiliza algún software de control de versiones para gestionar los cambios que se apliquen sobre los elementos del código. De esta manera podrás conocer en qué estado se encontraba el código antes y después de ser modificado. Algunos ejemplos son Git o Subversion, fundamentales para evitar errores graves." +
    "\n No reproduzcas fragmentos idénticos de código. Aunque hayas ideado un código estable y robusto, no debes copiar y pegar fragmentos para aprovecharlos en otros módulos. En su lugar, trata de encapsular esta parte del código en una función y aprovecharla cuando sea necesario." +
    "\n Evita los elementos no habituales. Algunos lenguajes contienen elementos únicos distintos al resto. Es habitual que estos elementos sean utilizados por programadores de alto nivel, pero no están al alcance de todo el mundo. Evita estos elementos para que tu código no sea excesivamente críptico." +
    "\n No utilices caracteres únicos del español. Ten en cuenta que caracteres como la «ñ» o las tildes generarán errores al no ser caracteres ASCII. Los archivos cuyo código contenga estos caracteres no recomendados podrían sufrir alteraciones al abrirse en diferentes equipos. Por ello, es recomendable que escribas código en inglés." +

    "\n Una vez establecidas estas buenas prácticas, se desglosan las características de un buen programador:" +
    "\n Es autodidacta. A pesar de que tengas una formación reglada, es importante que sigas aprendiendo, como decíamos antes. Por ello, es vital que te intereses por el tema de la programática, que tengas inquietudes al respecto y que, fundamentalmente, disfrutes programando." +
    "\n Evita la frustración. El desarrollo web o de aplicaciones requiere de paciencia y resistencia a la frustración. Trabajar con código ajeno o tratar con datos desactualizados puede dificultar tus tareas profesionales, pero debes perseverar y seguir adelante." +
    "\n Apuesta por la innovación. El mundo de la programación siempre dispone de varias maneras de solucionar los problemas. Esto hace necesario escuchar a otros profesionales del sector y aceptar consejos, pero también ir más allá e idear nuevas soluciones. Abrirse a los compañeros de equipo y experimentar hará que te conviertas en un especialista en lo que haces." +
    "\n Observa con detenimiento. El entorno de la programación es casi tan exacto como el de las matemáticas. Por ello, un pequeño error en una línea de código puede tener un impacto muy negativo en la arquitectura de un sitio o una app. De ahí que sea necesario que un buen programador sea una persona perfeccionista y observadora." +

    "\n\nAspectos técnicos adicionales:" +
    "\n• Seguridad: " +
    "\n   - OWASP Top 10: Protección contra inyecciones, autenticación rota y configuración insegura" +
    "\n   - Sanitización de inputs: Eliminar caracteres especiales y validar formatos (ej: emails, teléfonos)" +
    "\n   - SQL injection: Usar parámetros enlazados y ORMs con escape automático (Entity Framework, Dapper)" +

    "\n• Cifrado: " +
    "\n   - TLS 1.3: Perfect Forward Secrecy y handshake más rápido" +
    "\n   - AES-256: Algoritmo simétrico para bases de datos y almacenamiento cloud" +

    "\n• Autenticación: " +
    "\n   - OAuth2: Flujos Authorization Code para web y Client Credentials para servicios" +
    "\n   - JWT: Tokens firmados con HMAC o RSA, expiración corta + refresh tokens rotativos" +
    "\n   - MFA: Integración con Google Authenticator o hardware security keys" +

    "\n• Principios SOLID: " +
    "\n   - Single Responsibility: Clases con única razón para cambiar" +
    "\n   - Open/Closed: Extender funcionalidad sin modificar código existente" +
    "\n   - Liskov: Subtipos deben ser sustituibles por sus tipos base" +

    "\n• Patrones de diseño: " +
    "\n   - Factory: Centralizar creación de objetos complejos" +
    "\n   - Strategy: Intercambiar algoritmos en runtime (ej: métodos de pago)" +
    "\n   - CQRS: Separar modelos para consultas (Query) y comandos (Command)" +

    "\n• Protocolos RESTful: " +
    "\n   - HATEOAS: Incluir enlaces de navegación en respuestas JSON" +
    "\n   - Métodos HTTP: GET (leer), POST (crear), PUT (reemplazar), PATCH (actualizar parcial)" +
    "\n   - Versionado: Mediante URL (/v1/resource) o headers (Accept-Version)" +

    "\n• Pruebas: " +
    "\n   - Unitarias: Aislar componentes con frameworks como xUnit/NUnit" +
    "\n   - Integración: Verificar interacción entre servicios y bases de datos" +
    "\n   - E2E: Automatizar flujos completos con Selenium/Cypress" +

    "\n• Rendimiento: " +
    "\n   - Caché: Implementar Redis para datos frecuentes" +
    "\n   - Lazy loading: Carga bajo demanda en listados grandes" +
    "\n   - Queries: Usar índices compuestos y evitar SELECT *" +

    "\n• DevOps: " +
    "\n   - CI/CD: Automatizar builds con GitHub Actions/Azure Pipelines" +
    "\n   - Infra como código: Terraform para crear recursos cloud" +
    "\n   - Blue/Green: Despliegue sin downtime usando balanceadores" +

    "\n• Documentación: " +
    "\n   - Swagger: Generar UI interactiva para endpoints REST" +
    "\n   - ADRs: Archivos markdown que registran decisiones técnicas" +
    "\n   - Diagramas: C4 Model para diferentes niveles de abstracción" +

    "\n• Resiliencia: " +
    "\n   - Circuit Breaker: Patrón para fallos en cascada (ej: Polly en .NET)" +
    "\n   - Retries: Backoff exponencial (2s, 4s, 8s) con jitter aleatorio" +

    "\n• Internacionalización: " +
    "\n   - Diseño: Separar textos en resource files (.resx)" +
    "\n   - Fechas: UTC siempre y conversión según timezone usuario" +

    "\n• Accesibilidad: " +
    "\n   - WCAG 2.1: Nivel AA mínimo para contrastes y tamaño texto" +
    "\n   - ARIA: Roles y atributos para screen readers (ej: aria-label)" +

    "\n• Ética profesional: " +
    "\n   - Privacy by Design: Minimizar recolección de datos personales" +
    "\n   - GDPR: Consentimiento explícito y derecho al olvido" +

    "\n• Gestión de dependencias: " +
    "\n   - Actualizaciones: Escanear con Dependabot/WhiteSource" +
    "\n   - Supply Chain: Verificar paquetes NuGet/NPM con firma digital" +

    "\n• Event-Driven Architecture: " +
    "\n   - Kafka: Streams persistentes con retención configurable" +
    "\n   - Domain Events: Eventos como OrderCreated, InventoryUpdated" +

    "\n• Monitorización: " +
    "\n   - Logs: Formato estructurado (JSON) con Serilog/NLog" +
    "\n   - Prometheus: Métricas personalizadas y alertas con Grafana" +

    "\n• Cloud Native: " +
    "\n   - Escalado Horizontal: Auto-scaling groups en AWS/Azure" +
    "\n   - Stateless: Almacenar sesiones en Redis, no en memoria" +

    "\n• Patrones anti-corrupción: " +
    "\n   - Adaptadores: Convertir formatos legacy a modelos modernos" +
    "\n   - Capa anticorrupción: Aislamiento entre sistemas legacy y nuevo código" +

    "\n• Clean Architecture: " +
    "\n   - Dominio: Entidades y reglas de negocio puras" +
    "\n   - Aplicación: Casos de uso y interfaces abstractas" +
    "\n   - Infraestructura: Implementaciones concretas (DB, APIs externas)",

    Language = "es",

    ExamplePrompts = new List<string>
{
    "¿Qué es el patrón MVC?",
    "Cómo implementar autenticación JWT",
    "Ejemplo de clean architecture en C#",
    "Requisitos de seguridad para APIs REST",
    "Diferencia entre HTTP 1.1 y HTTP/2",
    "Mejores prácticas para manejo de excepciones",
    "Cómo implementar inyección de dependencias en .NET Core",
    "Patrón Repository vs Unit of Work: diferencias y casos de uso",
    "Ejemplo de implementación de CQRS con MediatR",
    "Configurar logging distribuido en microservicios",
    "Optimizar consultas EF Core con índices y queries compiladas",
    "Implementar circuit breaker en sistemas distribuidos",
    "Guía para migrar de .NET Framework a .NET 8",
    "Patrones de diseño para manejar estados complejos en frontend",
    "Cómo estructurar tests unitarios con xUnit y Moq",
    "Implementar Server-Sent Events (SSE) en ASP.NET Core",
    "Diseñar contratos de API con OpenAPI/Swagger",
    "Manejo de transacciones distribuidas con Saga Pattern",
    "Configurar autenticación multifactor en aplicaciones web",
    "Técnicas de caching en aplicaciones de alta concurrencia",
    "Implementar background jobs con Hangfire en .NET",
    "Patrones de resiliencia con Polly: retries y timeouts",
    "Cómo usar Value Objects en DDD con C#",
    "Integración continua: Configurar pipeline CI/CD en Azure DevOps",
    "Mejores prácticas para versionamiento de APIs REST",
    "Implementar validación de requests con FluentValidation",
    "Técnicas de monitorización con Prometheus y Grafana",
    "Cómo manejar migraciones de bases de datos con Flyway",
    "Diseñar sistemas event-driven con Apache Kafka",
    "Patrón Specification para queries complejas en EF Core",
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
