using ERP_API.Models.DTOs;
using ERP_API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP_API.Controllers
{
    [ApiController]
    [Route("api/v1/materias")]
    [Authorize]
    public class MateriaController : BaseController
    {
        private readonly IMateriaService _materiaService;
        private readonly ILogger<MateriaController> _logger;

        public MateriaController(IMateriaService materiaService, ILogger<MateriaController> logger)
        {
            _materiaService = materiaService;
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

                var materias = await _materiaService.GetAllByUsuarioIdAsync(usuarioId.Value);

                var response = materias.Select(m => new MateriaResponseDto
                {
                    Id = m.Id,
                    UsuarioId = m.UsuarioId,
                    Nome = m.Nome,
                    Cor = m.Cor,
                    CriadoEm = m.CriadoEm,
                    AtualizadoEm = m.AtualizadoEm
                });

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter matérias");
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

                var materia = await _materiaService.GetByIdAsync(id, usuarioId.Value);

                if (materia == null)
                {
                    return NotFound(new { message = "Matéria não encontrada" });
                }

                var response = new MateriaResponseDto
                {
                    Id = materia.Id,
                    UsuarioId = materia.UsuarioId,
                    Nome = materia.Nome,
                    Cor = materia.Cor,
                    CriadoEm = materia.CriadoEm,
                    AtualizadoEm = materia.AtualizadoEm
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter matéria {Id}", id);
                return StatusCode(500, new { message = "Ocorreu um erro interno no servidor." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MateriaRequestDto dto)
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

                var materia = await _materiaService.CreateAsync(dto, usuarioId.Value);

                var response = new MateriaResponseDto
                {
                    Id = materia.Id,
                    UsuarioId = materia.UsuarioId,
                    Nome = materia.Nome,
                    Cor = materia.Cor,
                    CriadoEm = materia.CriadoEm,
                    AtualizadoEm = materia.AtualizadoEm
                };

                return CreatedAtAction(nameof(GetById), new { id = materia.Id }, response);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Erro de validação ao criar matéria: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar matéria");
                return StatusCode(500, new { message = "Ocorreu um erro interno no servidor." });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] MateriaUpdateRequestDto dto)
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

                var materia = await _materiaService.UpdateAsync(id, dto, usuarioId.Value);

                if (materia == null)
                {
                    return NotFound(new { message = "Matéria não encontrada ou você não tem permissão para editá-la" });
                }

                var response = new MateriaResponseDto
                {
                    Id = materia.Id,
                    UsuarioId = materia.UsuarioId,
                    Nome = materia.Nome,
                    Cor = materia.Cor,
                    CriadoEm = materia.CriadoEm,
                    AtualizadoEm = materia.AtualizadoEm
                };

                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Erro de validação ao atualizar matéria {Id}: {Message}", id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar matéria {Id}", id);
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

                var deleted = await _materiaService.DeleteAsync(id, usuarioId.Value);

                if (!deleted)
                {
                    return NotFound(new { message = "Matéria não encontrada ou você não tem permissão para excluí-la" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir matéria {Id}", id);
                return StatusCode(500, new { message = "Ocorreu um erro interno no servidor." });
            }
        }
    }
}