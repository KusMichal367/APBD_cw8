using APBD_cw8.Models;
namespace APBD_cw8.Services;

public interface ITripService
{
    Task<IEnumerable<Trip>> GetTripsAsync();
}