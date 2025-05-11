using ERP_API.Models.Enums;
using System;

namespace ERP_API.Models
{
    public class Meta
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int? MateriaId { get; set; }
        public int? CategoriaId { get; set; }
        public int? TopicoId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public TipoMeta TipoMeta { get; set; }
        public decimal QuantidadeTotal { get; set; }
        public decimal QuantidadeAtual { get; set; } = 0;
        public UnidadeMeta Unidade { get; set; }
        public FrequenciaMeta? Frequencia { get; set; }
        public string? DiasSemana { get; set; } 
        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public bool Concluida { get; set; } = false;
        public bool NotificarQuandoConcluir { get; set; } = true;
        public int NotificarPorcentagem { get; set; } = 100;
        public DateTime? UltimaVerificacao { get; set; }
        public bool Ativa { get; set; } = true;
        public DateTime CriadoEm { get; set; }
        public DateTime AtualizadoEm { get; set; }
    }
}