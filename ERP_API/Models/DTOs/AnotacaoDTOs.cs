using System;
using System.ComponentModel.DataAnnotations;

namespace ERP_API.Models.DTOs
{
    public class AnotacaoRequestDto
    {
        [Required(ErrorMessage = "O ID da sessão é obrigatório")]
        public int SessaoId { get; set; }

        [Required(ErrorMessage = "O título é obrigatório")]
        [StringLength(100, ErrorMessage = "O título não pode ter mais de 100 caracteres")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "O conteúdo é obrigatório")]
        public string Conteudo { get; set; } = string.Empty;
    }

    public class AnotacaoUpdateDto
    {
        [StringLength(100, ErrorMessage = "O título não pode ter mais de 100 caracteres")]
        public string Titulo { get; set; } = string.Empty;

        public string Conteudo { get; set; } = string.Empty;
    }

    public class AnotacaoResponseDto
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