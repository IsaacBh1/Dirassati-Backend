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





    protected override void OnModelCreating(ModelBuilder builder)
    {
        SpecializationSeeder.SeedSpecializations(builder);
        SchoolTypeSeeders.SeedSchoolTypes(builder);
        SchoolLevelSeeder.SeedSchoolLevels(builder);
        ParentRelationshipSeeder.SeedParentRelationships(builder);
        SubjectSeeder.SeedSubjects(builder);

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


    }

    //     protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    // #warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
    //         => optionsBuilder.UseSqlite("Data Source=dirasatiDb.db");

    //     protected override void OnModelCreating(ModelBuilder modelBuilder)
    //     {
    //         modelBuilder.Entity<Absence>(entity =>
    //         {
    //             entity.HasOne(d => d.Student).WithMany(p => p.Absences).HasForeignKey(d => d.StudentId);
    //         });

    //         modelBuilder.Entity<AcademicYear>(entity =>
    //         {
    //             entity.HasOne(d => d.School).WithMany(p => p.AcademicYears)
    //                 .HasForeignKey(d => d.SchoolId)
    //                 .OnDelete(DeleteBehavior.ClientSetNull);
    //         });

    //         modelBuilder.Entity<Adress>(entity =>
    //         {
    //             entity.HasKey(e => e.AdresseId);
    //         });


    //         modelBuilder.Entity<Classroom>(entity =>
    //         {
    //             entity.HasOne(d => d.School).WithMany(p => p.Classrooms)
    //                 .HasForeignKey(d => d.SchoolId)
    //                 .OnDelete(DeleteBehavior.ClientSetNull);
    //         });

    //         modelBuilder.Entity<ContractType>(entity =>
    //         {
    //             entity.HasKey(e => e.ContractId);
    //         });



    //         modelBuilder.Entity<Group>(entity =>
    //         {
    //             entity.HasOne(d => d.AcademicYear).WithMany(p => p.Groups).HasForeignKey(d => d.AcademicYearId);

    //             entity.HasOne(d => d.Level).WithMany(p => p.Groups)
    //                 .HasForeignKey(d => d.LevelId)
    //                 .OnDelete(DeleteBehavior.ClientSetNull);

    //             entity.HasOne(d => d.School).WithMany(p => p.Groups)
    //                 .HasForeignKey(d => d.SchoolId)
    //                 .OnDelete(DeleteBehavior.ClientSetNull);

    //             entity.HasOne(d => d.Stream).WithMany(p => p.Groups).HasForeignKey(d => d.StreamId);
    //         });


    //         modelBuilder.Entity<PhoneNumber>(entity =>
    //         {
    //             entity.Property(e => e.PhoneNumber1).HasColumnName("PhoneNumber");

    //             entity.HasOne(d => d.School).WithMany(p => p.PhoneNumbers)
    //                 .HasForeignKey(d => d.SchoolId)
    //                 .OnDelete(DeleteBehavior.ClientSetNull);
    //         });

    //         modelBuilder.Entity<RelationshipToStudent>(entity =>
    //         {
    //             entity
    //                 .HasNoKey()
    //                 .ToTable("RelationshipToStudent");
    //         });

    //         modelBuilder.Entity<SchoolLevel>(entity =>
    //         {
    //             entity.HasKey(e => e.LevelId);

    //             entity.ToTable("SchoolLevel");

    //             entity.HasMany(d => d.Streams).WithMany(p => p.SchoolLevels)
    //                 .UsingEntity<Dictionary<string, object>>(
    //                     "StreamsSchoolLevel",
    //                     r => r.HasOne<Specialization>().WithMany()
    //                         .HasForeignKey("StreamId")
    //                         .OnDelete(DeleteBehavior.ClientSetNull),
    //                     l => l.HasOne<SchoolLevel>().WithMany()
    //                         .HasForeignKey("SchoolLevelId")
    //                         .OnDelete(DeleteBehavior.ClientSetNull),
    //                     j =>
    //                     {
    //                         j.HasKey("SchoolLevelId", "StreamId");
    //                         j.ToTable("StreamsSchoolLevels");
    //                     });
    //         });

    //         modelBuilder.Entity<Specialization>(entity =>
    //         {
    //             entity.HasKey(e => e.StreamId);

    //             entity.ToTable("Specialization");
    //         });

    //         modelBuilder.Entity<Student>(entity =>
    //         {
    //             entity.Property(e => e.AcademicYear).HasColumnType("INT");
    //             entity.Property(e => e.IsActive).HasColumnType("INT");

    //             entity.HasOne(d => d.Stream).WithMany(p => p.Students)
    //                 .HasForeignKey(d => d.StreamId)
    //                 .OnDelete(DeleteBehavior.ClientSetNull);
    //         });

    //         modelBuilder.Entity<Teach>(entity =>
    //         {
    //             entity.HasKey(e => new { e.SubjectId, e.TeacherId, e.GroupId });
    //         });

    //         modelBuilder.Entity<Teacher>(entity =>
    //         {
    //             entity.ToTable("Teacher");

    //             entity.Property(e => e.ContractTypeId).HasColumnType("INT");

    //             entity.HasOne(d => d.ContractType).WithMany(p => p.Teachers)
    //                 .HasForeignKey(d => d.ContractTypeId)
    //                 .OnDelete(DeleteBehavior.ClientSetNull);

    //             entity.HasOne(d => d.School).WithMany(p => p.Teachers)
    //                 .HasForeignKey(d => d.SchoolId)
    //                 .OnDelete(DeleteBehavior.ClientSetNull);


    //             entity.HasMany(d => d.Subjects).WithMany(p => p.Teachers)
    //                 .UsingEntity<Dictionary<string, object>>(
    //                     "TeacherSubject",
    //                     r => r.HasOne<Subject>().WithMany()
    //                         .HasForeignKey("SubjectId")
    //                         .OnDelete(DeleteBehavior.ClientSetNull),
    //                     l => l.HasOne<Teacher>().WithMany()
    //                         .HasForeignKey("TeacherId")
    //                         .OnDelete(DeleteBehavior.ClientSetNull),
    //                     j =>
    //                     {
    //                         j.HasKey("TeacherId", "SubjectId");
    //                         j.ToTable("TeacherSubjects");
    //                     });
    //         });

    //         OnModelCreatingPartial(modelBuilder);
    //     }

    //     partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

}