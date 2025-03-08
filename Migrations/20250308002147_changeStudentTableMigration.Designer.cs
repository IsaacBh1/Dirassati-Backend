﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Persistence;

#nullable disable

namespace Dirassati_Backend.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20250308002147_changeStudentTableMigration")]
    partial class changeStudentTableMigration
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.2");

            modelBuilder.Entity("AppUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("INTEGER");

                    b.Property<DateOnly>("BirthDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("TEXT");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("INTEGER");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("INTEGER");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("TEXT");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("TEXT");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("TEXT");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("INTEGER");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("TEXT");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("INTEGER");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex");

                    b.ToTable("AspNetUsers", (string)null);
                });

            modelBuilder.Entity("Dirassati_Backend.Domain.Models.Absence", b =>
                {
                    b.Property<Guid>("AbsenceId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("DateTIme")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsJustified")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Remark")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("StudentId")
                        .HasColumnType("TEXT");

                    b.HasKey("AbsenceId");

                    b.HasIndex("StudentId");

                    b.ToTable("Absences");
                });

            modelBuilder.Entity("Dirassati_Backend.Domain.Models.AcademicYear", b =>
                {
                    b.Property<int>("AcademicYearId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateOnly>("EndDate")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("SchoolId")
                        .HasColumnType("TEXT");

                    b.Property<DateOnly>("StartDate")
                        .HasColumnType("TEXT");

                    b.HasKey("AcademicYearId");

                    b.HasIndex("SchoolId");

                    b.ToTable("AcademicYears");
                });

            modelBuilder.Entity("Dirassati_Backend.Domain.Models.Address", b =>
                {
                    b.Property<int>("AdresseId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("City")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Country")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("PostalCode")
                        .HasColumnType("TEXT");

                    b.Property<string>("State")
                        .HasColumnType("TEXT");

                    b.Property<string>("Street")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("AdresseId");

                    b.ToTable("Adresses");
                });

            modelBuilder.Entity("Dirassati_Backend.Domain.Models.Classroom", b =>
                {
                    b.Property<int>("ClassroomId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ClassName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("SchoolId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("SchoolId1")
                        .HasColumnType("TEXT");

                    b.HasKey("ClassroomId");

                    b.HasIndex("SchoolId1");

                    b.ToTable("Classrooms");
                });

            modelBuilder.Entity("Dirassati_Backend.Domain.Models.ContractType", b =>
                {
                    b.Property<int>("ContractId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("ContractId");

                    b.ToTable("ContractTypes");
                });

            modelBuilder.Entity("Dirassati_Backend.Domain.Models.Employee", b =>
                {
                    b.Property<Guid>("EmployeeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("ContractType")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateOnly>("HireDate")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsActive")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Permissions")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Position")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("SchoolId")
                        .HasColumnType("TEXT");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("EmployeeId");

                    b.HasIndex("SchoolId");

                    b.HasIndex("UserId");

                    b.ToTable("Employees");
                });

            modelBuilder.Entity("Dirassati_Backend.Domain.Models.Group", b =>
                {
                    b.Property<int>("GroupId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("AcademicYearId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("GorupName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("GroupCapacity")
                        .HasColumnType("INTEGER");

                    b.Property<int>("LevelId")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("SchoolId")
                        .HasColumnType("TEXT");

                    b.Property<int?>("StreamId")
                        .HasColumnType("INTEGER");

                    b.HasKey("GroupId");

                    b.HasIndex("AcademicYearId");

                    b.HasIndex("LevelId");

                    b.HasIndex("SchoolId");

                    b.HasIndex("StreamId");

                    b.ToTable("Groups");
                });

            modelBuilder.Entity("Dirassati_Backend.Domain.Models.Parent", b =>
                {
                    b.Property<Guid>("ParentId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Occupation")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("RelationshipToStudentId")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("UserId")
                        .HasColumnType("TEXT");

                    b.Property<string>("UserId1")
                        .HasColumnType("TEXT");

                    b.HasKey("ParentId");

                    b.HasIndex("RelationshipToStudentId");

                    b.HasIndex("UserId1");

                    b.ToTable("Parents");
                });

            modelBuilder.Entity("Dirassati_Backend.Domain.Models.PhoneNumber", b =>
                {
                    b.Property<int>("PhoneNumberId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Number")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("SchoolId")
                        .HasColumnType("TEXT");

                    b.HasKey("PhoneNumberId");

                    b.HasIndex("SchoolId");

                    b.ToTable("PhoneNumbers");
                });

            modelBuilder.Entity("Dirassati_Backend.Domain.Models.RelationshipToStudent", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("RelationshipToStudents");
                });

            modelBuilder.Entity("Dirassati_Backend.Domain.Models.School", b =>
                {
                    b.Property<Guid>("SchoolId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<int>("AddressId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Logo")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("SchoolConfig")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("SchoolType")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("WebsiteUrl")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("SchoolId");

                    b.HasIndex("AddressId")
                        .IsUnique();

                    b.ToTable("Schools");
                });

            modelBuilder.Entity("Dirassati_Backend.Domain.Models.SchoolLevel", b =>
                {
                    b.Property<int>("LevelId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("LevelType")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("LevelYear")
                        .HasColumnType("INTEGER");

                    b.HasKey("LevelId");

                    b.ToTable("SchoolLevels");
                });

            modelBuilder.Entity("Dirassati_Backend.Domain.Models.Specialization", b =>
                {
                    b.Property<int>("StreamId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("StreamId");

                    b.ToTable("Specializations");
                });

            modelBuilder.Entity("Dirassati_Backend.Domain.Models.Student", b =>
                {
                    b.Property<Guid>("StudentId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<int>("AcademicYearId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("BirthDate")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("BirthPlace")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("EmergencyContact")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateOnly>("EnrollmentDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsActive")
                        .HasColumnType("INTEGER");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<byte>("LevelYear")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("ParentId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("SchoolId")
                        .HasColumnType("TEXT");

                    b.Property<int>("StreamId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("StudentIdNumber")
                        .HasColumnType("TEXT");

                    b.HasKey("StudentId");

                    b.HasIndex("AcademicYearId");

                    b.HasIndex("ParentId");

                    b.HasIndex("StreamId");

                    b.ToTable("Students");
                });

            modelBuilder.Entity("Dirassati_Backend.Domain.Models.Subject", b =>
                {
                    b.Property<int>("SubjectId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("SubjectId");

                    b.ToTable("Subjects");
                });

            modelBuilder.Entity("Dirassati_Backend.Domain.Models.Teach", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("GroupId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("SubjectId")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("TeacherId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("GroupId");

                    b.HasIndex("SubjectId");

                    b.HasIndex("TeacherId");

                    b.ToTable("Teaches");
                });

            modelBuilder.Entity("Dirassati_Backend.Domain.Models.Teacher", b =>
                {
                    b.Property<Guid>("TeacherId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<int>("ContractTypeId")
                        .HasColumnType("INTEGER");

                    b.Property<DateOnly>("HireDate")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("SchoolId")
                        .HasColumnType("TEXT");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("TeacherId");

                    b.HasIndex("ContractTypeId");

                    b.HasIndex("SchoolId");

                    b.HasIndex("UserId");

                    b.ToTable("Teachers");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ClaimType")
                        .HasColumnType("TEXT");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("TEXT");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ClaimType")
                        .HasColumnType("TEXT");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("TEXT");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("TEXT");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("TEXT");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("TEXT");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("TEXT");

                    b.Property<string>("RoleId")
                        .HasColumnType("TEXT");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("TEXT");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("Value")
                        .HasColumnType("TEXT");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("SchoolLevelSpecialization", b =>
                {
                    b.Property<int>("SchoolLevelsLevelId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("StreamsStreamId")
                        .HasColumnType("INTEGER");

                    b.HasKey("SchoolLevelsLevelId", "StreamsStreamId");

                    b.HasIndex("StreamsStreamId");

                    b.ToTable("SchoolLevelSpecialization");
                });

            modelBuilder.Entity("SubjectTeacher", b =>
                {
                    b.Property<int>("SubjectsSubjectId")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("TeachersTeacherId")
                        .HasColumnType("TEXT");

                    b.HasKey("SubjectsSubjectId", "TeachersTeacherId");

                    b.HasIndex("TeachersTeacherId");

                    b.ToTable("SubjectTeacher");
                });

            modelBuilder.Entity("Dirassati_Backend.Domain.Models.Absence", b =>
                {
                    b.HasOne("Dirassati_Backend.Domain.Models.Student", "Student")
                        .WithMany("Absences")
                        .HasForeignKey("StudentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Student");
                });

            modelBuilder.Entity("Dirassati_Backend.Domain.Models.AcademicYear", b =>
                {
                    b.HasOne("Dirassati_Backend.Domain.Models.School", "School")
                        .WithMany("AcademicYears")
                        .HasForeignKey("SchoolId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("School");
                });

            modelBuilder.Entity("Dirassati_Backend.Domain.Models.Classroom", b =>
                {
                    b.HasOne("Dirassati_Backend.Domain.Models.School", "School")
                        .WithMany("Classrooms")
                        .HasForeignKey("SchoolId1")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("School");
                });

            modelBuilder.Entity("Dirassati_Backend.Domain.Models.Employee", b =>
                {
                    b.HasOne("Dirassati_Backend.Domain.Models.School", "School")
                        .WithMany("Employees")
                        .HasForeignKey("SchoolId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("AppUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("School");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Dirassati_Backend.Domain.Models.Group", b =>
                {
                    b.HasOne("Dirassati_Backend.Domain.Models.AcademicYear", "AcademicYear")
                        .WithMany("Groups")
                        .HasForeignKey("AcademicYearId");

                    b.HasOne("Dirassati_Backend.Domain.Models.SchoolLevel", "Level")
                        .WithMany("Groups")
                        .HasForeignKey("LevelId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Dirassati_Backend.Domain.Models.School", "School")
                        .WithMany("Groups")
                        .HasForeignKey("SchoolId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Dirassati_Backend.Domain.Models.Specialization", "Stream")
                        .WithMany("Groups")
                        .HasForeignKey("StreamId");

                    b.Navigation("AcademicYear");

                    b.Navigation("Level");

                    b.Navigation("School");

                    b.Navigation("Stream");
                });

            modelBuilder.Entity("Dirassati_Backend.Domain.Models.Parent", b =>
                {
                    b.HasOne("Dirassati_Backend.Domain.Models.RelationshipToStudent", "relationshipToStudent")
                        .WithMany()
                        .HasForeignKey("RelationshipToStudentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("AppUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId1");

                    b.Navigation("User");

                    b.Navigation("relationshipToStudent");
                });

            modelBuilder.Entity("Dirassati_Backend.Domain.Models.PhoneNumber", b =>
                {
                    b.HasOne("Dirassati_Backend.Domain.Models.School", "School")
                        .WithMany("PhoneNumbers")
                        .HasForeignKey("SchoolId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("School");
                });

            modelBuilder.Entity("Dirassati_Backend.Domain.Models.School", b =>
                {
                    b.HasOne("Dirassati_Backend.Domain.Models.Address", "Address")
                        .WithOne("School")
                        .HasForeignKey("Dirassati_Backend.Domain.Models.School", "AddressId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Address");
                });

            modelBuilder.Entity("Dirassati_Backend.Domain.Models.Student", b =>
                {
                    b.HasOne("Dirassati_Backend.Domain.Models.AcademicYear", "AcademicYear")
                        .WithMany()
                        .HasForeignKey("AcademicYearId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Dirassati_Backend.Domain.Models.Parent", "parent")
                        .WithMany()
                        .HasForeignKey("ParentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Dirassati_Backend.Domain.Models.Specialization", "Stream")
                        .WithMany("Students")
                        .HasForeignKey("StreamId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("AcademicYear");

                    b.Navigation("Stream");

                    b.Navigation("parent");
                });

            modelBuilder.Entity("Dirassati_Backend.Domain.Models.Teach", b =>
                {
                    b.HasOne("Dirassati_Backend.Domain.Models.Group", "Group")
                        .WithMany()
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Dirassati_Backend.Domain.Models.Subject", "Subject")
                        .WithMany()
                        .HasForeignKey("SubjectId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Dirassati_Backend.Domain.Models.Teacher", "Teacher")
                        .WithMany()
                        .HasForeignKey("TeacherId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Group");

                    b.Navigation("Subject");

                    b.Navigation("Teacher");
                });

            modelBuilder.Entity("Dirassati_Backend.Domain.Models.Teacher", b =>
                {
                    b.HasOne("Dirassati_Backend.Domain.Models.ContractType", "ContractType")
                        .WithMany("Teachers")
                        .HasForeignKey("ContractTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Dirassati_Backend.Domain.Models.School", "School")
                        .WithMany("Teachers")
                        .HasForeignKey("SchoolId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("AppUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ContractType");

                    b.Navigation("School");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("AppUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("AppUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("AppUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("AppUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("SchoolLevelSpecialization", b =>
                {
                    b.HasOne("Dirassati_Backend.Domain.Models.SchoolLevel", null)
                        .WithMany()
                        .HasForeignKey("SchoolLevelsLevelId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Dirassati_Backend.Domain.Models.Specialization", null)
                        .WithMany()
                        .HasForeignKey("StreamsStreamId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("SubjectTeacher", b =>
                {
                    b.HasOne("Dirassati_Backend.Domain.Models.Subject", null)
                        .WithMany()
                        .HasForeignKey("SubjectsSubjectId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Dirassati_Backend.Domain.Models.Teacher", null)
                        .WithMany()
                        .HasForeignKey("TeachersTeacherId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Dirassati_Backend.Domain.Models.AcademicYear", b =>
                {
                    b.Navigation("Groups");
                });

            modelBuilder.Entity("Dirassati_Backend.Domain.Models.Address", b =>
                {
                    b.Navigation("School");
                });

            modelBuilder.Entity("Dirassati_Backend.Domain.Models.ContractType", b =>
                {
                    b.Navigation("Teachers");
                });

            modelBuilder.Entity("Dirassati_Backend.Domain.Models.School", b =>
                {
                    b.Navigation("AcademicYears");

                    b.Navigation("Classrooms");

                    b.Navigation("Employees");

                    b.Navigation("Groups");

                    b.Navigation("PhoneNumbers");

                    b.Navigation("Teachers");
                });

            modelBuilder.Entity("Dirassati_Backend.Domain.Models.SchoolLevel", b =>
                {
                    b.Navigation("Groups");
                });

            modelBuilder.Entity("Dirassati_Backend.Domain.Models.Specialization", b =>
                {
                    b.Navigation("Groups");

                    b.Navigation("Students");
                });

            modelBuilder.Entity("Dirassati_Backend.Domain.Models.Student", b =>
                {
                    b.Navigation("Absences");
                });
#pragma warning restore 612, 618
        }
    }
}
