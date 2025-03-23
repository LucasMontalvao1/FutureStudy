UPDATE anotacoes
SET titulo = @titulo, conteudo = @conteudo
WHERE id = @id AND usuario_id = @usuarioId;