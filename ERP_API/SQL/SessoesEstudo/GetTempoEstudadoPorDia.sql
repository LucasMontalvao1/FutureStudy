SELECT 
    DAY(s.data_inicio) AS dia,
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
WHERE s.usuario_id = @usuarioId
  AND MONTH(s.data_inicio) = @mes
  AND YEAR(s.data_inicio) = @ano
GROUP BY dia
ORDER BY dia;