namespace WebApp.ViewModels
{
    public class MyOrderVM
    {
        public DateTime OrderedAt { get; set; }
        public string? Notes { get; set; }

        public string ProductName { get; set; } = string.Empty;
        public string? ProductImagePath { get; set; }
    }

}
