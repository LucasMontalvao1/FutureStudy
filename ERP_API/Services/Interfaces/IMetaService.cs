using ERP_API.Models.DTOs;
using ERP_API.Models.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ERP_API.Services.Interfaces
{
    public interface IMetaService
    {
        /// <summary>
        /// Obtém todas as metas de um usuário
        /// </summary>
        /// <param name="usuarioId">ID do usuário</param>
        /// <returns>Lista de DTOs de metas</returns>
        Task<IEnumerable<MetaResponseDto>> GetAllByUsuarioIdAsync(int usuarioId);

        /// <summary>
        /// Obtém todas as metas de uma matéria
        /// </summary>
        /// <param name="materiaId">ID da matéria</param>
        /// <param name="usuarioId">ID do usuário</param>
        /// <returns>Lista de DTOs de metas</returns>
        Task<IEnumerable<MetaResponseDto>> GetAllByMateriaIdAsync(int materiaId, int usuarioId);

        Task<IEnumerable<MetaResponseDto>> GetByDateRangeAsync(int usuarioId, DateTime dataInicio, DateTime? dataFim);

        /// <summary>
        /// Obtém todas as metas de um tópico
        /// </summary>
        /// <param name="topicoId">ID do tópico</param>
        /// <param name="usuarioId">ID do usuário</param>
        /// <returns>Lista de DTOs de metas</returns>
        Task<IEnumerable<MetaResponseDto>> GetAllByTopicoIdAsync(int topicoId, int usuarioId);

        /// <summary>
        /// Obtém uma meta pelo ID
        /// </summary>
        /// <param name="id">ID da meta</param>
        /// <param name="usuarioId">ID do usuário</param>
        /// <returns>DTO da meta encontrada ou null</returns>
        Task<MetaResponseDto?> GetByIdAsync(int id, int usuarioId);

        /// <summary>
        /// Cria uma nova meta
        /// </summary>
        /// <param name="dto">Dados da meta</param>
        /// <param name="usuarioId">ID do usuário</param>
        /// <returns>DTO da meta criada</returns>
        Task<MetaResponseDto> CreateAsync(MetaRequestDto dto, int usuarioId);

        /// <summary>
        /// Atualiza uma meta existente
        /// </summary>
        /// <param name="id">ID da meta</param>
        /// <param name="dto">Dados atualizados da meta</param>
        /// <param name="usuarioId">ID do usuário</param>
        /// <returns>DTO da meta atualizada ou null se não encontrada</returns>
        Task<MetaResponseDto?> UpdateAsync(int id, MetaUpdateRequestDto dto, int usuarioId);

        /// <summary>
        /// Atualiza o progresso de uma meta
        /// </summary>
        /// <param name="id">ID da meta</param>
        /// <param name="quantidade">Quantidade a ser adicionada ao progresso atual</param>
        /// <param name="usuarioId">ID do usuário</param>
        /// <returns>DTO da meta atualizada ou null se não encontrada</returns>
        Task<MetaResponseDto?> UpdateProgressoAsync(int id, int quantidade, int usuarioId);

        /// <summary>
        /// Marca uma meta como concluída
        /// </summary>
        /// <param name="id">ID da meta</param>
        /// <param name="usuarioId">ID do usuário</param>
        /// <returns>True se a operação for bem-sucedida, False caso contrário</returns>
        Task<bool> CompleteAsync(int id, int usuarioId);

        /// <summary>
        /// Obtém metas ativas (não concluídas e dentro do prazo)
        /// </summary>
        /// <param name="usuarioId">ID do usuário</param>
        /// <returns>Lista de DTOs de metas ativas</returns>
        Task<IEnumerable<MetaResponseDto>> GetActiveAsync(int usuarioId);

        /// <summary>
        /// Exclui uma meta
        /// </summary>
        /// <param name="id">ID da meta</param>
        /// <param name="usuarioId">ID do usuário</param>
        /// <returns>True se a exclusão for bem-sucedida, False caso contrário</returns>
        Task<bool> DeleteAsync(int id, int usuarioId);
    }
}