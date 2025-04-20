using CsvHelper;
using Dirassati_Backend.Data.DTOs;
using Dirassati_Backend.Domain.Services;
using System.Globalization;

namespace Dirassati_Backend.Persistence.Services;

public class CsvService : ICsvService
{
    public CsvService()
    {
    }

    public async Task<List<CsvNoteRecord>> ProcessNotesCsv(IFormFile file)
    {
        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        stream.Position = 0;

        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        return csv.GetRecords<CsvNoteRecord>().ToList();
    }
}