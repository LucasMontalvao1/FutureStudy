SELECT h.*
FROM historico_anotacoes h
WHERE h.anotacao_id = @anotacaoId AND h.usuario_id = @usuarioId
ORDER BY h.editado_em DESC;