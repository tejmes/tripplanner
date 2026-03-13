using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripPlanner.Application.Dtos;
using TripPlanner.Application.ViewModels;

namespace TripPlanner.Application.Abstraction
{
    public interface IPlaceService
    {
        Task AddToDay(AddPlaceToDayDto dto);
        Task<int?> Delete(int placeId);
        Task MovePlace(int placeId, int newTripDayId);
        Task UpdatePlaceOrder(int tripDayId, IList<int> orderedPlaceIds);
        Task<List<PlaceViewModel>> GetGeneralPlaces(int tripId);
        Task<List<TripDayViewModel>> GetTripDaysWithPlaces(int tripId);
    }
}
