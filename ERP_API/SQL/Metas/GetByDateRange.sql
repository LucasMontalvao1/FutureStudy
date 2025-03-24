SELECT 
    m.*
FROM 
    metas m
WHERE 
    m.usuario_id = @usuarioId
    AND (
        (m.data_inicio <= @dataFim AND (m.data_fim IS NULL OR m.data_fim >= @dataInicio))
        OR
        (m.data_inicio BETWEEN @dataInicio AND @dataFim)
    )
ORDER BY 
    m.data_inicio;