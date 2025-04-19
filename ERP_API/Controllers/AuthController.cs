using ERP_API.Models.DTOs;
using ERP_API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP_API.Controllers
{
    [ApiController]
    [Route("api/v1")]
    public class AuthController : BaseController
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
                            Email = usuario.Email ?? string.Empty
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

        [HttpGet("validate-token")]
        public IActionResult ValidateToken([FromQuery] string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new { valid = false, message = "Token não fornecido" });
            }

            try
            {
                bool isValid = _authService.ValidateToken(token);
                return Ok(new { valid = isValid });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao validar token");
                return StatusCode(500, new { valid = false, message = "Erro ao validar token" });
            }
        }

        [HttpGet("validate-token-debug")]
        [Authorize]
        public IActionResult ValidateTokenDebug()
        {
            var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader))
            {
                return BadRequest(new { message = "Cabeçalho Authorization não encontrado" });
            }

            var userId = GetUsuarioIdFromToken();
            var username = GetUsernameFromToken();
            var name = GetUserNameFromToken();
            var email = GetEmailFromToken();

            return Ok(new
            {
                authorizationHeader = authHeader,
                hasBearer = authHeader.StartsWith("Bearer "),
                headerLength = authHeader.Length,
                tokenPart = authHeader.StartsWith("Bearer ") ? authHeader.Substring(7) : "N/A",
                claims = new
                {
                    userId,
                    username,
                    name,
                    email
                }
            });
        }

        [HttpGet("user-info")]
        [Authorize]
        public IActionResult GetUserInfo()
        {
            try
            {
                var userId = GetUsuarioIdFromToken();
                if (userId == null)
                {
                    return Unauthorized(new { message = "Usuário não identificado no token" });
                }

                return Ok(new
                {
                    id = userId,
                    username = GetUsernameFromToken(),
                    name = GetUserNameFromToken(),
                    email = GetEmailFromToken()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter informações do usuário");
                return StatusCode(500, new { message = "Ocorreu um erro interno no servidor." });
            }
        }
    }
}