namespace Dirassati_Backend.Features.SchoolLevels.DTOs;

public class GetSchoolLevelDTO
{
    public int LevelId { get; set; }
    public int SchoolTypeId { get; set; }
    public string SchoolTypeName { get; set; } = null!;
    public int LevelYear { get; set; }

}
