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
    public class SessaoEstudoRepository : ISessaoEstudoRepository
    {
        private readonly IDatabaseService _databaseService;
        private readonly ILogger<SessaoEstudoRepository> _logger;

        public SessaoEstudoRepository(IDatabaseService databaseService, ILogger<SessaoEstudoRepository> logger)
        {
            _databaseService = databaseService;
            _logger = logger;
        }

        public async Task<IEnumerable<SessaoEstudo>> GetAllByPeriodoAsync(int usuarioId, DateTime dataInicio, DateTime dataFim)
        {
            try
            {
                string query = @"
                    SELECT 
                        s.id, s.usuario_id, s.materia_id, s.topico_id, 
                        s.data_inicio, s.data_fim, s.status, 
                        s.criado_em, s.atualizado_em,
                        COALESCE(
                            TIMESTAMPDIFF(SECOND, 
                                s.data_inicio, 
                                IFNULL(s.data_fim, NOW())
                            ) - 
                            IFNULL((
                                SELECT SUM(
                                    TIMESTAMPDIFF(SECOND, 
                                        p.inicio, 
                                        IFNULL(p.fim, NOW())
                                    )
                                )
                                FROM pausas_sessao p
                                WHERE p.sessao_id = s.id
                            ), 0),
                            0
                        ) AS tempo_estudado_segundos
                    FROM sessoes_estudo s
                    WHERE s.usuario_id = @usuarioId 
                      AND s.data_inicio BETWEEN @dataInicio AND @dataFim
                    ORDER BY s.data_inicio DESC";

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

                _logger.LogInformation("Obtidas {Count} sessões para o usuário {UsuarioId} no período de {DataInicio} a {DataFim}",
                    sessoes.Count, usuarioId, dataInicio, dataFim);
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
                string query = @"
                    SELECT 
                        s.id, s.usuario_id, s.materia_id, s.topico_id, 
                        s.data_inicio, s.data_fim, s.status, 
                        s.criado_em, s.atualizado_em,
                        COALESCE(
                            TIMESTAMPDIFF(SECOND, 
                                s.data_inicio, 
                                IFNULL(s.data_fim, NOW())
                            ) - 
                            IFNULL((
                                SELECT SUM(
                                    TIMESTAMPDIFF(SECOND, 
                                        p.inicio, 
                                        IFNULL(p.fim, NOW())
                                    )
                                )
                                FROM pausas_sessao p
                                WHERE p.sessao_id = s.id
                            ), 0),
                            0
                        ) AS tempo_estudado_segundos
                    FROM sessoes_estudo s
                    WHERE s.id = @id AND s.usuario_id = @usuarioId";

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
                string query = @"
                    SELECT 
                        DAY(s.data_inicio) AS dia,
                        SUM(
                            TIMESTAMPDIFF(MINUTE, 
                                s.data_inicio, 
                                IFNULL(s.data_fim, NOW())
                            ) - 
                            IFNULL((
                                SELECT SUM(
                                    TIMESTAMPDIFF(MINUTE, 
                                        p.inicio, 
                                        IFNULL(p.fim, NOW())
                                    )
                                )
                                FROM pausas_sessao p
                                WHERE p.sessao_id = s.id
                            ), 0)
                        ) AS tempo_estudado_minutos
                    FROM sessoes_estudo s
                    WHERE s.usuario_id = @usuarioId
                      AND MONTH(s.data_inicio) = @mes
                      AND YEAR(s.data_inicio) = @ano
                    GROUP BY dia
                    ORDER BY dia";

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
                string query = @"
                    SELECT 
                        SUM(
                            TIMESTAMPDIFF(MINUTE, 
                                s.data_inicio, 
                                IFNULL(s.data_fim, NOW())
                            ) - 
                            IFNULL((
                                SELECT SUM(
                                    TIMESTAMPDIFF(MINUTE, 
                                        p.inicio, 
                                        IFNULL(p.fim, NOW())
                                    )
                                )
                                FROM pausas_sessao p
                                WHERE p.sessao_id = s.id
                            ), 0)
                        ) AS tempo_total_minutos,
                        COUNT(DISTINCT DATE(s.data_inicio)) AS dias_com_estudo,
                        DATEDIFF(@dataFim, @dataInicio) + 1 AS total_dias
                    FROM sessoes_estudo s
                    WHERE s.usuario_id = @usuarioId
                      AND s.data_inicio BETWEEN @dataInicio AND @dataFim";

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
                    MetasAlcancadas = 0,
                    TotalDias = 0,
                    MateriaMaisEstudada = string.Empty,
                    HorasMateriaMaisEstudada = 0
                };

                if (dataTable.Rows.Count > 0)
                {
                    DataRow row = dataTable.Rows[0];
                    int tempoTotalMinutos = Convert.ToInt32(row["tempo_total_minutos"]);
                    stats.TempoTotalEstudado = TimeSpan.FromMinutes(tempoTotalMinutos);
                    stats.MetasAlcancadas = Convert.ToInt32(row["dias_com_estudo"]);
                    stats.TotalDias = Convert.ToInt32(row["total_dias"]);
                }

                // Obter matéria mais estudada
                string materiaQuery = @"
                    SELECT 
                        m.nome AS materia_nome,
                        SUM(
                            TIMESTAMPDIFF(MINUTE, 
                                s.data_inicio, 
                                IFNULL(s.data_fim, NOW())
                            ) - 
                            IFNULL((
                                SELECT SUM(
                                    TIMESTAMPDIFF(MINUTE, 
                                        p.inicio, 
                                        IFNULL(p.fim, NOW())
                                    )
                                )
                                FROM pausas_sessao p
                                WHERE p.sessao_id = s.id
                            ), 0)
                        ) AS tempo_estudado_minutos
                    FROM sessoes_estudo s
                    JOIN materias m ON s.materia_id = m.id
                    WHERE s.usuario_id = @usuarioId
                      AND s.data_inicio BETWEEN @dataInicio AND @dataFim
                    GROUP BY s.materia_id, m.nome
                    ORDER BY tempo_estudado_minutos DESC
                    LIMIT 1";

                var materiaTable = await _databaseService.ExecuteQueryAsync(materiaQuery, parameters);

                if (materiaTable.Rows.Count > 0)
                {
                    DataRow materiaRow = materiaTable.Rows[0];
                    stats.MateriaMaisEstudada = materiaRow["materia_nome"].ToString() ?? string.Empty;
                    stats.HorasMateriaMaisEstudada = Convert.ToInt32(materiaRow["tempo_estudado_minutos"]) / 60;
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
                string query = @"
                    INSERT INTO sessoes_estudo (usuario_id, materia_id, topico_id, data_inicio, status)
                    VALUES (@usuarioId, @materiaId, @topicoId, NOW(), 'em_andamento');
                    SELECT LAST_INSERT_ID();";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@usuarioId", sessao.UsuarioId),
                    new MySqlParameter("@materiaId", sessao.MateriaId),
                    new MySqlParameter("@topicoId", sessao.TopicoId.HasValue ? (object)sessao.TopicoId.Value : DBNull.Value)
                };

                var id = await _databaseService.ExecuteScalarAsync(query, parameters);
                sessao.Id = Convert.ToInt32(id);

                // Buscar a sessão completa para obter as datas geradas pelo banco
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
                string query = @"
                    UPDATE sessoes_estudo
                    SET data_fim = NOW(), status = 'concluida'
                    WHERE id = @id AND usuario_id = @usuarioId AND status = 'em_andamento'";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@id", id),
                    new MySqlParameter("@usuarioId", usuarioId)
                };

                var affectedRows = await _databaseService.ExecuteNonQueryAsync(query, parameters);

                // Finalizar qualquer pausa ativa
                string pausaQuery = @"
                    UPDATE pausas_sessao
                    SET fim = NOW()
                    WHERE sessao_id = @sessaoId AND usuario_id = @usuarioId AND fim IS NULL";

                var pausaParameters = new MySqlParameter[]
                {
                    new MySqlParameter("@sessaoId", id),
                    new MySqlParameter("@usuarioId", usuarioId)
                };

                await _databaseService.ExecuteNonQueryAsync(pausaQuery, pausaParameters);

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
                string query = @"
                    INSERT INTO pausas_sessao (usuario_id, sessao_id, inicio)
                    VALUES (@usuarioId, @sessaoId, NOW());
                    SELECT LAST_INSERT_ID();";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@usuarioId", pausa.UsuarioId),
                    new MySqlParameter("@sessaoId", pausa.SessaoId)
                };

                var id = await _databaseService.ExecuteScalarAsync(query, parameters);
                pausa.Id = Convert.ToInt32(id);

                // Buscar dados completos da pausa
                string selectQuery = @"
                    SELECT id, usuario_id, sessao_id, inicio, fim, criado_em
                    FROM pausas_sessao
                    WHERE id = @id";

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
                string query = @"
                    UPDATE pausas_sessao
                    SET fim = NOW()
                    WHERE id = @id AND usuario_id = @usuarioId AND fim IS NULL";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@id", id),
                    new MySqlParameter("@usuarioId", usuarioId)
                };

                var affectedRows = await _databaseService.ExecuteNonQueryAsync(query, parameters);

                if (affectedRows > 0)
                {
                    _logger.LogInformation("Pausa {Id} finalizada com sucesso", id);
                    return true;
                }

                _logger.LogWarning("Pausa {Id} não encontrada ou já finalizada", id);
                return false;
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
                string query = @"
                    SELECT 
                        TIMESTAMPDIFF(SECOND, 
                            s.data_inicio, 
                            IFNULL(s.data_fim, NOW())
                        ) - 
                        IFNULL((
                            SELECT SUM(
                                TIMESTAMPDIFF(SECOND, 
                                    p.inicio, 
                                    IFNULL(p.fim, NOW())
                                )
                            )
                            FROM pausas_sessao p
                            WHERE p.sessao_id = s.id
                        ), 0) AS tempo_estudado_segundos
                    FROM sessoes_estudo s
                    WHERE s.id = @sessaoId";

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

        private SessaoEstudo MapRowToSessao(DataRow row)
        {
            var sessao = new SessaoEstudo
            {
                Id = Convert.ToInt32(row["id"]),
                UsuarioId = Convert.ToInt32(row["usuario_id"]),
                MateriaId = Convert.ToInt32(row["materia_id"]),
                TopicoId = row["topico_id"] != DBNull.Value ? Convert.ToInt32(row["topico_id"]) : null,
                DataInicio = Convert.ToDateTime(row["data_inicio"]),
                DataFim = row["data_fim"] != DBNull.Value ? Convert.ToDateTime(row["data_fim"]) : null,
                Status = row["status"].ToString() ?? "em_andamento",
                CriadoEm = Convert.ToDateTime(row["criado_em"]),
                AtualizadoEm = Convert.ToDateTime(row["atualizado_em"])
            };

            int segundos = Convert.ToInt32(row["tempo_estudado_segundos"]);
            sessao.TempoEstudado = TimeSpan.FromSeconds(segundos);

            return sessao;
        }
    }
}