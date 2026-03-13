using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripPlanner.Application.ViewModels
{
    public class SettingsViewModel
    {
        [Required]
        public string UserName { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Zadejte prosím platnou e-mailovou adresu.")]
        public string Email { get; set; }
    }
}
