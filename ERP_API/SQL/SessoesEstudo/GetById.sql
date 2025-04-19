SELECT 
    s.id, s.usuario_id, s.materia_id, s.topico_id, s.categoria_id,
    s.data_inicio, s.data_fim, s.status, 
    s.criado_em, s.atualizado_em,
    
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
    ) AS tempo_estudado_segundos,

    m.nome AS nome_materia,
    t.nome AS nome_topico,
    c.nome AS nome_categoria

FROM sessoes_estudo s
LEFT JOIN materias m ON s.materia_id = m.id
LEFT JOIN topicos t ON s.topico_id = t.id
LEFT JOIN categorias c ON s.categoria_id = c.id

WHERE s.id = @id AND s.usuario_id = @usuarioId;
