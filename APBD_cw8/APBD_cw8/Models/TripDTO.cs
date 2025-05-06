namespace APBD_cw8.Models;

public class Trip
{
    public int IdTrip { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public int MaxPeople { get; set; }

    public List<Country> Countries { get; set; } = new List<Country>();
}

public class Country_Trip
{
    public int IdCountry { get; set; }
    public int IdTrip { get; set; }
}

public class Country
{
    public int IdCountry { get; set; }
    public required string Name { get; set; }
}
