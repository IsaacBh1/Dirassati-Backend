namespace Dirassati_Backend.Features.Auth.Register.Dtos;

public class CreatedEmployeeDto
{

    public Guid EmployeeId { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
    public required int Permissions { get; set; }

}
