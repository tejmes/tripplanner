using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TripPlanner.Application.Abstraction;
using TripPlanner.Application.Dtos;

namespace TripPlanner.Web.Areas.User.Controllers
{
    [Area("User")]
    [Authorize]
    public class AccomodationController : Controller
    {
        private readonly IAccomodationService accomodationService;

        public AccomodationController(IAccomodationService accomodationService)
        {
            this.accomodationService = accomodationService;
        }

        [HttpPost]
        public async Task<IActionResult> SetAccomodation([FromBody] AccomodationDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var ok = await accomodationService.SetAccomodation(
                dto.TripId,
                dto.GooglePlaceId,
                dto.Name,
                dto.Latitude,
                dto.Longitude,
                userId
            );
            if (!ok) return Forbid();
            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccomodation(int tripId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            await accomodationService.DeleteAccomodation(tripId, userId);
            return RedirectToAction("TripDetail", "Trip", new { area = "User", tripId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAccomodationTimes(int tripId, int accomodationId, TimeSpan? checkIn, TimeSpan? checkOut)
        {
            await accomodationService.UpdateAccomodationTimes(accomodationId, checkIn, checkOut);
            return RedirectToAction("TripDetail", "Trip", new { area = "User", tripId });
        }
    }
}
