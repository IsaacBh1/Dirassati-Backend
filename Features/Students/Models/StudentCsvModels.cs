using CsvHelper.Configuration;

namespace Dirassati_Backend.Features.Students.Models;

public class StudentCsvRecord
{
    public required  string StudentFirstName { get; set; }
    public required  string StudentLastName { get; set; }
    public required  string StudentAddress { get; set; }
    public required  string StudentBirthDate { get; set; }
    public required  string StudentBirthPlace { get; set; }
    public required  string EmergencyContact { get; set; }
    public required  string LevelYear { get; set; }
    public   string? SpecializationId { get; set; }
    public required  string ParentNationalIdNumber { get; set; }
    public required  string ParentFirstName { get; set; }
    public required  string ParentLastName { get; set; }
    public required  string ParentEmail { get; set; }
    public required  string RelationshipToStudentId { get; set; }
    public required  string ParentOccupation { get; set; }
    public required  string ParentPhoneNumber { get; set; }
}

public sealed class StudentCsvRecordMap : ClassMap<StudentCsvRecord>
{
    public StudentCsvRecordMap()
    {
        // Student basic information
       Map(m => m.StudentFirstName).Name("Prénom");
        Map(m => m.StudentLastName).Name("Nom");
        Map(m => m.StudentBirthDate).Name("Date de Naissance");
        Map(m => m.StudentBirthPlace).Name("Lieu de Naissance");
        Map(m => m.StudentAddress).Name("Adresse");
        Map(m => m.EmergencyContact).Name("Contact d'Urgence");
        
        // Academic information
        Map(m => m.LevelYear).Name("Année");
        Map(m => m.SpecializationId).Name("Id de Spécialisation");
        
        // Parent information
        Map(m => m.ParentNationalIdNumber).Name("Numéro National d'Identité du Parent");
        Map(m => m.ParentFirstName).Name("Prénom du Parent");
        Map(m => m.ParentLastName).Name("Nom du Parent");
        Map(m => m.ParentEmail).Name("Email du Parent");
        Map(m => m.ParentPhoneNumber).Name("Numéro de Téléphone du Parent");
        Map(m => m.ParentOccupation).Name("Profession du Parent");
        Map(m => m.RelationshipToStudentId).Name("Relation avec l'Étudiant");
    }
}