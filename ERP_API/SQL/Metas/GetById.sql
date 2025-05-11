SELECT 
    id, usuario_id, materia_id, categoria_id, topico_id, titulo, descricao, tipo_meta, 
    quantidade_total, quantidade_atual, unidade, frequencia, dias_semana,
    data_inicio, data_fim, concluida, notificar_quando_concluir,
    notificar_porcentagem, ultima_verificacao, ativa, criado_em, atualizado_em
FROM metas
WHERE id = @id;