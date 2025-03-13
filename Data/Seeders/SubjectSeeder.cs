using Dirassati_Backend.Data.Enums;
using Dirassati_Backend.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Dirassati_Backend.Data.Seeders
{
    public class SubjectSeeder
    {
        public static void SeedSubjects(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Subject>()
                .HasData(
                    // Primaire (SchoolTypeEnum.Primaire)
                    new Subject { SubjectId = 1, Name = "général", Level = SchoolTypeEnum.Primaire },
                    new Subject { SubjectId = 2, Name = "Langue Française", Level = SchoolTypeEnum.Primaire },
                    new Subject { SubjectId = 3, Name = "Langue Anglaise", Level = SchoolTypeEnum.Primaire },
                    new Subject { SubjectId = 4, Name = "Tamazight", Level = SchoolTypeEnum.Primaire },

                    // Moyen (SchoolTypeEnum.Moyenne)
                    new Subject { SubjectId = 101, Name = "Histoire", Level = SchoolTypeEnum.Moyenne },
                    new Subject { SubjectId = 102, Name = "Géographie", Level = SchoolTypeEnum.Moyenne },
                    new Subject { SubjectId = 103, Name = "Éducation Islamique", Level = SchoolTypeEnum.Moyenne },
                    new Subject { SubjectId = 104, Name = "Éducation Civique", Level = SchoolTypeEnum.Moyenne },
                    new Subject { SubjectId = 105, Name = "Mathématiques", Level = SchoolTypeEnum.Moyenne },
                    new Subject { SubjectId = 106, Name = "Sciences de la Nature et de la Vie", Level = SchoolTypeEnum.Moyenne },
                    new Subject { SubjectId = 107, Name = "Sciences Physiques et Technologiques", Level = SchoolTypeEnum.Moyenne },
                    new Subject { SubjectId = 108, Name = "Langue Arabe", Level = SchoolTypeEnum.Moyenne },
                    new Subject { SubjectId = 109, Name = "Langue Tamazight", Level = SchoolTypeEnum.Moyenne },
                    new Subject { SubjectId = 110, Name = "Langue Française", Level = SchoolTypeEnum.Moyenne },
                    new Subject { SubjectId = 111, Name = "Langue Anglaise", Level = SchoolTypeEnum.Moyenne },
                    new Subject { SubjectId = 112, Name = "EPS", Level = SchoolTypeEnum.Moyenne },
                    new Subject { SubjectId = 113, Name = "Éducation Artistique", Level = SchoolTypeEnum.Moyenne },

                    // Lycée (SchoolTypeEnum.Lycee)
                    new Subject { SubjectId = 201, Name = "Économie et Management", Level = SchoolTypeEnum.Lycee },
                    new Subject { SubjectId = 202, Name = "Histoire", Level = SchoolTypeEnum.Lycee },
                    new Subject { SubjectId = 203, Name = "Géographie", Level = SchoolTypeEnum.Lycee },
                    new Subject { SubjectId = 204, Name = "Gestion Comptabilité et Finances", Level = SchoolTypeEnum.Lycee },
                    new Subject { SubjectId = 205, Name = "Mathématiques", Level = SchoolTypeEnum.Lycee },
                    new Subject { SubjectId = 206, Name = "Sciences Islamiques", Level = SchoolTypeEnum.Lycee },
                    new Subject { SubjectId = 207, Name = "Sciences de la Nature et de la Vie", Level = SchoolTypeEnum.Lycee },
                    new Subject { SubjectId = 208, Name = "Sciences Physiques", Level = SchoolTypeEnum.Lycee },
                    new Subject { SubjectId = 209, Name = "Philosophie", Level = SchoolTypeEnum.Lycee },
                    new Subject { SubjectId = 210, Name = "Droit", Level = SchoolTypeEnum.Lycee },
                    new Subject { SubjectId = 211, Name = "Espagnol", Level = SchoolTypeEnum.Lycee },
                    new Subject { SubjectId = 212, Name = "Allemand", Level = SchoolTypeEnum.Lycee },
                    new Subject { SubjectId = 213, Name = "Anglais", Level = SchoolTypeEnum.Lycee },
                    new Subject { SubjectId = 214, Name = "Italien", Level = SchoolTypeEnum.Lycee },
                    new Subject { SubjectId = 215, Name = "Français", Level = SchoolTypeEnum.Lycee },
                    new Subject { SubjectId = 216, Name = "Arabe", Level = SchoolTypeEnum.Lycee },
                    new Subject { SubjectId = 217, Name = "Tamazight", Level = SchoolTypeEnum.Lycee },
                    new Subject { SubjectId = 218, Name = "Informatique", Level = SchoolTypeEnum.Lycee },
                    new Subject { SubjectId = 219, Name = "Génie des Procédés", Level = SchoolTypeEnum.Lycee },
                    new Subject { SubjectId = 220, Name = "Génie Électrique", Level = SchoolTypeEnum.Lycee },
                    new Subject { SubjectId = 221, Name = "Génie Civil", Level = SchoolTypeEnum.Lycee },
                    new Subject { SubjectId = 222, Name = "Génie Mécanique", Level = SchoolTypeEnum.Lycee },
                    new Subject { SubjectId = 223, Name = "Éducation Physique et Sportive", Level = SchoolTypeEnum.Lycee },
                    new Subject { SubjectId = 224, Name = "Éducation Artistique", Level = SchoolTypeEnum.Lycee },
                    new Subject { SubjectId = 225, Name = "Technologie", Level = SchoolTypeEnum.Lycee },
                    new Subject { SubjectId = 226, Name = "Musique", Level = SchoolTypeEnum.Lycee }
                );
        }
    }
}