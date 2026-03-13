using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TripPlanner.Application.Abstraction;

namespace TripPlanner.Web.Areas.User.Controllers
{
    [Area("User")]
    [Authorize]
    public class ChecklistController : Controller
    {
        private readonly IChecklistService checklistService;

        public ChecklistController(IChecklistService checklistService)
        {
            this.checklistService = checklistService;
        }

        public async Task<IActionResult> Index(int tripId)
        {
            ViewData["TripId"] = tripId;
            var vm = await checklistService.GetOrCreateDefaultList(tripId);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateList(int tripId, string title)
        {
            await checklistService.CreateList(tripId, title);

            return RedirectToAction("TripDetail", "Trip", new { area = "User", tripId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rename(int listId, string title)
        {
            var tripId = await checklistService.RenameList(listId, title);

            return RedirectToAction("TripDetail", "Trip", new { area = "User", tripId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteList(int listId)
        {
            var tripId = await checklistService.DeleteList(listId);

            return RedirectToAction("TripDetail", "Trip", new { area = "User", tripId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItem(int listId, string text)
        {
            var tripId = await checklistService.AddItem(listId, text);

            return RedirectToAction("TripDetail", "Trip", new { area = "User", tripId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int itemId)
        {
            var tripId = await checklistService.ToggleItem(itemId);

            return RedirectToAction("TripDetail", "Trip", new { area = "User", tripId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int itemId)
        {
            var tripId = await checklistService.DeleteItem(itemId);

            return RedirectToAction("TripDetail", "Trip", new { area = "User", tripId });
        }
    }
}
