using Dirassati_Backend.Data.DTOs;
using Dirassati_Backend.Data.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dirassati_Backend.Domain.Services
{
    public interface ICsvService
    {
        Task<List<CsvNoteRecord>> ProcessNotesCsv(IFormFile file);
    }
}
