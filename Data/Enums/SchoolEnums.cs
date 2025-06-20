namespace Dirassati_Backend.Data.Enums;
#pragma warning disable S2344 // Enumeration type names should not have "Flags" or "Enum" suffixes

public enum SchoolTypeEnum
{
    Primaire = 1,
    Moyenne = 2,
    Lycee = 3
}

public enum SpecializationEnum
{
    Science = 1,
    Lettres,
    GestionEconomie,           // تسيير واقتصاد
    Mathematiques,             // رياضيات
    SciencesExperimentales,    // علوم تجريبية
    TechniqueGenieCivil,       // تقني رياضي تخصص هندسة مدنية
    TechniqueGenieElectrique,  // تقني رياضي تخصص هندسة كهربائية
    TechniqueGenieMecanique,  // تقني رياضي تخصص هندسة ميكانيكية
    LettresPhilosophie,
    LanguesEtrangeres,
}

public enum SchoolLevelsEnum
{
    Primaire1er = 1,
    Primaire2eme,
    Primaire3eme,
    Primaire4eme,
    Primaire5eme,
    Moyenne1er,
    Moyenne2eme,
    Moyenne3eme,
    Moyenne4eme,
    Lycee1er,
    Lycee2eme,
    Lycee3eme
}


public enum ReportStatusEnum
{
    Pending = 1,
    Sent,
    Viewed,
}




public enum PaymentStatus
{
    Pending,
    Paid,
    Failed
}
