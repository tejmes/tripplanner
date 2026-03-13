using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TripPlanner.Application.Abstraction;
using TripPlanner.Application.ViewModels;
using TripPlanner.Domain.Entities;
using TripPlanner.Infrastructure.Database;

namespace TripPlanner.Application.Implementation
{
    public class TripService : ITripService
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IGoogleAPIService googleApiService;

        public TripService(ApplicationDbContext dbContext, IGoogleAPIService googleApiService)
        {
            this.dbContext = dbContext;
            this.googleApiService = googleApiService;
        }

        public async Task<List<TripOverviewViewModel>> GetUserTrips(int userId, string sortOrder)
        {
            var trips = dbContext.Trips
                .Where(t => t.UserId == userId
                         || dbContext.TripUsers.Any(tu => tu.TripId == t.Id && tu.UserId == userId))
                .Select(t => new TripOverviewViewModel
                {
                    TripId = t.Id,
                    Name = t.Name,
                    StartDate = t.StartDate,
                    IsOwner = t.UserId == userId,
                    PhotoReference = t.Destination.PhotoReference
                });

            trips = sortOrder switch
            {
                "id_desc" => trips.OrderByDescending(vm => vm.TripId),
                "id_asc" => trips.OrderBy(vm => vm.TripId),
                "date_asc" => trips.OrderBy(vm => vm.StartDate),
                "date_desc" => trips.OrderByDescending(vm => vm.StartDate),
                _ => trips.OrderByDescending(vm => vm.TripId),
            };

            return await trips.ToListAsync();
        }

        public async Task Create(TripCreateViewModel vm, int userId)
        {
            var location = await dbContext.Locations
                .FirstOrDefaultAsync(l => l.GooglePlaceId == vm.DestinationLocation.GooglePlaceId);

            if (location == null)
            {
                location = new Location
                {
                    GooglePlaceId = vm.DestinationLocation.GooglePlaceId,
                    Latitude = vm.DestinationLocation.Latitude,
                    Longitude = vm.DestinationLocation.Longitude
                };
                dbContext.Locations.Add(location);
                await dbContext.SaveChangesAsync();
            }

            var destination = await dbContext.Destinations
                .FirstOrDefaultAsync(d => d.LocationId == location.Id);

            if (destination == null)
            {
                destination = new Destination
                {
                    Name = vm.DestinationName,
                    Country = vm.Country,
                    LocationId = location.Id
                };
                dbContext.Destinations.Add(destination);
                await dbContext.SaveChangesAsync();
            }

            var photo = await googleApiService.GetPhoto(vm.DestinationLocation.GooglePlaceId);

            if (!string.IsNullOrEmpty(photo))
            {
                destination.PhotoReference = photo;
                dbContext.Destinations.Update(destination);
                await dbContext.SaveChangesAsync();
            }

            var trip = new Trip
            {
                Name = vm.Name,
                StartDate = vm.StartDate,
                EndDate = vm.EndDate,
                Description = vm.Description,
                UserId = userId,
                DestinationId = destination.Id
            };
            dbContext.Trips.Add(trip);
            await dbContext.SaveChangesAsync();

            var tripDays = new List<TripDay>();
            for (var date = vm.StartDate.Date; date <= vm.EndDate.Date; date = date.AddDays(1))
                tripDays.Add(new TripDay { TripId = trip.Id, Date = date });

            dbContext.TripDays.AddRange(tripDays);
            await dbContext.SaveChangesAsync();
        }

        public async Task<bool> Delete(int tripId, int userId)
        {
            var trip = await dbContext.Trips
                .FirstOrDefaultAsync(t => t.Id == tripId && t.UserId == userId);
            if (trip == null) return false;

            dbContext.Trips.Remove(trip);
            await dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateName(int tripId, string newName, int userId)
        {
            var trip = await dbContext.Trips
                .FirstOrDefaultAsync(t => t.Id == tripId && t.UserId == userId);
            if (trip == null) return false;

            trip.Name = newName;
            await dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateDescription(int tripId, string description, int userId)
        {
            var trip = await dbContext.Trips
                .FirstOrDefaultAsync(t => t.Id == tripId && t.UserId == userId);
            if (trip == null) return false;

            trip.Description = description;
            await dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateDates(int tripId, DateTime newStart, DateTime newEnd, int userId)
        {
            var trip = await dbContext.Trips
                .FirstOrDefaultAsync(t => t.Id == tripId && t.UserId == userId);
            if (trip == null || newStart > newEnd) return false;

            trip.StartDate = newStart;
            trip.EndDate = newEnd;

            var existingDays = await dbContext.TripDays.Where(td => td.TripId == tripId).ToListAsync();
            var toRemove = existingDays.Where(d => d.Date < newStart || d.Date > newEnd).ToList();
            foreach (var d in toRemove)
            {
                dbContext.Places.RemoveRange(dbContext.Places.Where(p => p.TripDayId == d.Id));
            }
            dbContext.TripDays.RemoveRange(toRemove);

            for (var date = newStart; date <= newEnd; date = date.AddDays(1))
            {
                if (!existingDays.Any(d => d.Date == date))
                    dbContext.TripDays.Add(new TripDay { TripId = tripId, Date = date });
            }

            await dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<ItineraryViewModel> GetTripBasic(int tripId, int userId)
        {
            var trip = await dbContext.Trips
                .Include(t => t.Destination)
                    .ThenInclude(d => d.Location)
                .FirstOrDefaultAsync(t =>
                    t.Id == tripId &&
                    (t.UserId == userId || dbContext.TripUsers.Any(tu => tu.TripId == t.Id && tu.UserId == userId))
                );

            if (trip == null) return null!; // případně throw nebo vrátit null

            return new ItineraryViewModel
            {
                TripId = trip.Id,
                Name = trip.Name,
                StartDate = trip.StartDate,
                EndDate = trip.EndDate,
                Description = trip.Description,
                DestinationLocation = new LocationViewModel
                {
                    GooglePlaceId = trip.Destination.Location.GooglePlaceId,
                    Latitude = trip.Destination.Location.Latitude,
                    Longitude = trip.Destination.Location.Longitude
                }
            };
        }
    }
}
