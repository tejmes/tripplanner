using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripPlanner.Application.ViewModels;

namespace TripPlanner.Application.Abstraction
{
    public interface ICollaboratorService
    {
        Task<IEnumerable<CollaboratorViewModel>> SearchUsers(int tripId, int requesterId, string term);
        Task<bool> AddCollaborator(int tripId, int ownerId, int collaboratorUserId);
        Task<bool> RemoveCollaborator(int tripId, int ownerId, int userIdToRemove);
        Task<List<CollaboratorViewModel>> GetByTrip(int tripId);
    }
}
