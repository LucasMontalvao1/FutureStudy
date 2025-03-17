using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ERP_API.Models
{
    public class User
    {
        [Key]
        public int UsuarioID { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [JsonIgnore] // Não serializar a senha nas respostas JSON
        public string Password { get; set; } = string.Empty;

        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}