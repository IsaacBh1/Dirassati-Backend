using CsvHelper;
using Dirassati_Backend.Data.DTOs;
using Dirassati_Backend.Domain.Services;
using System.Globalization;

namespace Dirassati_Backend.Features.Notes.Services;

public class CsvService : ICsvService
{
    private readonly ILogger<CsvService> _logger;

    public CsvService(ILogger<CsvService> logger)
    {
        _logger = logger;
    }

    public async Task<List<CsvNoteRecord>> ProcessNotesCsv(IFormFile file)
    {
        _logger.LogInformation("Starting to process CSV file: {FileName}, Size: {FileSize} bytes", file.FileName, file.Length);

        try
        {
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var records = csv.GetRecords<CsvNoteRecord>().ToList();

            _logger.LogInformation("Successfully processed CSV file with {RecordCount} records", records.Count);
            return records;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing CSV file: {FileName}", file.FileName);
            throw;
        }
    }
}