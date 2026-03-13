using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripPlanner.Application.ViewModels;

namespace TripPlanner.Application.Abstraction
{
    public interface IChecklistService
    {
        Task<ChecklistViewModel> GetOrCreateDefaultList(int tripId);
        Task CreateList(int tripId, string title);
        Task<int> RenameList(int listId, string title);
        Task<int> DeleteList(int listId);
        Task<int> AddItem(int listId, string text);
        Task<int> ToggleItem(int itemId);
        Task<int> DeleteItem(int itemId);
        Task<List<ChecklistViewModel>> GetByTrip(int tripId);
    }
}
