using AutoMapper;
using ERP_API.Models;
using ERP_API.Models.DTOs;
using ERP_API.Models.Entities;
using ERP_API.Models.Enums;
using ERP_API.Repositorys.Interfaces;
using ERP_API.Services.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_API.Services
{
    public class SessaoEstudoService : ISessaoEstudoService
    {
        private readonly ISessaoEstudoRepository _sessaoRepository;
        private readonly IMateriaRepository _materiaRepository;
        private readonly ITopicoRepository _topicoRepository;
        private readonly ILogger<SessaoEstudoService> _logger;
        private readonly IMapper _mapper;
        private readonly IValidator<SessaoEstudoRequestDto> _sessaoRequestValidator;
        private readonly IValidator<SessaoEstudo> _sessaoValidator;
        private readonly IValidator<PausaSessao> _pausaValidator;

        public SessaoEstudoService(
            ISessaoEstudoRepository sessaoRepository,
            IMateriaRepository materiaRepository,
            ITopicoRepository topicoRepository,
            ILogger<SessaoEstudoService> logger,
            IMapper mapper,
            IValidator<SessaoEstudoRequestDto> sessaoRequestValidator,
            IValidator<SessaoEstudo> sessaoValidator,
            IValidator<PausaSessao> pausaValidator)
        {
            _sessaoRepository = sessaoRepository;
            _materiaRepository = materiaRepository;
            _topicoRepository = topicoRepository;
            _logger = logger;
            _mapper = mapper;
            _sessaoRequestValidator = sessaoRequestValidator;
            _sessaoValidator = sessaoValidator;
            _pausaValidator = pausaValidator;
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
            if (mes < 1 || mes > 12)
            {
                throw new ArgumentException("Mês inválido. Deve estar entre 1 e 12.");
            }

            if (ano < 2000 || ano > 2100)
            {
                throw new ArgumentException("Ano inválido. Deve estar entre 2000 e 2100.");
            }

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
            // Validar o DTO usando FluentValidation
            var validationResult = await _sessaoRequestValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            // Verificar se a matéria existe e pertence ao usuário
            var materiaExists = await _materiaRepository.BelongsToUsuarioAsync(dto.MateriaId, usuarioId);
            if (!materiaExists)
            {
                throw new InvalidOperationException("Matéria não encontrada ou não pertence ao usuário.");
            }

            // Se tiver tópico, verificar se pertence à matéria
            if (dto.TopicoId.HasValue)
            {
                var topicoExists = await _topicoRepository.BelongsToUsuarioAsync(dto.TopicoId.Value, usuarioId);
                if (!topicoExists)
                {
                    throw new InvalidOperationException("Tópico não encontrado ou não pertence ao usuário.");
                }

                var topico = await _topicoRepository.GetByIdAsync(dto.TopicoId.Value);
                if (topico == null || topico.MateriaId != dto.MateriaId)
                {
                    throw new InvalidOperationException("O tópico não pertence à matéria selecionada.");
                }
            }

            // Mapear para entidade
            var sessao = _mapper.Map<SessaoEstudo>(dto);
            sessao.UsuarioId = usuarioId;
            sessao.Status = StatusSessao.EmAndamento;

            // Validar entidade
            var entityValidation = await _sessaoValidator.ValidateAsync(sessao);
            if (!entityValidation.IsValid)
            {
                throw new ValidationException(entityValidation.Errors);
            }

            // Criar a sessão
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

            if (sessao.Status == StatusSessao.Concluida)
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

            if (sessao.Status != StatusSessao.EmAndamento)
            {
                throw new InvalidOperationException("Não é possível pausar uma sessão que não está em andamento.");
            }

            var pausa = new PausaSessao
            {
                UsuarioId = usuarioId,
                SessaoId = sessaoId,
                Inicio = DateTime.Now  
            };

            // Validar entidade
            var entityValidation = await _pausaValidator.ValidateAsync(pausa);
            if (!entityValidation.IsValid)
            {
                throw new ValidationException(entityValidation.Errors);
            }

            return await _sessaoRepository.CreatePausaAsync(pausa);
        }

        public async Task<bool> RetomarSessaoAsync(int pausaId, int usuarioId)
        {
            return await _sessaoRepository.FinalizarPausaAsync(pausaId, usuarioId);
        }
    }
}