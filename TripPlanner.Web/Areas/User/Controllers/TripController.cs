using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TripPlanner.Application.Abstraction;
using TripPlanner.Application.ViewModels;

namespace TripPlanner.Web.Areas.User.Controllers
{
    [Area("User")]
    [Authorize]
    public class TripController : Controller
    {
        private readonly ITripDetailService tripDetailService;
        private readonly ITripService tripService;

        public TripController(ITripDetailService tripDetailService, ITripService tripService)
        {
            this.tripDetailService = tripDetailService;
            this.tripService = tripService;
        }

        public async Task<IActionResult> TripsOverview(string sortOrder)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);

            ViewData["CurrentSort"] = sortOrder ?? "id_desc";
            var trips = await tripService.GetUserTrips(userId, sortOrder);

            return View(trips);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TripCreateViewModel vm)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);

            if (ModelState.IsValid)
            {
                await tripService.Create(vm, userId);

                return RedirectToAction(nameof(TripsOverview));
            }

            return RedirectToAction(nameof(TripsOverview));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int tripId)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);

            bool deleted = await tripService.Delete(tripId, userId);

            if (deleted) return RedirectToAction(nameof(TripsOverview));

            return NotFound();
        }
        
        public async Task<IActionResult> TripDetail(int tripId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var vm = await tripDetailService.GetTripDetail(tripId, userId);
            if (vm == null) return NotFound();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDescription(int tripId, string description)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var descriptionUpdated = await tripService.UpdateDescription(tripId, description, userId);
            if (!descriptionUpdated)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(TripDetail), new { area = "User", tripId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateName(int tripId, string newName)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var success = await tripService.UpdateName(tripId, newName, userId);
            if (!success) return BadRequest();

            return RedirectToAction(nameof(TripDetail), new { tripId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDates(int tripId, DateTime startDate, DateTime endDate)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var datesUpdatedSuccessfully = await tripService.UpdateDates(tripId, startDate, endDate, userId);

            if (!datesUpdatedSuccessfully)
            {
                return BadRequest("Nepodařilo se aktualizovat data výletu.");
            }

            return RedirectToAction(nameof(TripDetail), new { tripId });
        }
    }
}
