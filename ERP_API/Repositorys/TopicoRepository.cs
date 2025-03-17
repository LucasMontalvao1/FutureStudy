using ERP_API.Infra.Data;
using ERP_API.Models;
using ERP_API.Repositorys.Interfaces;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ERP_API.Repositorys
{
    public class TopicoRepository : ITopicoRepository
    {
        private readonly IDatabaseService _databaseService;
        private readonly ILogger<TopicoRepository> _logger;

        public TopicoRepository(IDatabaseService databaseService, ILogger<TopicoRepository> logger)
        {
            _databaseService = databaseService;
            _logger = logger;
        }

        public async Task<IEnumerable<Topico>> GetAllByUsuarioIdAsync(int usuarioId)
        {
            try
            {
                string query = @"
                    SELECT id, usuario_id, materia_id, nome, criado_em, atualizado_em
                    FROM topicos
                    WHERE usuario_id = @usuarioId
                    ORDER BY nome";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@usuarioId", usuarioId)
                };

                var dataTable = await _databaseService.ExecuteQueryAsync(query, parameters);
                var topicos = new List<Topico>();

                foreach (DataRow row in dataTable.Rows)
                {
                    topicos.Add(MapRowToTopico(row));
                }

                _logger.LogInformation("Obtidos {Count} tópicos para o usuário {UsuarioId}", topicos.Count, usuarioId);
                return topicos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter tópicos do usuário {UsuarioId}", usuarioId);
                throw;
            }
        }

        public async Task<IEnumerable<Topico>> GetAllByMateriaIdAsync(int materiaId, int usuarioId)
        {
            try
            {
                string query = @"
                    SELECT id, usuario_id, materia_id, nome, criado_em, atualizado_em
                    FROM topicos
                    WHERE materia_id = @materiaId AND usuario_id = @usuarioId
                    ORDER BY nome";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@materiaId", materiaId),
                    new MySqlParameter("@usuarioId", usuarioId)
                };

                var dataTable = await _databaseService.ExecuteQueryAsync(query, parameters);
                var topicos = new List<Topico>();

                foreach (DataRow row in dataTable.Rows)
                {
                    topicos.Add(MapRowToTopico(row));
                }

                _logger.LogInformation("Obtidos {Count} tópicos para a matéria {MateriaId}", topicos.Count, materiaId);
                return topicos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter tópicos da matéria {MateriaId}", materiaId);
                throw;
            }
        }

        public async Task<Topico?> GetByIdAsync(int id)
        {
            try
            {
                string query = @"
                    SELECT id, usuario_id, materia_id, nome, criado_em, atualizado_em
                    FROM topicos
                    WHERE id = @id";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@id", id)
                };

                var dataTable = await _databaseService.ExecuteQueryAsync(query, parameters);

                if (dataTable.Rows.Count == 0)
                {
                    _logger.LogWarning("Tópico {Id} não encontrado", id);
                    return null;
                }

                var topico = MapRowToTopico(dataTable.Rows[0]);
                _logger.LogInformation("Tópico {Id} obtido com sucesso", id);
                return topico;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter tópico {Id}", id);
                throw;
            }
        }

        public async Task<bool> BelongsToUsuarioAsync(int topicoId, int usuarioId)
        {
            try
            {
                string query = @"
                    SELECT COUNT(1)
                    FROM topicos
                    WHERE id = @topicoId AND usuario_id = @usuarioId";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@topicoId", topicoId),
                    new MySqlParameter("@usuarioId", usuarioId)
                };

                var result = await _databaseService.ExecuteScalarAsync(query, parameters);
                return Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar se tópico {TopicoId} pertence ao usuário {UsuarioId}", topicoId, usuarioId);
                throw;
            }
        }

        public async Task<bool> ExistsByNomeAndMateriaIdAsync(string nome, int materiaId, int usuarioId, int? ignoreTopicoId = null)
        {
            try
            {
                string query = @"
                    SELECT COUNT(1)
                    FROM topicos
                    WHERE usuario_id = @usuarioId AND materia_id = @materiaId AND nome = @nome";

                var parameters = new List<MySqlParameter>
                {
                    new MySqlParameter("@usuarioId", usuarioId),
                    new MySqlParameter("@materiaId", materiaId),
                    new MySqlParameter("@nome", nome)
                };

                // Se for uma atualização, ignore o próprio tópico na verificação de duplicidade
                if (ignoreTopicoId.HasValue)
                {
                    query += " AND id != @ignoreTopicoId";
                    parameters.Add(new MySqlParameter("@ignoreTopicoId", ignoreTopicoId.Value));
                }

                var result = await _databaseService.ExecuteScalarAsync(query, parameters.ToArray());
                return Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar existência do tópico {Nome} para a matéria {MateriaId}", nome, materiaId);
                throw;
            }
        }

        public async Task<Topico> CreateAsync(Topico topico)
        {
            try
            {
                string query = @"
                    INSERT INTO topicos (usuario_id, materia_id, nome)
                    VALUES (@usuarioId, @materiaId, @nome);
                    SELECT LAST_INSERT_ID();";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@usuarioId", topico.UsuarioId),
                    new MySqlParameter("@materiaId", topico.MateriaId),
                    new MySqlParameter("@nome", topico.Nome)
                };

                var id = await _databaseService.ExecuteScalarAsync(query, parameters);
                topico.Id = Convert.ToInt32(id);

                // Buscar o tópico completo para obter as datas geradas pelo banco
                var createdTopico = await GetByIdAsync(topico.Id);
                if (createdTopico != null)
                {
                    topico.CriadoEm = createdTopico.CriadoEm;
                    topico.AtualizadoEm = createdTopico.AtualizadoEm;
                }

                _logger.LogInformation("Tópico {Id} criado com sucesso", topico.Id);
                return topico;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar tópico {Nome} para a matéria {MateriaId}", topico.Nome, topico.MateriaId);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(Topico topico)
        {
            try
            {
                string query = @"
                    UPDATE topicos
                    SET nome = @nome, materia_id = @materiaId
                    WHERE id = @id AND usuario_id = @usuarioId";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@id", topico.Id),
                    new MySqlParameter("@usuarioId", topico.UsuarioId),
                    new MySqlParameter("@materiaId", topico.MateriaId),
                    new MySqlParameter("@nome", topico.Nome)
                };

                var affectedRows = await _databaseService.ExecuteNonQueryAsync(query, parameters);

                if (affectedRows > 0)
                {
                    _logger.LogInformation("Tópico {Id} atualizado com sucesso", topico.Id);
                    return true;
                }

                _logger.LogWarning("Nenhuma alteração realizada para o tópico {Id}", topico.Id);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar tópico {Id}", topico.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                string query = "DELETE FROM topicos WHERE id = @id";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@id", id)
                };

                var affectedRows = await _databaseService.ExecuteNonQueryAsync(query, parameters);

                if (affectedRows > 0)
                {
                    _logger.LogInformation("Tópico {Id} excluído com sucesso", id);
                    return true;
                }

                _logger.LogWarning("Tópico {Id} não encontrado para exclusão", id);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir tópico {Id}", id);
                throw;
            }
        }

        private Topico MapRowToTopico(DataRow row)
        {
            return new Topico
            {
                Id = Convert.ToInt32(row["id"]),
                UsuarioId = Convert.ToInt32(row["usuario_id"]),
                MateriaId = Convert.ToInt32(row["materia_id"]), 
                Nome = row["nome"].ToString() ?? string.Empty,
                CriadoEm = Convert.ToDateTime(row["criado_em"]),
                AtualizadoEm = Convert.ToDateTime(row["atualizado_em"])
            };
        }
    }
}