INSERT INTO pausas_sessao (usuario_id, sessao_id, inicio)
VALUES (@usuarioId, @sessaoId, NOW());
SELECT LAST_INSERT_ID();