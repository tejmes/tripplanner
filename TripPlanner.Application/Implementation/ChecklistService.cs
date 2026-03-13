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
    public class ChecklistService : IChecklistService
    {
        private readonly ApplicationDbContext dbContext;

        public ChecklistService(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<ChecklistViewModel> GetOrCreateDefaultList(int tripId)
        {
            var checklist = await dbContext.Checklists
                .Include(c => c.Items.OrderBy(i => i.OrderIndex))
                .FirstOrDefaultAsync(c => c.TripId == tripId);

            if (checklist == null)
            {
                checklist = new Checklist { TripId = tripId, Name = "Balení" };
                dbContext.Checklists.Add(checklist);
                await dbContext.SaveChangesAsync();
                await dbContext.Entry(checklist).Collection(c => c.Items).LoadAsync();
            }

            return new ChecklistViewModel
            {
                ListId = checklist.Id,
                Title = checklist.Name,
                Items = checklist.Items
                    .Select(i => new ChecklistItemViewModel
                    {
                        Id = i.Id,
                        Text = i.Text,
                        IsChecked = i.IsChecked
                    })
                    .ToList()
            };
        }

        public async Task CreateList(int tripId, string title)
        {
            dbContext.Checklists.Add(new Checklist { TripId = tripId, Name = title });
            await dbContext.SaveChangesAsync();
        }

        public async Task<int> RenameList(int listId, string title)
        {
            var checklist = await dbContext.Checklists.FindAsync(listId);
            if (checklist == null) throw new KeyNotFoundException($"Checklist {listId} not found");
            checklist.Name = title;
            var tripId = checklist.TripId;

            await dbContext.SaveChangesAsync();
            return tripId;
        }

        public async Task<int> DeleteList(int listId)
        {
            var checklist = await dbContext.Checklists
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == listId);
            if (checklist == null) throw new KeyNotFoundException();
            var tripId = checklist.TripId;

            dbContext.ChecklistItems.RemoveRange(checklist.Items);
            dbContext.Checklists.Remove(checklist);

            await dbContext.SaveChangesAsync();
            return tripId;
        }

        public async Task<int> AddItem(int listId, string text)
        {
            var checklist = await dbContext.Checklists.FindAsync(listId);
            if (checklist == null) throw new KeyNotFoundException();
            var item = new ChecklistItem
            {
                ChecklistId = listId,
                Text = text,
                IsChecked = false,
                OrderIndex = await dbContext.ChecklistItems.CountAsync(i => i.ChecklistId == listId)
            };
            dbContext.ChecklistItems.Add(item);

            await dbContext.SaveChangesAsync();
            return checklist.TripId;
        }

        public async Task<int> ToggleItem(int itemId)
        {
            var item = await dbContext.ChecklistItems.FindAsync(itemId);
            if (item == null) throw new KeyNotFoundException();
            item.IsChecked = !item.IsChecked;
            await dbContext.SaveChangesAsync();
            var tripId = await dbContext.Checklists
                .Where(c => c.Id == item.ChecklistId)
                .Select(c => c.TripId)
                .FirstAsync();

            return tripId;
        }

        public async Task<int> DeleteItem(int itemId)
        {
            var item = await dbContext.ChecklistItems.FindAsync(itemId);
            if (item == null) throw new KeyNotFoundException();
            var checklistId = item.ChecklistId;
            dbContext.ChecklistItems.Remove(item);
            await dbContext.SaveChangesAsync();
            var tripId = await dbContext.Checklists
                .Where(c => c.Id == checklistId)
                .Select(c => c.TripId)
                .FirstAsync();

            return tripId;
        }

        public async Task<List<ChecklistViewModel>> GetByTrip(int tripId)
        {
            var lists = await dbContext.Checklists
                .Include(c => c.Items.OrderBy(i => i.OrderIndex))
                .Where(c => c.TripId == tripId)
                .ToListAsync();

            return lists.Select(c => new ChecklistViewModel
            {
                ListId = c.Id,
                Title = c.Name,
                Items = c.Items.Select(i => new ChecklistItemViewModel
                {
                    Id = i.Id,
                    Text = i.Text,
                    IsChecked = i.IsChecked
                }).ToList()
            }).ToList();
        }
    }
}
