INSERT INTO anotacoes (usuario_id, sessao_id, titulo, conteudo)
VALUES (@usuarioId, @sessaoId, @titulo, @conteudo);
SELECT LAST_INSERT_ID();