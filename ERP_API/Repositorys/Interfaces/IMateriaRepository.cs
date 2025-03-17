using ERP_API.Models;

namespace ERP_API.Repositorys.Interfaces
{
    public interface IMateriaRepository
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
        /// <returns>Matéria encontrada ou null</returns>
        Task<Materia?> GetByIdAsync(int id);

        /// <summary>
        /// Verifica se uma matéria pertence a um usuário específico
        /// </summary>
        /// <param name="materiaId">ID da matéria</param>
        /// <param name="usuarioId">ID do usuário</param>
        /// <returns>True se a matéria pertencer ao usuário, False caso contrário</returns>
        Task<bool> BelongsToUsuarioAsync(int materiaId, int usuarioId);

        /// <summary>
        /// Verifica se já existe uma matéria com o mesmo nome para o usuário
        /// </summary>
        /// <param name="nome">Nome da matéria</param>
        /// <param name="usuarioId">ID do usuário</param>
        /// <param name="ignoreMateriaId">ID da matéria a ser ignorada na verificação (para updates)</param>
        /// <returns>True se já existir, False caso contrário</returns>
        Task<bool> ExistsByNomeAndUsuarioIdAsync(string nome, int usuarioId, int? ignoreMateriaId = null);

        /// <summary>
        /// Cria uma nova matéria
        /// </summary>
        /// <param name="materia">Dados da matéria</param>
        /// <returns>Matéria criada com ID preenchido</returns>
        Task<Materia> CreateAsync(Materia materia);

        /// <summary>
        /// Atualiza uma matéria existente
        /// </summary>
        /// <param name="materia">Dados atualizados da matéria</param>
        /// <returns>True se a atualização for bem-sucedida, False caso contrário</returns>
        Task<bool> UpdateAsync(Materia materia);

        /// <summary>
        /// Exclui uma matéria
        /// </summary>
        /// <param name="id">ID da matéria</param>
        /// <returns>True se a exclusão for bem-sucedida, False caso contrário</returns>
        Task<bool> DeleteAsync(int id);
    }
}