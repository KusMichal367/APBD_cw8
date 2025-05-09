using APBD_cw8.Models;
using Microsoft.Data.SqlClient;
namespace APBD_cw8.Services;

public class TripService : ITripService
{
    private string connectionString;

    public TripService(IConfiguration config)
    {
        connectionString = config.GetConnectionString("DefaultConnection");
    }


    public async Task<IEnumerable<Trip>> GetTripsAsync()
    {
        var trips = new Dictionary<int, Trip>();

        //pobieramy dane o wycieczce oraz kraje które obejmuje
        string query = @"Select
    trip.IdTrip,
    trip.Name,
    trip.Description,
    trip.DateFrom,
    trip.DateTo,
    trip.MaxPeople,
    country.IdCountry,
    country.Name as CountryName
from Trip trip
join Country_Trip countrytrip ON trip.IdTrip = countrytrip.IdTrip
join Country country on countrytrip.IdCountry = country.IdCountry";

        try
        {

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                await connection.OpenAsync();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int idOrdinal = reader.GetInt32(reader.GetOrdinal("IdTrip"));

                        if (!trips.TryGetValue(idOrdinal, out var dto))
                        {
                            dto = new Trip
                            {
                                IdTrip = idOrdinal,
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Description = reader.GetString(reader.GetOrdinal("Description")),
                                DateFrom = reader.GetDateTime(reader.GetOrdinal("DateFrom")),
                                DateTo = reader.GetDateTime(reader.GetOrdinal("DateTo")),
                                MaxPeople = reader.GetInt32(reader.GetOrdinal("MaxPeople")),
                            };
                            trips.Add(idOrdinal, dto);
                        }

                        if (!reader.IsDBNull(reader.GetOrdinal("IdCountry")))
                        {
                            dto.Countries.Add(new Country
                            {
                                IdCountry = reader.GetInt32(reader.GetOrdinal("IdCountry")),
                                Name = reader.GetString(reader.GetOrdinal("CountryName")),
                            });
                        }
                    }
                }
            }

            return trips.Values;
        }
        catch (SqlException ex)
        {
            throw new ApplicationException("Błąd "+ex);
        }

    }

    public async Task<List<Country_Trip>> GetCountryTripAsync()
    {
        var asociatedCountries = new List<Country_Trip>();

        //pobieramy dane o wycieczkach i ich kierunkach
        string query = "SELECT IdCountry, IdTrip FROM Country_Trip";
        using (SqlConnection connection = new SqlConnection(connectionString))
        using (SqlCommand command = new SqlCommand(query, connection))
        {
            await connection.OpenAsync();

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    int idOrdinal = reader.GetOrdinal("IdCountry");
                    asociatedCountries.Add(new Country_Trip()
                    {
                        IdCountry = reader.GetInt32(idOrdinal),
                        IdTrip = reader.GetInt32(idOrdinal)
                    });
                }
            }

        }

        return asociatedCountries;
    }

    public async Task<List<Country>> GetCountriesAsync()
    {
        var Countries = new List<Country>();

        //poiberamy wszystkie kraje
        string query = "SELECT IdCountry, Name FROM Country";
        using (SqlConnection connection = new SqlConnection(connectionString))
        using (SqlCommand command = new SqlCommand(query, connection))
        {
            await connection.OpenAsync();

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    int idOrdinal = reader.GetOrdinal("IdCountry");
                    Countries.Add(new Country()
                    {
                        IdCountry = reader.GetInt32(idOrdinal),
                        Name = reader.GetString(reader.GetOrdinal("Name"))
                    });
                }
            }

        }

        return Countries;
    }

}