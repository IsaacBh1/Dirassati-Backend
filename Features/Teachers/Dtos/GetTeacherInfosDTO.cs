using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Dirassati_Backend.Data.Models;
using Dirassati_Backend.Domain.Models;

public class GetTeacherInfosDTO
{

    public string TeacherId { get; set; } = null!;
    public string Email { get; set; } = string.Empty;


    public string FirstName { get; set; } = string.Empty;


    public string LastName { get; set; } = string.Empty;


    public string PhoneNumber { get; set; } = string.Empty;


    public DateOnly HireDate { get; set; }


    public int ContractTypeId { get; set; }
    public string ContractType { get; set; } = "";

    public List<int> SubjectIds { get; set; } = new List<int>();

    public Guid SchoolId { get; set; }

    public byte[]? Photo { get; set; }

    public Address? Address { get; set; }
}
