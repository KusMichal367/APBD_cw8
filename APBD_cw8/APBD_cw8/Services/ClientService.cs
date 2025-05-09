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
        //sprawdzamy czy istnieje klient o podanym id
        string findClient = @"Select count(1) from Client where IdClient = @ClientId;";

        //szukamy informacji o wycieczce i połączeniu z klientem dla podanych danych
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

    public async Task<int> CreateClient(CreateClient client)
    {
        //wstawiamy dane do tabeli klienci jako wynik zwracany jest nowy numer id
        string query = @"INSERT INTO Client (FirstName, LastName, Email, Telephone, Pesel)
            VALUES (@FirstName, @LastName, @Email, @Telephone, @Pesel);
            SELECT CAST(SCOPE_IDENTITY() AS int);";

        SqlConnection connection = new SqlConnection(connectionString);
        SqlCommand command = new SqlCommand(query, connection);

        command.Parameters.Add(new SqlParameter("@FirstName", SqlDbType.NVarChar, 50) { Value = client.FirstName });
        command.Parameters.Add(new SqlParameter("@LastName", SqlDbType.NVarChar, 50) { Value = client.LastName });
        command.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 100) { Value = client.Email });
        command.Parameters.Add(new SqlParameter("@Telephone", SqlDbType.NVarChar, 15) { Value = client.Telephone});
        command.Parameters.Add(new SqlParameter("@PESEL", SqlDbType.NVarChar, 11) { Value = client.PESEL });

        await connection.OpenAsync();
        try
        {
            var result = await command.ExecuteScalarAsync();

            if (result == null || result == DBNull.Value)
            {
                throw new ApplicationException("Nie udało się pobrać nowego ID klienta");
            }

            if (result is int id)
            {
                return id;
            }

            int converted = Convert.ToInt32(result);
            return converted;

        }
        catch (SqlException ex)
        {
            throw new ApplicationException("Błąd przy tworzeniu klienta: " + ex);
        }
    }

    public async Task RegisterClientToTrip(int clientId, int tripId)
    {
        //sprawdzamy czy istnieje klient o podanym id
        string findClient = @"Select count(1) from Client where IdClient = @ClientId;";

        //sprawdzamy czy istnieje wycieczka o podanym id
        string findTrip = @"Select MaxPeople from Trip where IdTrip = @TripId;";

        //liczymy uczestników danej wycieczki
        string countParticipants = @"Select count(1) from Client_trip where IdTrip = @TripId;";

        //sprawdzamy czy uczesnik nie jest już zapisany na daną wycieczkę
        string checkAlreadyParticipant = @"Select count(1) from Client_trip where IdTrip = @TripId and IdClient = @ClientId;";

        //wstawiamy dane do tabeli
        string insertQuery =
            @"INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt) VALUES (@ClientId, @TripId, @RegisteredAt);";

        using SqlConnection connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            using (SqlCommand findClientCommand = new SqlCommand(findClient, connection, transaction))
            using (SqlCommand findTripCommand = new SqlCommand(findTrip, connection, transaction))
            using (SqlCommand countParticipantsCommand = new SqlCommand(countParticipants, connection, transaction))
            using (SqlCommand checkAlreadyParticipantCommand =
                   new SqlCommand(checkAlreadyParticipant, connection, transaction))
            using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection, transaction))

            {
                findClientCommand.Parameters.Add(new SqlParameter("@ClientId", SqlDbType.Int) { Value = clientId });
                var found = (int)await findClientCommand.ExecuteScalarAsync();
                if (found == 0)
                {
                    throw new KeyNotFoundException($"Client {clientId} not found");
                }


                findTripCommand.Parameters.Add(new SqlParameter("@TripId", SqlDbType.Int) { Value = tripId });
                var result = await findTripCommand.ExecuteScalarAsync();

                if (result == null || result == DBNull.Value)
                {
                    throw new KeyNotFoundException($"Trip {tripId} not found");
                }

                int maxPeople = Convert.ToInt32(result);

                checkAlreadyParticipantCommand.Parameters.Add(new SqlParameter("@ClientId", SqlDbType.Int)
                    { Value = clientId });
                checkAlreadyParticipantCommand.Parameters.Add(new SqlParameter("@TripId", SqlDbType.Int)
                    { Value = tripId });

                var alreadyParticipants = (int)await checkAlreadyParticipantCommand.ExecuteScalarAsync();
                if (alreadyParticipants > 0)
                {
                    throw new InvalidOperationException($"Client {clientId} already registered at trip {tripId}");
                }

                countParticipantsCommand.Parameters.Add(new SqlParameter("@TripId", SqlDbType.Int) { Value = tripId });
                int currentCount = (int)await countParticipantsCommand.ExecuteScalarAsync();
                if (currentCount >= maxPeople)
                {
                    throw new InvalidOperationException($"Limit of trip {tripId} exceeded");
                }

                insertCommand.Parameters.Add(new SqlParameter("@ClientId", SqlDbType.Int) { Value = clientId });
                insertCommand.Parameters.Add(new SqlParameter("@TripId", SqlDbType.Int) { Value = tripId });

                DateTime now = DateTime.Now;
                int nowInt = now.Year * 10000 + now.Month * 100 + now.Day;

                insertCommand.Parameters.Add(new SqlParameter("@RegisteredAt", SqlDbType.Int)
                    { Value = nowInt });
                await insertCommand.ExecuteNonQueryAsync();

            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task DeleteClientFromTrip(int clientId, int tripId)
    {
        //sprawdzamy czy klient jest już na danej wycieczce
        string checkClientOnTrip = @"SELECT count(1) from Client_Trip where IdClient=@clientId and IdTrip = @tripId;";

        //usuwamy połączenie klienta z wycieczką
        string deleteQuery = @"DELETE FROM Client_Trip WHERE IdClient = @ClientId AND IdTrip   = @TripId;";

        using SqlConnection connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        try
        {
            using (SqlCommand checkClientOnTripCommand = new SqlCommand(checkClientOnTrip, connection))
            using (SqlCommand deleteCommand = new SqlCommand(deleteQuery, connection))
            {
                checkClientOnTripCommand.Parameters.Add(new SqlParameter("@ClientId", SqlDbType.Int)
                    { Value = clientId });
                checkClientOnTripCommand.Parameters.Add(new SqlParameter("@TripId", SqlDbType.Int) { Value = tripId });

                int found = (int)await checkClientOnTripCommand.ExecuteScalarAsync();
                if (found == 0)
                {
                    throw new KeyNotFoundException($"Client {clientId} not found on trip {tripId}");
                }

                deleteCommand.Parameters.Add(new SqlParameter("@ClientId", SqlDbType.Int) { Value = clientId });
                deleteCommand.Parameters.Add(new SqlParameter("@TripId", SqlDbType.Int) { Value = tripId });

                await deleteCommand.ExecuteNonQueryAsync();
            }
        }
        catch (SqlException sqlEx)
        {
            throw new ApplicationException("Błąd "+sqlEx);
        }
    }
}