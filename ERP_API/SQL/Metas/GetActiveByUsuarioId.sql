SELECT id, usuario_id, materia_id, topico_id, titulo, descricao, tipo, 
       quantidade_total, quantidade_atual, unidade, frequencia, dias_semana,
       data_inicio, data_fim, concluida, criado_em, atualizado_em
FROM metas
WHERE usuario_id = @usuarioId
  AND concluida = FALSE
  AND (data_fim IS NULL OR data_fim >= CURDATE())
ORDER BY data_inicio ASC