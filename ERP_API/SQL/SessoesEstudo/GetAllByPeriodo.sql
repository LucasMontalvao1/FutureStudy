SELECT 
    s.id,
    s.usuario_id,
    s.materia_id,
    m.nome AS nome_materia,
    s.topico_id,
    t.nome AS nome_topico,
    c.id AS categoria_id,
    c.nome AS nome_categoria,
    s.data_inicio,
    s.data_fim,
    s.status,
    s.criado_em,
    s.atualizado_em,
    COALESCE(
        TIMESTAMPDIFF(SECOND, 
            s.data_inicio, 
            IFNULL(s.data_fim, NOW())
        ) - 
        IFNULL((
            SELECT SUM(
                TIMESTAMPDIFF(SECOND, 
                    p.inicio, 
                    IFNULL(p.fim, NOW())
                )
            )
            FROM pausas_sessao p
            WHERE p.sessao_id = s.id
        ), 0),
        0
    ) AS tempo_estudado_segundos
FROM sessoes_estudo s
LEFT JOIN materias m ON s.materia_id = m.id
LEFT JOIN topicos t ON s.topico_id = t.id
LEFT JOIN categorias c ON m.categoria_id = c.id
WHERE s.usuario_id = @usuarioId 
  AND s.data_inicio BETWEEN @dataInicio AND @dataFim
ORDER BY s.data_inicio DESC;
