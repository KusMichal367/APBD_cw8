using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using APBD_cw8.Services;

namespace APBD_cw8.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripsController : ControllerBase
    {
        private readonly ITripService _tripService;

        public TripsController(ITripService tripService)
        {
            _tripService = tripService;
        }

        [HttpGet]
        public async Task<IActionResult> GetTrips()
        {
            try
            {
                var trips = await _tripService.GetTripsAsync();

                if (!trips.Any())
                {
                    return NoContent();
                }

                return Ok(trips);
            }
            catch (ApplicationException ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


    }
}