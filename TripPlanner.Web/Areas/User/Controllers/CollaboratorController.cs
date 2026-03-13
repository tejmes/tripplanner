using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TripPlanner.Application.Abstraction;

namespace TripPlanner.Web.Areas.User.Controllers
{
    [Area("User")]
    [Authorize]
    public class CollaboratorController : Controller
    {
        private readonly ICollaboratorService collaboratorService;

        public CollaboratorController(ICollaboratorService collaboratorService)
        {
            this.collaboratorService = collaboratorService;
        }

        [HttpGet]
        public async Task<JsonResult> SearchUsers(string term, int tripId)
        {
            var me = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var results = await collaboratorService.SearchUsers(tripId, me, term);

            return Json(results);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCollaborator(int tripId, int userId)
        {
            var requestorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var collaboratorAdded = await collaboratorService.AddCollaborator(tripId, requestorId, userId);
            if (!collaboratorAdded) return BadRequest();

            return RedirectToAction("TripDetail", "Trip", new { area = "User", tripId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveCollaborator(int tripId, int userId)
        {
            var ownerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var success = await collaboratorService.RemoveCollaborator(tripId, ownerId, userId);
            if (!success) return Forbid();

            return RedirectToAction("TripDetail", "Trip", new { area = "User", tripId });
        }
    }
}
