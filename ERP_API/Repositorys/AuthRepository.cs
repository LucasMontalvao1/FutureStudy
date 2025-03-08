using ERP_API.Infra.Data;
using ERP_API.Models;
using ERP_API.Repositorys.Interfaces;
using MySqlConnector;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace ERP_API.Repositorys
{
    public class AuthRepository : IAuthRepository
    {
        private readonly MySqlConnectionDB _mySqlConnectionDB;
        private readonly ILogger<AuthRepository> _logger;

        public AuthRepository(MySqlConnectionDB mySqlConnectionDB, ILogger<AuthRepository> logger)
        {
            _mySqlConnectionDB = mySqlConnectionDB;
            _logger = logger;
        }

        public User? ValidarUsuario(string username, string password)
        {
            User? validatedUser = null;

            try
            {
                using (MySqlConnection connection = _mySqlConnectionDB.CreateConnection())
                {
                    string query = @"SELECT UsuarioID, Username, Name, Foto, Email, Password 
                                   FROM usuario 
                                   WHERE Username = @Username 
                                   AND Password = @Password";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);
                        command.Parameters.AddWithValue("@Password", password);

                        _logger.LogInformation("Tentando validar usuário: {Username}", username);

                        connection.Open();
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                validatedUser = new User
                                {
                                    UsuarioID = reader.GetInt32("UsuarioID"),
                                    Username = reader.IsDBNull("Username") ? string.Empty : reader.GetString("Username"),
                                    Name = reader.IsDBNull("Name") ? string.Empty : reader.GetString("Name"),
                                    Foto = reader.IsDBNull("Foto") ? string.Empty : reader.GetString("Foto"),
                                    Email = reader.IsDBNull("Email") ? string.Empty : reader.GetString("Email"),
                                    Password = reader.IsDBNull("Password") ? string.Empty : reader.GetString("Password")
                                };

                                _logger.LogInformation("Usuário encontrado: {Username}", validatedUser.Username);
                            }
                            else
                            {
                                _logger.LogWarning("Nenhum usuário encontrado para o username: {Username}", username);
                            }
                        }
                    }
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
                using (MySqlConnection connection = _mySqlConnectionDB.CreateConnection())
                {
                    string query = "SELECT COUNT(1) FROM usuario WHERE UsuarioID = @UsuarioID";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UsuarioID", usuarioId);
                        await connection.OpenAsync();

                        var exists = await command.ExecuteScalarAsync();
                        return Convert.ToInt32(exists) > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar existência do usuário: {UsuarioId}", usuarioId);
                throw;
            }
        }

        // Método para criar um novo usuário
        public async Task<int> CriarUsuario(User user)
        {
            try
            {
                using (MySqlConnection connection = _mySqlConnectionDB.CreateConnection())
                {
                    string query = @"INSERT INTO usuario (Username, Password, Name, Foto, Email) 
                                  VALUES (@Username, @Password, @Name, @Foto, @Email);
                                  SELECT LAST_INSERT_ID();";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Username", user.Username);
                        command.Parameters.AddWithValue("@Password", user.Password); 
                        command.Parameters.AddWithValue("@Name", user.Name ?? string.Empty);
                        command.Parameters.AddWithValue("@Foto", user.Foto ?? string.Empty);
                        command.Parameters.AddWithValue("@Email", user.Email ?? string.Empty);

                        await connection.OpenAsync();
                        var result = await command.ExecuteScalarAsync();

                        return Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar usuário: {Username}", user.Username);
                throw;
            }
        }
    }
}
