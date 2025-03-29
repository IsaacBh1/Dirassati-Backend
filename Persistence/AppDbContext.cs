using Dirassati_Backend.Data.Models;
using Dirassati_Backend.Data.Seeders;
using Dirassati_Backend.Domain.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace Persistence;
public partial class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions options) : base(options)
    {
    }
    public virtual DbSet<Absence> Absences { get; set; }
    public virtual DbSet<AcademicYear> AcademicYears { get; set; }
    public virtual DbSet<Address> Adresses { get; set; }
    public virtual DbSet<Classroom> Classrooms { get; set; }
    public virtual DbSet<ContractType> ContractTypes { get; set; }
    public virtual DbSet<Employee> Employees { get; set; }
    public virtual DbSet<Group> Groups { get; set; }
    public virtual DbSet<Parent> Parents { get; set; }
    public virtual DbSet<PhoneNumber> PhoneNumbers { get; set; }
    public virtual DbSet<ParentRelationshipToStudentType> ParentRelationshipToStudentTypes { get; set; }
    public virtual DbSet<School> Schools { get; set; }
    public virtual DbSet<SchoolLevel> SchoolLevels { get; set; }
    public virtual DbSet<Specialization> Specializations { get; set; }
    public virtual DbSet<Student> Students { get; set; }
    public virtual DbSet<Subject> Subjects { get; set; }
    public virtual DbSet<Teach> Teaches { get; set; }
    public virtual DbSet<Teacher> Teachers { get; set; }
    public virtual DbSet<SchoolType> SchoolTypes { get; set; }
    public virtual DbSet<StudentReport> StudentReports { get; set; }
    public virtual DbSet<StudentReportStatus> StudentReportStatuses { get; set; }





    protected override void OnModelCreating(ModelBuilder builder)
    {
        SpecializationSeeder.SeedSpecializations(builder);
        SchoolTypeSeeders.SeedSchoolTypes(builder);
        SchoolLevelSeeder.SeedSchoolLevels(builder);
        ParentRelationshipSeeder.SeedParentRelationships(builder);
        SubjectSeeder.SeedSubjects(builder);
        TeacherSeeder.SeedContractType(builder);
        base.OnModelCreating(builder);
        builder.Entity<School>()
        .HasMany(sch => sch.Specializations)
        .WithMany(sp => sp.Schools);

        builder.Entity<Student>()
        .HasOne(s => s.ParentRelationshipToStudentType)
        .WithMany(r => r.Students)
        .OnDelete(DeleteBehavior.Restrict);


        builder.Entity<School>()
       .HasMany(sch => sch.Specializations)
       .WithMany(sp => sp.Schools);

        builder.Entity<Student>()
            .HasOne(s => s.ParentRelationshipToStudentType)
            .WithMany(r => r.Students)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure one-to-many relationship between School and Classroom
        builder.Entity<Classroom>()
            .HasOne(c => c.School)
            .WithMany(s => s.Classrooms)
            .HasForeignKey(c => c.SchoolId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        // Configure one-to-one relationship between Parent and AppUser
        builder.Entity<Parent>()
            .HasOne(p => p.User)
            .WithOne()
            .HasForeignKey<Parent>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure one-to-many relationship between School and AcademicYear
        builder.Entity<AcademicYear>()
            .HasOne(ay => ay.School);



        builder.Entity<Teacher>()
    .HasMany(t => t.Subjects)
    .WithMany(s => s.Teachers);


        builder.Entity<StudentReport>()
           .HasOne(sr => sr.Teacher)
           .WithMany(t => t.StudentReports)
           .HasForeignKey(sr => sr.TeacherId)
           .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<StudentReport>()
            .HasOne(sr => sr.Student)
            .WithMany(s => s.StudentReports)
            .HasForeignKey(sr => sr.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<StudentReport>()
          .HasOne(sr => sr.StudentReportStatus)
          .WithMany(srs => srs.StudentReports)
          .HasForeignKey(sr => sr.StudentReportStatusId);

        // Seed the ReportStatus table
        builder.Entity<StudentReportStatus>().HasData(
            new { StudentReportStatusId = 1, Name = "Pending" },
            new { StudentReportStatusId = 2, Name = "Sent" },
            new { StudentReportStatusId = 3, Name = "Viewed" }
        );
    }



}