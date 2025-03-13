using Dirassati_Backend.Data.Models;
using Dirassati_Backend.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Dirassati_Backend.Data.Seeders;

public static class ParentRelationshipSeeder
{
    public static void SeedParentRelationships(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ParentRelationshipToStudentType>().HasData(
            new ParentRelationshipToStudentType
            {
                Id = 1,
                Name = "Père",
            },
            new ParentRelationshipToStudentType
            {
                Id = 2,
                Name = "Mère",
            },
            new ParentRelationshipToStudentType
            {
                Id = 3,
                Name = "Tuteur légal",
            },
            new ParentRelationshipToStudentType
            {
                Id = 4,
                Name = "Grand-parent",
            },
            new ParentRelationshipToStudentType
            {
                Id = 5,
                Name = "Oncle/Tante",
            },
            new ParentRelationshipToStudentType
            {
                Id = 6,
                Name = "Frère/Sœur majeur(e)",
            },
            new ParentRelationshipToStudentType
            {
                Id = 7,
                Name = "Autre famille",
            }
        );
    }
}