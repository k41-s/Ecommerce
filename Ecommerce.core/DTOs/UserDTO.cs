namespace Ecommerce.core.DTOs
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? Phone { get; set; }
        public string? Role { get; set; } = "User";
    }
}
