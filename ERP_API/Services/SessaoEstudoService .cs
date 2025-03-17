using ERP_API.Models;
using ERP_API.Models.DTOs;
using ERP_API.Repositorys.Interfaces;
using ERP_API.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ERP_API.Services
{
    public class SessaoEstudoService : ISessaoEstudoService
    {
        private readonly ISessaoEstudoRepository _sessaoRepository;
        private readonly IMateriaRepository _materiaRepository;
        private readonly ILogger<SessaoEstudoService> _logger;

        public SessaoEstudoService(
            ISessaoEstudoRepository sessaoRepository,
            IMateriaRepository materiaRepository,
            ILogger<SessaoEstudoService> logger)
        {
            _sessaoRepository = sessaoRepository;
            _materiaRepository = materiaRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<SessaoEstudo>> GetAllByPeriodoAsync(int usuarioId, DateTime dataInicio, DateTime dataFim)
        {
            return await _sessaoRepository.GetAllByPeriodoAsync(usuarioId, dataInicio, dataFim);
        }

        public async Task<SessaoEstudo?> GetByIdAsync(int id, int usuarioId)
        {
            return await _sessaoRepository.GetByIdAsync(id, usuarioId);
        }

        public async Task<Dictionary<int, int>> GetCalendarioAsync(int usuarioId, int mes, int ano)
        {
            return await _sessaoRepository.GetTempoEstudadoPorDiaAsync(usuarioId, mes, ano);
        }

        public async Task<DashboardStats> GetDashboardStatsAsync(int usuarioId, string periodo, DateTime? data = null)
        {
            DateTime dataReferencia = data ?? DateTime.Today;
            DateTime dataInicio, dataFim;

            // Definir o período de consulta com base no tipo de período
            switch (periodo.ToLower())
            {
                case "semana":
                    // Obtém o primeiro dia da semana (domingo)
                    int diaSemana = (int)dataReferencia.DayOfWeek;
                    dataInicio = dataReferencia.AddDays(-diaSemana);
                    dataFim = dataInicio.AddDays(6);
                    break;
                case "mes":
                    // Primeiro dia do mês
                    dataInicio = new DateTime(dataReferencia.Year, dataReferencia.Month, 1);
                    // Último dia do mês
                    dataFim = dataInicio.AddMonths(1).AddDays(-1);
                    break;
                case "ano":
                    // Primeiro dia do ano
                    dataInicio = new DateTime(dataReferencia.Year, 1, 1);
                    // Último dia do ano
                    dataFim = new DateTime(dataReferencia.Year, 12, 31);
                    break;
                default:
                    throw new ArgumentException("Período inválido. Opções válidas: semana, mes, ano.");
            }

            return await _sessaoRepository.GetDashboardStatsAsync(usuarioId, dataInicio, dataFim);
        }

        public async Task<SessaoEstudo> IniciarSessaoAsync(SessaoEstudoRequestDto dto, int usuarioId)
        {
            // Verificar se a matéria existe e pertence ao usuário
            var materia = await _materiaRepository.GetByIdAsync(dto.MateriaId);
            if (materia == null || materia.UsuarioId != usuarioId)
            {
                throw new InvalidOperationException("Matéria não encontrada ou não pertence ao usuário.");
            }

            var sessao = new SessaoEstudo
            {
                UsuarioId = usuarioId,
                MateriaId = dto.MateriaId,
                TopicoId = dto.TopicoId,
                Status = "em_andamento"
            };

            return await _sessaoRepository.CreateSessaoAsync(sessao);
        }

        public async Task<bool> FinalizarSessaoAsync(int id, int usuarioId)
        {
            // Verificar se a sessão existe e está em andamento
            var sessao = await _sessaoRepository.GetByIdAsync(id, usuarioId);
            if (sessao == null)
            {
                throw new InvalidOperationException("Sessão não encontrada.");
            }

            if (sessao.Status != "em_andamento")
            {
                throw new InvalidOperationException("Sessão já foi finalizada.");
            }

            return await _sessaoRepository.FinalizarSessaoAsync(id, usuarioId);
        }

        public async Task<PausaSessao> PausarSessaoAsync(int sessaoId, int usuarioId)
        {
            // Verificar se a sessão existe e está em andamento
            var sessao = await _sessaoRepository.GetByIdAsync(sessaoId, usuarioId);
            if (sessao == null)
            {
                throw new InvalidOperationException("Sessão não encontrada.");
            }

            if (sessao.Status != "em_andamento")
            {
                throw new InvalidOperationException("Não é possível pausar uma sessão finalizada.");
            }

            var pausa = new PausaSessao
            {
                UsuarioId = usuarioId,
                SessaoId = sessaoId
            };

            return await _sessaoRepository.CreatePausaAsync(pausa);
        }

        public async Task<bool> RetomarSessaoAsync(int pausaId, int usuarioId)
        {
            // A validação da pausa deve ser implementada no repositório
            return await _sessaoRepository.FinalizarPausaAsync(pausaId, usuarioId);
        }
    }
}