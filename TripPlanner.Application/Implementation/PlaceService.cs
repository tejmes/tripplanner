using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TripPlanner.Application.Abstraction;
using TripPlanner.Application.Dtos;
using TripPlanner.Application.ViewModels;
using TripPlanner.Domain.Entities;
using TripPlanner.Infrastructure.Database;

namespace TripPlanner.Application.Implementation
{
    public class PlaceService : IPlaceService
    {
        private readonly ApplicationDbContext dbContext;

        public PlaceService(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task AddToDay(AddPlaceToDayDto dto)
        {
            var tripDay = await dbContext.TripDays.FindAsync(dto.TripDayId)
                          ?? throw new KeyNotFoundException($"TripDay {dto.TripDayId} not found");

            var location = await dbContext.Locations
                .FirstOrDefaultAsync(l => l.GooglePlaceId == dto.GooglePlaceId);
            if (location == null)
            {
                location = new Location
                {
                    GooglePlaceId = dto.GooglePlaceId,
                    Latitude = dto.Latitude,
                    Longitude = dto.Longitude
                };
                dbContext.Locations.Add(location);
                await dbContext.SaveChangesAsync();
            }

            var maxIndex = await dbContext.Places
                .Where(p => p.TripDayId == dto.TripDayId)
                .MaxAsync(p => (int?)p.OrderIndex) ?? -1;

            var place = new Place
            {
                Name = dto.Name,
                OrderIndex = maxIndex + 1,
                TripId = tripDay.TripId,
                TripDayId = tripDay.Id,
                LocationId = location.Id
            };

            dbContext.Places.Add(place);
            await dbContext.SaveChangesAsync();
        }

        public async Task<int?> Delete(int placeId)
        {
            var place = await dbContext.Places.FindAsync(placeId);
            if (place == null) return null;

            var tripId = place.TripId;
            dbContext.Places.Remove(place);
            await dbContext.SaveChangesAsync();

            return tripId;
        }

        public async Task MovePlace(int placeId, int newTripDayId)
        {
            var place = await dbContext.Places.FindAsync(placeId);
            if (place == null) throw new Exception("Place not found");
            place.TripDayId = newTripDayId;
            await dbContext.SaveChangesAsync();
        }

        public async Task UpdatePlaceOrder(int tripDayId, IList<int> orderedPlaceIds)
        {
            var places = await dbContext.Places
                .Where(p => p.TripDayId == tripDayId)
                .ToListAsync();

            for (int i = 0; i < orderedPlaceIds.Count; i++)
            {
                var p = places.FirstOrDefault(x => x.Id == orderedPlaceIds[i]);
                if (p != null) p.OrderIndex = i;
            }

            await dbContext.SaveChangesAsync();
        }

        public async Task<List<PlaceViewModel>> GetGeneralPlaces(int tripId)
        {
            var allPlaces = await dbContext.Places
                .Include(p => p.Location)
                .Where(p => p.TripId == tripId && p.TripDayId == null)
                .ToListAsync();

            return allPlaces.Select(p => new PlaceViewModel
            {
                Id = p.Id,
                Name = p.Name,
                OrderIndex = p.OrderIndex,
                Location = new LocationViewModel
                {
                    GooglePlaceId = p.Location.GooglePlaceId,
                    Latitude = p.Location.Latitude,
                    Longitude = p.Location.Longitude
                }
            }).ToList();
        }

        public async Task<List<TripDayViewModel>> GetTripDaysWithPlaces(int tripId)
        {
            var days = await dbContext.TripDays
                .Where(td => td.TripId == tripId)
                .OrderBy(td => td.Date)
                .ToListAsync();
            var places = await dbContext.Places
                .Include(p => p.Location)
                .Where(p => p.TripId == tripId && p.TripDayId != null)
                .ToListAsync();

            return days.Select(d => new TripDayViewModel
            {
                Id = d.Id,
                Date = d.Date,
                Places = places
                    .Where(p => p.TripDayId == d.Id)
                    .OrderBy(p => p.OrderIndex)
                    .Select(p => new PlaceViewModel
                    {
                        Id = p.Id,
                        Name = p.Name,
                        OrderIndex = p.OrderIndex,
                        Location = new LocationViewModel
                        {
                            GooglePlaceId = p.Location.GooglePlaceId,
                            Latitude = p.Location.Latitude,
                            Longitude = p.Location.Longitude
                        }
                    })
                    .ToList()
            }).ToList();
        }
    }
}
