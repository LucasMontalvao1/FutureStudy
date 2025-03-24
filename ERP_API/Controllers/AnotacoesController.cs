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
    [Route("api/v1/[controller]")]
    [Authorize]
    public class AnotacoesController : ControllerBase
    {
        private readonly IAnotacaoService _anotacaoService;
        private readonly ILogger<AnotacoesController> _logger;
        private readonly IMapper _mapper;

        public AnotacoesController(
            IAnotacaoService anotacaoService,
            ILogger<AnotacoesController> logger,
            IMapper mapper)
        {
            _anotacaoService = anotacaoService;
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
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var usuarioId = GetUsuarioId();
                var anotacoes = await _anotacaoService.GetAllByUsuarioAsync(usuarioId);
                var response = anotacoes.Select(a => _mapper.Map<AnotacaoResponseDto>(a));

                return Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Usuário não autenticado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter anotações");
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        [HttpGet("por-data")]
        public async Task<IActionResult> GetByDate([FromQuery] DateTime dataInicio, [FromQuery] DateTime? dataFim = null)
        {
            try
            {
                var usuarioId = GetUsuarioId();
                var anotacoes = await _anotacaoService.GetByDateRangeAsync(usuarioId, dataInicio, dataFim);
                var response = anotacoes.Select(a => _mapper.Map<AnotacaoResponseDto>(a));

                return Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Usuário não autenticado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter anotações por data. Início: {DataInicio}, Fim: {DataFim}",
                    dataInicio, dataFim);
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        [HttpGet("sessao/{sessaoId}")]
        public async Task<IActionResult> GetBySessao(int sessaoId)
        {
            try
            {
                var usuarioId = GetUsuarioId();
                var anotacoes = await _anotacaoService.GetAllBySessaoAsync(sessaoId, usuarioId);
                var response = anotacoes.Select(a => _mapper.Map<AnotacaoResponseDto>(a));

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
                _logger.LogError(ex, "Erro ao obter anotações da sessão {SessaoId}", sessaoId);
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var usuarioId = GetUsuarioId();
                var anotacao = await _anotacaoService.GetByIdAsync(id, usuarioId);

                if (anotacao == null)
                {
                    return NotFound(new { message = "Anotação não encontrada" });
                }

                var response = _mapper.Map<AnotacaoResponseDto>(anotacao);
                return Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Usuário não autenticado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter anotação {Id}", id);
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AnotacaoRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var usuarioId = GetUsuarioId();
                var anotacao = await _anotacaoService.CreateAsync(dto, usuarioId);
                var response = _mapper.Map<AnotacaoResponseDto>(anotacao);

                return CreatedAtAction(nameof(GetById), new { id = anotacao.Id }, response);
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
                _logger.LogError(ex, "Erro ao criar anotação");
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] AnotacaoUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var usuarioId = GetUsuarioId();
                var updated = await _anotacaoService.UpdateAsync(id, dto, usuarioId);

                if (!updated)
                {
                    return NotFound(new { message = "Anotação não encontrada" });
                }

                return Ok(new { message = "Anotação atualizada com sucesso" });
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
                _logger.LogError(ex, "Erro ao atualizar anotação {Id}", id);
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var usuarioId = GetUsuarioId();
                var deleted = await _anotacaoService.DeleteAsync(id, usuarioId);

                if (!deleted)
                {
                    return NotFound(new { message = "Anotação não encontrada" });
                }

                return Ok(new { message = "Anotação excluída com sucesso" });
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
                _logger.LogError(ex, "Erro ao excluir anotação {Id}", id);
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }
    }
}