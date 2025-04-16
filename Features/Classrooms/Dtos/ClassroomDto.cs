namespace Dirassati_Backend.Features.Classrooms.Dtos;

public class AddClassroomDto
{
    public string ClassName { get; set; } = null!;
    public string? Description { get; set; }
    public int SchoolLevelId { get; set; }
}

public class ClassroomDto
{
    public Guid ClassroomId { get; set; }
    public string ClassName { get; set; } = null!;
    public int SchoolLevelId { get; set; }
    public string LevelName { get; set; } = null!;
}