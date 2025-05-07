using System.ComponentModel.DataAnnotations;

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

public class Client_Trip
{
    public int IdTrip { get; set; }
    public string Name {get; set;}
    public string Description {get; set;}
    public DateTime DateFrom {get; set;}
    public DateTime DateTo {get; set;}
    public int MaxPeople {get; set;}

    public int RegisteredAt {get; set;}
    public int? PaymentDate {get; set;}
}

public class CreateClient
{
    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string FirstName { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string LastName { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [RegularExpression(@"^\+\d{9,15}$", ErrorMessage = "Please enter a valid phone number with prefix")]
    public string Telephone { get; set; }

    [Required]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "Please enter a valid PESEL - 11 numbers")]
    public string PESEL { get; set; }



}