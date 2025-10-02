using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModels
{
    public class CategoryVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Category name is required.")]
        [Display(Name = "Category Name")]
        public string Name { get; set; }
    }
}
