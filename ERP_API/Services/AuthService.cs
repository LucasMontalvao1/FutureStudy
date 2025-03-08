using ERP_API.Models;
using ERP_API.Repositorys.Interfaces;
using ERP_API.Services.Interfaces;

namespace ERP_API.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly ITokenRepository _tokenRepository;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IAuthRepository authRepository,
            ITokenRepository tokenRepository,
            ILogger<AuthService> logger)
        {
            _authRepository = authRepository;
            _tokenRepository = tokenRepository;
            _logger = logger;
        }

        public User? ValidarUsuario(string username, string password)
        {
            try
            {
                _logger.LogInformation("Validando usuário: {Username}", username);
                return _authRepository.ValidarUsuario(username, password);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao validar usuário: {Username}", username);
                throw;
            }
        }

        public string GenerateToken(User user)
        {
            try
            {
                _logger.LogInformation("Gerando token para o usuário: {Username}", user.Username);
                return _tokenRepository.CreateToken(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar token para usuário: {Username}", user.Username);
                throw;
            }
        }

        public bool ValidateToken(string token)
        {
            try
            {
                return _tokenRepository.IsValidToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao validar token");
                throw;
            }
        }
    }
}