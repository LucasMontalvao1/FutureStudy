using ERP_API.Models;

namespace ERP_API.Repositorys.Interfaces
{
    public interface ITokenRepository
    {
        string CreateToken(User user);
        bool IsValidToken(string token);
    }
}