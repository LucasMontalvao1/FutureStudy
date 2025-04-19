using ERP_API.Models;
using ERP_API.Models.DTOs;


namespace ERP_API.Services.Interfaces
{
    public interface IMateriaService
    {
        /// <summary>
        /// Obtém todas as matérias de um usuário
        /// </summary>
        /// <param name="usuarioId">ID do usuário</param>
        /// <returns>Lista de matérias</returns>
        Task<IEnumerable<Materia>> GetAllByUsuarioIdAsync(int usuarioId);

        /// <summary>
        /// Obtém uma matéria pelo ID
        /// </summary>
        /// <param name="id">ID da matéria</param>
        /// <param name="usuarioId">ID do usuário para verificação de acesso</param>
        /// <returns>Matéria encontrada ou null</returns>
        Task<Materia?> GetByIdAsync(int id, int usuarioId);

        /// <summary>
        /// Obtém matérias por código de categoria
        /// </summary>
        /// <param name="categoriaId">ID da categoria</param>
        /// <param name="usuarioId">ID do usuário para verificação de acesso</param>
        /// <returns>Lista de matérias da categoria</returns>
        Task<IEnumerable<Materia>> GetByCategoriaIdAsync(int categoriaId, int usuarioId);

        /// <summary>
        /// Cria uma nova matéria
        /// </summary>
        /// <param name="dto">DTO com os dados da matéria</param>
        /// <param name="usuarioId">ID do usuário que está criando a matéria</param>
        /// <returns>Matéria criada</returns>
        Task<Materia> CreateAsync(MateriaRequestDto dto, int usuarioId);

        /// <summary>
        /// Atualiza uma matéria existente
        /// </summary>
        /// <param name="id">ID da matéria</param>
        /// <param name="dto">DTO com os dados atualizados</param>
        /// <param name="usuarioId">ID do usuário para verificação de acesso</param>
        /// <returns>Matéria atualizada ou null se não encontrada ou sem acesso</returns>
        Task<Materia?> UpdateAsync(int id, MateriaUpdateRequestDto dto, int usuarioId);

        /// <summary>
        /// Exclui uma matéria
        /// </summary>
        /// <param name="id">ID da matéria</param>
        /// <param name="usuarioId">ID do usuário para verificação de acesso</param>
        /// <returns>True se a exclusão for bem-sucedida, False caso contrário</returns>
        Task<bool> DeleteAsync(int id, int usuarioId);
    }
}