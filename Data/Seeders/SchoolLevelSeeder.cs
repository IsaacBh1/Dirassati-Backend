using Dirassati_Backend.Data.Enums;
using Dirassati_Backend.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Dirassati_Backend.Data.Seeders;

public static class SchoolLevelSeeder
{
    public static void SeedSchoolLevels(ModelBuilder modelBuilder)
    {

        for (int year = 1; year <= 5; year++)
        {
            modelBuilder.Entity<SchoolLevel>().HasData(
                new SchoolLevel
                {
                    LevelId = year,
                    SchoolTypeId = (int)SchoolTypeEnum.Primaire,
                    LevelYear = year
                }
            );
        }


        for (int year = 1; year <= 4; year++)
        {
            int levelId = year + 5;
            modelBuilder.Entity<SchoolLevel>().HasData(
                new SchoolLevel
                {
                    LevelId = levelId,
                    SchoolTypeId = (int)SchoolTypeEnum.Moyenne,
                    LevelYear = year
                }
            );
        }


        modelBuilder.Entity<SchoolLevel>().HasData(
            new SchoolLevel
            {
                LevelId = 10,
                SchoolTypeId = (int)SchoolTypeEnum.Lycee,
                LevelYear = 1
            }
        );



        // High School Level 2 (2AS)
        modelBuilder.Entity<SchoolLevel>().HasData(
            new SchoolLevel
            {
                LevelId = 11,
                SchoolTypeId = (int)SchoolTypeEnum.Lycee,
                LevelYear = 2
            }
        );


        modelBuilder.Entity<SchoolLevel>().HasData(
            new SchoolLevel
            {
                LevelId = 12,
                SchoolTypeId = (int)SchoolTypeEnum.Lycee,
                LevelYear = 3
            }
        );

    }
}