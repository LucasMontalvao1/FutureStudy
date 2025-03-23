SELECT id, usuario_id, materia_id, topico_id, titulo, descricao, tipo, 
       quantidade_total, quantidade_atual, unidade, frequencia, dias_semana,
       data_inicio, data_fim, concluida, criado_em, atualizado_em
FROM metas
WHERE topico_id = @topicoId AND usuario_id = @usuarioId
ORDER BY data_inicio DESC