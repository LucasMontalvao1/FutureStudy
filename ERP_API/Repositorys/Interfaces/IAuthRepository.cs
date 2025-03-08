using ERP_API.Models;

namespace ERP_API.Repositorys.Interfaces
{
    public interface IAuthRepository
    {
        User? ValidarUsuario(string username, string password);
        Task<bool> UsuarioExiste(int usuarioId);
    }
}
