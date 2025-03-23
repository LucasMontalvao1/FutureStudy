INSERT INTO historico_anotacoes (usuario_id, anotacao_id, conteudo_anterior)
VALUES (@usuarioId, @anotacaoId, @conteudoAnterior);
SELECT LAST_INSERT_ID();