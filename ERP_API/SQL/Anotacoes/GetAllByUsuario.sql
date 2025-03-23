SELECT a.*
FROM anotacoes a
WHERE a.usuario_id = @usuarioId
ORDER BY a.criado_em DESC;