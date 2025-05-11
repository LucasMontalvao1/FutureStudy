using ERP_API.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ERP_API.Repositorys.Interfaces
{
    public interface IAnotacaoRepository
    {
        /// <summary>
        /// Obtém todas as anotações de um usuário
        /// </summary>
        /// <param name="usuarioId">ID do usuário</param>
        /// <returns>Lista de anotações</returns>
        Task<IEnumerable<Anotacao>> GetAllByUsuarioAsync(int usuarioId);

        /// <summary>
        /// Obtém anotações criadas dentro de um intervalo de datas
        /// </summary>
        /// <param name="usuarioId">ID do usuário</param>
        /// <param name="dataInicio">Data inicial do período</param>
        /// <param name="dataFim">Data final do período (opcional)</param>
        /// <returns>Lista de anotações no período especificado</returns>
        Task<IEnumerable<Anotacao>> GetByDateRangeAsync(int usuarioId, DateTime dataInicio, DateTime? dataFim);

        /// <summary>
        /// Obtém todas as anotações relacionadas a uma sessão
        /// </summary>
        /// <param name="sessaoId">ID da sessão</param>
        /// <param name="usuarioId">ID do usuário</param>
        /// <returns>Lista de anotações</returns>
        Task<IEnumerable<Anotacao>> GetAllBySessaoAsync(int sessaoId, int usuarioId);

        /// <summary>
        /// Obtém uma anotação pelo ID
        /// </summary>
        /// <param name="id">ID da anotação</param>
        /// <param name="usuarioId">ID do usuário</param>
        /// <returns>Anotação encontrada ou null</returns>
        Task<Anotacao?> GetByIdAsync(int id, int usuarioId);

        /// <summary>
        /// Cria uma nova anotação
        /// </summary>
        /// <param name="anotacao">Dados da anotação</param>
        /// <returns>Anotação criada com ID preenchido</returns>
        Task<Anotacao> CreateAsync(Anotacao anotacao);

        /// <summary>
        /// Atualiza uma anotação existente
        /// </summary>
        /// <param name="anotacao">Dados atualizados da anotação</param>
        /// <returns>True se a atualização for bem-sucedida, False caso contrário</returns>
        Task<bool> UpdateAsync(Anotacao anotacao);

        /// <summary>
        /// Exclui uma anotação
        /// </summary>
        /// <param name="id">ID da anotação</param>
        /// <param name="usuarioId">ID do usuário</param>
        /// <returns>True se a exclusão for bem-sucedida, False caso contrário</returns>
        Task<bool> DeleteAsync(int id, int usuarioId);
    }
}