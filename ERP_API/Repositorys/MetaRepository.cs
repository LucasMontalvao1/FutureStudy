using ERP_API.Infra.Data;
using ERP_API.Models;
using ERP_API.Models.Enums;
using ERP_API.Repositories.Interfaces;
using ERP_API.Repositorys.Interfaces;
using ERP_API.SQL;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ERP_API.Repositorys
{
    public class MetaRepository : IMetaRepository
    {
        private readonly IDatabaseService _databaseService;
        private readonly ILogger<MetaRepository> _logger;
        private readonly SqlLoader _sqlLoader;

        public MetaRepository(
            IDatabaseService databaseService,
            ILogger<MetaRepository> logger,
            SqlLoader sqlLoader)
        {
            _databaseService = databaseService;
            _logger = logger;
            _sqlLoader = sqlLoader;
        }

        public async Task<IEnumerable<Meta>> GetAllByUsuarioIdAsync(int usuarioId)
        {
            try
            {
                string query = await _sqlLoader.LoadSqlAsync("Metas/GetAllByUsuarioId.sql");

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@usuarioId", usuarioId)
                };

                var dataTable = await _databaseService.ExecuteQueryAsync(query, parameters);
                var metas = new List<Meta>();

                foreach (DataRow row in dataTable.Rows)
                {
                    metas.Add(MapRowToMeta(row));
                }

                _logger.LogInformation("Obtidas {Count} metas para o usuário {UsuarioId}", metas.Count, usuarioId);
                return metas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter metas do usuário {UsuarioId}", usuarioId);
                throw;
            }
        }

        public async Task<IEnumerable<Meta>> GetByDateRangeAsync(int usuarioId, DateTime dataInicio, DateTime? dataFim)
        {
            try
            {
                string query = await _sqlLoader.LoadSqlAsync("Metas/GetByDateRange.sql");

                var parameters = new List<MySqlParameter>
        {
            new MySqlParameter("@usuarioId", usuarioId),
            new MySqlParameter("@dataInicio", dataInicio)
        };

                if (dataFim.HasValue)
                {
                    parameters.Add(new MySqlParameter("@dataFim", dataFim.Value));
                }
                else
                {
                    parameters.Add(new MySqlParameter("@dataFim", DateTime.Now));
                }

                var dataTable = await _databaseService.ExecuteQueryAsync(query, parameters.ToArray());
                var metas = new List<Meta>();

                foreach (DataRow row in dataTable.Rows)
                {
                    metas.Add(MapRowToMeta(row));
                }

                _logger.LogInformation("Obtidas {Count} metas para o período de {DataInicio} a {DataFim}",
                    metas.Count, dataInicio, dataFim);
                return metas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter metas por data. Início: {DataInicio}, Fim: {DataFim}",
                    dataInicio, dataFim);
                throw;
            }
        }

        public async Task<IEnumerable<Meta>> GetAllByMateriaIdAsync(int materiaId, int usuarioId)
        {
            try
            {
                string query = await _sqlLoader.LoadSqlAsync("Metas/GetAllByMateriaId.sql");

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@materiaId", materiaId),
                    new MySqlParameter("@usuarioId", usuarioId)
                };

                var dataTable = await _databaseService.ExecuteQueryAsync(query, parameters);
                var metas = new List<Meta>();

                foreach (DataRow row in dataTable.Rows)
                {
                    metas.Add(MapRowToMeta(row));
                }

                _logger.LogInformation("Obtidas {Count} metas para a matéria {MateriaId}", metas.Count, materiaId);
                return metas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter metas da matéria {MateriaId}", materiaId);
                throw;
            }
        }

        public async Task<IEnumerable<Meta>> GetAllByTopicoIdAsync(int topicoId, int usuarioId)
        {
            try
            {
                string query = await _sqlLoader.LoadSqlAsync("Metas/GetAllByTopicoId.sql");

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@topicoId", topicoId),
                    new MySqlParameter("@usuarioId", usuarioId)
                };

                var dataTable = await _databaseService.ExecuteQueryAsync(query, parameters);
                var metas = new List<Meta>();

                foreach (DataRow row in dataTable.Rows)
                {
                    metas.Add(MapRowToMeta(row));
                }

                _logger.LogInformation("Obtidas {Count} metas para o tópico {TopicoId}", metas.Count, topicoId);
                return metas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter metas do tópico {TopicoId}", topicoId);
                throw;
            }
        }

        public async Task<Meta?> GetByIdAsync(int id)
        {
            try
            {
                string query = await _sqlLoader.LoadSqlAsync("Metas/GetById.sql");

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@id", id)
                };

                var dataTable = await _databaseService.ExecuteQueryAsync(query, parameters);

                if (dataTable.Rows.Count == 0)
                {
                    _logger.LogWarning("Meta {Id} não encontrada", id);
                    return null;
                }

                var meta = MapRowToMeta(dataTable.Rows[0]);
                _logger.LogInformation("Meta {Id} obtida com sucesso", id);
                return meta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter meta {Id}", id);
                throw;
            }
        }

        public async Task<bool> BelongsToUsuarioAsync(int metaId, int usuarioId)
        {
            try
            {
                string query = await _sqlLoader.LoadSqlAsync("Metas/BelongsToUsuario.sql");

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@metaId", metaId),
                    new MySqlParameter("@usuarioId", usuarioId)
                };

                var result = await _databaseService.ExecuteScalarAsync(query, parameters);
                return Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar se meta {MetaId} pertence ao usuário {UsuarioId}", metaId, usuarioId);
                throw;
            }
        }

        public async Task<Meta> CreateAsync(Meta meta)
        {
            try
            {
                string query = await _sqlLoader.LoadSqlAsync("Metas/Create.sql");

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@usuarioId", meta.UsuarioId),
                    new MySqlParameter("@materiaId", meta.MateriaId ?? (object)DBNull.Value),
                    new MySqlParameter("@topicoId", meta.TopicoId ?? (object)DBNull.Value),
                    new MySqlParameter("@titulo", meta.Titulo),
                    new MySqlParameter("@descricao", meta.Descricao ?? (object)DBNull.Value),
                    new MySqlParameter("@tipo", meta.Tipo.ToString().ToLower()),
                    new MySqlParameter("@quantidadeTotal", meta.QuantidadeTotal),
                    new MySqlParameter("@quantidadeAtual", meta.QuantidadeAtual),
                    new MySqlParameter("@unidade", meta.Unidade.ToString().ToLower()),
                    new MySqlParameter("@frequencia", meta.Frequencia?.ToString().ToLower() ?? (object)DBNull.Value),
                    new MySqlParameter("@diasSemana", meta.DiasSemana ?? (object)DBNull.Value),
                    new MySqlParameter("@dataInicio", meta.DataInicio),
                    new MySqlParameter("@dataFim", meta.DataFim ?? (object)DBNull.Value),
                    new MySqlParameter("@concluida", meta.Concluida)
                };

                var id = await _databaseService.ExecuteScalarAsync(query, parameters);
                meta.Id = Convert.ToInt32(id);

                var createdMeta = await GetByIdAsync(meta.Id);
                if (createdMeta != null)
                {
                    meta.CriadoEm = createdMeta.CriadoEm;
                    meta.AtualizadoEm = createdMeta.AtualizadoEm;
                }

                _logger.LogInformation("Meta {Id} criada com sucesso", meta.Id);
                return meta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar meta {Titulo}", meta.Titulo);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(Meta meta)
        {
            try
            {
                string query = await _sqlLoader.LoadSqlAsync("Metas/Update.sql");

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@id", meta.Id),
                    new MySqlParameter("@usuarioId", meta.UsuarioId),
                    new MySqlParameter("@materiaId", meta.MateriaId ?? (object)DBNull.Value),
                    new MySqlParameter("@topicoId", meta.TopicoId ?? (object)DBNull.Value),
                    new MySqlParameter("@titulo", meta.Titulo),
                    new MySqlParameter("@descricao", meta.Descricao ?? (object)DBNull.Value),
                    new MySqlParameter("@tipo", meta.Tipo.ToString().ToLower()),
                    new MySqlParameter("@quantidadeTotal", meta.QuantidadeTotal),
                    new MySqlParameter("@quantidadeAtual", meta.QuantidadeAtual),
                    new MySqlParameter("@unidade", meta.Unidade.ToString().ToLower()),
                    new MySqlParameter("@frequencia", meta.Frequencia?.ToString().ToLower() ?? (object)DBNull.Value),
                    new MySqlParameter("@diasSemana", meta.DiasSemana ?? (object)DBNull.Value),
                    new MySqlParameter("@dataInicio", meta.DataInicio),
                    new MySqlParameter("@dataFim", meta.DataFim ?? (object)DBNull.Value),
                    new MySqlParameter("@concluida", meta.Concluida)
                };

                var affectedRows = await _databaseService.ExecuteNonQueryAsync(query, parameters);

                if (affectedRows > 0)
                {
                    _logger.LogInformation("Meta {Id} atualizada com sucesso", meta.Id);
                    return true;
                }

                _logger.LogWarning("Nenhuma alteração realizada para a meta {Id}", meta.Id);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar meta {Id}", meta.Id);
                throw;
            }
        }

        public async Task<bool> UpdateProgressoAsync(int metaId, int quantidadeAtual)
        {
            try
            {
                string query = await _sqlLoader.LoadSqlAsync("Metas/UpdateProgresso.sql");

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@metaId", metaId),
                    new MySqlParameter("@quantidadeAtual", quantidadeAtual)
                };

                var affectedRows = await _databaseService.ExecuteNonQueryAsync(query, parameters);

                if (affectedRows > 0)
                {
                    _logger.LogInformation("Progresso da meta {Id} atualizado com sucesso", metaId);
                    return true;
                }

                _logger.LogWarning("Nenhuma alteração realizada para o progresso da meta {Id}", metaId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar progresso da meta {Id}", metaId);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                string query = await _sqlLoader.LoadSqlAsync("Metas/Delete.sql");

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@id", id)
                };

                var affectedRows = await _databaseService.ExecuteNonQueryAsync(query, parameters);

                if (affectedRows > 0)
                {
                    _logger.LogInformation("Meta {Id} excluída com sucesso", id);
                    return true;
                }

                _logger.LogWarning("Meta {Id} não encontrada para exclusão", id);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir meta {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Meta>> GetActiveAsync(int usuarioId)
        {
            try
            {
                string query = await _sqlLoader.LoadSqlAsync("Metas/GetActiveByUsuarioId.sql");

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@usuarioId", usuarioId)
                };

                var dataTable = await _databaseService.ExecuteQueryAsync(query, parameters);
                var metas = new List<Meta>();

                foreach (DataRow row in dataTable.Rows)
                {
                    metas.Add(MapRowToMeta(row));
                }

                _logger.LogInformation("Obtidas {Count} metas ativas para o usuário {UsuarioId}", metas.Count, usuarioId);
                return metas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter metas ativas do usuário {UsuarioId}", usuarioId);
                throw;
            }
        }

        private Meta MapRowToMeta(DataRow row)
        {
            return new Meta
            {
                Id = Convert.ToInt32(row["id"]),
                UsuarioId = Convert.ToInt32(row["usuario_id"]),
                MateriaId = row["materia_id"] != DBNull.Value ? Convert.ToInt32(row["materia_id"]) : null,
                TopicoId = row["topico_id"] != DBNull.Value ? Convert.ToInt32(row["topico_id"]) : null,
                Titulo = row["titulo"].ToString() ?? string.Empty,
                Descricao = row["descricao"] != DBNull.Value ? row["descricao"].ToString() : null,
                Tipo = Enum.Parse<TipoMeta>(row["tipo"].ToString() ?? "Tempo", true),
                QuantidadeTotal = Convert.ToInt32(row["quantidade_total"]),
                QuantidadeAtual = Convert.ToInt32(row["quantidade_atual"]),
                Unidade = Enum.Parse<UnidadeMeta>(row["unidade"].ToString() ?? "Minutos", true),
                Frequencia = row["frequencia"] != DBNull.Value ? Enum.Parse<FrequenciaMeta>(row["frequencia"].ToString() ?? "Semanal", true) : null,
                DiasSemana = row["dias_semana"] != DBNull.Value ? row["dias_semana"].ToString() : null,
                DataInicio = Convert.ToDateTime(row["data_inicio"]),
                DataFim = row["data_fim"] != DBNull.Value ? Convert.ToDateTime(row["data_fim"]) : null,
                Concluida = Convert.ToBoolean(row["concluida"]),
                CriadoEm = Convert.ToDateTime(row["criado_em"]),
                AtualizadoEm = Convert.ToDateTime(row["atualizado_em"])
            };
        }
    }
}