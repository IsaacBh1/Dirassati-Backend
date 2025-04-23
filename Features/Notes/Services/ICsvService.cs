using Dirassati_Backend.Data.DTOs;

namespace Dirassati_Backend.Domain.Services;

public interface ICsvService
{
    Task<List<CsvNoteRecord>> ProcessNotesCsv(IFormFile file);
}