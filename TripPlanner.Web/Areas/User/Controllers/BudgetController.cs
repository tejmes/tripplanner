using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TripPlanner.Application.Abstraction;

namespace TripPlanner.Web.Areas.User.Controllers
{
    [Area("User")]
    [Authorize]
    public class BudgetController : Controller
    {
        private readonly IBudgetService expenseService;

        public BudgetController(IBudgetService expenseService)
        {
            this.expenseService = expenseService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddExpense(int tripId, decimal amount, string description, List<int> sharedWithUserIds)
        {
            var paidByUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            await expenseService.AddExpense(tripId, paidByUserId, amount, description, sharedWithUserIds);

            return RedirectToAction("TripDetail", "Trip", new { area = "User", tripId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteExpense(int expenseId, int tripId)
        {
            var deleted = await expenseService.DeleteExpense(expenseId);
            if (!deleted) return NotFound();

            return RedirectToAction("TripDetail", "Trip", new { area = "User", tripId });
        }   
    }
}
