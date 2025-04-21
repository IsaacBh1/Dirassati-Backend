namespace Dirassati_Backend.Common.Dtos;

public class LevelDto
{
    public int? LevelId { get; set; }

    public int Year { get; set; }
    public string? Specialization { get; set; } = null!;
    public string? SchoolType { get; set; } = null!;

}

public class GetLevelDto
{
    public int Year { get; set; }
    public string? Specialization { get; set; } = null!;
    public string? SchoolType { get; set; } = null!;
}


