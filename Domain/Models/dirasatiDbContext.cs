using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Dirassati_Backend.Domain.Models;

public partial class dirasatiDbContext : DbContext
{
    public dirasatiDbContext()
    {
    }

    public dirasatiDbContext(DbContextOptions<dirasatiDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Absence> Absences { get; set; }

    public virtual DbSet<AcademicYear> AcademicYears { get; set; }

    public virtual DbSet<Adress> Adresses { get; set; }

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }

    public virtual DbSet<Classroom> Classrooms { get; set; }

    public virtual DbSet<ContractType> ContractTypes { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Group> Groups { get; set; }

    public virtual DbSet<Parent> Parents { get; set; }

    public virtual DbSet<PhoneNumber> PhoneNumbers { get; set; }

    public virtual DbSet<RelationshipToStudent> RelationshipToStudents { get; set; }

    public virtual DbSet<School> Schools { get; set; }

    public virtual DbSet<SchoolAdmin> SchoolAdmins { get; set; }

    public virtual DbSet<SchoolLevel> SchoolLevels { get; set; }

    public virtual DbSet<Stream> Streams { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<Subject> Subjects { get; set; }

    public virtual DbSet<Teach> Teaches { get; set; }

    public virtual DbSet<Teacher> Teachers { get; set; }

    public virtual DbSet<__EFMigrationsLock> __EFMigrationsLocks { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlite("Data Source=dirasatiDb.db");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Absence>(entity =>
        {
            entity.HasOne(d => d.Student).WithMany(p => p.Absences).HasForeignKey(d => d.StudentId);
        });

        modelBuilder.Entity<AcademicYear>(entity =>
        {
            entity.HasOne(d => d.School).WithMany(p => p.AcademicYears)
                .HasForeignKey(d => d.SchoolId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Adress>(entity =>
        {
            entity.HasKey(e => e.AdresseId);
        });

        modelBuilder.Entity<AspNetRole>(entity =>
        {
            entity.HasIndex(e => e.NormalizedName, "RoleNameIndex").IsUnique();
        });

        modelBuilder.Entity<AspNetRoleClaim>(entity =>
        {
            entity.HasIndex(e => e.RoleId, "IX_AspNetRoleClaims_RoleId");

            entity.HasOne(d => d.Role).WithMany(p => p.AspNetRoleClaims).HasForeignKey(d => d.RoleId);
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.HasIndex(e => e.NormalizedEmail, "EmailIndex");

            entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex").IsUnique();

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "AspNetUserRole",
                    r => r.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId"),
                    l => l.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("AspNetUserRoles");
                        j.HasIndex(new[] { "RoleId" }, "IX_AspNetUserRoles_RoleId");
                    });
        });

        modelBuilder.Entity<AspNetUserClaim>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_AspNetUserClaims_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserClaims).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserLogin>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

            entity.HasIndex(e => e.UserId, "IX_AspNetUserLogins_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserLogins).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserToken>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserTokens).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Classroom>(entity =>
        {
            entity.HasOne(d => d.School).WithMany(p => p.Classrooms)
                .HasForeignKey(d => d.SchoolId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<ContractType>(entity =>
        {
            entity.HasKey(e => e.ContractId);
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasOne(d => d.User).WithMany(p => p.Employees)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasOne(d => d.AcademicYear).WithMany(p => p.Groups).HasForeignKey(d => d.AcademicYearId);

            entity.HasOne(d => d.Level).WithMany(p => p.Groups)
                .HasForeignKey(d => d.LevelId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.School).WithMany(p => p.Groups)
                .HasForeignKey(d => d.SchoolId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Stream).WithMany(p => p.Groups).HasForeignKey(d => d.StreamId);
        });

        modelBuilder.Entity<Parent>(entity =>
        {
            entity.Property(e => e.ParentId).ValueGeneratedNever();

            entity.HasOne(d => d.User).WithMany(p => p.Parents)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<PhoneNumber>(entity =>
        {
            entity.Property(e => e.PhoneNumber1).HasColumnName("PhoneNumber");

            entity.HasOne(d => d.School).WithMany(p => p.PhoneNumbers)
                .HasForeignKey(d => d.SchoolId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<RelationshipToStudent>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("RelationshipToStudent");
        });

        modelBuilder.Entity<School>(entity =>
        {
            entity.HasOne(d => d.Admin).WithMany(p => p.Schools)
                .HasForeignKey(d => d.AdminId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<SchoolAdmin>(entity =>
        {
            entity.HasOne(d => d.School).WithMany(p => p.SchoolAdmins)
                .HasForeignKey(d => d.SchoolId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.User).WithMany(p => p.SchoolAdmins)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<SchoolLevel>(entity =>
        {
            entity.HasKey(e => e.LevelId);

            entity.ToTable("SchoolLevel");

            entity.HasMany(d => d.Streams).WithMany(p => p.SchoolLevels)
                .UsingEntity<Dictionary<string, object>>(
                    "StreamsSchoolLevel",
                    r => r.HasOne<Stream>().WithMany()
                        .HasForeignKey("StreamId")
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    l => l.HasOne<SchoolLevel>().WithMany()
                        .HasForeignKey("SchoolLevelId")
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j =>
                    {
                        j.HasKey("SchoolLevelId", "StreamId");
                        j.ToTable("StreamsSchoolLevels");
                    });
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.Property(e => e.AcademicYear).HasColumnType("INT");
            entity.Property(e => e.IsActive).HasColumnType("INT");

            entity.HasOne(d => d.Stream).WithMany(p => p.Students)
                .HasForeignKey(d => d.StreamId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Teach>(entity =>
        {
            entity.HasKey(e => new { e.SubjectId, e.TeacherId, e.GroupId });
        });

        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.ToTable("Teacher");

            entity.Property(e => e.ContractTypeId).HasColumnType("INT");

            entity.HasOne(d => d.ContractType).WithMany(p => p.Teachers)
                .HasForeignKey(d => d.ContractTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.School).WithMany(p => p.Teachers)
                .HasForeignKey(d => d.SchoolId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.User).WithMany(p => p.Teachers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasMany(d => d.Subjects).WithMany(p => p.Teachers)
                .UsingEntity<Dictionary<string, object>>(
                    "TeacherSubject",
                    r => r.HasOne<Subject>().WithMany()
                        .HasForeignKey("SubjectId")
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    l => l.HasOne<Teacher>().WithMany()
                        .HasForeignKey("TeacherId")
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j =>
                    {
                        j.HasKey("TeacherId", "SubjectId");
                        j.ToTable("TeacherSubjects");
                    });
        });

        modelBuilder.Entity<__EFMigrationsLock>(entity =>
        {
            entity.ToTable("__EFMigrationsLock");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
