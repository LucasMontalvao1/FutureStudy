using ERP_API.Models.Entities;
using ERP_API.Repositorys.Interfaces;
using ERP_API.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ERP_API.Services
{
    public class HistoricoAnotacaoService : IHistoricoAnotacaoService
    {
        private readonly IHistoricoAnotacaoRepository _historicoRepository;
        private readonly IAnotacaoRepository _anotacaoRepository;
        private readonly ILogger<HistoricoAnotacaoService> _logger;

        public HistoricoAnotacaoService(
            IHistoricoAnotacaoRepository historicoRepository,
            IAnotacaoRepository anotacaoRepository,
            ILogger<HistoricoAnotacaoService> logger)
        {
            _historicoRepository = historicoRepository;
            _anotacaoRepository = anotacaoRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<HistoricoAnotacao>> GetByAnotacaoAsync(int anotacaoId, int usuarioId)
        {
            // Verificar se a anotação existe e pertence ao usuário
            var anotacao = await _anotacaoRepository.GetByIdAsync(anotacaoId, usuarioId);
            if (anotacao == null)
            {
                throw new InvalidOperationException("Anotação não encontrada ou não pertence ao usuário.");
            }

            return await _historicoRepository.GetByAnotacaoAsync(anotacaoId, usuarioId);
        }

        public async Task<HistoricoAnotacao> RegistrarAlteracaoAsync(int anotacaoId, int usuarioId, string conteudoAnterior)
        {
            // Verificar se a anotação existe e pertence ao usuário
            var anotacao = await _anotacaoRepository.GetByIdAsync(anotacaoId, usuarioId);
            if (anotacao == null)
            {
                throw new InvalidOperationException("Anotação não encontrada ou não pertence ao usuário.");
            }

            var historico = new HistoricoAnotacao
            {
                UsuarioId = usuarioId,
                AnotacaoId = anotacaoId,
                ConteudoAnterior = conteudoAnterior
            };

            return await _historicoRepository.CreateAsync(historico);
        }
    }
}