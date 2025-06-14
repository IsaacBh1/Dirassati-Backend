using System.ComponentModel.DataAnnotations;

namespace Dirassati_Backend.Data.Models
{
    public partial class School
    {
        [Key]
        public Guid SchoolId { get; set; } = Guid.NewGuid();

        public required string Name { get; set; } = null!;

        public int AddressId { get; set; }


        public string Email { get; set; } = null!;

        public string LogoUrl { get; set; } = string.Empty;

        public string WebsiteUrl { get; set; } = string.Empty;

        public string SchoolConfig { get; set; } = string.Empty;

        public int SchoolTypeId { get; set; }


        public int AcademicYearId { get; set; }


        public int BillingCycleDays { get; set; } = 30;
        public string? BankCode { get; set; } = "";
        public SchoolScheduleConfig? ScheduleConfig { get; set; }

        public virtual Address Address { get; set; } = new();
        public virtual SchoolType SchoolType { get; set; } = null!;

        public virtual ICollection<Student> Student { get; set; } = [];

        public virtual AcademicYear AcademicYear { get; set; } = null!;

        public virtual ICollection<Classroom> Classrooms { get; set; } = [];
        public virtual ICollection<Employee> Employees { get; set; } = [];
        public virtual ICollection<Group> Groups { get; set; } = [];
        public virtual ICollection<PhoneNumber> PhoneNumbers { get; set; } = [];
        public virtual ICollection<Teacher> Teachers { get; set; } = [];
        public virtual ICollection<Specialization> Specializations { get; set; } = [];
        public virtual ICollection<Timeslot> Timeslots { get; set; } = [];

    }
}