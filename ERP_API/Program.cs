using ERP_API.Infra.Data;
using ERP_API.Models.DTOs;
using ERP_API.Repositorys;
using ERP_API.Repositorys.Interfaces;
using ERP_API.Services;
using ERP_API.Services.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using ERP_API.SQL;
using ERP_API.Repositories.Interfaces;
using ERP_API.Models.Enums;
using ERP_API.Validators;
using ERP_API.Mapping;
using ERP_API.Models.Entities;

var builder = WebApplication.CreateBuilder(args);

// Configuração do Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information() 
    .WriteTo.Console()
    .WriteTo.File("Logs/myapp-.log", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configuração do Swagger com suporte a JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ERP API", Version = "v1" });

    // Configuração para autenticação JWT no Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header usando o esquema Bearer.
                      Digite seu token no campo abaixo.
                      Exemplo: '12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configuração do AutoMapper
builder.Services.AddAutoMapper(typeof(MetaMappingProfile).Assembly);
builder.Services.AddAutoMapper(typeof(SessaoMappingProfile).Assembly);
builder.Services.AddAutoMapper(typeof(AnotacaoMappingProfile).Assembly);
builder.Services.AddAutoMapper(typeof(HistoricoAnotacaoMappingProfile).Assembly);


// Configuração do FluentValidation
builder.Services.AddScoped<IValidator<MetaRequestDto>, MetaRequestValidator>();
builder.Services.AddScoped<IValidator<Meta>, MetaValidator>();
builder.Services.AddScoped<IValidator<SessaoEstudoRequestDto>, SessaoEstudoRequestDtoValidator>();
builder.Services.AddScoped<IValidator<SessaoEstudo>, SessaoEstudoValidator>();
builder.Services.AddScoped<IValidator<PausaSessao>, PausaValidator>();
builder.Services.AddScoped<IValidator<PausaRequestDto>, PausaRequestDtoValidator>();
builder.Services.AddScoped<IValidator<AnotacaoRequestDto>, AnotacaoRequestDtoValidator>();
builder.Services.AddScoped<IValidator<AnotacaoUpdateDto>, AnotacaoUpdateDtoValidator>();
builder.Services.AddScoped<IValidator<Anotacao>, AnotacaoValidator>();

// Configuração do SQL Loader
builder.Services.AddSingleton<SqlLoader>();

// Registrar os arquivos SQL antes de construir a aplicação
SqlLoader.RegisterSqlFiles(builder.Services);

// Configuração do Database Service
builder.Services.AddScoped<IDatabaseService, MySqlDatabaseService>();

// Registrar repositórios
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<ITokenRepository, TokenRepository>();
builder.Services.AddScoped<IMateriaRepository, MateriaRepository>();
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<ISessaoEstudoRepository, SessaoEstudoRepository>();
builder.Services.AddScoped<ITopicoRepository, TopicoRepository>();
builder.Services.AddScoped<IMetaRepository, MetaRepository>();
builder.Services.AddScoped<IAnotacaoRepository, AnotacaoRepository>();
builder.Services.AddScoped<IHistoricoAnotacaoRepository, HistoricoAnotacaoRepository>();

// Registrar serviços
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMateriaService, MateriaService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<ISessaoEstudoService, SessaoEstudoService>();
builder.Services.AddScoped<ITopicoService, TopicoService>();
builder.Services.AddScoped<IMetaService, MetaService>();
builder.Services.AddScoped<IAnotacaoService, AnotacaoService>();
builder.Services.AddScoped<IHistoricoAnotacaoService, HistoricoAnotacaoService>();

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// Configurar autenticação JWT
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ??
    throw new InvalidOperationException("JWT Key não está configurada no appsettings.json"));

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Adicionar middleware de autenticação antes da autorização
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
