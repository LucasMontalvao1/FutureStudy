SELECT 
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
    ), 0) AS tempo_estudado_segundos
FROM sessoes_estudo s
WHERE s.id = @sessaoId;