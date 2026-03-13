using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TripPlanner.Application.Abstraction;
using TripPlanner.Application.Dtos;

namespace TripPlanner.Web.Areas.User.Controllers
{
    [Area("User")]
    [Authorize]
    public class PlaceController : Controller
    {

        private readonly IPlaceService placeService;
        private readonly IAntiforgery antiforgery;

        public PlaceController(IPlaceService placeService, IAntiforgery antiforgery)
        {
            this.placeService = placeService;
            this.antiforgery = antiforgery;
        }

        [HttpPost]
        public async Task<IActionResult> AddToDay([FromBody] AddPlaceToDayDto dto)
        {
            await antiforgery.ValidateRequestAsync(HttpContext);

            try
            {
                await placeService.AddToDay(dto);
                return Ok();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int placeId)
        {
            var tripId = await placeService.Delete(placeId);
            if (tripId == null) return NotFound();

            return RedirectToAction(nameof(TripController.TripDetail), "Trip", new { area = "User", tripId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Move([FromBody] MovePlaceDto dto)
        {
            await placeService.MovePlace(dto.PlaceId, dto.NewTripDayId);

            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reorder([FromBody] ReorderPlacesDto dto)
        {
            await placeService.UpdatePlaceOrder(dto.TripDayId, dto.OrderedPlaceIds);

            return NoContent();
        }
    }
}
