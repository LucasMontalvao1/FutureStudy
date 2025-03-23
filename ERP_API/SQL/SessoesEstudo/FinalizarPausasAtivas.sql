UPDATE pausas_sessao
SET fim = NOW()
WHERE sessao_id = @sessaoId AND usuario_id = @usuarioId AND fim IS NULL;