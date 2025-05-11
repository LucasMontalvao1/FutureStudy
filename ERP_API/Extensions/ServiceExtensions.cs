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
using System.Text;
using ERP_API.SQL;
using ERP_API.Repositories.Interfaces;
using ERP_API.Validators;
using ERP_API.Mapping;
using ERP_API.Models.Entities;
using ERP_API.Models;
using ERP_API.Filters;

namespace ERP_API.Extensions
{
    public static class ServiceExtensions
    {
        /// <summary>
        /// Adiciona a configuração de controladores e filtros
        /// </summary>
        public static IServiceCollection AddControllersConfiguration(this IServiceCollection services)
        {
            services.AddControllers(options =>
            {
                // options.Filters.Add<ApiExceptionFilter>();
            })
            .AddJsonOptions(options =>
             {
                 // Configurações adicionais do JSON, se necessário
                 options.JsonSerializerOptions.WriteIndented = true;
                 options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
             });
            // Registrar filtros de exceção como serviços para injeção de dependência
            services.AddScoped<ApiExceptionFilter>();

            return services;
        }
        /// <summary>
        /// Adiciona a configuração do Swagger com suporte JWT
        /// </summary>
        public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
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

            return services;
        }

        /// <summary>
        /// Adiciona configurações do AutoMapper
        /// </summary>
        public static IServiceCollection AddMappingConfiguration(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(MetaMappingProfile).Assembly);
            services.AddAutoMapper(typeof(SessaoMappingProfile).Assembly);
            services.AddAutoMapper(typeof(AnotacaoMappingProfile).Assembly);
            services.AddAutoMapper(typeof(HistoricoAnotacaoMappingProfile).Assembly);

            return services;
        }

        /// <summary>
        /// Adiciona configurações de validação com FluentValidation
        /// </summary>
        public static IServiceCollection AddValidationConfiguration(this IServiceCollection services)
        {
            // Validadores de DTOs
            services.AddScoped<IValidator<MetaRequestDto>, MetaRequestValidator>();
            services.AddScoped<IValidator<SessaoEstudoRequestDto>, SessaoEstudoRequestDtoValidator>();
            services.AddScoped<IValidator<PausaRequestDto>, PausaRequestDtoValidator>();
            services.AddScoped<IValidator<AnotacaoRequestDto>, AnotacaoRequestDtoValidator>();
            services.AddScoped<IValidator<AnotacaoUpdateDto>, AnotacaoUpdateDtoValidator>();

            // Validadores de entidades
            services.AddScoped<IValidator<Meta>, MetaValidator>();
            services.AddScoped<IValidator<SessaoEstudo>, SessaoEstudoValidator>();
            services.AddScoped<IValidator<PausaSessao>, PausaValidator>();
            services.AddScoped<IValidator<Anotacao>, AnotacaoValidator>();

            return services;
        }

        /// <summary>
        /// Adiciona configurações SQL
        /// </summary>
        public static IServiceCollection AddSqlConfiguration(this IServiceCollection services)
        {
            services.AddSingleton<SqlLoader>();

            // Registrar os arquivos SQL
            SqlLoader.RegisterSqlFiles(services);

            // Configuração do Database Service
            services.AddScoped<IDatabaseService, MySqlDatabaseService>();

            return services;
        }

        /// <summary>
        /// Adiciona repositórios ao container de serviços
        /// </summary>
        public static IServiceCollection AddRepositoryConfiguration(this IServiceCollection services)
        {
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<ITokenRepository, TokenRepository>();
            services.AddScoped<IMateriaRepository, MateriaRepository>();
            services.AddScoped<ICategoriaRepository, CategoriaRepository>();
            services.AddScoped<ISessaoEstudoRepository, SessaoEstudoRepository>();
            services.AddScoped<ITopicoRepository, TopicoRepository>();
            services.AddScoped<IMetaRepository, MetaRepository>();
            services.AddScoped<IAnotacaoRepository, AnotacaoRepository>();
            services.AddScoped<IHistoricoAnotacaoRepository, HistoricoAnotacaoRepository>();

            return services;
        }

        /// <summary>
        /// Adiciona serviços de negócio ao container de serviços
        /// </summary>
        public static IServiceCollection AddServiceConfiguration(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IMateriaService, MateriaService>();
            services.AddScoped<ICategoriaService, CategoriaService>();
            services.AddScoped<ISessaoEstudoService, SessaoEstudoService>();
            services.AddScoped<ITopicoService, TopicoService>();
            services.AddScoped<IMetaService, MetaService>();
            services.AddScoped<IAnotacaoService, AnotacaoService>();
            services.AddScoped<IHistoricoAnotacaoService, HistoricoAnotacaoService>();

            return services;
        }

        /// <summary>
        /// Adiciona configuração de CORS
        /// </summary>
        public static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder => builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
            });

            return services;
        }

        /// <summary>
        /// Adiciona configuração de autenticação JWT
        /// </summary>
        public static IServiceCollection AddAuthenticationConfiguration(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var jwtKey = configuration["Jwt:Key"];

            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new InvalidOperationException("JWT Key não está configurada no appsettings.json");
            }

            var key = Encoding.UTF8.GetBytes(jwtKey);

            services.AddAuthentication(x =>
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

            return services;
        }
    }
}