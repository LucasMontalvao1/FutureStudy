SELECT id, usuario_id, sessao_id, inicio, fim, criado_em
FROM pausas_sessao
WHERE id = @id;