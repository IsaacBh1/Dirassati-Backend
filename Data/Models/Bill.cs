namespace Dirassati_Backend.Data.Models;

public class Bill
{

    public Guid BillId { get; set; } = Guid.NewGuid();

    public Guid SchoolId { get; set; } // Link to the school that issued the bill

    public decimal Amount { get; set; }

    public string Title { get; set; } = null!; // e.g., "Q1 Tuition Fee", "Activity Fee 2025"

    public string? Description { get; set; }


    public bool IsActive { get; set; } = true; // To allow disabling old bills maybe

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual School School { get; set; } = null!;

    // Navigation property for the join table
    public virtual ICollection<StudentPayment> StudentPayments { get; set; } = [];
}

