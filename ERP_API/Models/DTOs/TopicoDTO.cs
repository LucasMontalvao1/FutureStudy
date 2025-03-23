using System;
using System.ComponentModel.DataAnnotations;

namespace ERP_API.Models.DTOs
{
    public class TopicoDTO
    {
        [Required(ErrorMessage = "O nome do tópico é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome do tópico deve ter no máximo 100 caracteres")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "A matéria é obrigatória")]
        public int MateriaId { get; set; }
    }

    public class TopicoResponseDto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int MateriaId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public DateTime CriadoEm { get; set; }
        public DateTime AtualizadoEm { get; set; }
    }

    public class TopicoUpdateRequestDto
    {
        [StringLength(100, ErrorMessage = "O nome do tópico deve ter no máximo 100 caracteres")]
        public string? Nome { get; set; }

        public int? MateriaId { get; set; }
    }
}