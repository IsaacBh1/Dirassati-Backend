using Dirassati_Backend.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Dirassati_Backend.Data.Seeders
{
    public static class ExamTypeSeeder
    {
        public static void SeedExamTypes(ModelBuilder builder)
        {
            builder.Entity<ExamType>()
                 .HasData(
                    new ExamType { ExamTypeId = 1, Name = "Devoire1" },
                    new ExamType { ExamTypeId = 2, Name = "Devoire2" },
                    new ExamType { ExamTypeId = 3, Name = "Examen" },
                    new ExamType { ExamTypeId = 4, Name = "Controle Continue" }
                );
        }
    }
}
