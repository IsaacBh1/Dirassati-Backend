using AutoMapper;
using Dirassati_Backend.Data.Models;
using Dirassati_Backend.Features.Common; // Contains PaginatedResult<T>
using Dirassati_Backend.Features.Parents.Dtos;
using Dirassati_Backend.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Dirassati_Backend.Features.Parents.Repositories
{
    public class ParentRepository : IParentRepository
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ParentRepository(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // Gets all parents for a given school (non-paginated)
        public async Task<IEnumerable<GetParentDto>> GetAllBySchoolIdAsync(Guid schoolId)
        {
            var parents = await _context.Parents
                .Include(p => p.User)
                // Removed relationshipToStudent include, since it's now determined from Student.
                .Where(p => _context.Students.Any(s => s.ParentId == p.ParentId && s.SchoolId == schoolId))
                .ToListAsync();

            return _mapper.Map<IEnumerable<GetParentDto>>(parents);
        }

        // Gets all parents for a given school, paginated
        public async Task<PaginatedResult<GetParentDto>> GetAllBySchoolIdAsync(Guid schoolId, int pageNumber, int pageSize)
        {
            var query = _context.Parents
                .Include(p => p.User)
                // Removed relationshipToStudent include.
                .Where(p => _context.Students.Any(s => s.ParentId == p.ParentId && s.SchoolId == schoolId));

            var totalRecords = await query.CountAsync();
            var parents = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var parentDtos = _mapper.Map<IEnumerable<GetParentDto>>(parents);

            return new PaginatedResult<GetParentDto>
            {
                Items = parentDtos,
                TotalRecords = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        // Gets a single parent by its ID
        public async Task<GetParentDto?> GetParentByIdAsync(Guid parentId)
        {
            var parent = await _context.Parents
                .Include(p => p.User)
                //.Include(p => p.relationshipToStudent) // Removed as per new logic.
                .FirstOrDefaultAsync(p => p.ParentId == parentId);

            return parent == null ? null : _mapper.Map<GetParentDto>(parent);
        }

        // Creates a new parent record
        public async Task<Parent> CreateAsync(Parent parent)
        {
            _context.Parents.Add(parent);
            await _context.SaveChangesAsync();
            return parent;
        }

        // Updates an existing parent record based on a DTO
        public async Task<GetParentDto?> UpdateAsync(UpdateParentDto updateDto)
        {
            var existingParent = await _context.Parents
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.ParentId == updateDto.ParentId);

            if (existingParent == null)
                return null;

            // Relationship validation removed since parent's relationship logic is now in Student.
            _mapper.Map(updateDto, existingParent);

            await _context.SaveChangesAsync();
            return _mapper.Map<GetParentDto>(existingParent);
        }

        // Deletes a parent record
        public async Task<bool> DeleteAsync(Guid id)
        {
            var parent = await _context.Parents.FindAsync(id);
            if (parent == null)
                return false;

            _context.Parents.Remove(parent);
            await _context.SaveChangesAsync();
            return true;
        }

        // Gets students by a parent's ID (returns a collection of getStudentDto)
        public async Task<IEnumerable<getStudentDto>> GetStudentsByParentIdAsync(Guid parentId)
        {
            var students = await _context.Students
                .Include(s => s.SchoolLevel) // Ensure this relationship is loaded
                .Include(s => s.Specialization) // Ensure this is loaded
                .Where(s => s.ParentId == parentId)
                .ToListAsync();

            return students.Select(s => new getStudentDto
            {
                StudentId = s.StudentId,
                FirstName = s.FirstName,
                LastName = s.LastName,
                EnrollmentDate = s.EnrollmentDate,
                Grade = (s.SchoolLevel != null ? s.SchoolLevel.LevelYear.ToString() : "Unknown") +
                        " " + (s.Specialization?.Name ?? string.Empty),
                IsActive = s.IsActive
            });
        }

        // Gets a parent by a student's ID (returns getStudentParentDto)
        public async Task<getStudentParentDto?> GetParentByStudentIdAsync(Guid studentId)
        {
            var student = await _context.Students
                .Include(s => s.Parent)   // Navigation property in Student
                    .ThenInclude(p => p.User)
                .Include(s => s.ParentRelationshipToStudentType) // Use the student's relationship type
                .FirstOrDefaultAsync(s => s.StudentId == studentId);

            if (student?.Parent == null)
                return null;

            return new getStudentParentDto
            {
                ParentId = student.Parent.ParentId,
                FirstName = student.Parent.User.FirstName,
                LastName = student.Parent.User.LastName,
                Occupation = student.Parent.Occupation,
                // Use the relationship type from the Student entity
                RelationshipToStudent = student.ParentRelationshipToStudentType?.Name ?? string.Empty,
                Email = student.Parent.User.Email ?? string.Empty,
                PhoneNumber = student.Parent.User.PhoneNumber ?? string.Empty,
            };
        }
    }
}
