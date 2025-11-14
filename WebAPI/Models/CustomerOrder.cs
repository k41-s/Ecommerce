using System;
using System.Collections.Generic;

namespace WebAPI.Models;

public partial class CustomerOrder
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int UserId { get; set; }

    public DateTime OrderedAt { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string? Notes { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
