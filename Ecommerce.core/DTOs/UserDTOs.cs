namespace Ecommerce.core.DTOs
{
    public class RegisterUserDTO
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
    }

    public class LoginDTO
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class ChangePasswordDTO
    {
        public string Username { get; set; } = null!;
        public string OldPassword { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }

    public class AuthenticatedUserDTO
    {
        public string Token { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!;
        public string? Name { get; set; }
        public string? Surname { get; set; }
    }
    

}
