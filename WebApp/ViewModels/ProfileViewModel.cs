using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModels
{
    public class ProfileViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string Surname { get; set; }

        [Required]
        [Phone]
        public string Phone { get; set; }
    }
}
