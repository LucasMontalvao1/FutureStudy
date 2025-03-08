using System.ComponentModel.DataAnnotations;

namespace ERP_API.Models.DTOs
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "O nome de usuário é obrigatório")]
        [StringLength(50, ErrorMessage = "O nome de usuário deve ter no máximo 50 caracteres")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória")]
        [StringLength(100, ErrorMessage = "A senha deve ter no máximo 100 caracteres")]
        public string Password { get; set; } = string.Empty;
    }
}