using AutoMapper;
using ERP_API.Models.DTOs;
using ERP_API.Models.Entities;
using ERP_API.Repositorys.Interfaces;
using ERP_API.Services.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ERP_API.Services
{
    public class AnotacaoService : IAnotacaoService
    {
        private readonly IAnotacaoRepository _anotacaoRepository;
        private readonly ISessaoEstudoRepository _sessaoRepository;
        private readonly IHistoricoAnotacaoService _historicoService;
        private readonly ILogger<AnotacaoService> _logger;
        private readonly IMapper _mapper;
        private readonly IValidator<AnotacaoRequestDto> _anotacaoRequestValidator;
        private readonly IValidator<AnotacaoUpdateDto> _anotacaoUpdateValidator;
        private readonly IValidator<Anotacao> _anotacaoValidator;

        public AnotacaoService(
            IAnotacaoRepository anotacaoRepository,
            ISessaoEstudoRepository sessaoRepository,
            IHistoricoAnotacaoService historicoService,
            ILogger<AnotacaoService> logger,
            IMapper mapper,
            IValidator<AnotacaoRequestDto> anotacaoRequestValidator,
            IValidator<AnotacaoUpdateDto> anotacaoUpdateValidator,
            IValidator<Anotacao> anotacaoValidator)
        {
            _anotacaoRepository = anotacaoRepository;
            _sessaoRepository = sessaoRepository;
            _historicoService = historicoService;
            _logger = logger;
            _mapper = mapper;
            _anotacaoRequestValidator = anotacaoRequestValidator;
            _anotacaoUpdateValidator = anotacaoUpdateValidator;
            _anotacaoValidator = anotacaoValidator;
        }

        public async Task<IEnumerable<Anotacao>> GetAllByUsuarioAsync(int usuarioId)
        {
            return await _anotacaoRepository.GetAllByUsuarioAsync(usuarioId);
        }

        public async Task<IEnumerable<Anotacao>> GetByDateRangeAsync(int usuarioId, DateTime dataInicio, DateTime? dataFim)
        {
            // Se a data final não for fornecida, use a data atual
            dataFim ??= DateTime.Now;

            var anotacoes = await _anotacaoRepository.GetByDateRangeAsync(usuarioId, dataInicio, dataFim.Value);

            _logger.LogInformation("Retornando {Count} anotações entre {DataInicio} e {DataFim} para o usuário {UsuarioId}",
                anotacoes.Count(), dataInicio, dataFim, usuarioId);

            return anotacoes;
        }
        public async Task<IEnumerable<Anotacao>> GetAllBySessaoAsync(int sessaoId, int usuarioId)
        {
            var sessao = await _sessaoRepository.GetByIdAsync(sessaoId, usuarioId);
            if (sessao == null)
            {
                throw new InvalidOperationException("Sessão não encontrada ou não pertence ao usuário.");
            }

            return await _anotacaoRepository.GetAllBySessaoAsync(sessaoId, usuarioId);
        }

        public async Task<Anotacao?> GetByIdAsync(int id, int usuarioId)
        {
            return await _anotacaoRepository.GetByIdAsync(id, usuarioId);
        }

        public async Task<Anotacao> CreateAsync(AnotacaoRequestDto dto, int usuarioId)
        {
            var validationResult = await _anotacaoRequestValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var sessao = await _sessaoRepository.GetByIdAsync(dto.SessaoId, usuarioId);
            if (sessao == null)
            {
                throw new InvalidOperationException("Sessão não encontrada ou não pertence ao usuário.");
            }

            var anotacao = _mapper.Map<Anotacao>(dto);
            anotacao.UsuarioId = usuarioId;

            var entityValidation = await _anotacaoValidator.ValidateAsync(anotacao);
            if (!entityValidation.IsValid)
            {
                throw new ValidationException(entityValidation.Errors);
            }

            return await _anotacaoRepository.CreateAsync(anotacao);
        }

        public async Task<bool> UpdateAsync(int id, AnotacaoUpdateDto dto, int usuarioId)
        {
            var validationResult = await _anotacaoUpdateValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var anotacao = await _anotacaoRepository.GetByIdAsync(id, usuarioId);
            if (anotacao == null)
            {
                throw new InvalidOperationException("Anotação não encontrada ou não pertence ao usuário.");
            }

            if (!string.IsNullOrEmpty(dto.Conteudo) && dto.Conteudo != anotacao.Conteudo)
            {
                await _historicoService.RegistrarAlteracaoAsync(id, usuarioId, anotacao.Conteudo);
            }

            anotacao.Titulo = !string.IsNullOrEmpty(dto.Titulo) ? dto.Titulo : anotacao.Titulo;
            anotacao.Conteudo = !string.IsNullOrEmpty(dto.Conteudo) ? dto.Conteudo : anotacao.Conteudo;

            var entityValidation = await _anotacaoValidator.ValidateAsync(anotacao);
            if (!entityValidation.IsValid)
            {
                throw new ValidationException(entityValidation.Errors);
            }

            return await _anotacaoRepository.UpdateAsync(anotacao);
        }

        public async Task<bool> DeleteAsync(int id, int usuarioId)
        {
            var anotacao = await _anotacaoRepository.GetByIdAsync(id, usuarioId);
            if (anotacao == null)
            {
                throw new InvalidOperationException("Anotação não encontrada ou não pertence ao usuário.");
            }

            return await _anotacaoRepository.DeleteAsync(id, usuarioId);
        }
    }
}