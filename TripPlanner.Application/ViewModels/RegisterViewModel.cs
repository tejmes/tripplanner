using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripPlanner.Application.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Username je povinné.")]
        public string UserName { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Email je povinný.")]
        [EmailAddress(ErrorMessage = "Zadejte prosím platnou e-mailovou adresu.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Heslo je povinné.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Potvrzení hesla je povinné.")]
        [Compare(nameof(Password), ErrorMessage = "Hesla se musí shodovat.")]
        public string RepeatedPassword { get; set; }
    }
}
