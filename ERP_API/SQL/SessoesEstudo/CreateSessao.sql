INSERT INTO sessoes_estudo (usuario_id, materia_id, topico_id, data_inicio, status)
VALUES (@usuarioId, @materiaId, @topicoId, NOW(), @status);
SELECT LAST_INSERT_ID();