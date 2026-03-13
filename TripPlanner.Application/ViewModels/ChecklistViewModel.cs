using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripPlanner.Application.ViewModels
{
    public class ChecklistViewModel
    {
        public int ListId { get; set; }
        public string Title { get; set; }
        public List<ChecklistItemViewModel> Items { get; set; } = new();
    }
}
