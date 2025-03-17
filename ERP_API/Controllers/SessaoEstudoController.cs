using ERP_API.Models.DTOs;
using ERP_API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_API.Controllers
{
    [ApiController]
    [Route("api/v1/sessoes")]
    [Authorize]
    public class SessaoEstudoController : BaseController
    {
        private readonly ISessaoEstudoService _sessaoService;
        private readonly ILogger<SessaoEstudoController> _logger;

        public SessaoEstudoController(ISessaoEstudoService sessaoService, ILogger<SessaoEstudoController> logger)
        {
            _sessaoService = sessaoService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetByPeriodo([FromQuery] DateTime? inicio, [FromQuery] DateTime? fim)
        {
            try
            {
                var usuarioId = GetUsuarioIdFromToken();
                if (usuarioId == null)
                {
                    return Unauthorized(new { message = "Usuário não identificado" });
                }

                DateTime dataInicio = inicio ?? DateTime.Today;
                DateTime dataFim = fim ?? DateTime.Today.AddDays(1).AddSeconds(-1);

                var sessoes = await _sessaoService.GetAllByPeriodoAsync(usuarioId.Value, dataInicio, dataFim);

                var response = sessoes.Select(s => new SessaoEstudoResponseDto
                {
                    Id = s.Id,
                    UsuarioId = s.UsuarioId,
                    MateriaId = s.MateriaId,
                    TopicoId = s.TopicoId,
                    DataInicio = s.DataInicio,
                    DataFim = s.DataFim,
                    Status = s.Status,
                    TempoEstudado = s.TempoEstudado,
                    CriadoEm = s.CriadoEm,
                    AtualizadoEm = s.AtualizadoEm
                });

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter sessões de estudo");
                return StatusCode(500, new { message = "Ocorreu um erro interno no servidor." });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var usuarioId = GetUsuarioIdFromToken();
                if (usuarioId == null)
                {
                    return Unauthorized(new { message = "Usuário não identificado" });
                }

                var sessao = await _sessaoService.GetByIdAsync(id, usuarioId.Value);

                if (sessao == null)
                {
                    return NotFound(new { message = "Sessão não encontrada" });
                }

                var response = new SessaoEstudoResponseDto
                {
                    Id = sessao.Id,
                    UsuarioId = sessao.UsuarioId,
                    MateriaId = sessao.MateriaId,
                    TopicoId = sessao.TopicoId,
                    DataInicio = sessao.DataInicio,
                    DataFim = sessao.DataFim,
                    Status = sessao.Status,
                    TempoEstudado = sessao.TempoEstudado,
                    CriadoEm = sessao.CriadoEm,
                    AtualizadoEm = sessao.AtualizadoEm
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter sessão {Id}", id);
                return StatusCode(500, new { message = "Ocorreu um erro interno no servidor." });
            }
        }

        [HttpGet("calendario")]
        public async Task<IActionResult> GetCalendario([FromQuery] int mes, [FromQuery] int ano)
        {
            try
            {
                var usuarioId = GetUsuarioIdFromToken();
                if (usuarioId == null)
                {
                    return Unauthorized(new { message = "Usuário não identificado" });
                }

                var dados = await _sessaoService.GetCalendarioAsync(usuarioId.Value, mes, ano);

                return Ok(dados);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter dados do calendário");
                return StatusCode(500, new { message = "Ocorreu um erro interno no servidor." });
            }
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard([FromQuery] string periodo = "semana", [FromQuery] DateTime? data = null)
        {
            try
            {
                var usuarioId = GetUsuarioIdFromToken();
                if (usuarioId == null)
                {
                    return Unauthorized(new { message = "Usuário não identificado" });
                }

                var stats = await _sessaoService.GetDashboardStatsAsync(usuarioId.Value, periodo, data);

                return Ok(stats);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter estatísticas do dashboard");
                return StatusCode(500, new { message = "Ocorreu um erro interno no servidor." });
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
                var usuarioId = GetUsuarioIdFromToken();
                if (usuarioId == null)
                {
                    return Unauthorized(new { message = "Usuário não identificado" });
                }

                var sessao = await _sessaoService.IniciarSessaoAsync(dto, usuarioId.Value);

                var response = new SessaoEstudoResponseDto
                {
                    Id = sessao.Id,
                    UsuarioId = sessao.UsuarioId,
                    MateriaId = sessao.MateriaId,
                    TopicoId = sessao.TopicoId,
                    DataInicio = sessao.DataInicio,
                    DataFim = sessao.DataFim,
                    Status = sessao.Status,
                    TempoEstudado = sessao.TempoEstudado,
                    CriadoEm = sessao.CriadoEm,
                    AtualizadoEm = sessao.AtualizadoEm
                };

                return CreatedAtAction(nameof(GetById), new { id = sessao.Id }, response);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Erro de validação ao iniciar sessão: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao iniciar sessão");
                return StatusCode(500, new { message = "Ocorreu um erro interno no servidor." });
            }
        }

        [HttpPost("{id}/pausar")]
        public async Task<IActionResult> PausarSessao(int id)
        {
            try
            {
                var usuarioId = GetUsuarioIdFromToken();
                if (usuarioId == null)
                {
                    return Unauthorized(new { message = "Usuário não identificado" });
                }

                var pausa = await _sessaoService.PausarSessaoAsync(id, usuarioId.Value);

                var response = new PausaResponseDto
                {
                    Id = pausa.Id,
                    UsuarioId = pausa.UsuarioId,
                    SessaoId = pausa.SessaoId,
                    Inicio = pausa.Inicio,
                    Fim = pausa.Fim
                };

                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Erro de validação ao pausar sessão {Id}: {Message}", id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao pausar sessão {Id}", id);
                return StatusCode(500, new { message = "Ocorreu um erro interno no servidor." });
            }
        }

        [HttpPost("pausas/{id}/retomar")]
        public async Task<IActionResult> RetomarSessao(int id)
        {
            try
            {
                var usuarioId = GetUsuarioIdFromToken();
                if (usuarioId == null)
                {
                    return Unauthorized(new { message = "Usuário não identificado" });
                }

                var retomada = await _sessaoService.RetomarSessaoAsync(id, usuarioId.Value);

                if (!retomada)
                {
                    return NotFound(new { message = "Pausa não encontrada ou já finalizada" });
                }

                return Ok(new { message = "Sessão retomada com sucesso" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Erro de validação ao retomar sessão: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao retomar sessão para a pausa {Id}", id);
                return StatusCode(500, new { message = "Ocorreu um erro interno no servidor." });
            }
        }

        [HttpPost("{id}/finalizar")]
        public async Task<IActionResult> FinalizarSessao(int id)
        {
            try
            {
                var usuarioId = GetUsuarioIdFromToken();
                if (usuarioId == null)
                {
                    return Unauthorized(new { message = "Usuário não identificado" });
                }

                var finalizada = await _sessaoService.FinalizarSessaoAsync(id, usuarioId.Value);

                if (!finalizada)
                {
                    return NotFound(new { message = "Sessão não encontrada ou já finalizada" });
                }

                return Ok(new { message = "Sessão finalizada com sucesso" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Erro de validação ao finalizar sessão {Id}: {Message}", id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao finalizar sessão {Id}", id);
                return StatusCode(500, new { message = "Ocorreu um erro interno no servidor." });
            }
        }
    }
}