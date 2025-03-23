using System;

namespace ERP_API.Models.Entities
{
    public class Anotacao
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int SessaoId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Conteudo { get; set; } = string.Empty;
        public DateTime CriadoEm { get; set; }
        public DateTime AtualizadoEm { get; set; }
    }
}