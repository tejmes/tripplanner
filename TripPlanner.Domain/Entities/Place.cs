using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripPlanner.Domain.Entities
{
    public class Place : Entity
    {
        public string Name { get; set; }
        public int OrderIndex { get; set; }
        public int LocationId { get; set; }
        public Location Location { get; set; }
        public int TripId { get; set; }
        public Trip Trip { get; set; }
        public int? TripDayId { get; set; }
        public TripDay? TripDay { get; set; }
    }
}
