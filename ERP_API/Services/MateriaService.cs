using ERP_API.Models;
using ERP_API.Models.DTOs;
using ERP_API.Repositorys.Interfaces;
using ERP_API.Services.Interfaces;

namespace ERP_API.Services
{
    public class MateriaService : IMateriaService
    {
        private readonly IMateriaRepository _materiaRepository;
        private readonly IAuthRepository _authRepository;
        private readonly ILogger<MateriaService> _logger;

        public MateriaService(
            IMateriaRepository materiaRepository,
            IAuthRepository authRepository,
            ILogger<MateriaService> logger)
        {
            _materiaRepository = materiaRepository;
            _authRepository = authRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Materia>> GetAllByUsuarioIdAsync(int usuarioId)
        {
            try
            {
                _logger.LogInformation("Obtendo todas as matérias do usuário {UsuarioId}", usuarioId);

                // Verificar se o usuário existe
                if (!await _authRepository.UsuarioExiste(usuarioId))
                {
                    _logger.LogWarning("Usuário {UsuarioId} não encontrado ao obter matérias", usuarioId);
                    return Enumerable.Empty<Materia>();
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
                    _logger.LogWarning("Matéria {Id} não encontrada", id);
                    return null;
                }

                // Verificar se a matéria pertence ao usuário
                if (materia.UsuarioId != usuarioId)
                {
                    _logger.LogWarning("Matéria {Id} não pertence ao usuário {UsuarioId}", id, usuarioId);
                    return null;
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

                // Verificar se o usuário existe
                if (!await _authRepository.UsuarioExiste(usuarioId))
                {
                    throw new InvalidOperationException($"Usuário {usuarioId} não encontrado");
                }

                // Verificar se já existe uma matéria com o mesmo nome para este usuário
                if (await _materiaRepository.ExistsByNomeAndUsuarioIdAsync(dto.Nome, usuarioId))
                {
                    throw new InvalidOperationException($"Já existe uma matéria com o nome '{dto.Nome}' para este usuário");
                }

                var materia = new Materia
                {
                    UsuarioId = usuarioId,
                    Nome = dto.Nome,
                    Cor = dto.Cor
                };

                return await _materiaRepository.CreateAsync(materia);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar matéria {Nome} para o usuário {UsuarioId}", dto.Nome, usuarioId);
                throw;
            }
        }

        public async Task<Materia?> UpdateAsync(int id, MateriaUpdateRequestDto dto, int usuarioId)
        {
            try
            {
                _logger.LogInformation("Atualizando matéria {Id} para o usuário {UsuarioId}", id, usuarioId);

                // Verificar se a matéria existe e pertence ao usuário
                var materia = await GetByIdAsync(id, usuarioId);
                if (materia == null)
                {
                    return null;
                }

                // Atualizar apenas os campos que foram fornecidos
                if (!string.IsNullOrEmpty(dto.Nome))
                {
                    // Verificar se já existe outra matéria com o mesmo nome para este usuário
                    if (await _materiaRepository.ExistsByNomeAndUsuarioIdAsync(dto.Nome, usuarioId, id))
                    {
                        throw new InvalidOperationException($"Já existe outra matéria com o nome '{dto.Nome}' para este usuário");
                    }

                    materia.Nome = dto.Nome;
                }

                if (!string.IsNullOrEmpty(dto.Cor))
                {
                    materia.Cor = dto.Cor;
                }

                // Se nenhum campo foi alterado, retornar a matéria sem fazer a atualização
                if (string.IsNullOrEmpty(dto.Nome) && string.IsNullOrEmpty(dto.Cor))
                {
                    _logger.LogInformation("Nenhum campo para atualizar na matéria {Id}", id);
                    return materia;
                }

                var updated = await _materiaRepository.UpdateAsync(materia);
                if (!updated)
                {
                    _logger.LogWarning("Falha ao atualizar matéria {Id}", id);
                    return null;
                }

                // Retornar a matéria atualizada (buscar novamente para ter as datas atualizadas)
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

                // Verificar se a matéria existe e pertence ao usuário
                if (!await _materiaRepository.BelongsToUsuarioAsync(id, usuarioId))
                {
                    _logger.LogWarning("Matéria {Id} não encontrada ou não pertence ao usuário {UsuarioId}", id, usuarioId);
                    return false;
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
}