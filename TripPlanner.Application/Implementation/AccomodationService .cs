using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripPlanner.Application.Abstraction;
using TripPlanner.Application.ViewModels;
using TripPlanner.Domain.Entities;
using TripPlanner.Infrastructure.Database;

namespace TripPlanner.Application.Implementation
{
    public class AccomodationService : IAccomodationService
    {
        private readonly ApplicationDbContext dbContext;

        public AccomodationService(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<bool> SetAccomodation(int tripId, string googlePlaceId, string name, double latitude, double longitude, int userId)
        {
            var trip = await dbContext.Trips
                .Include(t => t.TripUsers)
                .FirstOrDefaultAsync(t => t.Id == tripId &&
                    (t.UserId == userId || t.TripUsers.Any(u => u.UserId == userId)));

            if (trip == null) return false;

            var loc = await dbContext.Locations.FirstOrDefaultAsync(l => l.GooglePlaceId == googlePlaceId);
            if (loc == null)
            {
                loc = new Location
                {
                    GooglePlaceId = googlePlaceId,
                    Latitude = latitude,
                    Longitude = longitude
                };
                await dbContext.Locations.AddAsync(loc);
                await dbContext.SaveChangesAsync();
            }

            var accom = await dbContext.Accomodations
                .FirstOrDefaultAsync(a => a.TripId == tripId);

            if (accom == null)
            {
                accom = new Accomodation
                {
                    TripId = tripId,
                    LocationId = loc.Id,
                    Location = loc,
                    Name = name
                };
                await dbContext.Accomodations.AddAsync(accom);
            }
            else
            {
                accom.LocationId = loc.Id;
                accom.Name = name;
                dbContext.Accomodations.Update(accom);
            }

            await dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAccomodation(int tripId, int userId)
        {
            var accom = await dbContext.Accomodations
                .Include(a => a.Trip)
                .FirstOrDefaultAsync(a => a.TripId == tripId && a.Trip.UserId == userId);
            if (accom == null) return false;

            dbContext.Accomodations.Remove(accom);
            await dbContext.SaveChangesAsync();
            return true;
        }

        public async Task UpdateAccomodationTimes(int accomodationId, TimeSpan? checkIn, TimeSpan? checkOut)
        {
            var acc = await dbContext.Accomodations.FindAsync(accomodationId) ?? throw new KeyNotFoundException($"Accomodation {accomodationId} not found");

            if (checkIn.HasValue)
            {
                var baseDate = acc.CheckIn?.Date ?? DateTime.Today;
                acc.CheckIn = baseDate.Add(checkIn.Value);
            }

            if (checkOut.HasValue)
            {
                var baseDate = acc.CheckOut?.Date ?? DateTime.Today;
                acc.CheckOut = baseDate.Add(checkOut.Value);
            }

            await dbContext.SaveChangesAsync();
        }

        public async Task<AccomodationViewModel?> GetByTrip(int tripId)
        {
            var acc = await dbContext.Accomodations
                .Include(x => x.Location)
                .FirstOrDefaultAsync(x => x.TripId == tripId);
            if (acc == null) return null;

            return new AccomodationViewModel
            {
                Id = acc.Id,
                Name = acc.Name,
                CheckIn = acc.CheckIn,
                CheckOut = acc.CheckOut,
                Location = new LocationViewModel
                {
                    GooglePlaceId = acc.Location.GooglePlaceId,
                    Latitude = acc.Location.Latitude,
                    Longitude = acc.Location.Longitude
                }
            };
        }
    }
}
