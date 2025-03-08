using ERP_API.Models.DTOs;
using ERP_API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP_API.Controllers
{
    [ApiController]
    [Route("api/v1")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest loginRequest)
        {
            // Verifica se o modelo é válido (validações via DataAnnotations)
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Tentativa de login para o usuário: {Username}", loginRequest.Username);

            try
            {
                var usuario = _authService.ValidarUsuario(loginRequest.Username, loginRequest.Password);

                if (usuario != null)
                {
                    var token = _authService.GenerateToken(usuario);

                    _logger.LogInformation("Login bem-sucedido para o usuário: {Username}", usuario.Username);

                    return Ok(new LoginResponse
                    {
                        Token = token,
                        User = new UserResponseDto
                        {
                            UsuarioID = usuario.UsuarioID,
                            Username = usuario.Username,
                            Name = usuario.Name,
                            Foto = usuario.Foto,
                            Email = usuario.Email
                        }
                    });
                }

                _logger.LogWarning("Falha no login para o usuário: {Username}", loginRequest.Username);
                return Unauthorized(new { message = "Login ou senha incorretos" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao tentar realizar login para o usuário: {Username}", loginRequest.Username);
                return StatusCode(500, new { message = "Ocorreu um erro interno no servidor." });
            }
        }

        [HttpGet("validate-token-debug")]
        [Authorize]
        public IActionResult ValidateTokenDebug()
        {
            // Verifica o cabeçalho Authorization
            var authHeader = HttpContext.Request.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(authHeader))
            {
                return BadRequest(new { message = "Cabeçalho Authorization não encontrado" });
            }

            // Retorna informações sobre o cabeçalho para debug
            return Ok(new
            {
                authorizationHeader = authHeader,
                hasBearer = authHeader.StartsWith("Bearer "),
                headerLength = authHeader.Length,
                tokenPart = authHeader.StartsWith("Bearer ") ? authHeader.Substring(7) : "N/A"
            });
        }
    }
}