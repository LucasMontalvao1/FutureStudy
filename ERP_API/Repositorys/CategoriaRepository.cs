using ERP_API.Infra.Data;
using ERP_API.Models;
using ERP_API.Repositorys.Interfaces;
using MySqlConnector;
using System.Data;

namespace ERP_API.Repositorys
{
    public class CategoriaRepository : ICategoriaRepository
    {
        private readonly IDatabaseService _databaseService;
        private readonly ILogger<CategoriaRepository> _logger;

        public CategoriaRepository(IDatabaseService databaseService, ILogger<CategoriaRepository> logger)
        {
            _databaseService = databaseService;
            _logger = logger;
        }

        public async Task<IEnumerable<Categoria>> GetAllByUsuarioIdAsync(int usuarioId)
        {
            try
            {
                string query = @"
                    SELECT id, usuario_id, nome, cor, criado_em, atualizado_em
                    FROM categorias
                    WHERE usuario_id = @usuarioId
                    ORDER BY nome";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@usuarioId", usuarioId)
                };

                var dataTable = await _databaseService.ExecuteQueryAsync(query, parameters);
                var categorias = new List<Categoria>();

                foreach (DataRow row in dataTable.Rows)
                {
                    categorias.Add(MapRowToCategoria(row));
                }

                _logger.LogInformation("Obtidas {Count} categorias para o usuário {UsuarioId}", categorias.Count, usuarioId);
                return categorias;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter categorias do usuário {UsuarioId}", usuarioId);
                throw;
            }
        }

        public async Task<Categoria?> GetByIdAsync(int id)
        {
            try
            {
                string query = @"
                    SELECT id, usuario_id, nome, cor, criado_em, atualizado_em
                    FROM categorias
                    WHERE id = @id";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@id", id)
                };

                var dataTable = await _databaseService.ExecuteQueryAsync(query, parameters);

                if (dataTable.Rows.Count == 0)
                {
                    _logger.LogWarning("Categoria {Id} não encontrada", id);
                    return null;
                }

                var categoria = MapRowToCategoria(dataTable.Rows[0]);
                _logger.LogInformation("Categoria {Id} obtida com sucesso", id);
                return categoria;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter categoria {Id}", id);
                throw;
            }
        }

        public async Task<bool> BelongsToUsuarioAsync(int categoriaId, int usuarioId)
        {
            try
            {
                string query = @"
                    SELECT COUNT(1)
                    FROM categorias
                    WHERE id = @categoriaId AND usuario_id = @usuarioId";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@categoriaId", categoriaId),
                    new MySqlParameter("@usuarioId", usuarioId)
                };

                var result = await _databaseService.ExecuteScalarAsync(query, parameters);
                return Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar se categoria {CategoriaId} pertence ao usuário {UsuarioId}", categoriaId, usuarioId);
                throw;
            }
        }

        public async Task<bool> ExistsByNomeAndUsuarioIdAsync(string nome, int usuarioId, int? ignoreCategoriaId = null)
        {
            try
            {
                string query = @"
                    SELECT COUNT(1)
                    FROM categorias
                    WHERE usuario_id = @usuarioId AND nome = @nome";

                var parameters = new List<MySqlParameter>
                {
                    new MySqlParameter("@usuarioId", usuarioId),
                    new MySqlParameter("@nome", nome)
                };

                if (ignoreCategoriaId.HasValue)
                {
                    query += " AND id != @ignoreCategoriaId";
                    parameters.Add(new MySqlParameter("@ignoreCategoriaId", ignoreCategoriaId.Value));
                }

                var result = await _databaseService.ExecuteScalarAsync(query, parameters.ToArray());
                return Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar existência da categoria {Nome} para o usuário {UsuarioId}", nome, usuarioId);
                throw;
            }
        }

        public async Task<Categoria> CreateAsync(Categoria categoria)
        {
            try
            {
                string query = @"
                    INSERT INTO categorias (usuario_id, nome, cor)
                    VALUES (@usuarioId, @nome, @cor);
                    SELECT LAST_INSERT_ID();";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@usuarioId", categoria.UsuarioId),
                    new MySqlParameter("@nome", categoria.Nome),
                    new MySqlParameter("@cor", categoria.Cor)
                };

                var id = await _databaseService.ExecuteScalarAsync(query, parameters);
                categoria.Id = Convert.ToInt32(id);

                var createdCategoria = await GetByIdAsync(categoria.Id);
                if (createdCategoria != null)
                {
                    categoria.CriadoEm = createdCategoria.CriadoEm;
                    categoria.AtualizadoEm = createdCategoria.AtualizadoEm;
                }

                _logger.LogInformation("Categoria {Id} criada com sucesso", categoria.Id);
                return categoria;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar categoria {Nome} para o usuário {UsuarioId}", categoria.Nome, categoria.UsuarioId);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(Categoria categoria)
        {
            try
            {
                string query = @"
                    UPDATE categorias
                    SET nome = @nome, cor = @cor
                    WHERE id = @id AND usuario_id = @usuarioId";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@id", categoria.Id),
                    new MySqlParameter("@usuarioId", categoria.UsuarioId),
                    new MySqlParameter("@nome", categoria.Nome),
                    new MySqlParameter("@cor", categoria.Cor)
                };

                var affectedRows = await _databaseService.ExecuteNonQueryAsync(query, parameters);

                if (affectedRows > 0)
                {
                    _logger.LogInformation("Categoria {Id} atualizada com sucesso", categoria.Id);
                    return true;
                }

                _logger.LogWarning("Nenhuma alteração realizada para a categoria {Id}", categoria.Id);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar categoria {Id}", categoria.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                string query = "DELETE FROM categorias WHERE id = @id";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@id", id)
                };

                var affectedRows = await _databaseService.ExecuteNonQueryAsync(query, parameters);

                if (affectedRows > 0)
                {
                    _logger.LogInformation("Categoria {Id} excluída com sucesso", id);
                    return true;
                }

                _logger.LogWarning("Categoria {Id} não encontrada para exclusão", id);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir categoria {Id}", id);
                throw;
            }
        }

        private Categoria MapRowToCategoria(DataRow row)
        {
            return new Categoria
            {
                Id = Convert.ToInt32(row["id"]),
                UsuarioId = Convert.ToInt32(row["usuario_id"]),
                Nome = row["nome"].ToString() ?? string.Empty,
                Cor = row["cor"].ToString() ?? "#CCCCCC",
                CriadoEm = Convert.ToDateTime(row["criado_em"]),
                AtualizadoEm = Convert.ToDateTime(row["atualizado_em"])
            };
        }
    }
}