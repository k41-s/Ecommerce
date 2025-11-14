namespace WebApp.ViewModels
{
    public class UserWithOrdersVM
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? Role { get; set; }

        public List<OrderInfoViewModel> CustomerOrders { get; set; } = new();
    }

    public class OrderInfoViewModel
    {
        public DateTime OrderedAt { get; set; }
        public string? Notes { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
    }
}
