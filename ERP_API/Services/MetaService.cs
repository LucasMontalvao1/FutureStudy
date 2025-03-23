using AutoMapper;
using ERP_API.Models;
using ERP_API.Models.DTOs;
using ERP_API.Models.Enums;
using ERP_API.Repositories.Interfaces;
using ERP_API.Repositorys.Interfaces;
using ERP_API.Services.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace ERP_API.Services
{
    public class MetaService : IMetaService
    {
        private readonly IMetaRepository _metaRepository;
        private readonly IMateriaRepository _materiaRepository;
        private readonly ITopicoRepository _topicoRepository;
        private readonly ILogger<MetaService> _logger;
        private readonly IMapper _mapper;
        private readonly IValidator<MetaRequestDto> _metaRequestValidator;
        private readonly IValidator<Meta> _metaValidator;

        public MetaService(
            IMetaRepository metaRepository,
            IMateriaRepository materiaRepository,
            ITopicoRepository topicoRepository,
            ILogger<MetaService> logger,
            IMapper mapper,
            IValidator<MetaRequestDto> metaRequestValidator,
            IValidator<Meta> metaValidator)
        {
            _metaRepository = metaRepository;
            _materiaRepository = materiaRepository;
            _topicoRepository = topicoRepository;
            _logger = logger;
            _mapper = mapper;
            _metaRequestValidator = metaRequestValidator;
            _metaValidator = metaValidator;
        }

        public async Task<IEnumerable<MetaResponseDto>> GetAllByUsuarioIdAsync(int usuarioId)
        {
            var metas = await _metaRepository.GetAllByUsuarioIdAsync(usuarioId);
            return _mapper.Map<IEnumerable<MetaResponseDto>>(metas);
        }

        public async Task<IEnumerable<MetaResponseDto>> GetAllByMateriaIdAsync(int materiaId, int usuarioId)
        {
            // Verifica se a matéria pertence ao usuário
            var materiaExists = await _materiaRepository.BelongsToUsuarioAsync(materiaId, usuarioId);
            if (!materiaExists)
            {
                _logger.LogWarning("Matéria {MateriaId} não encontrada ou não pertence ao usuário {UsuarioId}", materiaId, usuarioId);
                return new List<MetaResponseDto>();
            }

            var metas = await _metaRepository.GetAllByMateriaIdAsync(materiaId, usuarioId);
            return _mapper.Map<IEnumerable<MetaResponseDto>>(metas);
        }

        public async Task<IEnumerable<MetaResponseDto>> GetAllByTopicoIdAsync(int topicoId, int usuarioId)
        {
            // Verifica se o tópico pertence ao usuário
            var topicoExists = await _topicoRepository.BelongsToUsuarioAsync(topicoId, usuarioId);
            if (!topicoExists)
            {
                _logger.LogWarning("Tópico {TopicoId} não encontrado ou não pertence ao usuário {UsuarioId}", topicoId, usuarioId);
                return new List<MetaResponseDto>();
            }

            var metas = await _metaRepository.GetAllByTopicoIdAsync(topicoId, usuarioId);
            return _mapper.Map<IEnumerable<MetaResponseDto>>(metas);
        }

        public async Task<MetaResponseDto?> GetByIdAsync(int id, int usuarioId)
        {
            var meta = await _metaRepository.GetByIdAsync(id);
            if (meta == null || meta.UsuarioId != usuarioId)
            {
                return null;
            }

            return _mapper.Map<MetaResponseDto>(meta);
        }

        public async Task<MetaResponseDto> CreateAsync(MetaRequestDto dto, int usuarioId)
        {
            // Validar o DTO usando FluentValidation
            var validationResult = await _metaRequestValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                throw new FluentValidation.ValidationException(validationResult.Errors);
            }

            // Validações relacionadas a entidades
            await ValidateRelatedEntities(dto, usuarioId);

            // Mapear para a entidade
            var meta = _mapper.Map<Meta>(dto);
            meta.UsuarioId = usuarioId;
            meta.QuantidadeAtual = 0; // Sempre inicia com zero
            meta.Concluida = false;   // Sempre inicia como não concluída

            // Criar no repositório
            var createdMeta = await _metaRepository.CreateAsync(meta);

            return _mapper.Map<MetaResponseDto>(createdMeta);
        }

        public async Task<MetaResponseDto?> UpdateAsync(int id, MetaUpdateRequestDto dto, int usuarioId)
        {
            // Verifica se a meta existe e pertence ao usuário
            var meta = await _metaRepository.GetByIdAsync(id);
            if (meta == null || meta.UsuarioId != usuarioId)
            {
                return null;
            }

            // Validações relacionadas a entidades
            await ValidateRelatedEntitiesForUpdate(dto, meta, usuarioId);

            // Atualizar os campos da entidade com os valores do DTO
            _mapper.Map(dto, meta);

            // Se a quantidade atual atingir ou ultrapassar a meta, marca como concluída
            if (meta.QuantidadeAtual >= meta.QuantidadeTotal)
            {
                meta.Concluida = true;
            }

            // Validar a entidade após as alterações
            var validationResult = await _metaValidator.ValidateAsync(meta);
            if (!validationResult.IsValid)
            {
                throw new FluentValidation.ValidationException(validationResult.Errors);
            }

            // Atualizar no repositório
            var success = await _metaRepository.UpdateAsync(meta);
            if (!success)
            {
                return null;
            }

            return _mapper.Map<MetaResponseDto>(meta);
        }

        public async Task<MetaResponseDto?> UpdateProgressoAsync(int id, int quantidade, int usuarioId)
        {
            // Verifica se a meta existe e pertence ao usuário
            var meta = await _metaRepository.GetByIdAsync(id);
            if (meta == null || meta.UsuarioId != usuarioId)
            {
                return null;
            }

            // Calcula a nova quantidade atual
            int novaQuantidade = meta.QuantidadeAtual + quantidade;
            if (novaQuantidade < 0)
            {
                novaQuantidade = 0; // Não permite valores negativos
            }

            // Atualiza o progresso no banco de dados
            var success = await _metaRepository.UpdateProgressoAsync(id, novaQuantidade);
            if (!success)
            {
                return null;
            }

            // Busca a meta atualizada
            var metaAtualizada = await _metaRepository.GetByIdAsync(id);
            return _mapper.Map<MetaResponseDto>(metaAtualizada);
        }

        public async Task<bool> CompleteAsync(int id, int usuarioId)
        {
            // Verifica se a meta existe e pertence ao usuário
            var meta = await _metaRepository.GetByIdAsync(id);
            if (meta == null || meta.UsuarioId != usuarioId)
            {
                return false;
            }

            // Se já estiver concluída, não faz nada
            if (meta.Concluida)
            {
                return true;
            }

            // Marca como concluída e atualiza a quantidade para o total
            meta.Concluida = true;
            meta.QuantidadeAtual = meta.QuantidadeTotal;

            return await _metaRepository.UpdateAsync(meta);
        }

        public async Task<IEnumerable<MetaResponseDto>> GetActiveAsync(int usuarioId)
        {
            var metas = await _metaRepository.GetActiveAsync(usuarioId);
            return _mapper.Map<IEnumerable<MetaResponseDto>>(metas);
        }

        public async Task<bool> DeleteAsync(int id, int usuarioId)
        {
            // Verifica se a meta existe e pertence ao usuário
            var belongs = await _metaRepository.BelongsToUsuarioAsync(id, usuarioId);
            if (!belongs)
            {
                return false;
            }

            return await _metaRepository.DeleteAsync(id);
        }

        #region Validation Methods

        private async Task ValidateRelatedEntities(MetaRequestDto dto, int usuarioId)
        {
            // Valida a matéria se fornecida
            if (dto.MateriaId.HasValue)
            {
                var materiaExists = await _materiaRepository.BelongsToUsuarioAsync(dto.MateriaId.Value, usuarioId);
                if (!materiaExists)
                {
                    throw new InvalidOperationException("A matéria especificada não foi encontrada ou não pertence ao usuário");
                }
            }

            // Valida o tópico se fornecido
            if (dto.TopicoId.HasValue)
            {
                var topicoExists = await _topicoRepository.BelongsToUsuarioAsync(dto.TopicoId.Value, usuarioId);
                if (!topicoExists)
                {
                    throw new InvalidOperationException("O tópico especificado não foi encontrado ou não pertence ao usuário");
                }

                // Se tiver tópico, a matéria deve ser a do tópico
                var topico = await _topicoRepository.GetByIdAsync(dto.TopicoId.Value);
                if (topico != null && dto.MateriaId.HasValue && topico.MateriaId != dto.MateriaId.Value)
                {
                    throw new InvalidOperationException("O tópico não pertence à matéria especificada");
                }
            }

            // Validações específicas por tipo de meta
            ValidateTipoMeta(dto);
        }

        private async Task ValidateRelatedEntitiesForUpdate(MetaUpdateRequestDto dto, Meta meta, int usuarioId)
        {
            // Valida a matéria se for alterada
            if (dto.MateriaId.HasValue && dto.MateriaId.Value != meta.MateriaId)
            {
                var materiaExists = await _materiaRepository.BelongsToUsuarioAsync(dto.MateriaId.Value, usuarioId);
                if (!materiaExists)
                {
                    throw new InvalidOperationException("A matéria especificada não foi encontrada ou não pertence ao usuário");
                }
                meta.MateriaId = dto.MateriaId.Value;
            }

            // Valida o tópico se for alterado
            if (dto.TopicoId.HasValue && dto.TopicoId.Value != meta.TopicoId)
            {
                var topicoExists = await _topicoRepository.BelongsToUsuarioAsync(dto.TopicoId.Value, usuarioId);
                if (!topicoExists)
                {
                    throw new InvalidOperationException("O tópico especificado não foi encontrado ou não pertence ao usuário");
                }

                // Se tiver tópico, a matéria deve ser a do tópico
                var topico = await _topicoRepository.GetByIdAsync(dto.TopicoId.Value);
                if (topico != null && meta.MateriaId.HasValue && topico.MateriaId != meta.MateriaId.Value)
                {
                    throw new InvalidOperationException("O tópico não pertence à matéria da meta");
                }
                meta.TopicoId = dto.TopicoId.Value;
            }
        }

        private void ValidateTipoMeta(MetaRequestDto dto)
        {
            // Validações específicas por tipo de meta
            switch (dto.Tipo)
            {
                case TipoMeta.Tempo:
                    if (dto.Unidade != UnidadeMeta.Minutos && dto.Unidade != UnidadeMeta.Horas)
                    {
                        throw new InvalidOperationException("Para metas de tempo, a unidade deve ser 'minutos' ou 'horas'");
                    }
                    break;

                case TipoMeta.QtdSessoes:
                    if (dto.Unidade != UnidadeMeta.Sessoes)
                    {
                        throw new InvalidOperationException("Para metas de quantidade de sessões, a unidade deve ser 'sessoes'");
                    }
                    break;

                case TipoMeta.Topicos:
                    if (dto.Unidade != UnidadeMeta.Topicos)
                    {
                        throw new InvalidOperationException("Para metas de tópicos, a unidade deve ser 'topicos'");
                    }
                    if (!dto.MateriaId.HasValue)
                    {
                        throw new InvalidOperationException("Para metas de tópicos, a matéria é obrigatória");
                    }
                    break;
            }

            // Validações de frequência
            if (dto.Frequencia.HasValue && dto.Frequencia.Value == FrequenciaMeta.Semanal && string.IsNullOrEmpty(dto.DiasSemana))
            {
                throw new InvalidOperationException("Para metas com frequência semanal, os dias da semana são obrigatórios");
            }
        }

        #endregion
    }
}