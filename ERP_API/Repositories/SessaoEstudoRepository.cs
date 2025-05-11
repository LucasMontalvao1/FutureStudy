using ERP_API.Infra.Data;
using ERP_API.Models;
using ERP_API.Models.Entities;
using ERP_API.Models.Enums;
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
    public class SessaoEstudoRepository : ISessaoEstudoRepository
    {
        private readonly IDatabaseService _databaseService;
        private readonly ILogger<SessaoEstudoRepository> _logger;
        private readonly SqlLoader _sqlLoader;

        public SessaoEstudoRepository(
            IDatabaseService databaseService,
            ILogger<SessaoEstudoRepository> logger,
            SqlLoader sqlLoader)
        {
            _databaseService = databaseService;
            _logger = logger;
            _sqlLoader = sqlLoader;
        }

        private string ConvertStatusToDbValue(StatusSessao status)
        {
            return status switch
            {
                StatusSessao.EmAndamento => "em_andamento",
                StatusSessao.Pausada => "pausada",
                StatusSessao.Concluida => "concluida",
                _ => throw new ArgumentException($"Status inválido: {status}")
            };
        }

        public async Task<IEnumerable<SessaoEstudo>> GetAllByPeriodoAsync(int usuarioId, DateTime dataInicio, DateTime dataFim)
        {
            try
            {
                string query = await _sqlLoader.LoadSqlAsync("SessoesEstudo/GetAllByPeriodo.sql");

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@usuarioId", usuarioId),
                    new MySqlParameter("@dataInicio", dataInicio),
                    new MySqlParameter("@dataFim", dataFim)
                };

                var dataTable = await _databaseService.ExecuteQueryAsync(query, parameters);
                var sessoes = new List<SessaoEstudo>();

                foreach (DataRow row in dataTable.Rows)
                {
                    sessoes.Add(MapRowToSessao(row));
                }

                _logger.LogInformation("Obtidas {Count} sessões para o usuário {UsuarioId} no período",
                    sessoes.Count, usuarioId);

                return sessoes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter sessões do usuário {UsuarioId} no período", usuarioId);
                throw;
            }
        }

        public async Task<SessaoEstudo?> GetByIdAsync(int id, int usuarioId)
        {
            try
            {
                string query = await _sqlLoader.LoadSqlAsync("SessoesEstudo/GetById.sql");

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@id", id),
                    new MySqlParameter("@usuarioId", usuarioId)
                };

                var dataTable = await _databaseService.ExecuteQueryAsync(query, parameters);

                if (dataTable.Rows.Count == 0)
                {
                    _logger.LogWarning("Sessão {Id} não encontrada para o usuário {UsuarioId}", id, usuarioId);
                    return null;
                }

                var sessao = MapRowToSessao(dataTable.Rows[0]);
                _logger.LogInformation("Sessão {Id} obtida com sucesso", id);
                return sessao;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter sessão {Id}", id);
                throw;
            }
        }

        public async Task<Dictionary<int, int>> GetTempoEstudadoPorDiaAsync(int usuarioId, int mes, int ano)
        {
            try
            {
                string query = await _sqlLoader.LoadSqlAsync("SessoesEstudo/GetTempoEstudadoPorDia.sql");

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@usuarioId", usuarioId),
                    new MySqlParameter("@mes", mes),
                    new MySqlParameter("@ano", ano)
                };

                var dataTable = await _databaseService.ExecuteQueryAsync(query, parameters);
                var resultados = new Dictionary<int, int>();

                foreach (DataRow row in dataTable.Rows)
                {
                    int dia = Convert.ToInt32(row["dia"]);
                    int minutos = Convert.ToInt32(row["tempo_estudado_minutos"]);
                    resultados[dia] = minutos;
                }

                _logger.LogInformation("Obtidos tempos de estudo por dia para o usuário {UsuarioId} em {Mes}/{Ano}",
                    usuarioId, mes, ano);

                return resultados;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter tempos de estudo por dia para o usuário {UsuarioId}", usuarioId);
                throw;
            }
        }

        public async Task<DashboardStats> GetDashboardStatsAsync(int usuarioId, DateTime dataInicio, DateTime dataFim)
        {
            try
            {
                string query = await _sqlLoader.LoadSqlAsync("SessoesEstudo/GetDashboardStats.sql");

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@usuarioId", usuarioId),
                    new MySqlParameter("@dataInicio", dataInicio),
                    new MySqlParameter("@dataFim", dataFim)
                };

                var dataTable = await _databaseService.ExecuteQueryAsync(query, parameters);

                var stats = new DashboardStats
                {
                    TempoTotalEstudado = TimeSpan.Zero,
                    DiasEstudados = 0,
                    TotalDias = 0,
                    MateriaMaisEstudada = string.Empty,
                    HorasMateriaMaisEstudada = 0
                };

                if (dataTable.Rows.Count > 0)
                {
                    DataRow row = dataTable.Rows[0];
                    int tempoTotalMinutos = row["tempo_total_minutos"] != DBNull.Value
                        ? Convert.ToInt32(row["tempo_total_minutos"])
                        : 0;

                    stats.TempoTotalEstudado = TimeSpan.FromMinutes(tempoTotalMinutos);
                    stats.DiasEstudados = Convert.ToInt32(row["dias_com_estudo"]);
                    stats.TotalDias = Convert.ToInt32(row["total_dias"]);
                }

                string materiaQuery = await _sqlLoader.LoadSqlAsync("SessoesEstudo/GetMateriaMaisEstudada.sql");
                var materiaTable = await _databaseService.ExecuteQueryAsync(materiaQuery, parameters);

                if (materiaTable.Rows.Count > 0)
                {
                    DataRow materiaRow = materiaTable.Rows[0];
                    stats.MateriaMaisEstudada = materiaRow["materia_nome"].ToString() ?? string.Empty;

                    int minutosMateria = materiaRow["tempo_estudado_minutos"] != DBNull.Value
                        ? Convert.ToInt32(materiaRow["tempo_estudado_minutos"])
                        : 0;

                    stats.HorasMateriaMaisEstudada = minutosMateria / 60.0;
                }

                _logger.LogInformation("Obtidas estatísticas do dashboard para o usuário {UsuarioId}", usuarioId);
                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter estatísticas do dashboard para o usuário {UsuarioId}", usuarioId);
                throw;
            }
        }

        public async Task<SessaoEstudo> CreateSessaoAsync(SessaoEstudo sessao)
        {
            try
            {
                string query = await _sqlLoader.LoadSqlAsync("SessoesEstudo/CreateSessao.sql");

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@usuarioId", sessao.UsuarioId),
                    new MySqlParameter("@materiaId", sessao.MateriaId),
                    new MySqlParameter("@categoriaId", sessao.CategoriaId),
                    new MySqlParameter("@topicoId", sessao.TopicoId.HasValue ? (object)sessao.TopicoId.Value : DBNull.Value),
                    new MySqlParameter("@status", ConvertStatusToDbValue(sessao.Status))
                };

                var id = await _databaseService.ExecuteScalarAsync(query, parameters);
                sessao.Id = Convert.ToInt32(id);

                var createdSessao = await GetByIdAsync(sessao.Id, sessao.UsuarioId);
                if (createdSessao != null)
                {
                    sessao.DataInicio = createdSessao.DataInicio;
                    sessao.DataFim = createdSessao.DataFim;
                    sessao.Status = createdSessao.Status;
                    sessao.CriadoEm = createdSessao.CriadoEm;
                    sessao.AtualizadoEm = createdSessao.AtualizadoEm;
                    sessao.TempoEstudado = createdSessao.TempoEstudado;
                }

                _logger.LogInformation("Sessão {Id} criada com sucesso", sessao.Id);
                return sessao;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar sessão para a matéria {MateriaId}", sessao.MateriaId);
                throw;
            }
        }

        public async Task<bool> FinalizarSessaoAsync(int id, int usuarioId)
        {
            try
            {
                string pausaQuery = await _sqlLoader.LoadSqlAsync("SessoesEstudo/FinalizarPausasAtivas.sql");
                var pausaParameters = new MySqlParameter[]
                {
                    new MySqlParameter("@sessaoId", id),
                    new MySqlParameter("@usuarioId", usuarioId)
                };

                await _databaseService.ExecuteNonQueryAsync(pausaQuery, pausaParameters);
                await AtualizarTempoEstudadoAsync(id);

                string query = await _sqlLoader.LoadSqlAsync("SessoesEstudo/FinalizarSessao.sql");
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@id", id),
                    new MySqlParameter("@usuarioId", usuarioId),
                    new MySqlParameter("@status", ConvertStatusToDbValue(StatusSessao.Concluida))
                };

                var affectedRows = await _databaseService.ExecuteNonQueryAsync(query, parameters);             

                if (affectedRows > 0)
                {
                    _logger.LogInformation("Sessão {Id} finalizada com sucesso", id);
                    return true;
                }

                _logger.LogWarning("Sessão {Id} não encontrada ou já finalizada", id);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao finalizar sessão {Id}", id);
                throw;
            }
        }

        public async Task<PausaSessao> CreatePausaAsync(PausaSessao pausa)
        {
            try
            {
                pausa.Inicio = DateTime.Now;

                string statusQuery = await _sqlLoader.LoadSqlAsync("SessoesEstudo/UpdateStatus.sql");
                var statusParameters = new MySqlParameter[]
                {
                    new MySqlParameter("@id", pausa.SessaoId),
                    new MySqlParameter("@usuarioId", pausa.UsuarioId),
                    new MySqlParameter("@status", ConvertStatusToDbValue(StatusSessao.Pausada))
                };

                await _databaseService.ExecuteNonQueryAsync(statusQuery, statusParameters);

                string query = await _sqlLoader.LoadSqlAsync("SessoesEstudo/CreatePausa.sql");
                var parameters = new MySqlParameter[]
                {
            new MySqlParameter("@usuarioId", pausa.UsuarioId),
            new MySqlParameter("@sessaoId", pausa.SessaoId)
                };

                var id = await _databaseService.ExecuteScalarAsync(query, parameters);
                pausa.Id = Convert.ToInt32(id);

                string selectQuery = await _sqlLoader.LoadSqlAsync("SessoesEstudo/GetPausaById.sql");
                var selectParameters = new MySqlParameter[]
                {
                    new MySqlParameter("@id", pausa.Id)
                };

                var dataTable = await _databaseService.ExecuteQueryAsync(selectQuery, selectParameters);

                if (dataTable.Rows.Count > 0)
                {
                    DataRow row = dataTable.Rows[0];
                    pausa.Inicio = Convert.ToDateTime(row["inicio"]);
                    pausa.Fim = row["fim"] != DBNull.Value ? Convert.ToDateTime(row["fim"]) : null;
                    pausa.CriadoEm = Convert.ToDateTime(row["criado_em"]);
                }

                _logger.LogInformation("Pausa {Id} criada com sucesso para a sessão {SessaoId}", pausa.Id, pausa.SessaoId);
                return pausa;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar pausa para a sessão {SessaoId}", pausa.SessaoId);
                throw;
            }
        }

        public async Task<bool> FinalizarPausaAsync(int id, int usuarioId)
        {
            try
            {
                string query = await _sqlLoader.LoadSqlAsync("SessoesEstudo/FinalizarPausa.sql");
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@id", id),
                    new MySqlParameter("@usuarioId", usuarioId)
                };

                var affectedRows = await _databaseService.ExecuteNonQueryAsync(query, parameters);

                if (affectedRows <= 0)
                {
                    _logger.LogWarning("Pausa {Id} não encontrada ou já finalizada", id);
                    return false;
                }

                string getPausaQuery = await _sqlLoader.LoadSqlAsync("SessoesEstudo/GetPausaById.sql");
                var getPausaParams = new MySqlParameter[]
                {
                    new MySqlParameter("@id", id)
                };

                var dataTable = await _databaseService.ExecuteQueryAsync(getPausaQuery, getPausaParams);
                if (dataTable.Rows.Count == 0)
                {
                    return false;
                }

                int sessaoId = Convert.ToInt32(dataTable.Rows[0]["sessao_id"]);

                string statusQuery = await _sqlLoader.LoadSqlAsync("SessoesEstudo/UpdateStatus.sql");
                var statusParameters = new MySqlParameter[]
                {
                    new MySqlParameter("@id", sessaoId),
                    new MySqlParameter("@usuarioId", usuarioId),
                    new MySqlParameter("@status", ConvertStatusToDbValue(StatusSessao.EmAndamento))
                };

                await _databaseService.ExecuteNonQueryAsync(statusQuery, statusParameters);

                _logger.LogInformation("Pausa {Id} finalizada com sucesso", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao finalizar pausa {Id}", id);
                throw;
            }
        }

        public async Task<int> CalcularTempoEstudadoAsync(int sessaoId)
        {
            try
            {
                string query = await _sqlLoader.LoadSqlAsync("SessoesEstudo/CalcularTempoEstudado.sql");
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@sessaoId", sessaoId)
                };

                var result = await _databaseService.ExecuteScalarAsync(query, parameters);
                return Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao calcular tempo estudado da sessão {SessaoId}", sessaoId);
                throw;
            }
        }

        public async Task AtualizarTempoEstudadoAsync(int sessaoId)
        {
            int tempoEstudado = await CalcularTempoEstudadoAsync(sessaoId);

            string updateSql = @"UPDATE sessoes_estudo
                         SET tempo_estudado_segundos = @tempoEstudado
                         WHERE id = @sessaoId";

            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@tempoEstudado", tempoEstudado),
                new MySqlParameter("@sessaoId", sessaoId)
            };

            await _databaseService.ExecuteNonQueryAsync(updateSql, parameters);
        }

        private SessaoEstudo MapRowToSessao(DataRow row)
        {
            var sessao = new SessaoEstudo
            {
                Id = Convert.ToInt32(row["id"]),
                UsuarioId = Convert.ToInt32(row["usuario_id"]),
                MateriaId = Convert.ToInt32(row["materia_id"]),
                NomeMateria = row["nome_materia"]?.ToString(),
                TopicoId = row["topico_id"] != DBNull.Value ? Convert.ToInt32(row["topico_id"]) : null,
                NomeTopico = row["nome_topico"]?.ToString(),
                CategoriaId = row["categoria_id"] != DBNull.Value ? Convert.ToInt32(row["categoria_id"]) : null,
                NomeCategoria = row["nome_categoria"]?.ToString(),
                DataInicio = Convert.ToDateTime(row["data_inicio"]),
                DataFim = row["data_fim"] != DBNull.Value ? Convert.ToDateTime(row["data_fim"]) : null,
                Status = ConvertDbValueToStatus(row["status"].ToString() ?? "em_andamento"),
                CriadoEm = Convert.ToDateTime(row["criado_em"]),
                AtualizadoEm = Convert.ToDateTime(row["atualizado_em"])
            };

            int segundos = Convert.ToInt32(row["tempo_estudado_segundos"]);
            sessao.TempoEstudado = TimeSpan.FromSeconds(segundos);

            return sessao;
        }

        private StatusSessao ConvertDbValueToStatus(string dbValue)
        {
            return dbValue.ToLower() switch
            {
                "em_andamento" => StatusSessao.EmAndamento,
                "pausada" => StatusSessao.Pausada,
                "concluida" => StatusSessao.Concluida,
                _ => StatusSessao.EmAndamento 
            };
        }
    }
}