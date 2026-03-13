using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripPlanner.Domain.Entities.Interfaces;

namespace TripPlanner.Domain.Entities
{
    public class TripUsers : Entity
    {
        public int TripId { get; set; }
        public Trip Trip { get; set; }
        public int UserId { get; set; }
    }
}
