using Dirassati_Backend.Data.Enums;
using Dirassati_Backend.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Dirassati_Backend.Data.Seeders;

public static class SpecializationSeeder
{
    public static void SeedSpecializations(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Specialization>().HasData(
            new Specialization
            {
                SpecializationId = (int)SpecializationEnum.Science,
                Name = "Science"
            },
            new Specialization
            {
                SpecializationId = (int)SpecializationEnum.Lettres,
                Name = "Lettres"
            },
            new Specialization
            {
                SpecializationId = (int)SpecializationEnum.GestionEconomie,
                Name = "Gestion et Économie"
            },
            new Specialization
            {
                SpecializationId = (int)SpecializationEnum.Mathematiques,
                Name = "Mathématiques"
            },
            new Specialization
            {
                SpecializationId = (int)SpecializationEnum.SciencesExperimentales,
                Name = "Sciences Expérimentales"
            },
            new Specialization
            {
                SpecializationId = (int)SpecializationEnum.TechniqueGenieCivil,
                Name = "Technique Mathématiques - Génie Civil"
            },
            new Specialization
            {
                SpecializationId = (int)SpecializationEnum.TechniqueGenieElectrique,
                Name = "Technique Mathématiques - Génie Électrique"
            },
            new Specialization
            {
                SpecializationId = (int)SpecializationEnum.TechniqueGenieMecanique,
                Name = "Technique Mathématiques - Génie Mécanique"
            },
            new Specialization
            {
                SpecializationId = (int)SpecializationEnum.LettresPhilosophie,
                Name = "Lettres et Philosophie"
            },
            new Specialization
            {
                SpecializationId = (int)SpecializationEnum.LanguesEtrangeres,
                Name = "Langues Etrangeres"
            }

        );
    }
}