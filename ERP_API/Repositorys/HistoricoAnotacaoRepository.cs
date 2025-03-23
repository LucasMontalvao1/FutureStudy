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
    public class HistoricoAnotacaoRepository : IHistoricoAnotacaoRepository
    {
        private readonly IDatabaseService _databaseService;
        private readonly ILogger<HistoricoAnotacaoRepository> _logger;
        private readonly SqlLoader _sqlLoader;

        public HistoricoAnotacaoRepository(
            IDatabaseService databaseService,
            ILogger<HistoricoAnotacaoRepository> logger,
            SqlLoader sqlLoader)
        {
            _databaseService = databaseService;
            _logger = logger;
            _sqlLoader = sqlLoader;
        }

        public async Task<IEnumerable<HistoricoAnotacao>> GetByAnotacaoAsync(int anotacaoId, int usuarioId)
        {
            try
            {
                string query = await _sqlLoader.LoadSqlAsync("HistoricoAnotacoes/GetByAnotacao.sql");

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@anotacaoId", anotacaoId),
                    new MySqlParameter("@usuarioId", usuarioId)
                };

                var dataTable = await _databaseService.ExecuteQueryAsync(query, parameters);
                var historicos = new List<HistoricoAnotacao>();

                foreach (DataRow row in dataTable.Rows)
                {
                    historicos.Add(MapRowToHistorico(row));
                }

                _logger.LogInformation("Obtidos {Count} registros de histórico para a anotação {AnotacaoId}",
                    historicos.Count, anotacaoId);

                return historicos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter histórico da anotação {AnotacaoId}", anotacaoId);
                throw;
            }
        }

        public async Task<HistoricoAnotacao> CreateAsync(HistoricoAnotacao historico)
        {
            try
            {
                string query = await _sqlLoader.LoadSqlAsync("HistoricoAnotacoes/Create.sql");

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@usuarioId", historico.UsuarioId),
                    new MySqlParameter("@anotacaoId", historico.AnotacaoId),
                    new MySqlParameter("@conteudoAnterior", historico.ConteudoAnterior)
                };

                var id = await _databaseService.ExecuteScalarAsync(query, parameters);
                historico.Id = Convert.ToInt32(id);

                // Buscar o registro completo para obter a data gerada pelo banco
                string selectQuery = await _sqlLoader.LoadSqlAsync("HistoricoAnotacoes/GetById.sql");
                var selectParameters = new MySqlParameter[]
                {
                    new MySqlParameter("@id", historico.Id)
                };

                var dataTable = await _databaseService.ExecuteQueryAsync(selectQuery, selectParameters);

                if (dataTable.Rows.Count > 0)
                {
                    historico.EditadoEm = Convert.ToDateTime(dataTable.Rows[0]["editado_em"]);
                }

                _logger.LogInformation("Histórico {Id} criado com sucesso para a anotação {AnotacaoId}",
                    historico.Id, historico.AnotacaoId);
                return historico;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar histórico para a anotação {AnotacaoId}", historico.AnotacaoId);
                throw;
            }
        }

        private HistoricoAnotacao MapRowToHistorico(DataRow row)
        {
            return new HistoricoAnotacao
            {
                Id = Convert.ToInt32(row["id"]),
                UsuarioId = Convert.ToInt32(row["usuario_id"]),
                AnotacaoId = Convert.ToInt32(row["anotacao_id"]),
                ConteudoAnterior = row["conteudo_anterior"].ToString() ?? string.Empty,
                EditadoEm = Convert.ToDateTime(row["editado_em"])
            };
        }
    }
}