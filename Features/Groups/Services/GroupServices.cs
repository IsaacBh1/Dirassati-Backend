using System;
using Dirassati_Backend.Common;
using Dirassati_Backend.Features.Groups.Dtos;
using Dirassati_Backend.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Net;
using Microsoft.AspNetCore.SignalR.Protocol;
using Dirassati_Backend.Common.Dtos;
using Dirassati_Backend.Persistence;

namespace Dirassati_Backend.Features.Groups.Services;

public class GroupServices(AppDbContext context) : IGroupServices
{
    private readonly AppDbContext _context = context;

    public async Task<Result<GroupDto, string>> AddGroupAsync(AddGroupDto groupDto, string schoolId)
    {
        var result = new Result<GroupDto, string>();
        // Validate input
        if (groupDto.StudentIds.Count < 2)
        {
            return result.Failure("At least two students must be selected for a group", (int)HttpStatusCode.BadRequest);
        }

        // Get the current school ID from claims

        if (string.IsNullOrEmpty(schoolId) || !Guid.TryParse(schoolId, out var parsedSchoolId))
        {
            return result.Failure("Invalid school context", (int)HttpStatusCode.BadRequest);
        }

        // Check if the school level exists
        var level = await _context.SchoolLevels.FirstOrDefaultAsync(l => l.LevelId == groupDto.LevelId);
        if (level is null)
        {
            return result.Failure("Invalid school level", (int)HttpStatusCode.BadRequest);
        }

        // Check if all students exist and belong to the specified school
        var students = await _context.Students
            .Where(s => groupDto.StudentIds.Contains(s.StudentId) && s.SchoolId == parsedSchoolId)
            .ToListAsync();

        if (students.Count != groupDto.StudentIds.Count)
        {
            return result.Failure("One or more students do not exist or don't belong to this school", (int)HttpStatusCode.BadRequest);
        }

        // Check if any of the students are already in another group
        var studentsInGroups = students.Where(s => s.GroupId != null).ToList();
        if (studentsInGroups.Count != 0)
        {
            var studentNames = string.Join(", ", studentsInGroups.Select(s => $"{s.FirstName} {s.LastName} {s.GroupId}"));
            return result.Failure($"The following students are already assigned to groups: {studentNames}", (int)HttpStatusCode.BadRequest);
        }
        var tempStudent = students.FirstOrDefault();
        if (tempStudent is null)
            return result.Failure($"Error Fetchin Specialization", (int)HttpStatusCode.BadRequest);
        await _context.Entry(tempStudent).Reference(s => s.Specialization).LoadAsync();
        // Create new group
        var group = new Group
        {
            GroupId = Guid.NewGuid(),
            GroupName = groupDto.GroupName,
            LevelId = groupDto.LevelId,
            GroupCapacity = groupDto.GroupCapacity,
            SchoolId = parsedSchoolId,
            SpecializationId = tempStudent.SpecializationId
        };

        // Add group to the database
        await _context.Groups.AddAsync(group);

        // Update students with the new GroupId
        foreach (var student in students)
        {
            student.GroupId = group.GroupId;
        }

        // Save changes
        await _context.SaveChangesAsync();


        // Create response DTO
        var responseDto = new GroupDto
        {
            GroupId = group.GroupId,
            GroupName = group.GroupName,
            LevelId = group.LevelId,
            GroupCapacity = group.GroupCapacity,
            SchoolId = group.SchoolId,
            Level = new LevelDto
            {

                Year = level.LevelYear,
                Specialization = tempStudent.Specialization?.Name,

            },

            Students = [.. students.Select(s => new StudentGroupDto
            {
                StudentId = s.StudentId,
                FirstName = s.FirstName,
                LastName = s.LastName,
                StudentIdNumber = s.StudentIdNumber ?? string.Empty,
            })]
        };

        return result.Success(responseDto);
    }
    public async Task<Result<List<GroupListingDto>, string>> GetGroupsByLevelIdAsync(int levelId, string schoolId)
    {
        var result = new Result<List<GroupListingDto>, string>();

        // Validate inputs
        if (string.IsNullOrEmpty(schoolId) || !Guid.TryParse(schoolId, out var parsedSchoolId))
        {
            return result.Failure("Invalid school context", (int)HttpStatusCode.BadRequest);
        }

        // Check if the level exists and belongs to the school
        var level = await _context.SchoolLevels.FirstOrDefaultAsync(l => l.LevelId == levelId);
        if (level is null)
        {
            return result.Failure("Invalid school level", (int)HttpStatusCode.BadRequest);
        }
        if (!Guid.TryParse(schoolId, out var schoolIdGuid))
            return result.Failure("Invalid School Id", (int)HttpStatusCode.BadRequest);
        var schooType = (await _context.Schools.Include(s => s.SchoolType).FirstOrDefaultAsync(s => s.SchoolId == schoolIdGuid))?.SchoolType.Name;

        var groupsQuery = _context.Groups

         .Where(g => g.LevelId == levelId && g.SchoolId == parsedSchoolId)
         .Select(g => new GroupListingDto
         {
             GroupId = g.GroupId,
             GroupName = g.GroupName,
             LevelId = g.LevelId,
             GroupCapacity = g.GroupCapacity,
             StudentCount = g.Students.Count,
             Level = new LevelDto
             {
                 Year = level.LevelYear,
                 Specialization = g.Specialization.Name,
                 SchoolType = schooType

             }
         });

        var groups = await groupsQuery.ToListAsync();

        return result.Success(groups);
    }
}