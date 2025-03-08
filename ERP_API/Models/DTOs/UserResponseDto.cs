namespace ERP_API.Models.DTOs
{
    public class UserResponseDto
    {
        public int UsuarioID { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Foto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}