INSERT INTO sessoes_estudo (usuario_id, materia_id, topico_id, categoria_Id, data_inicio, status)
VALUES (@usuarioId, @materiaId, @topicoId, @categoriaId, NOW(), @status);

SELECT LAST_INSERT_ID();