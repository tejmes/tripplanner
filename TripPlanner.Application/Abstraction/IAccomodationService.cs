using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripPlanner.Application.ViewModels;

namespace TripPlanner.Application.Abstraction
{
    public interface IAccomodationService
    {
        Task<bool> SetAccomodation(int tripId, string googlePlaceId, string name, double latitude, double longitude, int userId);
        Task<bool> DeleteAccomodation(int tripId, int userId);
        Task UpdateAccomodationTimes(int accomodationId, TimeSpan? checkIn, TimeSpan? checkOut);
        Task<AccomodationViewModel?> GetByTrip(int tripId);
    }
}
