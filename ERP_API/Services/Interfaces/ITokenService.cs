using ERP_API.Models;

namespace ERP_API.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(User user);
        bool ValidateToken(string token);
    }
}