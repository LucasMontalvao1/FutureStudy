using ERP_API.Models;
using ERP_API.Models.DTOs;

namespace ERP_API.Services.Interfaces
{
    public interface ICategoriaService
    {
        /// <summary>
        /// Obtém todas as categorias de um usuário
        /// </summary>
        /// <param name="usuarioId">ID do usuário</param>
        /// <returns>Lista de categorias</returns>
        Task<IEnumerable<Categoria>> GetAllByUsuarioIdAsync(int usuarioId);

        /// <summary>
        /// Obtém uma categoria pelo ID
        /// </summary>
        /// <param name="id">ID da categoria</param>
        /// <param name="usuarioId">ID do usuário para verificação de acesso</param>
        /// <returns>Categoria encontrada ou null</returns>
        Task<Categoria?> GetByIdAsync(int id, int usuarioId);

        /// <summary>
        /// Cria uma nova categoria
        /// </summary>
        /// <param name="dto">DTO com os dados da categoria</param>
        /// <param name="usuarioId">ID do usuário que está criando a categoria</param>
        /// <returns>Categoria criada</returns>
        Task<Categoria> CreateAsync(CategoriaRequestDto dto, int usuarioId);

        /// <summary>
        /// Atualiza uma categoria existente
        /// </summary>
        /// <param name="id">ID da categoria</param>
        /// <param name="dto">DTO com os dados atualizados</param>
        /// <param name="usuarioId">ID do usuário para verificação de acesso</param>
        /// <returns>Categoria atualizada ou null se não encontrada ou sem acesso</returns>
        Task<Categoria?> UpdateAsync(int id, CategoriaUpdateRequestDto dto, int usuarioId);

        /// <summary>
        /// Exclui uma categoria
        /// </summary>
        /// <param name="id">ID da categoria</param>
        /// <param name="usuarioId">ID do usuário para verificação de acesso</param>
        /// <returns>True se a exclusão for bem-sucedida, False caso contrário</returns>
        Task<bool> DeleteAsync(int id, int usuarioId);
    }
}