using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModels
{
    public class OrderVM
    {
        public int Id { get; set; }
        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime OrderedAt { get; set; }

        [Required(ErrorMessage = "Please enter a payment method.")]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } = null!;

        [Required(ErrorMessage = "Please enter a note.")]
        [Display(Name = "Order Notes")]
        public string? Notes { get; set; }
        public bool IsProductDeleted { get; set; }

    }
}
