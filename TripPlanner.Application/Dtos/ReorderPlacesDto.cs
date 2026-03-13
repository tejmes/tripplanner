using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripPlanner.Application.Dtos
{
    public class ReorderPlacesDto
    {
        public int TripDayId { get; set; }
        public List<int> OrderedPlaceIds { get; set; }
    }
}
