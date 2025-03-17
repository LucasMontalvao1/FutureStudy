using ERP_API.Models;
using ERP_API.Models.DTOs;
using ERP_API.Repositorys.Interfaces;
using ERP_API.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ERP_API.Services
{
    public class TopicoService : ITopicoService
    {
        private readonly ITopicoRepository _topicoRepository;
        private readonly IMateriaRepository _materiaRepository;
        private readonly ILogger<TopicoService> _logger;

        public TopicoService(
            ITopicoRepository topicoRepository,
            IMateriaRepository materiaRepository,
            ILogger<TopicoService> logger)
        {
            _topicoRepository = topicoRepository;
            _materiaRepository = materiaRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Topico>> GetAllByUsuarioIdAsync(int usuarioId)
        {
            return await _topicoRepository.GetAllByUsuarioIdAsync(usuarioId);
        }

        public async Task<IEnumerable<Topico>> GetAllByMateriaIdAsync(int materiaId, int usuarioId)
        {
            // Verifica se a matéria pertence ao usuário
            var materiaExists = await _materiaRepository.BelongsToUsuarioAsync(materiaId, usuarioId);
            if (!materiaExists)
            {
                _logger.LogWarning("Matéria {MateriaId} não encontrada ou não pertence ao usuário {UsuarioId}", materiaId, usuarioId);
                return new List<Topico>();
            }

            return await _topicoRepository.GetAllByMateriaIdAsync(materiaId, usuarioId);
        }

        public async Task<Topico?> GetByIdAsync(int id, int usuarioId)
        {
            var topico = await _topicoRepository.GetByIdAsync(id);
            if (topico == null || topico.UsuarioId != usuarioId)
            {
                return null;
            }

            return topico;
        }

        public async Task<Topico> CreateAsync(TopicoRequestDto dto, int usuarioId)
        {
            // Verifica se a matéria existe e pertence ao usuário
            var materiaExists = await _materiaRepository.BelongsToUsuarioAsync(dto.MateriaId, usuarioId);
            if (!materiaExists)
            {
                throw new InvalidOperationException("A matéria especificada não foi encontrada ou não pertence ao usuário");
            }

            // Verifica se já existe um tópico com o mesmo nome para a mesma matéria
            var exists = await _topicoRepository.ExistsByNomeAndMateriaIdAsync(dto.Nome, dto.MateriaId, usuarioId);
            if (exists)
            {
                throw new InvalidOperationException($"Já existe um tópico com o nome '{dto.Nome}' para esta matéria");
            }

            var topico = new Topico
            {
                UsuarioId = usuarioId,
                MateriaId = dto.MateriaId,
                Nome = dto.Nome
            };

            return await _topicoRepository.CreateAsync(topico);
        }

        public async Task<Topico?> UpdateAsync(int id, TopicoUpdateRequestDto dto, int usuarioId)
        {
            // Verifica se o tópico existe e pertence ao usuário
            var topico = await _topicoRepository.GetByIdAsync(id);
            if (topico == null || topico.UsuarioId != usuarioId)
            {
                return null;
            }

            // Se a matéria for alterada, verifica se a nova matéria existe e pertence ao usuário
            if (dto.MateriaId.HasValue && dto.MateriaId.Value != topico.MateriaId)
            {
                var materiaExists = await _materiaRepository.BelongsToUsuarioAsync(dto.MateriaId.Value, usuarioId);
                if (!materiaExists)
                {
                    throw new InvalidOperationException("A matéria especificada não foi encontrada ou não pertence ao usuário");
                }
                topico.MateriaId = dto.MateriaId.Value;
            }

            // Atualiza o nome se fornecido
            if (!string.IsNullOrEmpty(dto.Nome) && dto.Nome != topico.Nome)
            {
                // Verifica se já existe um tópico com o mesmo nome para a mesma matéria
                var materiaId = dto.MateriaId ?? topico.MateriaId;
                var exists = await _topicoRepository.ExistsByNomeAndMateriaIdAsync(dto.Nome, materiaId, usuarioId, id);
                if (exists)
                {
                    throw new InvalidOperationException($"Já existe um tópico com o nome '{dto.Nome}' para esta matéria");
                }
                topico.Nome = dto.Nome;
            }

            var success = await _topicoRepository.UpdateAsync(topico);
            return success ? topico : null;
        }

        public async Task<bool> DeleteAsync(int id, int usuarioId)
        {
            // Verifica se o tópico existe e pertence ao usuário
            var belongs = await _topicoRepository.BelongsToUsuarioAsync(id, usuarioId);
            if (!belongs)
            {
                return false;
            }

            return await _topicoRepository.DeleteAsync(id);
        }
    }
}