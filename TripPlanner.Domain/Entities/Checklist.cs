using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripPlanner.Domain.Entities
{
    public class Checklist : Entity
    {
        public string Name { get; set; }
        public int TripId { get; set; }
        public Trip Trip { get; set; }

        public List<ChecklistItem> Items { get; set; } = new();
    }
}
