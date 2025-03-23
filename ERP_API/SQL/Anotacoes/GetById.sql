SELECT a.*
FROM anotacoes a
WHERE a.id = @id AND a.usuario_id = @usuarioId;