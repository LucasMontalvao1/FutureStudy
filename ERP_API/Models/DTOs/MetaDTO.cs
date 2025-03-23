using System;
using System.ComponentModel.DataAnnotations;
using ERP_API.Models;
using ERP_API.Models.Enums;

namespace ERP_API.Models.DTOs
{
    public class MetaRequestDto
    {
        [Required(ErrorMessage = "O título da meta é obrigatório")]
        [StringLength(100, ErrorMessage = "O título deve ter no máximo 100 caracteres")]
        public string Titulo { get; set; } = string.Empty;

        public string? Descricao { get; set; }

        public int? MateriaId { get; set; }

        public int? TopicoId { get; set; }

        [Required(ErrorMessage = "O tipo da meta é obrigatório")]
        public TipoMeta Tipo { get; set; }

        [Required(ErrorMessage = "A quantidade total é obrigatória")]
        [Range(1, int.MaxValue, ErrorMessage = "A quantidade total deve ser maior que zero")]
        public int QuantidadeTotal { get; set; }

        [Required(ErrorMessage = "A unidade da meta é obrigatória")]
        public UnidadeMeta Unidade { get; set; }

        public FrequenciaMeta? Frequencia { get; set; }

        public string? DiasSemana { get; set; }

        [Required(ErrorMessage = "A data de início é obrigatória")]
        public DateTime DataInicio { get; set; }

        public DateTime? DataFim { get; set; }
    }

    public class MetaUpdateRequestDto
    {
        [StringLength(100, ErrorMessage = "O título deve ter no máximo 100 caracteres")]
        public string? Titulo { get; set; }

        public string? Descricao { get; set; }

        public int? MateriaId { get; set; }

        public int? TopicoId { get; set; }

        public TipoMeta? Tipo { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "A quantidade total deve ser maior que zero")]
        public int? QuantidadeTotal { get; set; }

        public int? QuantidadeAtual { get; set; }

        public UnidadeMeta? Unidade { get; set; }

        public FrequenciaMeta? Frequencia { get; set; }

        public string? DiasSemana { get; set; }

        public DateTime? DataInicio { get; set; }

        public DateTime? DataFim { get; set; }

        public bool? Concluida { get; set; }
    }

    public class MetaResponseDto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int? MateriaId { get; set; }
        public int? TopicoId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public TipoMeta Tipo { get; set; }
        public int QuantidadeTotal { get; set; }
        public int QuantidadeAtual { get; set; }
        public UnidadeMeta Unidade { get; set; }
        public FrequenciaMeta? Frequencia { get; set; }
        public string? DiasSemana { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public bool Concluida { get; set; }
        public DateTime CriadoEm { get; set; }
        public DateTime AtualizadoEm { get; set; }
        public decimal PercentualConcluido { get; set; }
    }

    public class MetaProgresso
    {
        public int UsuarioId { get; set; }
        public int MetaId { get; set; }
        public int QuantidadeRegistrada { get; set; }
        public DateTime DataRegistro { get; set; }
    }
}