﻿using AutoMapper;
using ERP_API.Models;
using ERP_API.Models.DTOs;
using ERP_API.Models.Entities;
using ERP_API.Models.Enums;
using ERP_API.Repositorys;
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
        private readonly ICategoriaRepository _categoriaRepository;
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
            ICategoriaRepository categoriaRepository,
            ILogger<SessaoEstudoService> logger,
            IMapper mapper,
            IValidator<SessaoEstudoRequestDto> sessaoRequestValidator,
            IValidator<SessaoEstudo> sessaoValidator,
            IValidator<PausaSessao> pausaValidator)
        {
            _sessaoRepository = sessaoRepository;
            _materiaRepository = materiaRepository;
            _topicoRepository = topicoRepository;
            _categoriaRepository = categoriaRepository;
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

            switch (periodo.ToLower())
            {
                case "dia":
                    dataInicio = dataReferencia.Date; 
                    dataFim = dataReferencia.Date.AddDays(1).AddSeconds(-1); 
                    break;
                case "semana":
                    int diff = (int)dataReferencia.DayOfWeek - 1;
                    if (diff < 0) diff = 6; // domingo vira 6
                    dataInicio = dataReferencia.AddDays(-diff);
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
                    throw new ArgumentException("Período inválido. Opções válidas: dia, semana, mes, ano.");
            }

            return await _sessaoRepository.GetDashboardStatsAsync(usuarioId, dataInicio, dataFim);
        }

        public async Task<SessaoEstudo> IniciarSessaoAsync(SessaoEstudoRequestDto dto, int usuarioId)
        {
            var validationResult = await _sessaoRequestValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            // Validar se a categoria existe e pertence ao usuário
            if (_categoriaRepository == null)
                throw new Exception("_categoriaRepository está null");

            if (dto == null)
                throw new Exception("dto está null");

            if (dto.CategoriaId == null)
                throw new Exception("dto.CategoriaId está null");
            var categoriaExists = await _categoriaRepository.BelongsToUsuarioAsync(dto.CategoriaId, usuarioId);
            if (!categoriaExists)
            {
                throw new InvalidOperationException("Categoria não encontrada ou não pertence ao usuário.");
            }

            // Validar se a matéria existe e pertence ao usuário
            var materiaExists = await _materiaRepository.BelongsToUsuarioAsync(dto.MateriaId, usuarioId);
            if (!materiaExists)
            {
                throw new InvalidOperationException("Matéria não encontrada ou não pertence ao usuário.");
            }

            // Validar relacionamento entre matéria e categoria
            var materia = await _materiaRepository.GetByIdAsync(dto.MateriaId);
            if (materia == null || materia.CategoriaId != dto.CategoriaId)
            {
                throw new InvalidOperationException("A matéria não pertence à categoria selecionada.");
            }

            // Validar se o tópico existe e pertence ao usuário
            var topicoExists = await _topicoRepository.BelongsToUsuarioAsync(dto.TopicoId, usuarioId);
            if (!topicoExists)
            {
                throw new InvalidOperationException("Tópico não encontrado ou não pertence ao usuário.");
            }

            // Validar relacionamento entre tópico e matéria
            var topico = await _topicoRepository.GetByIdAsync(dto.TopicoId);
            if (topico == null || topico.MateriaId != dto.MateriaId)
            {
                throw new InvalidOperationException("O tópico não pertence à matéria selecionada.");
            }

            var sessao = _mapper.Map<SessaoEstudo>(dto);
            sessao.UsuarioId = usuarioId;
            sessao.Status = StatusSessao.EmAndamento;
            sessao.DataInicio = DateTime.Now;
            sessao.CategoriaId = dto.CategoriaId; 

            var entityValidation = await _sessaoValidator.ValidateAsync(sessao);
            if (!entityValidation.IsValid)
            {
                throw new ValidationException(entityValidation.Errors);
            }

            return await _sessaoRepository.CreateSessaoAsync(sessao);
        }

        public async Task<bool> FinalizarSessaoAsync(int id, int usuarioId)
        {
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