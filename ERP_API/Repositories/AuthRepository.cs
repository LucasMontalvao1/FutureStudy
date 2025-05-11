using ERP_API.Infra.Data;
using ERP_API.Models;
using ERP_API.Repositorys.Interfaces;
using MySqlConnector;
using System.Data;

namespace ERP_API.Repositorys
{
    public class AuthRepository : IAuthRepository
    {
        private readonly IDatabaseService _databaseService;
        private readonly ILogger<AuthRepository> _logger;

        public AuthRepository(IDatabaseService databaseService, ILogger<AuthRepository> logger)
        {
            _databaseService = databaseService;
            _logger = logger;
        }

        public User? ValidarUsuario(string username, string password)
        {
            User? validatedUser = null;

            try
            {
                string query = @"SELECT id, username, nome, email, password FROM usuarios
                               WHERE Username = @Username 
                               AND Password = @Password";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@Username", username),
                    new MySqlParameter("@Password", password)
                };

                _logger.LogInformation("Tentando validar usuário: {Username}", username);

                var dataTable = _databaseService.ExecuteQueryAsync(query, parameters).Result;

                if (dataTable.Rows.Count > 0)
                {
                    var row = dataTable.Rows[0];
                    validatedUser = new User
                    {
                        UsuarioID = Convert.ToInt32(row["id"]),
                        Username = row["username"] != DBNull.Value ? Convert.ToString(row["username"]) : string.Empty,
                        Name = row["nome"] != DBNull.Value ? Convert.ToString(row["nome"]) : string.Empty,
                        Email = row["email"] != DBNull.Value ? Convert.ToString(row["email"]) : string.Empty,
                    };

                    _logger.LogInformation("Usuário encontrado: {Username}", validatedUser.Username);
                }
                else
                {
                    _logger.LogWarning("Nenhum usuário encontrado para o username: {Username}", username);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao validar usuário: {Username}", username);
                throw;
            }

            return validatedUser;
        }

        public async Task<bool> UsuarioExiste(int usuarioId)
        {
            try
            {
                string query = "SELECT COUNT(1) FROM usuarios WHERE id = @id";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@id", usuarioId)
                };

                var result = await _databaseService.ExecuteScalarAsync(query, parameters);
                return Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar existência do usuário: {id}", usuarioId);
                throw;
            }
        }
    }
}