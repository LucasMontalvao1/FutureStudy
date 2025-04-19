using ERP_API.Infra.Data;
using ERP_API.Models.Entities;
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
    public class AnotacaoRepository : IAnotacaoRepository
    {
        private readonly IDatabaseService _databaseService;
        private readonly ILogger<AnotacaoRepository> _logger;
        private readonly SqlLoader _sqlLoader;

        public AnotacaoRepository(
            IDatabaseService databaseService,
            ILogger<AnotacaoRepository> logger,
            SqlLoader sqlLoader)
        {
            _databaseService = databaseService;
            _logger = logger;
            _sqlLoader = sqlLoader;
        }

        public async Task<IEnumerable<Anotacao>> GetAllByUsuarioAsync(int usuarioId)
        {
            try
            {
                string query = await _sqlLoader.LoadSqlAsync("Anotacoes/GetAllByUsuario.sql");

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@usuarioId", usuarioId)
                };

                var dataTable = await _databaseService.ExecuteQueryAsync(query, parameters);
                var anotacoes = new List<Anotacao>();

                foreach (DataRow row in dataTable.Rows)
                {
                    anotacoes.Add(MapRowToAnotacao(row));
                }

                _logger.LogInformation("Obtidas {Count} anotações para o usuário {UsuarioId}",
                    anotacoes.Count, usuarioId);

                return anotacoes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter anotações do usuário {UsuarioId}", usuarioId);
                throw;
            }
        }

        public async Task<IEnumerable<Anotacao>> GetByDateRangeAsync(int usuarioId, DateTime dataInicio, DateTime? dataFim)
        {
            try
            {
                string query = await _sqlLoader.LoadSqlAsync("Anotacoes/GetByDateRange.sql");

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
                var anotacoes = new List<Anotacao>();

                foreach (DataRow row in dataTable.Rows)
                {
                    anotacoes.Add(MapRowToAnotacao(row));
                }

                _logger.LogInformation("Obtidas {Count} anotações para o período de {DataInicio} a {DataFim}",
                    anotacoes.Count, dataInicio, dataFim);

                return anotacoes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter anotações por data. Início: {DataInicio}, Fim: {DataFim}",
                    dataInicio, dataFim);
                throw;
            }
        }
        public async Task<IEnumerable<Anotacao>> GetAllBySessaoAsync(int sessaoId, int usuarioId)
        {
            try
            {
                string query = await _sqlLoader.LoadSqlAsync("Anotacoes/GetAllBySessao.sql");

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@sessaoId", sessaoId),
                    new MySqlParameter("@usuarioId", usuarioId)
                };

                var dataTable = await _databaseService.ExecuteQueryAsync(query, parameters);
                var anotacoes = new List<Anotacao>();

                foreach (DataRow row in dataTable.Rows)
                {
                    anotacoes.Add(MapRowToAnotacao(row));
                }

                _logger.LogInformation("Obtidas {Count} anotações para a sessão {SessaoId}",
                    anotacoes.Count, sessaoId);

                return anotacoes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter anotações da sessão {SessaoId}", sessaoId);
                throw;
            }
        }

        public async Task<Anotacao?> GetByIdAsync(int id, int usuarioId)
        {
            try
            {
                string query = await _sqlLoader.LoadSqlAsync("Anotacoes/GetById.sql");

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@id", id),
                    new MySqlParameter("@usuarioId", usuarioId)
                };

                var dataTable = await _databaseService.ExecuteQueryAsync(query, parameters);

                if (dataTable.Rows.Count == 0)
                {
                    _logger.LogWarning("Anotação {Id} não encontrada para o usuário {UsuarioId}", id, usuarioId);
                    return null;
                }

                var anotacao = MapRowToAnotacao(dataTable.Rows[0]);
                _logger.LogInformation("Anotação {Id} obtida com sucesso", id);
                return anotacao;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter anotação {Id}", id);
                throw;
            }
        }

        public async Task<Anotacao> CreateAsync(Anotacao anotacao)
        {
            try
            {
                string query = await _sqlLoader.LoadSqlAsync("Anotacoes/Create.sql");

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@usuarioId", anotacao.UsuarioId),
                    new MySqlParameter("@sessaoId", anotacao.SessaoId),
                    new MySqlParameter("@titulo", anotacao.Titulo),
                    new MySqlParameter("@conteudo", anotacao.Conteudo)
                };

                var id = await _databaseService.ExecuteScalarAsync(query, parameters);
                anotacao.Id = Convert.ToInt32(id);

                var createdAnotacao = await GetByIdAsync(anotacao.Id, anotacao.UsuarioId);
                if (createdAnotacao != null)
                {
                    anotacao.CriadoEm = createdAnotacao.CriadoEm;
                    anotacao.AtualizadoEm = createdAnotacao.AtualizadoEm;
                }

                _logger.LogInformation("Anotação {Id} criada com sucesso", anotacao.Id);
                return anotacao;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar anotação para a sessão {SessaoId}", anotacao.SessaoId);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(Anotacao anotacao)
        {
            try
            {
                string query = await _sqlLoader.LoadSqlAsync("Anotacoes/Update.sql");

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@id", anotacao.Id),
                    new MySqlParameter("@usuarioId", anotacao.UsuarioId),
                    new MySqlParameter("@titulo", anotacao.Titulo),
                    new MySqlParameter("@conteudo", anotacao.Conteudo)
                };

                var affectedRows = await _databaseService.ExecuteNonQueryAsync(query, parameters);

                if (affectedRows > 0)
                {
                    _logger.LogInformation("Anotação {Id} atualizada com sucesso", anotacao.Id);
                    return true;
                }

                _logger.LogWarning("Anotação {Id} não encontrada ou não pertence ao usuário {UsuarioId}",
                    anotacao.Id, anotacao.UsuarioId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar anotação {Id}", anotacao.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id, int usuarioId)
        {
            try
            {
                string query = await _sqlLoader.LoadSqlAsync("Anotacoes/Delete.sql");

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@id", id),
                    new MySqlParameter("@usuarioId", usuarioId)
                };

                var affectedRows = await _databaseService.ExecuteNonQueryAsync(query, parameters);

                if (affectedRows > 0)
                {
                    _logger.LogInformation("Anotação {Id} excluída com sucesso", id);
                    return true;
                }

                _logger.LogWarning("Anotação {Id} não encontrada ou não pertence ao usuário {UsuarioId}", id, usuarioId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir anotação {Id}", id);
                throw;
            }
        }

        private Anotacao MapRowToAnotacao(DataRow row)
        {
            return new Anotacao
            {
                Id = Convert.ToInt32(row["id"]),
                UsuarioId = Convert.ToInt32(row["usuario_id"]),
                SessaoId = Convert.ToInt32(row["sessao_id"]),
                Titulo = row["titulo"].ToString() ?? string.Empty,
                Conteudo = row["conteudo"].ToString() ?? string.Empty,
                CriadoEm = Convert.ToDateTime(row["criado_em"]),
                AtualizadoEm = Convert.ToDateTime(row["atualizado_em"])
            };
        }
    }
}