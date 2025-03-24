UPDATE sessoes_estudo 
SET status = @status,
    data_fim = CURRENT_TIMESTAMP
WHERE id = @id 
AND usuario_id = @usuarioId
AND status IN ('em_andamento', 'pausada');