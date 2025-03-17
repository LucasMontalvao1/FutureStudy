using ERP_API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ERP_API.Repositorys.Interfaces
{
    public interface ISessaoEstudoRepository
    {
        /// <summary>
        /// Obtém todas as sessões de estudo de um usuário em um período específico
        /// </summary>
        /// <param name="usuarioId">ID do usuário</param>
        /// <param name="dataInicio">Data de início do período</param>
        /// <param name="dataFim">Data de fim do período</param>
        /// <returns>Lista de sessões de estudo</returns>
        Task<IEnumerable<SessaoEstudo>> GetAllByPeriodoAsync(int usuarioId, DateTime dataInicio, DateTime dataFim);

        /// <summary>
        /// Obtém uma sessão de estudo pelo ID
        /// </summary>
        /// <param name="id">ID da sessão</param>
        /// <param name="usuarioId">ID do usuário</param>
        /// <returns>Sessão encontrada ou null</returns>
        Task<SessaoEstudo?> GetByIdAsync(int id, int usuarioId);

        /// <summary>
        /// Obtém estatísticas de tempo estudado por dia em um período específico
        /// </summary>
        /// <param name="usuarioId">ID do usuário</param>
        /// <param name="mes">Mês (1-12)</param>
        /// <param name="ano">Ano</param>
        /// <returns>Dicionário com dia como chave e minutos estudados como valor</returns>
        Task<Dictionary<int, int>> GetTempoEstudadoPorDiaAsync(int usuarioId, int mes, int ano);

        /// <summary>
        /// Obtém estatísticas para o dashboard
        /// </summary>
        /// <param name="usuarioId">ID do usuário</param>
        /// <param name="dataInicio">Data de início</param>
        /// <param name="dataFim">Data de fim</param>
        /// <returns>Objeto com estatísticas</returns>
        Task<DashboardStats> GetDashboardStatsAsync(int usuarioId, DateTime dataInicio, DateTime dataFim);

        /// <summary>
        /// Cria uma nova sessão de estudo
        /// </summary>
        /// <param name="sessao">Dados da sessão</param>
        /// <returns>Sessão criada com ID preenchido</returns>
        Task<SessaoEstudo> CreateSessaoAsync(SessaoEstudo sessao);

        /// <summary>
        /// Finaliza uma sessão de estudo
        /// </summary>
        /// <param name="id">ID da sessão</param>
        /// <param name="usuarioId">ID do usuário</param>
        /// <returns>True se a finalização for bem-sucedida, False caso contrário</returns>
        Task<bool> FinalizarSessaoAsync(int id, int usuarioId);

        /// <summary>
        /// Cria uma pausa para uma sessão
        /// </summary>
        /// <param name="pausa">Dados da pausa</param>
        /// <returns>Pausa criada com ID preenchido</returns>
        Task<PausaSessao> CreatePausaAsync(PausaSessao pausa);

        /// <summary>
        /// Finaliza uma pausa
        /// </summary>
        /// <param name="id">ID da pausa</param>
        /// <param name="usuarioId">ID do usuário</param>
        /// <returns>True se a finalização for bem-sucedida, False caso contrário</returns>
        Task<bool> FinalizarPausaAsync(int id, int usuarioId);

        /// <summary>
        /// Calcula o tempo total estudado de uma sessão (excluindo pausas)
        /// </summary>
        /// <param name="sessaoId">ID da sessão</param>
        /// <returns>Tempo estudado em segundos</returns>
        Task<int> CalcularTempoEstudadoAsync(int sessaoId);
    }

    public class DashboardStats
    {
        public TimeSpan TempoTotalEstudado { get; set; }
        public int MetasAlcancadas { get; set; }
        public int TotalDias { get; set; }
        public string MateriaMaisEstudada { get; set; } = string.Empty;
        public int HorasMateriaMaisEstudada { get; set; }
    }
}