using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModels
{
    public class CountryVM
    {
        public int Id { get; set; }


        [Required(ErrorMessage = "Country name is required.")]
        [Display(Name = "Country Name")]
        public string Name { get; set; }

    }
}
