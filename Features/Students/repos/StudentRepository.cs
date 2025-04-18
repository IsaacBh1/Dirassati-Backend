using Dirassati_Backend.Features.Students.DTOs;
using Dirassati_Backend.Features.Common;
using Microsoft.EntityFrameworkCore;

using Dirassati_Backend.Persistence;
using Dirassati_Backend.Data.Models;


namespace Dirassati_Backend.Features.Students.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly AppDbContext _context;

        public StudentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<StudentDetailsDto?> GetStudentByIdAsync(Guid studentId)
        {
            var student = await _context.Students
            .Include(s => s.Parent)
            .Include(s => s.School)
            .FirstOrDefaultAsync(s => s.StudentId == studentId);

            if (student == null) return null;

            return new StudentDetailsDto(
                student.StudentId,
                student.FirstName,
                student.LastName,
                student.Address ?? "",
                student.BirthDate,
                student.BirthPlace ?? "",
                student.SchoolId,
                student.StudentIdNumber,
                student.EmergencyContact ?? "",
                student.SchoolLevelId,
                student.SpecializationId,
                student.ParentRelationshipToStudentTypeId,
                student.PhotoUrl,
                student.EnrollmentDate,
                student.ParentId,
                student.IsActive,
                student.GroupId
            );
        }

        public async Task<List<Student>> GetStudentsByGroupAsync(Guid groupId)

        {
            var students = await _context.Students
            .Where(s => s.GroupId == groupId)
            .OrderBy(s => s.LastName)
            .AsNoTracking()
            .ToListAsync();
            return students;
        }



        public async Task<PaginatedResult<StudentDto>> GetStudentsBySchoolIdAsync(Guid schoolId, int page, int pageSize)
        {
            var query = _context.Students
                .Where(s => s.SchoolId == schoolId)
                .Include(s => s.Parent);

            var totalRecords = await query.CountAsync();

            var items = await query
                .OrderBy(s => s.LastName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new StudentDto(
                    s.StudentId,
                    s.FirstName,
                    s.LastName,
                    s.StudentIdNumber ?? "",
                    $"{s.Parent.User.FirstName} {s.Parent.User.LastName}",
                    s.PhotoUrl,
                    s.Parent.User.Email ?? s.Parent.User.PhoneNumber ?? "not found"
                ))
                .AsNoTracking()
                .ToListAsync();

            return new PaginatedResult<StudentDto>
            {
                Items = items,
                TotalRecords = totalRecords,
                PageNumber = page,
                PageSize = pageSize
            };
        }


        public async Task<bool> SchoolExistsAsync(Guid schoolId)
        {
            return await _context.Schools.AnyAsync(s => s.SchoolId == schoolId);
        }


    }
}
