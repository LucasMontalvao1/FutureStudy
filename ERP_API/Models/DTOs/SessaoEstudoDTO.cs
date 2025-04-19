using System;
using System.ComponentModel.DataAnnotations;
using ERP_API.Models.Enums;

namespace ERP_API.Models.DTOs
{
    public class SessaoEstudoRequestDto
    {
        [Required(ErrorMessage = "A categoria é obrigatória")]
        public int CategoriaId { get; set; }

        [Required(ErrorMessage = "A matéria é obrigatória")]
        public int MateriaId { get; set; }

        [Required(ErrorMessage = "O tópico é obrigatório")]
        public int TopicoId { get; set; }
    }

    public class SessaoEstudoResponseDto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int MateriaId { get; set; }
        public int? TopicoId { get; set; }
        public int? CategoriaId { get; set; }         
        public string? NomeCategoria { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public StatusSessao Status { get; set; }
        public TimeSpan TempoEstudado { get; set; }
        public DateTime CriadoEm { get; set; }
        public DateTime AtualizadoEm { get; set; }
        public string NomeMateria { get; set; } = string.Empty;
        public string? NomeTopico { get; set; }
    }

    public class PausaRequestDto
    {
        [Required(ErrorMessage = "O ID da sessão é obrigatório")]
        public int SessaoId { get; set; }
    }

    public class PausaResponseDto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int SessaoId { get; set; }
        public DateTime Inicio { get; set; }
        public DateTime? Fim { get; set; }
        public TimeSpan? DuracaoPausa => Fim.HasValue ? Fim.Value - Inicio : null;
    }

    public class RetomadaRequestDto
    {
        [Required(ErrorMessage = "O ID da pausa é obrigatório")]
        public int PausaId { get; set; }
    }

    public class SessaoDashboardStatsDto
    {
        public TimeSpan TempoTotalEstudado { get; set; }
        public int DiasEstudados { get; set; }
        public int TotalDias { get; set; }
        public string MateriaMaisEstudada { get; set; } = string.Empty;
        public double HorasMateriaMaisEstudada { get; set; }
        public double MediaHorasDiarias => TotalDias > 0 ? Math.Round(TempoTotalEstudado.TotalHours / TotalDias, 1) : 0;
        public double PercentualDiasEstudados => TotalDias > 0 ? Math.Round((double)DiasEstudados / TotalDias * 100, 1) : 0;
    }

    public class SessaoCalendarioDto
    {
        public int Dia { get; set; }
        public int MinutosEstudados { get; set; }
    }
}