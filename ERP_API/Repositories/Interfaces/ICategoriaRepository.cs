using ERP_API.Models;

namespace ERP_API.Repositorys.Interfaces
{
    public interface ICategoriaRepository
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
        /// <returns>Categoria encontrada ou null</returns>
        Task<Categoria?> GetByIdAsync(int id);

        /// <summary>
        /// Verifica se uma categoria pertence a um usuário específico
        /// </summary>
        /// <param name="categoriaId">ID da categoria</param>
        /// <param name="usuarioId">ID do usuário</param>
        /// <returns>True se a categoria pertencer ao usuário, False caso contrário</returns>
        Task<bool> BelongsToUsuarioAsync(int categoriaId, int usuarioId);

        /// <summary>
        /// Verifica se já existe uma categoria com o mesmo nome para o usuário
        /// </summary>
        /// <param name="nome">Nome da categoria</param>
        /// <param name="usuarioId">ID do usuário</param>
        /// <param name="ignoreCategoriaId">ID da categoria a ser ignorada na verificação (para updates)</param>
        /// <returns>True se já existir, False caso contrário</returns>
        Task<bool> ExistsByNomeAndUsuarioIdAsync(string nome, int usuarioId, int? ignoreCategoriaId = null);

        /// <summary>
        /// Cria uma nova categoria
        /// </summary>
        /// <param name="categoria">Dados da categoria</param>
        /// <returns>Categoria criada com ID preenchido</returns>
        Task<Categoria> CreateAsync(Categoria categoria);

        /// <summary>
        /// Atualiza uma categoria existente
        /// </summary>
        /// <param name="categoria">Dados atualizados da categoria</param>
        /// <returns>True se a atualização for bem-sucedida, False caso contrário</returns>
        Task<bool> UpdateAsync(Categoria categoria);

        /// <summary>
        /// Exclui uma categoria
        /// </summary>
        /// <param name="id">ID da categoria</param>
        /// <returns>True se a exclusão for bem-sucedida, False caso contrário</returns>
        Task<bool> DeleteAsync(int id);
    }
}