using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using TripPlanner.Application.Abstraction;
using TripPlanner.Application.ViewModels;

namespace TripPlanner.Application.Implementation
{
    public class TripDetailService : ITripDetailService
    {
        private readonly ITripService tripService;
        private readonly IPlaceService placeService;
        private readonly IBudgetService expenseService;
        private readonly ICollaboratorService collabService;
        private readonly IChecklistService checklistService;
        private readonly IAccomodationService accomService;
        private readonly IWeatherService weatherService;

        public TripDetailService(
            ITripService tripService,
            IPlaceService placeService,
            IBudgetService expenseService,
            ICollaboratorService collabService,
            IChecklistService checklistService,
            IAccomodationService accomService,
            IWeatherService weatherService)
        {
            this.tripService = tripService;
            this.placeService = placeService;
            this.expenseService = expenseService;
            this.collabService = collabService;
            this.checklistService = checklistService;
            this.accomService = accomService;
            this.weatherService = weatherService;
        }

        public async Task<TripDetailViewModel> GetTripDetail(int tripId, int userId)
        {
            var basic = await tripService.GetTripBasic(tripId, userId);
            if (basic == null) return null!;

            var itinerary = new ItineraryViewModel
            {
                TripId = basic.TripId,
                Name = basic.Name,
                StartDate = basic.StartDate,
                EndDate = basic.EndDate,
                Description = basic.Description,
                DestinationLocation = basic.DestinationLocation,
                Accomodation = await accomService.GetByTrip(tripId),
                TripDays = await placeService.GetTripDaysWithPlaces(tripId)
            };

            await weatherService.ApplyForecastToTripDays(
                itinerary.TripDays,
                itinerary.DestinationLocation);

            var budget = new BudgetViewModel
            {
                TripId = tripId,
                Expenses = await expenseService.GetByTrip(tripId),
                UserBalances = await expenseService.CalculateUserBalances(tripId),
                Settlements = await expenseService.CalculateSettlement(tripId),
                Collaborators = await collabService.GetByTrip(tripId),
                UserTotals = await expenseService.CalculateUserTotals(tripId)
            };

            var checklists = await checklistService.GetByTrip(tripId);

            return new TripDetailViewModel
            {
                Itinerary = itinerary,
                Budget = budget,
                Checklists = checklists,

                TripPlacesJson = JsonSerializer.Serialize(
                    itinerary.TripDays.SelectMany(d => d.Places.Select(p => new
                    {
                        id = p.Id,
                        lat = p.Location.Latitude,
                        lng = p.Location.Longitude,
                        name = p.Name,
                        tripDayId = d.Id,
                        orderIndex = p.OrderIndex
                    })),
                    new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }
                ),

                AccomodationJson = itinerary.Accomodation == null
                    ? "null"
                    : JsonSerializer.Serialize(new
                    {
                        lat = itinerary.Accomodation.Location.Latitude,
                        lng = itinerary.Accomodation.Location.Longitude,
                        name = itinerary.Accomodation.Name,
                        tripId = itinerary.TripId
                    }),

                TripDaysListJson = JsonSerializer.Serialize(
                    itinerary.TripDays.Select(d => new
                    {
                        id = d.Id,
                        display = d.Date?.ToString("dddd, dd. MMMM", new CultureInfo("cs-CZ"))
                    }),
                    new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }
                ),

                DestinationLat = itinerary.DestinationLocation.Latitude.ToString("G", CultureInfo.InvariantCulture),
                DestinationLng = itinerary.DestinationLocation.Longitude.ToString("G", CultureInfo.InvariantCulture)
            };
        }
    }
}
