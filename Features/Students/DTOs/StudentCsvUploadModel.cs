using Microsoft.Build.Framework;

namespace Dirassati_Backend.Features.Students.DTOs;

public class StudentCsvUploadModel
{
    [Required]
    public required IFormFile CsvFile { get; set; }
        
    public bool HasHeaders { get; set; } = true;
}