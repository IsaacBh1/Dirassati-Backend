using Dirassati_Backend.Data.Models;

namespace Dirassati_Backend.Features.Absences.Repos
{
    public interface IAbsenceRepository
    {
        Task AddAbsenceAsync(Absence absence);
        Task UpdateAbsenceAsync(Absence absence);
        Task SaveChangesAsync();
        Task<List<Absence>> GetAbsencesByStudentIdsAsync(List<Guid> studentIds, bool isNotified);
        Task<List<Absence>> GetAbsencesByStudentIdAsync(Guid studentId);
    }
}