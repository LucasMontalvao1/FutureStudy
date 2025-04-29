using System.ComponentModel.DataAnnotations;

namespace ERP_API.Models.DTOs
{
    public class MateriaRequestDto
    {
        [Required(ErrorMessage = "O nome da matéria é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome da matéria deve ter no máximo 100 caracteres")]
        public string Nome { get; set; } = string.Empty;

        [StringLength(7, MinimumLength = 7, ErrorMessage = "A cor deve estar no formato hexadecimal #RRGGBB")]
        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "A cor deve estar no formato hexadecimal #RRGGBB")]
        public string Cor { get; set; } = "#CCCCCC";

        [Required(ErrorMessage = "A categoria é obrigatória")]
        public int CategoriaId { get; set; }
    }

    public class MateriaResponseDto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Cor { get; set; } = "#CCCCCC";
        public int CategoriaId { get; set; }
        public DateTime? CriadoEm { get; set; }
        public DateTime? AtualizadoEm { get; set; }
    }

    public class MateriaUpdateRequestDto
    {
        [StringLength(100, ErrorMessage = "O nome da matéria deve ter no máximo 100 caracteres")]
        public string? Nome { get; set; }

        [StringLength(7, MinimumLength = 7, ErrorMessage = "A cor deve estar no formato hexadecimal #RRGGBB")]
        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "A cor deve estar no formato hexadecimal #RRGGBB")]
        public string? Cor { get; set; }

        public int? CategoriaId { get; set; } 
    }
}