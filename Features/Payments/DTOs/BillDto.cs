using System;
using System.ComponentModel.DataAnnotations;

namespace Dirassati_Backend.Features.Payments.DTOs;

public class AddBillDto
{
    [Required]
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public decimal? Amount { get; set; }

}


public class GetBillDto
{
    public required string Title { get; set; } = null!;
    public string? Description { get; set; }
    public required decimal Amount { get; set; }

}
public class StudentPaymentBillDto
{
    public Guid BillId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public string PaymentStatus { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
