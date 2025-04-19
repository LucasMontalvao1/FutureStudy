using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ERP_API.SQL
{
    public class SqlLoader
    {
        private static Dictionary<string, string> _sqlQueries = new Dictionary<string, string>();
        private readonly ILogger<SqlLoader> _logger;

        public SqlLoader(ILogger<SqlLoader> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Carrega um arquivo SQL do assembly atual
        /// </summary>
        /// <param name="path">Caminho relativo do arquivo SQL </param>
        /// <returns>Conteúdo do arquivo SQL</returns>
        public async Task<string> LoadSqlAsync(string path)
        {
            // Verifica se o SQL já foi carregado
            if (_sqlQueries.TryGetValue(path, out string sql))
            {
                return sql;
            }

            _logger.LogWarning("SQL não encontrado em cache: {Path}", path);

            // Tenta encontrar o recurso diretamente
            var assembly = Assembly.GetExecutingAssembly();
            var resources = assembly.GetManifestResourceNames();

            _logger.LogInformation("Recursos disponíveis: {Resources}", string.Join(", ", resources));

            // Tenta encontrar o recurso por diferentes estratégias
            string resourceName = null;

            // Estratégia 1: Procurar pelo nome exato
            string normalizedPath = path.Replace("/", ".");
            string fullResourceName = $"{assembly.GetName().Name}.SQL.{normalizedPath}";
            if (resources.Contains(fullResourceName))
            {
                resourceName = fullResourceName;
                _logger.LogInformation("Recurso encontrado pelo nome exato: {Name}", resourceName);
            }

            // Estratégia 2: Procurar por substring que contenha o caminho
            if (resourceName == null)
            {
                var possibleResources = resources.Where(r => r.Contains(normalizedPath)).ToList();
                if (possibleResources.Any())
                {
                    resourceName = possibleResources.First();
                    _logger.LogInformation("Recurso encontrado por substring: {Name}", resourceName);
                }
            }

            // Estratégia 3: Procurar qualquer recurso que termine com o nome do arquivo
            if (resourceName == null)
            {
                string fileName = Path.GetFileName(path);
                var possibleResources = resources.Where(r => r.EndsWith(fileName, StringComparison.OrdinalIgnoreCase)).ToList();
                if (possibleResources.Any())
                {
                    resourceName = possibleResources.First();
                    _logger.LogInformation("Recurso encontrado pelo nome do arquivo: {Name}", resourceName);
                }
            }

            // Se encontrou o recurso, carrega-o
            if (resourceName != null)
            {
                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream != null)
                {
                    using var reader = new StreamReader(stream);
                    sql = await reader.ReadToEndAsync();
                    _sqlQueries[path] = sql; // Atualiza o cache
                    return sql;
                }
            }

            // Tenta ler do sistema de arquivos como último recurso
            try
            {
                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                string fullPath = Path.Combine(basePath, "SQL", path);

                if (File.Exists(fullPath))
                {
                    _logger.LogInformation("Arquivo encontrado no sistema de arquivos: {Path}", fullPath);
                    sql = await File.ReadAllTextAsync(fullPath);
                    _sqlQueries[path] = sql; // Atualiza o cache
                    return sql;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao tentar ler arquivo do disco: {Path}", path);
            }

            // Última instância - SQL hardcoded para queries essenciais
            if (path == "Metas/GetAllByUsuarioId.sql")
            {
                _logger.LogWarning("Usando SQL hardcoded para: {Path}", path);
                sql = @"SELECT 
                    id, usuario_id, materia_id, topico_id, titulo, descricao, tipo, 
                    quantidade_total, quantidade_atual, unidade, frequencia, dias_semana,
                    data_inicio, data_fim, concluida, criado_em, atualizado_em
                FROM metas
                WHERE usuario_id = @usuarioId
                ORDER BY data_inicio DESC";

                _sqlQueries[path] = sql; // Atualiza o cache
                return sql;
            }

            // Se não foi carregado, lança uma exceção
            throw new FileNotFoundException(
                $"Arquivo SQL '{path}' não encontrado. Recursos disponíveis: {string.Join(", ", resources)}",
                path);
        }

        /// <summary>
        /// Registra os arquivos SQL embutidos no assembly como recursos
        /// </summary>
        /// <param name="services">Coleção de serviços</param>
        public static void RegisterSqlFiles(IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            // Obtém a lista de recursos embutidos
            var allResources = assembly.GetManifestResourceNames();
            Console.WriteLine($"Todos os recursos encontrados: {string.Join(", ", allResources)}");

            // Filtra apenas arquivos SQL
            var sqlResources = allResources
                .Where(name => name.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
                .ToList();

            Console.WriteLine($"Recursos SQL encontrados: {string.Join(", ", sqlResources)}");

            if (sqlResources.Count == 0)
            {
                Console.WriteLine("AVISO: Nenhum arquivo SQL encontrado como recurso embutido!");
                return;
            }

            foreach (var resourceName in sqlResources)
            {
                try
                {
                    // Lê o conteúdo do arquivo SQL
                    using var stream = assembly.GetManifestResourceStream(resourceName);
                    if (stream == null)
                    {
                        Console.WriteLine($"Não foi possível ler o recurso: {resourceName}");
                        continue;
                    }

                    using var reader = new StreamReader(stream);
                    var sqlContent = reader.ReadToEnd();

                    // Converte o nome do recurso para um caminho relativo
                    var path = ConvertResourceNameToPath(resourceName);

                    Console.WriteLine($"Registrando SQL: {resourceName} como {path}");

                    // Armazena o SQL no dicionário para uso posterior
                    _sqlQueries[path] = sqlContent;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao processar recurso {resourceName}: {ex.Message}");
                }
            }

            Console.WriteLine($"Total de SQLs registrados: {_sqlQueries.Count}");
            Console.WriteLine($"SQLs disponíveis: {string.Join(", ", _sqlQueries.Keys)}");
        }

        /// <summary>
        /// Converte um nome de recurso embutido em um caminho relativo
        /// </summary>
        /// <param name="resourceName">Nome do recurso (por exemplo: "ERP_API.SQL.Metas.GetAll.sql")</param>
        /// <returns>Caminho relativo (por exemplo: "Metas/GetAll.sql")</returns>
        private static string ConvertResourceNameToPath(string resourceName)
        {
            try
            {
                // Tenta encontrar "SQL" no caminho do recurso
                var parts = resourceName.Split('.');
                int sqlIndex = Array.IndexOf(parts, "SQL");

                if (sqlIndex >= 0 && sqlIndex < parts.Length - 1)
                {
                    // Pega somente as partes após "SQL" (exceto a última, que é a extensão)
                    var pathParts = parts.Skip(sqlIndex + 1).Take(parts.Length - sqlIndex - 2);
                    return (string?)(string.Join('/', pathParts) + "/" + parts[parts.Length - 2] + ".sql");
                }

                // Estratégia alternativa: extrair tudo depois de '.SQL.'
                int dotSqlDotIndex = resourceName.IndexOf(".SQL.");
                if (dotSqlDotIndex >= 0)
                {
                    var pathWithDots = resourceName.Substring(dotSqlDotIndex + 5);
                    // Remove a extensão duplicada se houver
                    if (pathWithDots.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
                    {
                        pathWithDots = pathWithDots.Substring(0, pathWithDots.Length - 4);
                    }
                    return pathWithDots.Replace('.', '/') + ".sql";
                }

                // Fallback caso não encontre "SQL" no caminho
                string result = resourceName.Replace('.', '/');
                if (result.EndsWith("/sql", StringComparison.OrdinalIgnoreCase))
                {
                    result = result.Substring(0, result.Length - 4) + ".sql";
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao converter nome de recurso: {ex.Message}");
                // Fallback simples
                return resourceName;
            }
        }
    }
}