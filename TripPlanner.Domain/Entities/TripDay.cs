using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripPlanner.Domain.Entities
{
    public class TripDay : Entity
    {
        public DateTime Date { get; set; }
        public int TripId { get; set; }
        public Trip Trip { get; set; }
    }
}
