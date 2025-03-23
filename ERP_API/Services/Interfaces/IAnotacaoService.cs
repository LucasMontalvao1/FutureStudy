using ERP_API.Models.DTOs;
using ERP_API.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ERP_API.Services.Interfaces
{
    public interface IAnotacaoService
    {
        /// <summary>
        /// Obtém todas as anotações de um usuário
        /// </summary>
        /// <param name="usuarioId">ID do usuário</param>
        /// <returns>Lista de anotações</returns>
        Task<IEnumerable<Anotacao>> GetAllByUsuarioAsync(int usuarioId);

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
        /// <param name="dto">Dados da anotação</param>
        /// <param name="usuarioId">ID do usuário</param>
        /// <returns>Anotação criada</returns>
        Task<Anotacao> CreateAsync(AnotacaoRequestDto dto, int usuarioId);

        /// <summary>
        /// Atualiza uma anotação existente
        /// </summary>
        /// <param name="id">ID da anotação</param>
        /// <param name="dto">Dados atualizados da anotação</param>
        /// <param name="usuarioId">ID do usuário</param>
        /// <returns>True se a atualização for bem-sucedida, False caso contrário</returns>
        Task<bool> UpdateAsync(int id, AnotacaoUpdateDto dto, int usuarioId);

        /// <summary>
        /// Exclui uma anotação
        /// </summary>
        /// <param name="id">ID da anotação</param>
        /// <param name="usuarioId">ID do usuário</param>
        /// <returns>True se a exclusão for bem-sucedida, False caso contrário</returns>
        Task<bool> DeleteAsync(int id, int usuarioId);
    }
}