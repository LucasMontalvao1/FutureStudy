using ERP_API.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ERP_API.Services.Interfaces
{
    public interface IHistoricoAnotacaoService
    {
        /// <summary>
        /// Obtém todo o histórico de uma anotação
        /// </summary>
        /// <param name="anotacaoId">ID da anotação</param>
        /// <param name="usuarioId">ID do usuário</param>
        /// <returns>Lista de registros de histórico</returns>
        Task<IEnumerable<HistoricoAnotacao>> GetByAnotacaoAsync(int anotacaoId, int usuarioId);

        /// <summary>
        /// Registra uma alteração no histórico
        /// </summary>
        /// <param name="anotacaoId">ID da anotação</param>
        /// <param name="usuarioId">ID do usuário</param>
        /// <param name="conteudoAnterior">Conteúdo antes da alteração</param>
        /// <returns>Registro de histórico criado</returns>
        Task<HistoricoAnotacao> RegistrarAlteracaoAsync(int anotacaoId, int usuarioId, string conteudoAnterior);
    }
}