using System;
using System.Collections.Generic;

namespace WebAPI.Models;

public partial class ProductImage
{
    public int Id { get; set; }

    public byte[] Data { get; set; } = null!;

    public string MimeType { get; set; } = null!;

    public int ProductId { get; set; }

    public virtual Product Product { get; set; } = null!;
}
