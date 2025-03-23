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
    [Route("api/[controller]")]
    [Authorize]
    public class SessoesEstudoController : ControllerBase
    {
        private readonly ISessaoEstudoService _sessaoService;
        private readonly ILogger<SessoesEstudoController> _logger;
        private readonly IMapper _mapper;

        public SessoesEstudoController(
            ISessaoEstudoService sessaoService,
            ILogger<SessoesEstudoController> logger,
            IMapper mapper)
        {
            _sessaoService = sessaoService;
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
        public async Task<IActionResult> GetByPeriodo([FromQuery] DateTime? inicio, [FromQuery] DateTime? fim)
        {
            try
            {
                var usuarioId = GetUsuarioId();

                DateTime dataInicio = inicio ?? DateTime.Today;
                DateTime dataFim = fim ?? DateTime.Today.AddDays(1).AddSeconds(-1);

                var sessoes = await _sessaoService.GetAllByPeriodoAsync(usuarioId, dataInicio, dataFim);
                var response = sessoes.Select(s => _mapper.Map<SessaoEstudoResponseDto>(s));

                return Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Usuário não autenticado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter sessões de estudo");
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var usuarioId = GetUsuarioId();

                var sessao = await _sessaoService.GetByIdAsync(id, usuarioId);
                if (sessao == null)
                {
                    return NotFound(new { message = "Sessão não encontrada" });
                }

                var response = _mapper.Map<SessaoEstudoResponseDto>(sessao);
                return Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Usuário não autenticado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter sessão {Id}", id);
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        [HttpGet("calendario")]
        public async Task<IActionResult> GetCalendario([FromQuery] int mes, [FromQuery] int ano)
        {
            try
            {
                var usuarioId = GetUsuarioId();

                var dados = await _sessaoService.GetCalendarioAsync(usuarioId, mes, ano);
                var response = dados.Select(kvp => new SessaoCalendarioDto
                {
                    Dia = kvp.Key,
                    MinutosEstudados = kvp.Value
                });

                return Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Usuário não autenticado" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter dados do calendário");
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard([FromQuery] string periodo = "semana", [FromQuery] DateTime? data = null)
        {
            try
            {
                var usuarioId = GetUsuarioId();

                var stats = await _sessaoService.GetDashboardStatsAsync(usuarioId, periodo, data);
                var response = new SessaoDashboardStatsDto
                {
                    TempoTotalEstudado = stats.TempoTotalEstudado,
                    DiasEstudados = stats.DiasEstudados,
                    TotalDias = stats.TotalDias,
                    MateriaMaisEstudada = stats.MateriaMaisEstudada,
                    HorasMateriaMaisEstudada = stats.HorasMateriaMaisEstudada
                };

                return Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Usuário não autenticado" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter estatísticas do dashboard");
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> IniciarSessao([FromBody] SessaoEstudoRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var usuarioId = GetUsuarioId();

                var sessao = await _sessaoService.IniciarSessaoAsync(dto, usuarioId);
                var response = _mapper.Map<SessaoEstudoResponseDto>(sessao);

                return CreatedAtAction(nameof(GetById), new { id = sessao.Id }, response);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Usuário não autenticado" });
            }
            catch (FluentValidation.ValidationException ex)
            {
                return BadRequest(ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao iniciar sessão");
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        [HttpPost("{id}/pausar")]
        public async Task<IActionResult> PausarSessao(int id)
        {
            try
            {
                var usuarioId = GetUsuarioId();

                var pausa = await _sessaoService.PausarSessaoAsync(id, usuarioId);
                var response = _mapper.Map<PausaResponseDto>(pausa);

                return Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Usuário não autenticado" });
            }
            catch (FluentValidation.ValidationException ex)
            {
                return BadRequest(ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao pausar sessão {Id}", id);
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        [HttpPost("pausas/{id}/retomar")]
        public async Task<IActionResult> RetomarSessao(int id)
        {
            try
            {
                var usuarioId = GetUsuarioId();

                var retomada = await _sessaoService.RetomarSessaoAsync(id, usuarioId);
                if (!retomada)
                {
                    return NotFound(new { message = "Pausa não encontrada ou já finalizada" });
                }

                return Ok(new { message = "Sessão retomada com sucesso" });
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
                _logger.LogError(ex, "Erro ao retomar sessão para a pausa {Id}", id);
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        [HttpPost("{id}/finalizar")]
        public async Task<IActionResult> FinalizarSessao(int id)
        {
            try
            {
                var usuarioId = GetUsuarioId();

                var finalizada = await _sessaoService.FinalizarSessaoAsync(id, usuarioId);
                if (!finalizada)
                {
                    return NotFound(new { message = "Sessão não encontrada ou já finalizada" });
                }

                return Ok(new { message = "Sessão finalizada com sucesso" });
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
                _logger.LogError(ex, "Erro ao finalizar sessão {Id}", id);
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }
    }
}