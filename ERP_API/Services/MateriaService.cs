using ERP_API.Models;
using ERP_API.Models.DTOs;
using ERP_API.Repositorys;
using ERP_API.Repositorys.Interfaces;
using ERP_API.Services.Interfaces;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace ERP_API.Services
{
    public class MateriaService : IMateriaService
    {
        private readonly IMateriaRepository _materiaRepository;
        private readonly ICategoriaRepository _categoriaRepository;
        private readonly IAuthRepository _authRepository;
        private readonly ILogger<MateriaService> _logger;

        public MateriaService(
            IMateriaRepository materiaRepository,
            ICategoriaRepository categoriaRepository,
            IAuthRepository authRepository,
            ILogger<MateriaService> logger)
        {
            _materiaRepository = materiaRepository;
            _categoriaRepository = categoriaRepository;
            _authRepository = authRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Materia>> GetAllByUsuarioIdAsync(int usuarioId)
        {
            try
            {
                _logger.LogInformation("Obtendo todas as matérias do usuário {UsuarioId}", usuarioId);

                if (!await _authRepository.UsuarioExiste(usuarioId))
                {
                    throw new UserNotFoundException($"Usuário {usuarioId} não encontrado");
                }

                return await _materiaRepository.GetAllByUsuarioIdAsync(usuarioId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter matérias do usuário {UsuarioId}", usuarioId);
                throw;
            }
        }

        public async Task<Materia?> GetByIdAsync(int id, int usuarioId)
        {
            try
            {
                _logger.LogInformation("Obtendo matéria {Id} para o usuário {UsuarioId}", id, usuarioId);

                var materia = await _materiaRepository.GetByIdAsync(id);
                if (materia == null)
                {
                    throw new MateriaNotFoundException($"Matéria {id} não encontrada");
                }

                if (materia.UsuarioId != usuarioId)
                {
                    throw new MateriaNotFoundException($"A matéria {id} não pertence ao usuário {usuarioId}");
                }

                return materia;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter matéria {Id} para o usuário {UsuarioId}", id, usuarioId);
                throw;
            }
        }

        public async Task<Materia> CreateAsync(MateriaRequestDto dto, int usuarioId)
        {
            try
            {
                _logger.LogInformation("Criando matéria {Nome} para o usuário {UsuarioId}", dto.Nome, usuarioId);

                if (!await _authRepository.UsuarioExiste(usuarioId))
                {
                    throw new UserNotFoundException($"Usuário {usuarioId} não encontrado");
                }

                if (await _materiaRepository.ExistsByNomeAndUsuarioIdAsync(dto.Nome, usuarioId))
                {
                    throw new ValidationException($"Já existe uma matéria com o nome '{dto.Nome}' para este usuário");
                }

                // Verificar se a categoria existe para o usuário
                if (!await _categoriaRepository.BelongsToUsuarioAsync(dto.CategoriaId, usuarioId))
                {
                    throw new ValidationException($"Categoria {dto.CategoriaId} não pertence ao usuário {usuarioId}");
                }

                var materia = new Materia
                {
                    UsuarioId = usuarioId,
                    Nome = dto.Nome,
                    Cor = dto.Cor,
                    CategoriaId = dto.CategoriaId 
                };

                return await _materiaRepository.CreateAsync(materia);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar matéria {Nome} para o usuário {UsuarioId}", dto.Nome, usuarioId);
                throw;
            }
        }


        public async Task<IEnumerable<Materia>> GetByCategoriaIdAsync(int categoriaId, int usuarioId)
        {
            try
            {
                _logger.LogInformation("Obtendo matérias da categoria {CategoriaId} para o usuário {UsuarioId}",
                    categoriaId, usuarioId);

                if (!await _authRepository.UsuarioExiste(usuarioId))
                {
                    throw new UserNotFoundException($"Usuário {usuarioId} não encontrado");
                }

                if (!await _categoriaRepository.BelongsToUsuarioAsync(categoriaId, usuarioId))
                {
                    throw new ValidationException($"Categoria {categoriaId} não pertence ao usuário {usuarioId}");
                }

                return await _materiaRepository.GetByCategoriaIdAsync(categoriaId, usuarioId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter matérias da categoria {CategoriaId} para o usuário {UsuarioId}",
                    categoriaId, usuarioId);
                throw;
            }
        }

        public async Task<Materia?> UpdateAsync(int id, MateriaUpdateRequestDto dto, int usuarioId)
        {
            try
            {
                _logger.LogInformation("Atualizando matéria {Id} para o usuário {UsuarioId}", id, usuarioId);

                var materia = await GetByIdAsync(id, usuarioId);
                if (materia == null)
                {
                    return null;
                }

                if (!string.IsNullOrEmpty(dto.Nome))
                {
                    if (await _materiaRepository.ExistsByNomeAndUsuarioIdAsync(dto.Nome, usuarioId, id))
                    {
                        throw new ValidationException($"Já existe outra matéria com o nome '{dto.Nome}' para este usuário");
                    }

                    materia.Nome = dto.Nome;
                }

                if (!string.IsNullOrEmpty(dto.Cor))
                {
                    materia.Cor = dto.Cor;
                }

                if (string.IsNullOrEmpty(dto.Nome) && string.IsNullOrEmpty(dto.Cor))
                {
                    _logger.LogInformation("Nenhum campo para atualizar na matéria {Id}", id);
                    return materia;
                }

                var updated = await _materiaRepository.UpdateAsync(materia);
                if (!updated)
                {
                    throw new ValidationException($"Falha ao atualizar matéria {id}");
                }

                return await _materiaRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar matéria {Id} para o usuário {UsuarioId}", id, usuarioId);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id, int usuarioId)
        {
            try
            {
                _logger.LogInformation("Excluindo matéria {Id} para o usuário {UsuarioId}", id, usuarioId);

                if (!await _materiaRepository.BelongsToUsuarioAsync(id, usuarioId))
                {
                    throw new MateriaNotFoundException($"Matéria {id} não encontrada ou não pertence ao usuário {usuarioId}");
                }

                return await _materiaRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir matéria {Id} para o usuário {UsuarioId}", id, usuarioId);
                throw;
            }
        }
    }

    public class UserNotFoundException : Exception
    {
        public UserNotFoundException(string message) : base(message) { }
    }

    public class MateriaNotFoundException : Exception
    {
        public MateriaNotFoundException(string message) : base(message) { }
    }
}
