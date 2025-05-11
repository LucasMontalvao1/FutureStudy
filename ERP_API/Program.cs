using ERP_API.Extensions;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog 
ConfigureLogger();

// Usar Serilog como provider de logging
builder.Host.UseSerilog();

// Adicionar serviços ao container
builder.Services.AddEndpointsApiExplorer();

// Configurar serviços da aplicação
builder.Services
    .AddControllersConfiguration()  // Já configura AddControllers() internamente
    .AddSwaggerConfiguration()
    .AddMappingConfiguration()
    .AddValidationConfiguration()
    .AddSqlConfiguration()
    .AddRepositoryConfiguration()
    .AddServiceConfiguration()
    .AddCorsConfiguration()
    .AddAuthenticationConfiguration(builder.Configuration);

// Construir o aplicativo
var app = builder.Build();

// Configurar pipeline de requisições
app.ConfigureApplicationPipeline(app.Environment);

try
{
    Log.Information("Iniciando aplicação...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Erro fatal ao iniciar o aplicativo");
}
finally
{
    Log.CloseAndFlush();
}

// Configuração do logger
void ConfigureLogger()
{
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "Study_API")
        .WriteTo.Console()
        .WriteTo.File("Logs/myapp-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 25,
            fileSizeLimitBytes: 10_000_000,
            rollOnFileSizeLimit: true)
        .CreateLogger();
}