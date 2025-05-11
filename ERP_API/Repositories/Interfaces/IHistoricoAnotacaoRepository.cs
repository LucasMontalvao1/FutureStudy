using ERP_API.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ERP_API.Repositorys.Interfaces
{
    public interface IHistoricoAnotacaoRepository
    {
        /// <summary>
        /// Obtém todo o histórico de uma anotação
        /// </summary>
        /// <param name="anotacaoId">ID da anotação</param>
        /// <param name="usuarioId">ID do usuário</param>
        /// <returns>Lista de registros de histórico</returns>
        Task<IEnumerable<HistoricoAnotacao>> GetByAnotacaoAsync(int anotacaoId, int usuarioId);

        /// <summary>
        /// Adiciona um novo registro de histórico
        /// </summary>
        /// <param name="historico">Dados do histórico</param>
        /// <returns>Registro de histórico criado</returns>
        Task<HistoricoAnotacao> CreateAsync(HistoricoAnotacao historico);
    }
}