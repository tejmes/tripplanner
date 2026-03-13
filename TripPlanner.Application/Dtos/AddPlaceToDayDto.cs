using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripPlanner.Application.Dtos
{
    public class AddPlaceToDayDto
    {
        public int TripDayId { get; set; }
        public string Name { get; set; }
        public string GooglePlaceId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
