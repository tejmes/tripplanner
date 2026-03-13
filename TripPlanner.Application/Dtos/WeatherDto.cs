using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripPlanner.Application.Dtos
{
    public class WeatherDto
    {
        public DateTime Date { get; set; }
        public double TemperatureMax { get; set; }
        public double TemperatureMin { get; set; }
        public int WeatherCode { get; set; }
    }
}
