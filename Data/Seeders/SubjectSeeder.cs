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
                    new Subject { SubjectId = 1, Name = "général", SchoolType = SchoolTypeEnum.Primaire },
                    new Subject { SubjectId = 2, Name = "Langue Française", SchoolType = SchoolTypeEnum.Primaire },
                    new Subject { SubjectId = 3, Name = "Langue Anglaise", SchoolType = SchoolTypeEnum.Primaire },
                    new Subject { SubjectId = 4, Name = "Tamazight", SchoolType = SchoolTypeEnum.Primaire },

                    // Moyen (SchoolTypeEnum.Moyenne)
                    new Subject { SubjectId = 101, Name = "Histoire", SchoolType = SchoolTypeEnum.Moyenne },
                    new Subject { SubjectId = 102, Name = "Géographie", SchoolType = SchoolTypeEnum.Moyenne },
                    new Subject { SubjectId = 103, Name = "Éducation Islamique", SchoolType = SchoolTypeEnum.Moyenne },
                    new Subject { SubjectId = 104, Name = "Éducation Civique", SchoolType = SchoolTypeEnum.Moyenne },
                    new Subject { SubjectId = 105, Name = "Mathématiques", SchoolType = SchoolTypeEnum.Moyenne },
                    new Subject { SubjectId = 106, Name = "Sciences de la Nature et de la Vie", SchoolType = SchoolTypeEnum.Moyenne },
                    new Subject { SubjectId = 107, Name = "Sciences Physiques et Technologiques", SchoolType = SchoolTypeEnum.Moyenne },
                    new Subject { SubjectId = 108, Name = "Langue Arabe", SchoolType = SchoolTypeEnum.Moyenne },
                    new Subject { SubjectId = 109, Name = "Langue Tamazight", SchoolType = SchoolTypeEnum.Moyenne },
                    new Subject { SubjectId = 110, Name = "Langue Française", SchoolType = SchoolTypeEnum.Moyenne },
                    new Subject { SubjectId = 111, Name = "Langue Anglaise", SchoolType = SchoolTypeEnum.Moyenne },
                    new Subject { SubjectId = 112, Name = "EPS", SchoolType = SchoolTypeEnum.Moyenne },
                    new Subject { SubjectId = 113, Name = "Éducation Artistique", SchoolType = SchoolTypeEnum.Moyenne },

                    // Lycée (SchoolTypeEnum.Lycee)
                    new Subject { SubjectId = 201, Name = "Économie et Management", SchoolType = SchoolTypeEnum.Lycee },
                    new Subject { SubjectId = 202, Name = "Histoire", SchoolType = SchoolTypeEnum.Lycee },
                    new Subject { SubjectId = 203, Name = "Géographie", SchoolType = SchoolTypeEnum.Lycee },
                    new Subject { SubjectId = 204, Name = "Gestion Comptabilité et Finances", SchoolType = SchoolTypeEnum.Lycee },
                    new Subject { SubjectId = 205, Name = "Mathématiques", SchoolType = SchoolTypeEnum.Lycee },
                    new Subject { SubjectId = 206, Name = "Sciences Islamiques", SchoolType = SchoolTypeEnum.Lycee },
                    new Subject { SubjectId = 207, Name = "Sciences de la Nature et de la Vie", SchoolType = SchoolTypeEnum.Lycee },
                    new Subject { SubjectId = 208, Name = "Sciences Physiques", SchoolType = SchoolTypeEnum.Lycee },
                    new Subject { SubjectId = 209, Name = "Philosophie", SchoolType = SchoolTypeEnum.Lycee },
                    new Subject { SubjectId = 210, Name = "Droit", SchoolType = SchoolTypeEnum.Lycee },
                    new Subject { SubjectId = 211, Name = "Espagnol", SchoolType = SchoolTypeEnum.Lycee },
                    new Subject { SubjectId = 212, Name = "Allemand", SchoolType = SchoolTypeEnum.Lycee },
                    new Subject { SubjectId = 213, Name = "Anglais", SchoolType = SchoolTypeEnum.Lycee },
                    new Subject { SubjectId = 214, Name = "Italien", SchoolType = SchoolTypeEnum.Lycee },
                    new Subject { SubjectId = 215, Name = "Français", SchoolType = SchoolTypeEnum.Lycee },
                    new Subject { SubjectId = 216, Name = "Arabe", SchoolType = SchoolTypeEnum.Lycee },
                    new Subject { SubjectId = 217, Name = "Tamazight", SchoolType = SchoolTypeEnum.Lycee },
                    new Subject { SubjectId = 218, Name = "Informatique", SchoolType = SchoolTypeEnum.Lycee },
                    new Subject { SubjectId = 219, Name = "Génie des Procédés", SchoolType = SchoolTypeEnum.Lycee },
                    new Subject { SubjectId = 220, Name = "Génie Électrique", SchoolType = SchoolTypeEnum.Lycee },
                    new Subject { SubjectId = 221, Name = "Génie Civil", SchoolType = SchoolTypeEnum.Lycee },
                    new Subject { SubjectId = 222, Name = "Génie Mécanique", SchoolType = SchoolTypeEnum.Lycee },
                    new Subject { SubjectId = 223, Name = "Éducation Physique et Sportive", SchoolType = SchoolTypeEnum.Lycee },
                    new Subject { SubjectId = 224, Name = "Éducation Artistique", SchoolType = SchoolTypeEnum.Lycee },
                    new Subject { SubjectId = 225, Name = "Technologie", SchoolType = SchoolTypeEnum.Lycee },
                    new Subject { SubjectId = 226, Name = "Musique", SchoolType = SchoolTypeEnum.Lycee }
                );
        }
    }
}