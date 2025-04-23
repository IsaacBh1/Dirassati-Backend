namespace Dirassati_Backend.Features.Classrooms.Dtos
{
    public class AddClassroomDto
    {
        public required string ClassName { get; set; } = null!;
        public required int SchoolLevelId { get; set; }
        public int? SpecializationId { get; set; }
    }

    public class UpdateClassroomDto
    {
        public required string ClassName { get; set; } = null!;
        public int? SchoolLevelId { get; set; }
        public int? SpecializationId { get; set; }
    }

    public class ClassroomDto
    {
        public Guid ClassroomId { get; set; }
        public string ClassName { get; set; } = null!;
        public int SchoolLevelId { get; set; }
        public int? SpecializationId { get; set; }
        public string LevelName { get; set; } = null!;
        public string SpecializationName { get; set; } = null!;
        public string SchoolType { get; set; } = null!;
    }

    public class ClassroomDetailDto
    {
        public Guid ClassroomId { get; set; }
        public string ClassName { get; set; } = null!;
        public int SchoolLevelId { get; set; }
        public int? SpecializationId { get; set; }
        public string LevelName { get; set; } = null!;
        public string SpecializationName { get; set; } = null!;
        public string SchoolType { get; set; } = null!;
        public ClassroomGroupDto? Group { get; set; } = null!;
    }

    public class ClassroomGroupDto
    {
        public Guid GroupId { get; set; }
        public string GroupName { get; set; } = null!;
        public int StudentCount { get; set; }
        public int GroupCapacity { get; set; }
    }
}