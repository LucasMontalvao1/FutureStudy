using ERP_API.Models;
using ERP_API.Repositorys.Interfaces;
using ERP_API.Services.Interfaces;

namespace ERP_API.Services
{
    public class TokenService : ITokenService
    {
        private readonly ITokenRepository _tokenRepository;
        private readonly ILogger<TokenService> _logger;

        public TokenService(ITokenRepository tokenRepository, ILogger<TokenService> logger)
        {
            _tokenRepository = tokenRepository;
            _logger = logger;
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
