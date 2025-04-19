using ERP_API.Infra.Data;
using ERP_API.Models;
using ERP_API.Repositorys.Interfaces;
using MySqlConnector;
using System.Data;

namespace ERP_API.Repositorys
{
    public class MateriaRepository : IMateriaRepository
    {
        private readonly IDatabaseService _databaseService;
        private readonly ILogger<MateriaRepository> _logger;

        public MateriaRepository(IDatabaseService databaseService, ILogger<MateriaRepository> logger)
        {
            _databaseService = databaseService;
            _logger = logger;
        }

        public async Task<IEnumerable<Materia>> GetAllByUsuarioIdAsync(int usuarioId)
        {
            try
            {
                string query = @"
                    SELECT id, usuario_id, nome, cor, criado_em, atualizado_em, categoria_id
                    FROM materias
                    WHERE usuario_id = @usuarioId
                    ORDER BY nome";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@usuarioId", usuarioId)
                };

                var dataTable = await _databaseService.ExecuteQueryAsync(query, parameters);
                var materias = new List<Materia>();

                foreach (DataRow row in dataTable.Rows)
                {
                    materias.Add(MapRowToMateria(row));
                }

                _logger.LogInformation("Obtidas {Count} matérias para o usuário {UsuarioId}", materias.Count, usuarioId);
                return materias;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter matérias do usuário {UsuarioId}", usuarioId);
                throw;
            }
        }

        public async Task<Materia?> GetByIdAsync(int id)
        {
            try
            {
                string query = @"
                    SELECT id, usuario_id, nome, cor, criado_em, atualizado_em, categoria_id
                    FROM materias
                    WHERE id = @id";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@id", id)
                };

                var dataTable = await _databaseService.ExecuteQueryAsync(query, parameters);

                if (dataTable.Rows.Count == 0)
                {
                    _logger.LogWarning("Matéria {Id} não encontrada", id);
                    return null;
                }

                var materia = MapRowToMateria(dataTable.Rows[0]);
                _logger.LogInformation("Matéria {Id} obtida com sucesso", id);
                return materia;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter matéria {Id}", id);
                throw;
            }
        }

        public async Task<bool> BelongsToUsuarioAsync(int materiaId, int usuarioId)
        {
            try
            {
                string query = @"
                    SELECT COUNT(1)
                    FROM materias
                    WHERE id = @materiaId AND usuario_id = @usuarioId";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@materiaId", materiaId),
                    new MySqlParameter("@usuarioId", usuarioId)
                };

                var result = await _databaseService.ExecuteScalarAsync(query, parameters);
                return Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar se matéria {MateriaId} pertence ao usuário {UsuarioId}", materiaId, usuarioId);
                throw;
            }
        }

        public async Task<bool> ExistsByNomeAndUsuarioIdAsync(string nome, int usuarioId, int? ignoreMateriaId = null)
        {
            try
            {
                string query = @"
                    SELECT COUNT(1)
                    FROM materias
                    WHERE usuario_id = @usuarioId AND nome = @nome";

                var parameters = new List<MySqlParameter>
                {
                    new MySqlParameter("@usuarioId", usuarioId),
                    new MySqlParameter("@nome", nome)
                };

                if (ignoreMateriaId.HasValue)
                {
                    query += " AND id != @ignoreMateriaId";
                    parameters.Add(new MySqlParameter("@ignoreMateriaId", ignoreMateriaId.Value));
                }

                var result = await _databaseService.ExecuteScalarAsync(query, parameters.ToArray());
                return Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar existência da matéria {Nome} para o usuário {UsuarioId}", nome, usuarioId);
                throw;
            }
        }

        public async Task<IEnumerable<Materia>> GetByCategoriaIdAsync(int categoriaId, int usuarioId)
        {
            try
            {
                string query = @"
            SELECT m.id, m.usuario_id, m.nome, m.cor, m.criado_em, m.atualizado_em, m.categoria_id
            FROM materias m
            INNER JOIN categorias c ON m.categoria_id = c.id
            WHERE m.categoria_id = @categoriaId AND m.usuario_id = @usuarioId
            ORDER BY m.nome";

                var parameters = new MySqlParameter[]
                {
            new MySqlParameter("@categoriaId", categoriaId),
            new MySqlParameter("@usuarioId", usuarioId)
                };

                var dataTable = await _databaseService.ExecuteQueryAsync(query, parameters);
                var materias = new List<Materia>();

                foreach (DataRow row in dataTable.Rows)
                {
                    materias.Add(MapRowToMateria(row));
                }

                _logger.LogInformation("Obtidas {Count} matérias para a categoria {CategoriaId} do usuário {UsuarioId}",
                    materias.Count, categoriaId, usuarioId);
                return materias;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter matérias da categoria {CategoriaId} para o usuário {UsuarioId}",
                    categoriaId, usuarioId);
                throw;
            }
        }
        public async Task<Materia> CreateAsync(Materia materia)
        {
            try
            {
                string query = @"
                    INSERT INTO materias (usuario_id, nome, cor)
                    VALUES (@usuarioId, @nome, @cor);
                    SELECT LAST_INSERT_ID();";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@usuarioId", materia.UsuarioId),
                    new MySqlParameter("@nome", materia.Nome),
                    new MySqlParameter("@cor", materia.Cor)
                };

                var id = await _databaseService.ExecuteScalarAsync(query, parameters);
                materia.Id = Convert.ToInt32(id);

                var createdMateria = await GetByIdAsync(materia.Id);
                if (createdMateria != null)
                {
                    materia.CriadoEm = createdMateria.CriadoEm;
                    materia.AtualizadoEm = createdMateria.AtualizadoEm;
                }

                _logger.LogInformation("Matéria {Id} criada com sucesso", materia.Id);
                return materia;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar matéria {Nome} para o usuário {UsuarioId}", materia.Nome, materia.UsuarioId);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(Materia materia)
        {
            try
            {
                string query = @"
                    UPDATE materias
                    SET nome = @nome, cor = @cor
                    WHERE id = @id AND usuario_id = @usuarioId";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@id", materia.Id),
                    new MySqlParameter("@usuarioId", materia.UsuarioId),
                    new MySqlParameter("@nome", materia.Nome),
                    new MySqlParameter("@cor", materia.Cor)
                };

                var affectedRows = await _databaseService.ExecuteNonQueryAsync(query, parameters);

                if (affectedRows > 0)
                {
                    _logger.LogInformation("Matéria {Id} atualizada com sucesso", materia.Id);
                    return true;
                }

                _logger.LogWarning("Nenhuma alteração realizada para a matéria {Id}", materia.Id);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar matéria {Id}", materia.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                string query = "DELETE FROM materias WHERE id = @id";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@id", id)
                };

                var affectedRows = await _databaseService.ExecuteNonQueryAsync(query, parameters);

                if (affectedRows > 0)
                {
                    _logger.LogInformation("Matéria {Id} excluída com sucesso", id);
                    return true;
                }

                _logger.LogWarning("Matéria {Id} não encontrada para exclusão", id);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir matéria {Id}", id);
                throw;
            }
        }

        private Materia MapRowToMateria(DataRow row)
        {
            return new Materia
            {
                Id = Convert.ToInt32(row["id"]),
                UsuarioId = Convert.ToInt32(row["usuario_id"]),
                Nome = row["nome"].ToString() ?? string.Empty,
                Cor = row["cor"].ToString() ?? "#CCCCCC",
                CriadoEm = Convert.ToDateTime(row["criado_em"]),
                AtualizadoEm = Convert.ToDateTime(row["atualizado_em"]),
                CategoriaId = Convert.ToInt32(row["categoria_id"]),
            };
        }
    }
}