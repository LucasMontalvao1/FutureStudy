﻿using System;

namespace ERP_API.Models
{
    public class SessaoEstudo
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int MateriaId { get; set; }
        public int? TopicoId { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public string Status { get; set; } = "em_andamento";
        public TimeSpan TempoEstudado { get; set; }
        public DateTime CriadoEm { get; set; }
        public DateTime AtualizadoEm { get; set; }
    }

    public class PausaSessao
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int SessaoId { get; set; }
        public DateTime Inicio { get; set; }
        public DateTime? Fim { get; set; }
        public DateTime CriadoEm { get; set; }
    }
}