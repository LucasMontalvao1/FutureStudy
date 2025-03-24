SELECT 
    a.*
FROM 
    anotacoes a
WHERE 
    a.usuario_id = @usuarioId 
    AND a.criado_em >= @dataInicio 
    AND a.criado_em <= @dataFim
ORDER BY 
    a.criado_em DESC;