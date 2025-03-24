using ERP_API.Models;
using ERP_API.Models.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ERP_API.Repositories.Interfaces
{
    public interface IMetaRepository
    {
        /// <summary>
        /// Obtém todas as metas de um usuário
        /// </summary>
        /// <param name="usuarioId">ID do usuário</param>
        /// <returns>Lista de metas</returns>
        Task<IEnumerable<Meta>> GetAllByUsuarioIdAsync(int usuarioId);

        /// <summary>
        /// Obtém todas as metas de uma matéria
        /// </summary>
        /// <param name="materiaId">ID da matéria</param>
        /// <param name="usuarioId">ID do usuário</param>
        /// <returns>Lista de metas</returns>
        Task<IEnumerable<Meta>> GetAllByMateriaIdAsync(int materiaId, int usuarioId);

        Task<IEnumerable<Meta>> GetByDateRangeAsync(int usuarioId, DateTime dataInicio, DateTime? dataFim);

        /// <summary>
        /// Obtém todas as metas de um tópico
        /// </summary>
        /// <param name="topicoId">ID do tópico</param>
        /// <param name="usuarioId">ID do usuário</param>
        /// <returns>Lista de metas</returns>
        Task<IEnumerable<Meta>> GetAllByTopicoIdAsync(int topicoId, int usuarioId);

        /// <summary>
        /// Obtém uma meta pelo ID
        /// </summary>
        /// <param name="id">ID da meta</param>
        /// <returns>Meta encontrada ou null</returns>
        Task<Meta?> GetByIdAsync(int id);

        /// <summary>
        /// Verifica se uma meta pertence a um usuário específico
        /// </summary>
        /// <param name="metaId">ID da meta</param>
        /// <param name="usuarioId">ID do usuário</param>
        /// <returns>True se a meta pertencer ao usuário, False caso contrário</returns>
        Task<bool> BelongsToUsuarioAsync(int metaId, int usuarioId);

        /// <summary>
        /// Cria uma nova meta
        /// </summary>
        /// <param name="meta">Dados da meta</param>
        /// <returns>Meta criada com ID preenchido</returns>
        Task<Meta> CreateAsync(Meta meta);

        /// <summary>
        /// Atualiza uma meta existente
        /// </summary>
        /// <param name="meta">Dados atualizados da meta</param>
        /// <returns>True se a atualização for bem-sucedida, False caso contrário</returns>
        Task<bool> UpdateAsync(Meta meta);

        /// <summary>
        /// Exclui uma meta
        /// </summary>
        /// <param name="id">ID da meta</param>
        /// <returns>True se a exclusão for bem-sucedida, False caso contrário</returns>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Atualiza o progresso de uma meta
        /// </summary>
        /// <param name="metaId">ID da meta</param>
        /// <param name="quantidadeAtual">Nova quantidade atual</param>
        /// <returns>True se a atualização for bem-sucedida, False caso contrário</returns>
        Task<bool> UpdateProgressoAsync(int metaId, int quantidadeAtual);

        /// <summary>
        /// Obtém metas ativas (não concluídas e dentro do prazo)
        /// </summary>
        /// <param name="usuarioId">ID do usuário</param>
        /// <returns>Lista de metas ativas</returns>
        Task<IEnumerable<Meta>> GetActiveAsync(int usuarioId);
    }
}