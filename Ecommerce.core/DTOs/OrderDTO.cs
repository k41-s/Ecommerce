namespace Ecommerce.core.DTOs
{
    public class OrderDTO
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public int UserId { get; set; }
        public string UserName { get; set; } = null!;
        public DateTime OrderedAt { get; set; }
        public string PaymentMethod { get; set; } = null!;
        public string? Notes { get; set; }
    }
    
}
