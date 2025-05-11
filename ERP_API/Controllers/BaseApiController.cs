using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;

namespace ERP_API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public abstract class BaseApiController : ControllerBase
    {
        /// <summary>
        /// Obtém o ID do usuário autenticado a partir do token JWT
        /// </summary>
        /// <returns>ID do usuário</returns>
        /// <exception cref="UnauthorizedAccessException">Lançada quando o usuário não está autenticado</exception>
        protected int GetUsuarioId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException("Usuário não autenticado ou token inválido");
            }

            return int.Parse(userIdClaim.Value);
        }

        /// <summary>
        /// Obtém o nome do usuário autenticado a partir do token JWT
        /// </summary>
        /// <returns>Nome do usuário</returns>
        protected string GetUsuarioNome()
        {
            return User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
        }

        /// <summary>
        /// Verifica se o usuário possui uma determinada role
        /// </summary>
        /// <param name="role">Role a verificar</param>
        /// <returns>Verdadeiro se o usuário possui a role</returns>
        protected bool UsuarioTemRole(string role)
        {
            return User.IsInRole(role);
        }
    }
}