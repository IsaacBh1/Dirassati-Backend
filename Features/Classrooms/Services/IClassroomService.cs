using Dirassati_Backend.Common;
using Dirassati_Backend.Features.Classrooms.Dtos;

namespace Dirassati_Backend.Features.Classrooms.Services;

public interface IClassroomServices
{
    Task<Result<ClassroomDto, string>> AddClassroomAsync(AddClassroomDto addClassroomDto, string schoolId);
    Task<Result<List<ClassroomDto>, string>> GetClassroomsBySchoolLevelAsync(int levelId, string schoolId);
    Task<Result<List<ClassroomDto>, string>> GetAllClassroomsBySchoolAsync(string schoolId);
    Task<Result<ClassroomDetailDto, string>> GetClassroomDetailsAsync(Guid classroomId, string schoolId);
    Task<Result<ClassroomDto, string>> UpdateClassroomAsync(Guid classroomId, UpdateClassroomDto updateClassroomDto, string schoolId);

    /// <summary>
    /// Deletes a classroom from the system
    /// </summary>
    /// <param name="classroomId">The ID of the classroom to delete</param>
    /// <param name="schoolId">The ID of the school that owns the classroom</param>
    /// <returns>A success or error message</returns>
    Task<Result<Unit, string>> DeleteClassroomAsync(Guid classroomId, string schoolId);
}