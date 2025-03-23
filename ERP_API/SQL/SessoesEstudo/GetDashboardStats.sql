SELECT 
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
    ) AS tempo_total_minutos,
    COUNT(DISTINCT DATE(s.data_inicio)) AS dias_com_estudo,
    DATEDIFF(@dataFim, @dataInicio) + 1 AS total_dias
FROM sessoes_estudo s
WHERE s.usuario_id = @usuarioId
  AND s.data_inicio BETWEEN @dataInicio AND @dataFim;
