using System;

namespace ERP_API.Models.Entities
{
    public class HistoricoAnotacao
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int AnotacaoId { get; set; }
        public string ConteudoAnterior { get; set; } = string.Empty;
        public DateTime EditadoEm { get; set; }
    }
}