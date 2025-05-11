using ERP_API.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace ERP_API.Models.DTOs
{
    public class MetaRequestDto
    {
        [Required(ErrorMessage = "O título da meta é obrigatório")]
        [StringLength(100, ErrorMessage = "O título deve ter no máximo 100 caracteres")]
        public string Titulo { get; set; } = string.Empty;

        public string? Descricao { get; set; }
        public int? MateriaId { get; set; }
        public int? CategoriaId { get; set; }
        public int? TopicoId { get; set; }

        [Required(ErrorMessage = "O tipo da meta é obrigatório")]
        public TipoMeta TipoMeta { get; set; }

        [Required(ErrorMessage = "A quantidade total é obrigatória")]
        [Range(0.01, double.MaxValue, ErrorMessage = "A quantidade total deve ser maior que zero")]
        public decimal QuantidadeTotal { get; set; }

        [Required(ErrorMessage = "A unidade da meta é obrigatória")]
        public UnidadeMeta Unidade { get; set; }

        public FrequenciaMeta? Frequencia { get; set; }
        public string? DiasSemana { get; set; } // Ex: "seg,ter,qua"

        [Required(ErrorMessage = "A data de início é obrigatória")]
        public DateTime DataInicio { get; set; }

        public DateTime? DataFim { get; set; }
        public bool NotificarQuandoConcluir { get; set; } = true;
        public int NotificarPorcentagem { get; set; } = 100;
        public bool Ativa { get; set; } = true;
    }

    public class MetaUpdateRequestDto
    {
        [StringLength(100, ErrorMessage = "O título deve ter no máximo 100 caracteres")]
        public string? Titulo { get; set; }

        public string? Descricao { get; set; }
        public int? MateriaId { get; set; }
        public int? CategoriaId { get; set; }
        public int? TopicoId { get; set; }
        public TipoMeta? TipoMeta { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "A quantidade total deve ser maior que zero")]
        public decimal? QuantidadeTotal { get; set; }

        public decimal? QuantidadeAtual { get; set; }
        public UnidadeMeta? Unidade { get; set; }
        public FrequenciaMeta? Frequencia { get; set; }
        public string? DiasSemana { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public bool? Concluida { get; set; }
        public bool? NotificarQuandoConcluir { get; set; }
        public int? NotificarPorcentagem { get; set; }
        public bool? Ativa { get; set; }
    }

    public class MetaResponseDto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int? MateriaId { get; set; }
        public string? MateriaNome { get; set; }
        public int? CategoriaId { get; set; }
        public string? CategoriaNome { get; set; }
        public int? TopicoId { get; set; }
        public string? TopicoNome { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public TipoMeta TipoMeta { get; set; }
        public decimal QuantidadeTotal { get; set; }
        public decimal QuantidadeAtual { get; set; }
        public UnidadeMeta Unidade { get; set; }
        public FrequenciaMeta? Frequencia { get; set; }
        public string? DiasSemana { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public bool Concluida { get; set; }
        public bool NotificarQuandoConcluir { get; set; }
        public int NotificarPorcentagem { get; set; }
        public DateTime? UltimaVerificacao { get; set; }
        public bool Ativa { get; set; }
        public DateTime CriadoEm { get; set; }
        public DateTime AtualizadoEm { get; set; }
        public decimal PercentualConcluido { get; set; }
    }
}
