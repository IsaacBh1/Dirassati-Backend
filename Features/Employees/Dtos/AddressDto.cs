

namespace Dirassati_Backend.Features.Employees.Dtos
{

    public class AddressDto
    {
        public int AdresseId { get; set; }
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
    }
}