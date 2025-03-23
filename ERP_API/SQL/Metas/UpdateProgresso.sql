UPDATE metas
SET quantidade_atual = @quantidadeAtual,
    concluida = CASE WHEN @quantidadeAtual >= quantidade_total THEN TRUE ELSE concluida END
WHERE id = @metaId