using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TripPlanner.Application.Abstraction;
using TripPlanner.Application.ViewModels;
using TripPlanner.Domain.Entities;
using TripPlanner.Infrastructure.Database;

namespace TripPlanner.Application.Implementation
{
    public class CollaboratorService : ICollaboratorService
    {
        private readonly ApplicationDbContext dbContext;

        public CollaboratorService(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<IEnumerable<CollaboratorViewModel>> SearchUsers(int tripId, int requesterId, string term)
        {
            var existing = await dbContext.TripUsers
                .Where(tu => tu.TripId == tripId)
                .Select(tu => tu.UserId)
                .ToListAsync();

            return await dbContext.Users
                .Where(u =>
                    u.Id != requesterId &&
                    !existing.Contains(u.Id) &&
                    (u.UserName.Contains(term) || u.Email.Contains(term)))
                .Select(u => new CollaboratorViewModel
                {
                    UserId = u.Id,
                    UserName = u.UserName,
                    Email = u.Email
                })
                .Take(10)
                .ToListAsync();
        }

        public async Task<bool> AddCollaborator(int tripId, int ownerId, int collaboratorUserId)
        {
            var isOwner = await dbContext.Trips
                .AnyAsync(t => t.Id == tripId && t.UserId == ownerId);

            if (!isOwner) return false;

            var alreadyCollaborator = await dbContext.TripUsers
                .AnyAsync(tu => tu.TripId == tripId && tu.UserId == collaboratorUserId);

            if (!alreadyCollaborator)
            {
                dbContext.TripUsers.Add(new TripUsers
                {
                    TripId = tripId,
                    UserId = collaboratorUserId
                });
                await dbContext.SaveChangesAsync();
            }

            return true;
        }

        public async Task<bool> RemoveCollaborator(int tripId, int ownerId, int userIdToRemove)
        {
            var isOwner = await dbContext.Trips
                .AnyAsync(t => t.Id == tripId && t.UserId == ownerId);
            if (!isOwner) return false;

            var relation = await dbContext.TripUsers
                .FirstOrDefaultAsync(tu => tu.TripId == tripId && tu.UserId == userIdToRemove);

            if (relation != null)
            {
                dbContext.TripUsers.Remove(relation);
                await dbContext.SaveChangesAsync();
            }

            return true;
        }

        public async Task<List<CollaboratorViewModel>> GetByTrip(int tripId)
        {
            var ids = await dbContext.TripUsers
                .Where(tu => tu.TripId == tripId)
                .Select(tu => tu.UserId)
                .ToListAsync();

            return await dbContext.Users
                .Where(u => ids.Contains(u.Id))
                .Select(u => new CollaboratorViewModel
                {
                    UserId = u.Id,
                    UserName = u.UserName,
                    Email = u.Email
                })
                .ToListAsync();
        }
    }
}
