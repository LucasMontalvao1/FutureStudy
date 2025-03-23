INSERT INTO metas (
    usuario_id, materia_id, topico_id, titulo, descricao, tipo, 
    quantidade_total, quantidade_atual, unidade, frequencia, dias_semana,
    data_inicio, data_fim, concluida
)
VALUES (
    @usuarioId, @materiaId, @topicoId, @titulo, @descricao, @tipo, 
    @quantidadeTotal, @quantidadeAtual, @unidade, @frequencia, @diasSemana,
    @dataInicio, @dataFim, @concluida
);
SELECT LAST_INSERT_ID();