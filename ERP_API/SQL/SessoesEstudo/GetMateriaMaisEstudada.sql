SELECT 
    m.nome AS materia_nome,
    SUM(
        TIMESTAMPDIFF(MINUTE, 
            s.data_inicio, 
            IFNULL(s.data_fim, NOW())
        ) - 
        IFNULL((
            SELECT SUM(
                TIMESTAMPDIFF(MINUTE, 
                    p.inicio, 
                    IFNULL(p.fim, NOW())
                )
            )
            FROM pausas_sessao p
            WHERE p.sessao_id = s.id
        ), 0)
    ) AS tempo_estudado_minutos
FROM sessoes_estudo s
JOIN materias m ON s.materia_id = m.id
WHERE s.usuario_id = @usuarioId
  AND s.data_inicio BETWEEN @dataInicio AND @dataFim
GROUP BY s.materia_id, m.nome
ORDER BY tempo_estudado_minutos DESC
LIMIT 1;