using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripPlanner.Application.ViewModels;

namespace TripPlanner.Application.Abstraction
{
    public interface ITripService
    {
        Task<List<TripOverviewViewModel>> GetUserTrips(int userId, string sortOrder);
        Task Create(TripCreateViewModel vm, int userId);
        Task<bool> Delete(int tripId, int userId);
        Task<ItineraryViewModel> GetTripBasic(int tripId, int userId);
        Task<bool> UpdateDescription(int tripId, string description, int userId);
        Task<bool> UpdateName(int tripId, string newName, int userId);
        Task<bool> UpdateDates(int tripId, DateTime startDate, DateTime endDate, int userId);
    }
}
