using AutoMapper;
using ERP_API.Models.DTOs;
using ERP_API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_API.Controllers
{
    [ApiController]
    [Route("api/anotacoes/{anotacaoId}/historico")]
    [Authorize]
    public class HistoricoAnotacoesController : ControllerBase
    {
        private readonly IHistoricoAnotacaoService _historicoService;
        private readonly ILogger<HistoricoAnotacoesController> _logger;
        private readonly IMapper _mapper;

        public HistoricoAnotacoesController(
            IHistoricoAnotacaoService historicoService,
            ILogger<HistoricoAnotacoesController> logger,
            IMapper mapper)
        {
            _historicoService = historicoService;
            _logger = logger;
            _mapper = mapper;
        }

        // Método auxiliar para obter o ID do usuário logado
        private int GetUsuarioId()
        {
            // Obtém o ID do usuário a partir do token JWT
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException("Usuário não autenticado ou token inválido");
            }

            return int.Parse(userIdClaim.Value);
        }

        [HttpGet]
        public async Task<IActionResult> GetHistorico(int anotacaoId)
        {
            try
            {
                var usuarioId = GetUsuarioId();
                var historicos = await _historicoService.GetByAnotacaoAsync(anotacaoId, usuarioId);
                var response = historicos.Select(h => _mapper.Map<HistoricoAnotacaoResponseDto>(h));

                return Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Usuário não autenticado" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter histórico da anotação {AnotacaoId}", anotacaoId);
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }
    }
}