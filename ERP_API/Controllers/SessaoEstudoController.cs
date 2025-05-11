using AutoMapper;
using ERP_API.Filters;
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
    [Authorize]
    [ServiceFilter(typeof(ApiExceptionFilter))]
    public class SessoesEstudoController : BaseApiController
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

        /// <summary>
        /// Obtém as sessões de estudo por período
        /// </summary>
        /// <param name="inicio">Data de início (opcional, padrão: hoje)</param>
        /// <param name="fim">Data de fim (opcional, padrão: fim do dia atual)</param>
        /// <returns>Lista de sessões de estudo</returns>
        [HttpGet]
        [ProducesResponseType(typeof(SessaoEstudoResponseDto[]), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByPeriodo([FromQuery] DateTime? inicio, [FromQuery] DateTime? fim)
        {
            var usuarioId = GetUsuarioId();

            DateTime dataInicio = inicio ?? DateTime.Today;
            DateTime dataFim = fim ?? DateTime.Today.AddDays(1).AddSeconds(-1);

            var sessoes = await _sessaoService.GetAllByPeriodoAsync(usuarioId, dataInicio, dataFim);
            var response = sessoes.Select(s => _mapper.Map<SessaoEstudoResponseDto>(s));

            return Ok(response);
        }

        /// <summary>
        /// Obtém uma sessão de estudo por ID
        /// </summary>
        /// <param name="id">ID da sessão</param>
        /// <returns>Sessão de estudo</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(SessaoEstudoResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int id)
        {
            var usuarioId = GetUsuarioId();

            var sessao = await _sessaoService.GetByIdAsync(id, usuarioId);
            if (sessao == null)
            {
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Recurso não encontrado",
                    Detail = "Sessão não encontrada"
                });
            }

            var response = _mapper.Map<SessaoEstudoResponseDto>(sessao);
            return Ok(response);
        }

        /// <summary>
        /// Obtém dados do calendário de estudos
        /// </summary>
        /// <param name="mes">Mês (1-12)</param>
        /// <param name="ano">Ano</param>
        /// <returns>Dados do calendário</returns>
        [HttpGet("calendario")]
        [ProducesResponseType(typeof(SessaoCalendarioDto[]), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCalendario([FromQuery] int mes, [FromQuery] int ano)
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

        /// <summary>
        /// Obtém estatísticas para dashboard
        /// </summary>
        /// <param name="periodo">Período (semana, mes, ano)</param>
        /// <param name="data">Data de referência (opcional)</param>
        /// <returns>Estatísticas do dashboard</returns>
        [HttpGet("dashboard")]
        [ProducesResponseType(typeof(SessaoDashboardStatsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDashboard([FromQuery] string periodo = "semana", [FromQuery] DateTime? data = null)
        {
            var usuarioId = GetUsuarioId();

            var stats = await _sessaoService.GetDashboardStatsAsync(usuarioId, periodo, data);
            var response = _mapper.Map<SessaoDashboardStatsDto>(stats);

            return Ok(response);
        }

        /// <summary>
        /// Inicia uma nova sessão de estudo
        /// </summary>
        /// <param name="dto">Dados da sessão</param>
        /// <returns>Sessão criada</returns>
        [HttpPost]
        [ProducesResponseType(typeof(SessaoEstudoResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> IniciarSessao([FromBody] SessaoEstudoRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var usuarioId = GetUsuarioId();

            var sessao = await _sessaoService.IniciarSessaoAsync(dto, usuarioId);
            var response = _mapper.Map<SessaoEstudoResponseDto>(sessao);

            return CreatedAtAction(nameof(GetById), new { id = sessao.Id }, response);
        }

        /// <summary>
        /// Pausa uma sessão de estudo em andamento
        /// </summary>
        /// <param name="id">ID da sessão</param>
        /// <returns>Dados da pausa</returns>
        [HttpPost("{id}/pausar")]
        [ProducesResponseType(typeof(PausaResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PausarSessao(int id)
        {
            var usuarioId = GetUsuarioId();

            var pausa = await _sessaoService.PausarSessaoAsync(id, usuarioId);
            var response = _mapper.Map<PausaResponseDto>(pausa);

            return Ok(response);
        }

        /// <summary>
        /// Retoma uma sessão de estudo pausada
        /// </summary>
        /// <param name="id">ID da pausa</param>
        /// <returns>Confirmação da operação</returns>
        [HttpPost("pausas/{id}/retomar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RetomarSessao(int id)
        {
            var usuarioId = GetUsuarioId();

            var retomada = await _sessaoService.RetomarSessaoAsync(id, usuarioId);
            if (!retomada)
            {
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Recurso não encontrado",
                    Detail = "Pausa não encontrada ou já finalizada"
                });
            }

            return Ok(new { message = "Sessão retomada com sucesso" });
        }

        /// <summary>
        /// Finaliza uma sessão de estudo
        /// </summary>
        /// <param name="id">ID da sessão</param>
        /// <returns>Confirmação da operação</returns>
        [HttpPost("{id}/finalizar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> FinalizarSessao(int id)
        {
            var usuarioId = GetUsuarioId();

            var finalizada = await _sessaoService.FinalizarSessaoAsync(id, usuarioId);
            if (!finalizada)
            {
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Recurso não encontrado",
                    Detail = "Sessão não encontrada ou já finalizada"
                });
            }

            return Ok(new { message = "Sessão finalizada com sucesso" });
        }
    }
}