using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripPlanner.Domain.Entities
{
    public class ChecklistItem : Entity
    {
        public int OrderIndex { get; set; }
        public string Text { get; set; }
        public bool IsChecked { get; set; }
        public int ChecklistId { get; set; }
        public Checklist Checklist { get; set; }
    }
}
