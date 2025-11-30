namespace Ecommerce.core.DTOs
{
    public class ProductDTO
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public bool IsDeleted { get; set; }
        public List<int> ImageIds { get; set; }
        public List<int> CountryIds { get; set; }
        public List<string> CountryNames { get; set; }
    }
}
