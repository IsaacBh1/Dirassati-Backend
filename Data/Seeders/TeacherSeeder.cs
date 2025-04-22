using Dirassati_Backend.Domain.Models;
using Dirassati_Backend.Features.Teachers.Dtos;
using Dirassati_Backend.Features.Teachers.Services;
using Microsoft.EntityFrameworkCore;

namespace Dirassati_Backend.Data.Seeders;

public static class TeacherSeeder
{
    public static void SeedContractType(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ContractType>()
        .HasData(
            new ContractType { ContractId = 1, Name = "Contrats Permanents" },
            new ContractType { ContractId = 2, Name = "Contrats à Durée Déterminée" },
            new ContractType { ContractId = 3, Name = "Contrats à Temps Partiel ou Horaire" },
            new ContractType { ContractId = 4, Name = "Stagiaire" },
            new ContractType { ContractId = 5, Name = "Consultant Pédagogique" },
            new ContractType { ContractId = 6, Name = "Bénévole" }
        );
    }
    public static async Task SeedTeachersAsync(Guid schoolId, TeacherServices teacherServices)
    {

        var teachers = new List<TeacherInfosDto>
        {
            new() {
                    FirstName = "Alice",
                    LastName = "Johnson",
                    Email = "alice.johnson@example.com",
                    PhoneNumber = "+1234567891",

                HireDate = DateOnly.FromDateTime(DateTime.Now.AddYears(-5)),
                ContractTypeId = 1, // Contrats Permanents
                SchoolId = schoolId
            },
            new() {
                    FirstName = "Bob",
                    LastName = "Smith",
                    Email = "bob.smith@example.com",
                    PhoneNumber = "+1234567892",

                HireDate = DateOnly.FromDateTime(DateTime.Now.AddYears(-3)),
                ContractTypeId = 2, // Contrats à Durée Déterminée
                SchoolId = schoolId
            },
            new() {
                    FirstName = "Charlie",
                    LastName = "Brown",
                    Email = "charlie.brown@example.com",
                    PhoneNumber = "+1234567893",

                HireDate = DateOnly.FromDateTime(DateTime.Now.AddYears(-2)),
                ContractTypeId = 3, // Contrats à Temps Partiel ou Horaire
                SchoolId = schoolId
            },
            new() {
                    FirstName = "Diana",
                    LastName = "Prince",
                    Email = "diana.prince@example.com",
                    PhoneNumber = "+1234567894",

                HireDate = DateOnly.FromDateTime(DateTime.Now.AddYears(-1)),
                ContractTypeId = 4, // Stagiaire
                SchoolId = schoolId
            },
            new() {
                    FirstName = "Eve",
                    LastName = "Adams",
                    Email = "eve.adams@example.com",
                    PhoneNumber = "+1234567895",

                HireDate = DateOnly.FromDateTime(DateTime.Now.AddYears(-4)),
                ContractTypeId = 5, // Consultant Pédagogique
                SchoolId = schoolId
            },
            new() {
                    FirstName = "Frank",
                    LastName = "Miller",
                    Email = "frank.miller@example.com",
                    PhoneNumber = "+1234567896",

                HireDate = DateOnly.FromDateTime(DateTime.Now.AddYears(-6)),
                ContractTypeId = 6, // Bénévole
                SchoolId = schoolId
            },
            new() {
                    FirstName = "Grace",
                    LastName = "Hopper",
                    Email = "grace.hopper@example.com",
                    PhoneNumber = "+1234567897",

                HireDate = DateOnly.FromDateTime(DateTime.Now.AddYears(-7)),
                ContractTypeId = 1, // Contrats Permanents
                SchoolId = schoolId
            },
            new() {
                    FirstName = "Henry",
                    LastName = "Ford",
                    Email = "henry.ford@example.com",
                    PhoneNumber = "+1234567898",

                HireDate = DateOnly.FromDateTime(DateTime.Now.AddYears(-8)),
                ContractTypeId = 2, // Contrats à Durée Déterminée
                SchoolId = schoolId
            },
            new() {
                    FirstName = "Ivy",
                    LastName = "Green",
                    Email = "ivy.green@example.com",
                    PhoneNumber = "+1234567899",

                HireDate = DateOnly.FromDateTime(DateTime.Now.AddYears(-9)),
                ContractTypeId = 3, // Contrats à Temps Partiel ou Horaire
                SchoolId = schoolId
            },
            new() {
                    FirstName = "Jack",
                    LastName = "White",
                    Email = "jack.white@example.com",
                    PhoneNumber = "+1234567800",

                HireDate = DateOnly.FromDateTime(DateTime.Now.AddYears(-10)),
                ContractTypeId = 4, // Stagiaire
                SchoolId = schoolId
            }
        };
        foreach (var teacher in teachers)
        {
            await teacherServices.RegisterTeacherAsync(teacher, schoolId.ToString(), true);

        }
    }
}
