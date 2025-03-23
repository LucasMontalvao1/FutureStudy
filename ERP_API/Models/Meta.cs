using ERP_API.Models.Enums;
using System;

namespace ERP_API.Models.Enums
{
    public class Meta
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int? MateriaId { get; set; }
        public int? TopicoId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public TipoMeta Tipo { get; set; }
        public int QuantidadeTotal { get; set; }
        public int QuantidadeAtual { get; set; } = 0;
        public UnidadeMeta Unidade { get; set; }
        public FrequenciaMeta? Frequencia { get; set; }
        public string? DiasSemana { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public bool Concluida { get; set; } = false;
        public DateTime CriadoEm { get; set; }
        public DateTime AtualizadoEm { get; set; }
    }
}