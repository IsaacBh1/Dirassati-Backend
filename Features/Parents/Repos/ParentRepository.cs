using AutoMapper;
using Dirassati_Backend.Domain.Models;
using Dirassati_Backend.Features.Parents.Dtos;
using Microsoft.EntityFrameworkCore;
using Persistence;
using static Dirassati_Backend.Features.Parents.Dtos.ParentDtos;

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
        public async Task<IEnumerable<GetParentDto>> GetAllAsync()
        {
            var parents = await _context.Parents
                .Include(p => p.User)
                .Include(p => p.relationshipToStudent)
                .ToListAsync();

            return _mapper.Map<IEnumerable<GetParentDto>>(parents);
        }



        public async Task<GetParentDto?> GetParentByIdAsync(Guid parentId)
        {
            var parent = await _context.Parents
                .Include(p => p.User)
                .Include(p => p.relationshipToStudent)
                .FirstOrDefaultAsync(p => p.ParentId == parentId);

            return parent == null ? null : _mapper.Map<GetParentDto>(parent);
        }



        public async Task<Parent> CreateAsync(Parent parent)
        {
            _context.Parents.Add(parent);
            await _context.SaveChangesAsync();
            return parent;
        }

        public async Task<GetParentDto?> UpdateAsync(UpdateParentDto updateDto)
        {
            var existingParent = await _context.Parents
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.ParentId == updateDto.ParentId);

            if (existingParent == null)
                return null;


            // note , this is for front end if he send a wrong RelationshipToStudentId

            bool isValidRelationship = await _context.RelationshipToStudents
        .AnyAsync(r => r.Id == updateDto.RelationshipToStudentId);

            if (!isValidRelationship)
                throw new InvalidOperationException("Invalid RelationshipToStudentId.");


            _mapper.Map(updateDto, existingParent);

            await _context.SaveChangesAsync();
            return _mapper.Map<GetParentDto>(existingParent);
        }


        public async Task<bool> DeleteAsync(Guid id)
        {
            var parent = await _context.Parents.FindAsync(id);
            if (parent == null)
                return false;

            _context.Parents.Remove(parent);
            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<IEnumerable<getStudentDto>> GetStudentsByParentIdAsync(Guid parentId)
        {
            var students = await _context.Students.Where(s => s.ParentId == parentId).ToListAsync();
            return students.Select(s => new getStudentDto
            {
                StudentId = s.StudentId,
                FirstName = s.FirstName,
                LastName = s.LastName,
                EnrollmentDate = s.EnrollmentDate,
                Grade = s.LevelYear + " " + s.Stream?.Name,
                IsActive = s.IsActive
            });

        }


        public async Task<getStudentParentDto?> GetParentByStudentIdAsync(Guid studentId)
        {
            var student = await _context.Students
            .Include(s => s.parent)
                .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(s => s.StudentId == studentId);

            if (student?.parent == null)
                return null;

            return new getStudentParentDto
            {
                ParentId = student.parent.ParentId,
                FirstName = student.parent.User.FirstName,
                LastName = student.parent.User.LastName,
                Occupation = student.parent.Occupation,
                RelationshipToStudent = student.parent.relationshipToStudent?.Name ?? string.Empty,
                Email = student.parent?.User?.Email ?? string.Empty,
                PhoneNumber = student.parent?.User?.PhoneNumber ?? string.Empty,
            };
        }


    }
}
