namespace Ecommerce.core.DTOs
{
    public class UserWithOrdersDTO
    {
        public string Username { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? Role { get; set; }

        public List<OrderDTO> Orders { get; set; } = new();
    }
}
