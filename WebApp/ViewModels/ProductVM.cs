using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModels
{
    public class ProductVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Display(Name = "Category")]
        public string CategoryName { get; set; } = string.Empty;

        [Display(Name = "Image Path")]
        public string? ImagePath { get; set; } = string.Empty;

        [Required(ErrorMessage = "Countries are required")]
        [Display(Name = "Selected Countries")]
        public List<int> CountryIds { get; set; } = new();

        [Display(Name = "Countries Available")]
        public List<string> CountryNames { get; set; } = new();
    }
}
