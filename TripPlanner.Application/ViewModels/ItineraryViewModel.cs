    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    namespace TripPlanner.Application.ViewModels
    {
        public class ItineraryViewModel
        {
            public int TripId { get; set; }
            public string Name { get; set; }

            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string? Description { get; set; }

            public LocationViewModel DestinationLocation { get; set; }
            public AccomodationViewModel? Accomodation { get; set; }
            public List<TripDayViewModel> TripDays { get; set; } = new();
        }
    }
