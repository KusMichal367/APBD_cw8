using APBD_cw8.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace APBD_cw8.Controllers;

[ApiController]
[Route("api/[controller]")]

public class ClientsController: ControllerBase
{
    private IClientService _clientService;

    public ClientsController(IClientService clientService)
    {
        _clientService = clientService;
    }

    [HttpGet("{clientId}/trips")]
    public async Task<IActionResult> GetTripsForClient(int clientId)
    {
        try
        {
            var trips = (await _clientService.GetTripsForClient(clientId)).ToList();

            if (!trips.Any())
            {
                return NoContent();
            }

            return Ok(trips);
        }
        catch (KeyNotFoundException knfEx)
        {
            return NotFound(new { message = knfEx.Message });
        }
        catch (SqlException sqlEx)
        {
            return StatusCode(500, new { message = "Błąd bazy danych" + sqlEx.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Błąd"+ex.Message });
        }
    }
}