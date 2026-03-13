using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripPlanner.Domain.Entities
{
    public class Destination : Entity
    {
        public string Name { get; set; }
        public string Country { get; set; }
        public int LocationId { get; set; }
        public Location Location { get; set; }
        public string? PhotoReference { get; set; }
    }
}
