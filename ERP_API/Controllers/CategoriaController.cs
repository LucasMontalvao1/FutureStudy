using ERP_API.Models.DTOs;
using ERP_API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP_API.Controllers
{
    [ApiController]
    [Route("api/v1/categorias")]
    [Authorize]
    public class CategoriaController : BaseController
    {
        private readonly ICategoriaService _categoriaService;
        private readonly ILogger<CategoriaController> _logger;

        public CategoriaController(ICategoriaService categoriaService, ILogger<CategoriaController> logger)
        {
            _categoriaService = categoriaService;
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

                var categorias = await _categoriaService.GetAllByUsuarioIdAsync(usuarioId.Value);

                var response = categorias.Select(c => new CategoriaResponseDto
                {
                    Id = c.Id,
                    UsuarioId = c.UsuarioId,
                    Nome = c.Nome,
                    Cor = c.Cor,
                    CriadoEm = c.CriadoEm,
                    AtualizadoEm = c.AtualizadoEm
                });

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter categorias");
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

                var categoria = await _categoriaService.GetByIdAsync(id, usuarioId.Value);

                if (categoria == null)
                {
                    return NotFound(new { message = "Categoria não encontrada" });
                }

                var response = new CategoriaResponseDto
                {
                    Id = categoria.Id,
                    UsuarioId = categoria.UsuarioId,
                    Nome = categoria.Nome,
                    Cor = categoria.Cor,
                    CriadoEm = categoria.CriadoEm,
                    AtualizadoEm = categoria.AtualizadoEm
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter categoria {Id}", id);
                return StatusCode(500, new { message = "Ocorreu um erro interno no servidor." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CategoriaRequestDto dto)
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

                var categoria = await _categoriaService.CreateAsync(dto, usuarioId.Value);

                var response = new CategoriaResponseDto
                {
                    Id = categoria.Id,
                    UsuarioId = categoria.UsuarioId,
                    Nome = categoria.Nome,
                    Cor = categoria.Cor,
                    CriadoEm = categoria.CriadoEm,
                    AtualizadoEm = categoria.AtualizadoEm
                };

                return CreatedAtAction(nameof(GetById), new { id = categoria.Id }, response);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Erro de validação ao criar categoria: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar categoria");
                return StatusCode(500, new { message = "Ocorreu um erro interno no servidor." });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CategoriaUpdateRequestDto dto)
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

                var categoria = await _categoriaService.UpdateAsync(id, dto, usuarioId.Value);

                if (categoria == null)
                {
                    return NotFound(new { message = "Categoria não encontrada ou você não tem permissão para editá-la" });
                }

                var response = new CategoriaResponseDto
                {
                    Id = categoria.Id,
                    UsuarioId = categoria.UsuarioId,
                    Nome = categoria.Nome,
                    Cor = categoria.Cor,
                    CriadoEm = categoria.CriadoEm,
                    AtualizadoEm = categoria.AtualizadoEm
                };

                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Erro de validação ao atualizar categoria {Id}: {Message}", id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar categoria {Id}", id);
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

                var deleted = await _categoriaService.DeleteAsync(id, usuarioId.Value);

                if (!deleted)
                {
                    return NotFound(new { message = "Categoria não encontrada ou você não tem permissão para excluí-la" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir categoria {Id}", id);
                return StatusCode(500, new { message = "Ocorreu um erro interno no servidor." });
            }
        }
    }
}