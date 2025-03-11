using Dirassati_Backend.Data.Enums;
using Dirassati_Backend.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Dirassati_Backend.Data.Seeders;

public static class SchoolTypeSeeders
{
    public static void SeedSchoolTypes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SchoolType>()
        .HasData(
            new SchoolType { SchoolTypeId = (int)SchoolTypeEnum.Primaire, Name = "Primaire" },
            new SchoolType { SchoolTypeId = (int)SchoolTypeEnum.Moyenne, Name = "Moyenne" },
            new SchoolType { SchoolTypeId = (int)SchoolTypeEnum.Lycee, Name = "Lycee" }
        );
    }
}
