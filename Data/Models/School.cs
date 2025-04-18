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

        public string Logo { get; set; } = string.Empty;

        public string WebsiteUrl { get; set; } = string.Empty;

        public string SchoolConfig { get; set; } = string.Empty;

        public int SchoolTypeId { get; set; }


        public int AcademicYearId { get; set; }


        public int BillingCycleDays { get; set; } = 30;


        public SchoolScheduleConfig? ScheduleConfig { get; set; }

        // Navigation properties
        public virtual Address Address { get; set; } = new();
        public virtual SchoolType SchoolType { get; set; } = null!;

        // Collections
        public virtual ICollection<Student> Student { get; set; } = [];

        public virtual AcademicYear AcademicYear { get; set; } = null!;

        public virtual ICollection<Classroom> Classrooms { get; set; } = new List<Classroom>();
        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
        public virtual ICollection<Group> Groups { get; set; } = new List<Group>();
        public virtual ICollection<PhoneNumber> PhoneNumbers { get; set; } = new List<PhoneNumber>();
        public virtual ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();
        public virtual ICollection<Specialization> Specializations { get; set; } = new List<Specialization>();
        public virtual ICollection<Timeslot> Timeslots { get; set; } = new List<Timeslot>();

    }
}