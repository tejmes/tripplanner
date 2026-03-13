using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripPlanner.Application.ViewModels
{
    public class TripDayViewModel
    {
        public int Id { get; set; }
        public DateTime? Date { get; set; }
        public List<PlaceViewModel> Places { get; set; } = new();

        public double? TempMax { get; set; }
        public double? TempMin { get; set; }
        public int? WeatherCode { get; set; }
    }
}
