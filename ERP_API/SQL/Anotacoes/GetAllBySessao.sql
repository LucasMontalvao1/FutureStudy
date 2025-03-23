SELECT a.*
FROM anotacoes a
WHERE a.sessao_id = @sessaoId AND a.usuario_id = @usuarioId
ORDER BY a.criado_em DESC;