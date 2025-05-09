using APBD_cw8.Models;
namespace APBD_cw8.Services;

public interface IClientService
{
    Task<IEnumerable<Client_Trip>> GetTripsForClient(int clientId);
    Task<int> CreateClient(CreateClient client);

    Task RegisterClientToTrip(int clientId, int tripId);
    Task DeleteClientFromTrip(int clientId, int tripId);
}
