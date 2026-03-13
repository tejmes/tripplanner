using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripPlanner.Application.ViewModels
{
    public class AccomodationViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public LocationViewModel Location { get; set; }
    }
}
