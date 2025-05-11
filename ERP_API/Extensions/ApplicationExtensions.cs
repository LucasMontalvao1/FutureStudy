using ERP_API.Middlewares;

namespace ERP_API.Extensions
{
    public static class ApplicationExtensions
    {
        /// <summary>
        /// Configura o pipeline de requisições HTTP
        /// </summary>
        public static WebApplication ConfigureApplicationPipeline(
            this WebApplication app,
            IWebHostEnvironment env)
        {
            // Configurações específicas de desenvolvimento
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // Em produção, redirecionar erros para um controller de erro
                app.UseExceptionHandler("/Error");
                // HSTS padrão é 30 dias. Ajuste para cenários de produção.
                app.UseHsts();
            }

            // Middleware personalizado para tratamento global de exceções
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            app.UseHttpsRedirection();
            app.UseCors("AllowAll");

            // Adicionar middleware de autenticação antes da autorização
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            return app;
        }
    }
}