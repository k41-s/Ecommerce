namespace Ecommerce.core.DTOs
{
    // For creating new products
    public class ProductDTO
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? ImagePath { get; set; }
        public List<int> CountryIds { get; set; }
        public List<string> CountryNames { get; set; }
    }
}
