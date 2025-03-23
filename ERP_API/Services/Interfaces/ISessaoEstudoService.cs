using ERP_API.Models;
using ERP_API.Models.DTOs;
using ERP_API.Models.Entities;
using ERP_API.Repositorys.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ERP_API.Services.Interfaces
{
    public interface ISessaoEstudoService
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
        /// Obtém estatísticas para o calendário
        /// </summary>
        /// <param name="usuarioId">ID do usuário</param>
        /// <param name="mes">Mês (1-12)</param>
        /// <param name="ano">Ano</param>
        /// <returns>Dicionário com dia como chave e minutos estudados como valor</returns>
        Task<Dictionary<int, int>> GetCalendarioAsync(int usuarioId, int mes, int ano);

        /// <summary>
        /// Obtém estatísticas para o dashboard
        /// </summary>
        /// <param name="usuarioId">ID do usuário</param>
        /// <param name="periodo">semana, mes ou ano</param>
        /// <param name="data">Data de referência</param>
        /// <returns>Objeto com estatísticas</returns>
        Task<DashboardStats> GetDashboardStatsAsync(int usuarioId, string periodo, DateTime? data = null);

        /// <summary>
        /// Inicia uma nova sessão de estudo
        /// </summary>
        /// <param name="dto">Dados da sessão</param>
        /// <param name="usuarioId">ID do usuário</param>
        /// <returns>Sessão criada</returns>
        Task<SessaoEstudo> IniciarSessaoAsync(SessaoEstudoRequestDto dto, int usuarioId);

        /// <summary>
        /// Finaliza uma sessão de estudo
        /// </summary>
        /// <param name="id">ID da sessão</param>
        /// <param name="usuarioId">ID do usuário</param>
        /// <returns>True se a finalização for bem-sucedida, False caso contrário</returns>
        Task<bool> FinalizarSessaoAsync(int id, int usuarioId);

        /// <summary>
        /// Pausa uma sessão de estudo
        /// </summary>
        /// <param name="sessaoId">ID da sessão</param>
        /// <param name="usuarioId">ID do usuário</param>
        /// <returns>Dados da pausa criada</returns>
        Task<PausaSessao> PausarSessaoAsync(int sessaoId, int usuarioId);

        /// <summary>
        /// Retoma uma sessão de estudo
        /// </summary>
        /// <param name="pausaId">ID da pausa</param>
        /// <param name="usuarioId">ID do usuário</param>
        /// <returns>True se a retomada for bem-sucedida, False caso contrário</returns>
        Task<bool> RetomarSessaoAsync(int pausaId, int usuarioId);
    }
}