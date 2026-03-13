using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripPlanner.Domain.Entities
{
    public class Accomodation : Entity
    {
        public string Name { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public int TripId { get; set; }
        public Trip Trip { get; set; }
        public int LocationId { get; set; }
        public Location Location { get; set; }
    }
}
