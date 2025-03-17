using System;
using System.ComponentModel.DataAnnotations;

namespace ERP_API.Models.DTOs
{
    public class SessaoEstudoRequestDto
    {
        [Required(ErrorMessage = "A matéria é obrigatória")]
        public int MateriaId { get; set; }

        public int? TopicoId { get; set; }
    }

    public class SessaoEstudoResponseDto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int MateriaId { get; set; }
        public int? TopicoId { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public string Status { get; set; } = string.Empty;
        public TimeSpan TempoEstudado { get; set; }
        public DateTime CriadoEm { get; set; }
        public DateTime AtualizadoEm { get; set; }
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
    }

    public class RetomadaRequestDto
    {
        [Required(ErrorMessage = "O ID da pausa é obrigatório")]
        public int PausaId { get; set; }
    }
}