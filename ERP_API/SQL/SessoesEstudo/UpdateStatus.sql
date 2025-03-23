UPDATE sessoes_estudo
SET status = @status
WHERE id = @id AND usuario_id = @usuarioId;