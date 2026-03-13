using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripPlanner.Application.ViewModels
{
    public class ChecklistItemViewModel
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public bool IsChecked { get; set; }

    }
}
