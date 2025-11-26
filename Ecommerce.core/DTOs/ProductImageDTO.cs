using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.core.DTOs
{
    public class ProductImageDTO
    {
        public int Id { get; set; }
        public string? Url { get; set; }
        public string? MimeType { get; set; }
    }
}
