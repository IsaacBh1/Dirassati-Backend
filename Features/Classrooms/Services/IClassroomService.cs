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
    }