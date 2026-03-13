using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripPlanner.Domain.Entities.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace TripPlanner.Domain.Entities
{
    public class Trip : Entity
    {
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Description { get; set; }
        public int UserId { get; set; }
        public List<TripUsers> TripUsers { get; set; }
        public int DestinationId { get; set; }
        public Destination Destination { get; set; }

        public Accomodation? Accomodation { get; set; }

        public List<Checklist> Checklists { get; set; } = new();
        public List<Expense> Expenses { get; set; } = new();
    }
}
