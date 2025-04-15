using System;
using Dirassati_Backend.Common.Dtos;

namespace Dirassati_Backend.Features.Groups.Dtos;

public class GroupDto
{

    public Guid GroupId { get; set; } // Primary Key for the Group

    public string GroupName { get; set; } = null!; // Name of the group (Corrected typo from GorupName)

    public int LevelId { get; set; } // Foreign Key referencing the SchoolLevel



    public int GroupCapacity { get; set; } // Maximum number of students in the group

    public Guid SchoolId { get; set; } = Guid.Empty; // Foreign Key referencing the School


    // Navigation properties

    public virtual LevelDto Level { get; set; } = null!;

    public virtual ICollection<StudentGroupDto> Students { get; set; } = [];

}

public class GroupListingDto
{
    public Guid GroupId { get; set; }
    public string GroupName { get; set; } = null!;
    public int LevelId { get; set; }
    public int GroupCapacity { get; set; }
    public int StudentCount { get; set; }
    public LevelDto Level { get; set; } = null!;
}
