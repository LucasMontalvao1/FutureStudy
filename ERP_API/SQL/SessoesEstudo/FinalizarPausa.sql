UPDATE pausas_sessao
SET fim = NOW()
WHERE id = @id AND usuario_id = @usuarioId AND fim IS NULL;