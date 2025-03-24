using ERP_API.Models.DTOs;
using ERP_API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ERP_API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class MetasController : ControllerBase
    {
        private readonly IMetaService _metaService;
        private readonly ILogger<MetasController> _logger;

        public MetasController(
            IMetaService metaService,
            ILogger<MetasController> logger)
        {
            _metaService = metaService;
            _logger = logger;
        }

        // Método auxiliar para obter o ID do usuário logado
        private int GetUsuarioId()
        {
            // Obtém o ID do usuário a partir do token JWT
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
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
                var metas = await _metaService.GetAllByUsuarioIdAsync(usuarioId);
                return Ok(metas);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter metas do usuário");
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        [HttpGet("por-data")]
        public async Task<IActionResult> GetMetasByData([FromQuery] DateTime dataInicio, [FromQuery] DateTime? dataFim = null)
        {
            try
            {
                var usuarioId = GetUsuarioId();
                var metas = await _metaService.GetByDateRangeAsync(usuarioId, dataInicio, dataFim);
                return Ok(metas);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter metas por data. Início: {DataInicio}, Fim: {DataFim}", dataInicio, dataFim);
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        [HttpGet("ativas")]
        public async Task<IActionResult> GetAtivas()
        {
            try
            {
                var usuarioId = GetUsuarioId();
                var metas = await _metaService.GetActiveAsync(usuarioId);
                return Ok(metas);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter metas ativas do usuário");
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        [HttpGet("materia/{materiaId}")]
        public async Task<IActionResult> GetByMateria(int materiaId)
        {
            try
            {
                var usuarioId = GetUsuarioId();
                var metas = await _metaService.GetAllByMateriaIdAsync(materiaId, usuarioId);
                return Ok(metas);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter metas da matéria {MateriaId}", materiaId);
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        [HttpGet("topico/{topicoId}")]
        public async Task<IActionResult> GetByTopico(int topicoId)
        {
            try
            {
                var usuarioId = GetUsuarioId();
                var metas = await _metaService.GetAllByTopicoIdAsync(topicoId, usuarioId);
                return Ok(metas);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter metas do tópico {TopicoId}", topicoId);
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var usuarioId = GetUsuarioId();
                var meta = await _metaService.GetByIdAsync(id, usuarioId);

                if (meta == null)
                {
                    return NotFound();
                }

                return Ok(meta);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter meta {Id}", id);
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MetaRequestDto dto)
        {
            try
            {
                var usuarioId = GetUsuarioId();
                var meta = await _metaService.CreateAsync(dto, usuarioId);
                return CreatedAtAction(nameof(GetById), new { id = meta.Id }, meta);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (FluentValidation.ValidationException ex)
            {
                return BadRequest(ex.Errors);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar meta");
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] MetaUpdateRequestDto dto)
        {
            try
            {
                var usuarioId = GetUsuarioId();
                var meta = await _metaService.UpdateAsync(id, dto, usuarioId);

                if (meta == null)
                {
                    return NotFound();
                }

                return Ok(meta);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (FluentValidation.ValidationException ex)
            {
                return BadRequest(ex.Errors);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar meta {Id}", id);
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        [HttpPatch("{id}/progresso")]
        public async Task<IActionResult> UpdateProgresso(int id, [FromBody] int quantidade)
        {
            try
            {
                var usuarioId = GetUsuarioId();
                var meta = await _metaService.UpdateProgressoAsync(id, quantidade, usuarioId);

                if (meta == null)
                {
                    return NotFound();
                }

                return Ok(meta);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar progresso da meta {Id}", id);
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        [HttpPatch("{id}/concluir")]
        public async Task<IActionResult> Complete(int id)
        {
            try
            {
                var usuarioId = GetUsuarioId();
                var success = await _metaService.CompleteAsync(id, usuarioId);

                if (!success)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao concluir meta {Id}", id);
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var usuarioId = GetUsuarioId();
                var success = await _metaService.DeleteAsync(id, usuarioId);

                if (!success)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir meta {Id}", id);
                return StatusCode(500, "Erro interno do servidor");
            }
        }
    }
}