using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripPlanner.Domain.Entities;

namespace TripPlanner.Application.ViewModels
{
    public class TripCreateViewModel
    {
        [Required(ErrorMessage = "Výlet musí být pojmenovaný.")]
        [Display(Name = "Jméno")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Výlet musí mít začátek.")]
        [Display(Name = "Datum začátku")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Výlet musí mít konec.")]
        [Display(Name = "Datum konce")]
        public DateTime EndDate { get; set; }

        [Display(Name = "Popis výletu")]
        [MaxLength(500, ErrorMessage = "Popis nesmí přesáhnout 500 znaků.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Musíte vybrat destinaci.")]
        public string DestinationName { get; set; }
        public string Country {  get; set; }
        public LocationViewModel DestinationLocation { get; set; }
    }
}
