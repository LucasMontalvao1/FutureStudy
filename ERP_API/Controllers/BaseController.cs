using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ERP_API.Controllers
{
    /// <summary>
    /// Controlador base com funcionalidades comuns para todos os controladores
    /// </summary>
    public abstract class BaseController : ControllerBase
    {
        /// <summary>
        /// Obtém o ID do usuário a partir do token JWT
        /// </summary>
        /// <returns>ID do usuário ou null se não encontrado</returns>
        protected int? GetUsuarioIdFromToken()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return null;
            }

            var userIdClaim = identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return null;
            }

            return userId;
        }

        /// <summary>
        /// Obtém o nome de usuário a partir do token JWT
        /// </summary>
        /// <returns>Nome de usuário ou null se não encontrado</returns>
        protected string? GetUsernameFromToken()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return null;
            }

            var usernameClaim = identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName);
            return usernameClaim?.Value;
        }

        /// <summary>
        /// Obtém o nome completo do usuário a partir do token JWT
        /// </summary>
        /// <returns>Nome completo ou null se não encontrado</returns>
        protected string? GetUserNameFromToken()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return null;
            }

            var nameClaim = identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
            return nameClaim?.Value;
        }

        /// <summary>
        /// Obtém o email do usuário a partir do token JWT
        /// </summary>
        /// <returns>Email ou null se não encontrado</returns>
        protected string? GetEmailFromToken()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return null;
            }

            var emailClaim = identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
            return emailClaim?.Value;
        }
    }
}