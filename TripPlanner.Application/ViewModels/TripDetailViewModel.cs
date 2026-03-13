using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripPlanner.Application.Dtos;

namespace TripPlanner.Application.ViewModels
{
    public class TripDetailViewModel
    {
        public ItineraryViewModel Itinerary { get; set; } = new();
        public BudgetViewModel Budget { get; set; } = new();
        public List<ChecklistViewModel> Checklists { get; set; } = new();

        public string TripPlacesJson { get; set; }
        public string AccomodationJson { get; set; }
        public string TripDaysListJson { get; set; }
        public string DestinationLat { get; set; }
        public string DestinationLng { get; set; }
    }
}
