namespace ERP_API.Models.DTOs
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public UserResponseDto User { get; set; } = new UserResponseDto();
    }
}