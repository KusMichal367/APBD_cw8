using System.Data;
using APBD_cw8.Models;
using Microsoft.Data.SqlClient;

namespace APBD_cw8.Services;

public class ClientService : IClientService
{
    private string connectionString;

    public ClientService(IConfiguration configuration)
    {
        connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<IEnumerable<Client_Trip>> GetTripsForClient(int clientId)
    {
        string findClient = @"Select count(1) from Client where IdClient = @ClientId;";
        string query = @"Select
    trip.IdTrip,
    trip.Name,
    trip.Description,
    trip.DateFrom,
    trip.DateTo,
    trip.MaxPeople,
    clienttrip.RegisteredAt,
    clienttrip.PaymentDate
from Client_Trip clienttrip
join Trip trip ON clienttrip.IdTrip = trip.IdTrip
where clienttrip.IdClient = @ClientId;";

        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(findClient, connection))
            using (SqlCommand command1 = new SqlCommand(query, connection))
            {
                await connection.OpenAsync();
                command.Parameters.Add(new SqlParameter("@ClientId", SqlDbType.Int) { Value = clientId });

                var found = (int)await command.ExecuteScalarAsync();
                if (found == 0)
                {
                    throw new KeyNotFoundException($"Client {clientId} not found");
                }

                command1.Parameters.Add(new SqlParameter("@ClientId", SqlDbType.Int) { Value = clientId });

                var list = new List<Client_Trip>();
                using var reader = await command1.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new Client_Trip
                    {
                        IdTrip = reader.GetInt32(reader.GetOrdinal("IdTrip")),
                        Name = reader.GetString(reader.GetOrdinal("Name")),
                        Description = reader.GetString(reader.GetOrdinal("Description")),
                        DateFrom = reader.GetDateTime(reader.GetOrdinal("DateFrom")),
                        DateTo = reader.GetDateTime(reader.GetOrdinal("DateTo")),
                        MaxPeople = reader.GetInt32(reader.GetOrdinal("MaxPeople")),

                        RegisteredAt = reader.GetInt32(reader.GetOrdinal("RegisteredAt")),
                        PaymentDate = reader.GetInt32(reader.GetOrdinal("PaymentDate"))
                    });
                }

                return list;
            }
        }
        catch (SqlException ex)
        {
            throw new ApplicationException("Błąd "+ex);
        }
    }
}