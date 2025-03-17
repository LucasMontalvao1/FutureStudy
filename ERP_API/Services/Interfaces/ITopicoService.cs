using ERP_API.Models;
using ERP_API.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ERP_API.Services.Interfaces
{
    public interface ITopicoService
    {
        /// <summary>
        /// Obtém todos os tópicos de um usuário
        /// </summary>
        /// <param name="usuarioId">ID do usuário</param>
        /// <returns>Lista de tópicos</returns>
        Task<IEnumerable<Topico>> GetAllByUsuarioIdAsync(int usuarioId);

        /// <summary>
        /// Obtém todos os tópicos de uma matéria
        /// </summary>
        /// <param name="materiaId">ID da matéria</param>
        /// <param name="usuarioId">ID do usuário</param>
        /// <returns>Lista de tópicos</returns>
        Task<IEnumerable<Topico>> GetAllByMateriaIdAsync(int materiaId, int usuarioId);

        /// <summary>
        /// Obtém um tópico pelo ID
        /// </summary>
        /// <param name="id">ID do tópico</param>
        /// <param name="usuarioId">ID do usuário</param>
        /// <returns>Tópico encontrado ou null</returns>
        Task<Topico?> GetByIdAsync(int id, int usuarioId);

        /// <summary>
        /// Cria um novo tópico
        /// </summary>
        /// <param name="dto">Dados do tópico</param>
        /// <param name="usuarioId">ID do usuário</param>
        /// <returns>Tópico criado</returns>
        Task<Topico> CreateAsync(TopicoRequestDto dto, int usuarioId);

        /// <summary>
        /// Atualiza um tópico existente
        /// </summary>
        /// <param name="id">ID do tópico</param>
        /// <param name="dto">Dados atualizados do tópico</param>
        /// <param name="usuarioId">ID do usuário</param>
        /// <returns>Tópico atualizado ou null se não encontrado</returns>
        Task<Topico?> UpdateAsync(int id, TopicoUpdateRequestDto dto, int usuarioId);

        /// <summary>
        /// Exclui um tópico
        /// </summary>
        /// <param name="id">ID do tópico</param>
        /// <param name="usuarioId">ID do usuário</param>
        /// <returns>True se a exclusão for bem-sucedida, False caso contrário</returns>
        Task<bool> DeleteAsync(int id, int usuarioId);
    }
}