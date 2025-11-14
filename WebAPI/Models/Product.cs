using System;
using System.Collections.Generic;

namespace WebAPI.Models;

public partial class Product
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public int CategoryId { get; set; }

    public string? ImagePath { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<CustomerOrder> CustomerOrders { get; set; } = new List<CustomerOrder>();

    public virtual ICollection<Country> Countries { get; set; } = new List<Country>();
}
