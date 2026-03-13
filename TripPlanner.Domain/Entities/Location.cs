using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripPlanner.Domain.Entities
{
    public class Location : Entity
    {
        public string GooglePlaceId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
