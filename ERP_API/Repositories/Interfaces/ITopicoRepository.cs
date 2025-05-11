using ERP_API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ERP_API.Repositorys.Interfaces
{
    public interface ITopicoRepository
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
        /// <returns>Tópico encontrado ou null</returns>
        Task<Topico?> GetByIdAsync(int id);

        /// <summary>
        /// Verifica se um tópico pertence a um usuário específico
        /// </summary>
        /// <param name="topicoId">ID do tópico</param>
        /// <param name="usuarioId">ID do usuário</param>
        /// <returns>True se o tópico pertencer ao usuário, False caso contrário</returns>
        Task<bool> BelongsToUsuarioAsync(int topicoId, int usuarioId);

        /// <summary>
        /// Verifica se já existe um tópico com o mesmo nome para a mesma matéria
        /// </summary>
        /// <param name="nome">Nome do tópico</param>
        /// <param name="materiaId">ID da matéria</param>
        /// <param name="usuarioId">ID do usuário</param>
        /// <param name="ignoreTopicoId">ID do tópico a ser ignorado na verificação (para updates)</param>
        /// <returns>True se já existir, False caso contrário</returns>
        Task<bool> ExistsByNomeAndMateriaIdAsync(string nome, int materiaId, int usuarioId, int? ignoreTopicoId = null);

        /// <summary>
        /// Cria um novo tópico
        /// </summary>
        /// <param name="topico">Dados do tópico</param>
        /// <returns>Tópico criado com ID preenchido</returns>
        Task<Topico> CreateAsync(Topico topico);

        /// <summary>
        /// Atualiza um tópico existente
        /// </summary>
        /// <param name="topico">Dados atualizados do tópico</param>
        /// <returns>True se a atualização for bem-sucedida, False caso contrário</returns>
        Task<bool> UpdateAsync(Topico topico);

        /// <summary>
        /// Exclui um tópico
        /// </summary>
        /// <param name="id">ID do tópico</param>
        /// <returns>True se a exclusão for bem-sucedida, False caso contrário</returns>
        Task<bool> DeleteAsync(int id);
    }
}