using CsvHelper.Configuration.Attributes;
using Microsoft.AspNetCore.Http;

namespace Dirassati_Backend.Data.DTOs
{
    public class CsvNoteRecord
    {
        [Name("StudentId")]
        public Guid StudentId { get; set; }

        [Name("FirstName")]
        public string FirstName { get; set; } = null!;

        [Name("LastName")]
        public string LastName { get; set; } = null!;

        [Name("Value")]
        public double? Value { get; set; }
    }

    public class BulkNoteCreateDto
    {
        public int Tremester { get; set; }
        public int AcademicYearId { get; set; }
        public int ExamTypeId { get; set; }
        public int SubjectId { get; set; }
        public Guid GroupId { get; set; }
        public IFormFile CsvFile { get; set; } = null!;
    }
}