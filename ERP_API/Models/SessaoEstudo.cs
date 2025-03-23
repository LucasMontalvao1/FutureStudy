using System;
using ERP_API.Models.Enums;

namespace ERP_API.Models.Entities
{
    public class SessaoEstudo
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int MateriaId { get; set; }
        public int? TopicoId { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public StatusSessao Status { get; set; } = StatusSessao.EmAndamento;
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