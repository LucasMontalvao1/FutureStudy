using ERP_API.Models;

namespace ERP_API.Services.Interfaces
{
    public interface IAuthService
    {
        User? ValidarUsuario(string username, string password);
        string GenerateToken(User user);
        bool ValidateToken(string token);
    }
}