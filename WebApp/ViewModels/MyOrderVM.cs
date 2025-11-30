namespace WebApp.ViewModels
{
    public class MyOrderVM
    {
        public DateTime OrderedAt { get; set; }
        public string? Notes { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int MainImageId { get; set; }
        public bool IsProductDeleted { get; set; }
    }

}
