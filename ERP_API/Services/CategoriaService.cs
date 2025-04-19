using ERP_API.Models;
using ERP_API.Models.DTOs;
using ERP_API.Repositorys.Interfaces;
using ERP_API.Services.Interfaces;

namespace ERP_API.Services
{
    public class CategoriaService : ICategoriaService
    {
        private readonly ICategoriaRepository _categoriaRepository;
        private readonly IAuthRepository _authRepository;
        private readonly ILogger<CategoriaService> _logger;

        public CategoriaService(
            ICategoriaRepository categoriaRepository,
            IAuthRepository authRepository,
            ILogger<CategoriaService> logger)
        {
            _categoriaRepository = categoriaRepository;
            _authRepository = authRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Categoria>> GetAllByUsuarioIdAsync(int usuarioId)
        {
            try
            {
                _logger.LogInformation("Obtendo todas as categorias do usuário {UsuarioId}", usuarioId);

                if (!await _authRepository.UsuarioExiste(usuarioId))
                {
                    _logger.LogWarning("Usuário {UsuarioId} não encontrado ao obter categorias", usuarioId);
                    return Enumerable.Empty<Categoria>();
                }

                return await _categoriaRepository.GetAllByUsuarioIdAsync(usuarioId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter categorias do usuário {UsuarioId}", usuarioId);
                throw;
            }
        }

        public async Task<Categoria?> GetByIdAsync(int id, int usuarioId)
        {
            try
            {
                _logger.LogInformation("Obtendo categoria {Id} para o usuário {UsuarioId}", id, usuarioId);

                var categoria = await _categoriaRepository.GetByIdAsync(id);

                if (categoria == null)
                {
                    _logger.LogWarning("Categoria {Id} não encontrada", id);
                    return null;
                }

                if (categoria.UsuarioId != usuarioId)
                {
                    _logger.LogWarning("Categoria {Id} não pertence ao usuário {UsuarioId}", id, usuarioId);
                    return null;
                }

                return categoria;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter categoria {Id} para o usuário {UsuarioId}", id, usuarioId);
                throw;
            }
        }

        public async Task<Categoria> CreateAsync(CategoriaRequestDto dto, int usuarioId)
        {
            try
            {
                _logger.LogInformation("Criando categoria {Nome} para o usuário {UsuarioId}", dto.Nome, usuarioId);

                if (!await _authRepository.UsuarioExiste(usuarioId))
                {
                    throw new InvalidOperationException($"Usuário {usuarioId} não encontrado");
                }

                if (await _categoriaRepository.ExistsByNomeAndUsuarioIdAsync(dto.Nome, usuarioId))
                {
                    throw new InvalidOperationException($"Já existe uma categoria com o nome '{dto.Nome}' para este usuário");
                }

                var categoria = new Categoria
                {
                    UsuarioId = usuarioId,
                    Nome = dto.Nome,
                    Cor = dto.Cor
                };

                return await _categoriaRepository.CreateAsync(categoria);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar categoria {Nome} para o usuário {UsuarioId}", dto.Nome, usuarioId);
                throw;
            }
        }

        public async Task<Categoria?> UpdateAsync(int id, CategoriaUpdateRequestDto dto, int usuarioId)
        {
            try
            {
                _logger.LogInformation("Atualizando categoria {Id} para o usuário {UsuarioId}", id, usuarioId);

                var categoria = await GetByIdAsync(id, usuarioId);
                if (categoria == null)
                {
                    return null;
                }

                if (!string.IsNullOrEmpty(dto.Nome))
                {
                    if (await _categoriaRepository.ExistsByNomeAndUsuarioIdAsync(dto.Nome, usuarioId, id))
                    {
                        throw new InvalidOperationException($"Já existe outra categoria com o nome '{dto.Nome}' para este usuário");
                    }

                    categoria.Nome = dto.Nome;
                }

                if (!string.IsNullOrEmpty(dto.Cor))
                {
                    categoria.Cor = dto.Cor;
                }

                if (string.IsNullOrEmpty(dto.Nome) && string.IsNullOrEmpty(dto.Cor))
                {
                    _logger.LogInformation("Nenhum campo para atualizar na categoria {Id}", id);
                    return categoria;
                }

                var updated = await _categoriaRepository.UpdateAsync(categoria);
                if (!updated)
                {
                    _logger.LogWarning("Falha ao atualizar categoria {Id}", id);
                    return null;
                }

                return await _categoriaRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar categoria {Id} para o usuário {UsuarioId}", id, usuarioId);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id, int usuarioId)
        {
            try
            {
                _logger.LogInformation("Excluindo categoria {Id} para o usuário {UsuarioId}", id, usuarioId);

                if (!await _categoriaRepository.BelongsToUsuarioAsync(id, usuarioId))
                {
                    _logger.LogWarning("Categoria {Id} não encontrada ou não pertence ao usuário {UsuarioId}", id, usuarioId);
                    return false;
                }

                return await _categoriaRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir categoria {Id} para o usuário {UsuarioId}", id, usuarioId);
                throw;
            }
        }
    }
}