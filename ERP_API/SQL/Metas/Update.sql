UPDATE metas
SET
    usuario_id = @usuarioId,
    materia_id = @materiaId,
    categoria_id = @categoriaId,
    topico_id = @topicoId,
    titulo = @titulo,
    descricao = @descricao,
    tipo_meta = @tipoMeta,
    quantidade_total = @quantidadeTotal,
    quantidade_atual = @quantidadeAtual,
    unidade = @unidade,
    frequencia = @frequencia,
    dias_semana = @diasSemana,
    data_inicio = @dataInicio,
    data_fim = @dataFim,
    concluida = @concluida,
    notificar_quando_concluir = @notificarQuandoConcluir,
    notificar_porcentagem = @notificarPorcentagem,
    ativa = @ativa
WHERE
    id = @id;