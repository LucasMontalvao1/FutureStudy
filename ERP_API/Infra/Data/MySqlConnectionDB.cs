using MySqlConnector;
using System;

namespace ERP_API.Infra.Data
{
    public class MySqlConnectionDB
    {
        private readonly string _connectionString;
        private readonly ILogger<MySqlConnectionDB> _logger;

        public MySqlConnectionDB(IConfiguration configuration, ILogger<MySqlConnectionDB> logger)
        {
            _connectionString = configuration.GetConnectionString("ConfiguracaoPadrao")
                ?? throw new ArgumentNullException("A string de conexão 'ConfiguracaoPadrao' não foi encontrada");
            _logger = logger;
        }

        public MySqlConnection CreateConnection()
        {
            try
            {
                var connection = new MySqlConnection(_connectionString);
                return connection;
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "Erro ao criar conexão com o banco de dados");
                throw new Exception("Erro ao criar conexão com o banco de dados.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Um erro inesperado ocorreu ao criar a conexão com o banco de dados");
                throw new Exception("Um erro inesperado ocorreu ao criar a conexão.", ex);
            }
        }
    }
}