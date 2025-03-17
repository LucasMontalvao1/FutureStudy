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
    [Route("api/v1/topicos")]
    [Authorize]
    public class TopicoController : BaseController
    {
        private readonly ITopicoService _topicoService;
        private readonly ILogger<TopicoController> _logger;

        public TopicoController(ITopicoService topicoService, ILogger<TopicoController> logger)
        {
            _topicoService = topicoService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var usuarioId = GetUsuarioIdFromToken();
                if (usuarioId == null)
                {
                    return Unauthorized(new { message = "Usuário não identificado" });
                }

                var topicos = await _topicoService.GetAllByUsuarioIdAsync(usuarioId.Value);

                var response = topicos.Select(t => new TopicoResponseDto
                {
                    Id = t.Id,
                    UsuarioId = t.UsuarioId,
                    MateriaId = t.MateriaId,
                    Nome = t.Nome,
                    CriadoEm = t.CriadoEm,
                    AtualizadoEm = t.AtualizadoEm
                });

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter tópicos");
                return StatusCode(500, new { message = "Ocorreu um erro interno no servidor." });
            }
        }

        [HttpGet("materia/{materiaId}")]
        public async Task<IActionResult> GetByMateriaId(int materiaId)
        {
            try
            {
                var usuarioId = GetUsuarioIdFromToken();
                if (usuarioId == null)
                {
                    return Unauthorized(new { message = "Usuário não identificado" });
                }

                var topicos = await _topicoService.GetAllByMateriaIdAsync(materiaId, usuarioId.Value);

                var response = topicos.Select(t => new TopicoResponseDto
                {
                    Id = t.Id,
                    UsuarioId = t.UsuarioId,
                    MateriaId = t.MateriaId,
                    Nome = t.Nome,
                    CriadoEm = t.CriadoEm,
                    AtualizadoEm = t.AtualizadoEm
                });

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter tópicos da matéria {MateriaId}", materiaId);
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

                var topico = await _topicoService.GetByIdAsync(id, usuarioId.Value);

                if (topico == null)
                {
                    return NotFound(new { message = "Tópico não encontrado" });
                }

                var response = new TopicoResponseDto
                {
                    Id = topico.Id,
                    UsuarioId = topico.UsuarioId,
                    MateriaId = topico.MateriaId,
                    Nome = topico.Nome,
                    CriadoEm = topico.CriadoEm,
                    AtualizadoEm = topico.AtualizadoEm
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter tópico {Id}", id);
                return StatusCode(500, new { message = "Ocorreu um erro interno no servidor." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TopicoRequestDto dto)
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

                var topico = await _topicoService.CreateAsync(dto, usuarioId.Value);

                var response = new TopicoResponseDto
                {
                    Id = topico.Id,
                    UsuarioId = topico.UsuarioId,
                    MateriaId = topico.MateriaId,
                    Nome = topico.Nome,
                    CriadoEm = topico.CriadoEm,
                    AtualizadoEm = topico.AtualizadoEm
                };

                return CreatedAtAction(nameof(GetById), new { id = topico.Id }, response);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Erro de validação ao criar tópico: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar tópico");
                return StatusCode(500, new { message = "Ocorreu um erro interno no servidor." });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TopicoUpdateRequestDto dto)
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

                var topico = await _topicoService.UpdateAsync(id, dto, usuarioId.Value);

                if (topico == null)
                {
                    return NotFound(new { message = "Tópico não encontrado ou você não tem permissão para editá-lo" });
                }

                var response = new TopicoResponseDto
                {
                    Id = topico.Id,
                    UsuarioId = topico.UsuarioId,
                    MateriaId = topico.MateriaId,
                    Nome = topico.Nome,
                    CriadoEm = topico.CriadoEm,
                    AtualizadoEm = topico.AtualizadoEm
                };

                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Erro de validação ao atualizar tópico {Id}: {Message}", id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar tópico {Id}", id);
                return StatusCode(500, new { message = "Ocorreu um erro interno no servidor." });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var usuarioId = GetUsuarioIdFromToken();
                if (usuarioId == null)
                {
                    return Unauthorized(new { message = "Usuário não identificado" });
                }

                var deleted = await _topicoService.DeleteAsync(id, usuarioId.Value);

                if (!deleted)
                {
                    return NotFound(new { message = "Tópico não encontrado ou você não tem permissão para excluí-lo" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir tópico {Id}", id);
                return StatusCode(500, new { message = "Ocorreu um erro interno no servidor." });
            }
        }
    }
}