UPDATE sessoes_estudo
SET data_fim = NOW(), status = @status
WHERE id = @id AND usuario_id = @usuarioId AND status = @statusAtual;