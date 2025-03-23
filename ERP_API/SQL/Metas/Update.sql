UPDATE metas
SET 
    materia_id = @materiaId,
    topico_id = @topicoId,
    titulo = @titulo,
    descricao = @descricao,
    tipo = @tipo,
    quantidade_total = @quantidadeTotal,
    quantidade_atual = @quantidadeAtual,
    unidade = @unidade,
    frequencia = @frequencia,
    dias_semana = @diasSemana,
    data_inicio = @dataInicio,
    data_fim = @dataFim,
    concluida = @concluida
WHERE id = @id AND usuario_id = @usuarioId