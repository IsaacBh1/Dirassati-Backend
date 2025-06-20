using Dirassati_Backend.Common.Extensions;
using Dirassati_Backend.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Dirassati_Backend.Features.Auth.Register.Services;
using Dirassati_Backend.Data.Models;
using Dirassati_Backend.Common;
using Dirassati_Backend.Features.Teachers.Dtos;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Dirassati_Backend.Hubs.Interfaces;
using Dirassati_Backend.Data.Enums;
using Dirassati_Backend.Common.Dtos;
using Dirassati_Backend.Data;
using Dirassati_Backend.Hubs;
using Dirassati_Backend.Persistence;
using Dirassati_Backend.Common.Services.EmailService;
using AutoMapper.QueryableExtensions;
using Dirassati_Backend.Features.School.DTOs;
namespace Dirassati_Backend.Features.Teachers.Services;

public class TeacherServices(
    ITeacherRepository teacherRepository,
    AppDbContext dbContext,
    UserManager<AppUser> userManager,
    LinkGenerator linkGenerator,
    IHttpContextAccessor httpContext,
    IEmailService emailService,
    IMapper mapper,
     IHubContext<ParentNotificationHub, IParentClient> hubContext)
{
    private readonly ITeacherRepository _teacherRepository = teacherRepository;
    private readonly AppDbContext _dbContext = dbContext;
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly LinkGenerator _linkGenerator = linkGenerator;
    private readonly IHttpContextAccessor _httpContext = httpContext;
    private readonly IEmailService _emailService = emailService;
    private readonly IMapper _mapper = mapper;
    private readonly IHubContext<ParentNotificationHub, IParentClient> _hubContext = hubContext;

    public async Task<Guid> RegisterTeacherAsync(TeacherInfosDto teacherDto, string schoolId, bool isSeed = false)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {

            var (user, teacher) = await CreateTeacherEntitiesAsync(teacherDto, schoolId);

            await SaveTeacherWithTransactionAsync(teacher);
            if (isSeed)
            {
                var t = await _userManager.FindByEmailAsync(teacherDto.Email);
                if (t != null)
                    await _userManager.AddPasswordAsync(t, "P@ssword123");
            }
            if (!isSeed)
                await SendConfirmationEmailAsync(user);

            await transaction.CommitAsync();
            return teacher.TeacherId;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }


    public async Task<(AppUser user, Teacher teacher)> CreateTeacherEntitiesAsync(TeacherInfosDto teacherDto, string SchoolId)
    {
        await ValidateSchoolClaims(SchoolId);
        var schoolIdGuid = Guid.Parse(SchoolId);
        await ValidateContractTypeAsync(teacherDto.ContractTypeId);
        await ValidateSchoolExistsAsync(schoolIdGuid);

        Address? address = null;
        if (teacherDto.Address != null)
        {
            address = new Address
            {
                Street = teacherDto.Address.FullAddress,
                City = "",
                State = "",
                PostalCode = "",
                Country = "Algerie"
            };
            _dbContext.Adresses.Add(address);
            await _dbContext.SaveChangesAsync(); // Generate Address ID
        }



        var user = new AppUser
        {
            UserName = teacherDto.Email,
            Email = teacherDto.Email,
            FirstName = teacherDto.FirstName,
            LastName = teacherDto.LastName,
            PhoneNumber = teacherDto.PhoneNumber,
            EmailConfirmed = false,
            AdresseId = address?.AdresseId
        };

        var createResult = await _userManager.CreateAsync(user);
        if (!createResult.Succeeded)
            throw new InvalidOperationException($"User creation failed: {createResult.Errors.ToCustomString()}");

        var teacher = new Teacher
        {
            UserId = user.Id,
            HireDate = teacherDto.HireDate,
            ContractTypeId = teacherDto.ContractTypeId,
            SchoolId = schoolIdGuid,
            User = user,
            Photo = teacherDto.Photo
        };

        await AssignSubjectsAsync(teacher, teacherDto.SubjectIds);
        return (user, teacher);
    }
    private async Task ValidateSchoolClaims(string SchoolId)
    {
        if (!Guid.TryParse(SchoolId, out var parsedId))
        {
            throw new ArgumentException("Invalid School Id format");
        }
        if ((await _dbContext.Schools.FirstOrDefaultAsync(s => s.SchoolId == parsedId)) == null)
        {
            throw new InvalidOperationException("School is not found");
        }


    }

    private async Task ValidateContractTypeAsync(int contractTypeId)
    {
        if (!await _dbContext.ContractTypes.AnyAsync(ct => ct.ContractId == contractTypeId))
            throw new InvalidOperationException($"Contract type {contractTypeId} not found");
    }

    private async Task ValidateSchoolExistsAsync(Guid schoolId)
    {
        if (!await _dbContext.Schools.AnyAsync(s => s.SchoolId == schoolId))
            throw new InvalidOperationException($"School {schoolId} not found");
    }

    private async Task AssignSubjectsAsync(Teacher teacher, List<int>? subjectIds)
    {
        if (subjectIds is not null && subjectIds.Count != 0)
        {
            var subjects = await _dbContext.Subjects
                .Where(s => subjectIds.Contains(s.SubjectId))
                .ToListAsync();

            foreach (var subject in subjects)
            {
                teacher.Subjects.Add(subject);
            }
        }
    }

    private async Task SaveTeacherWithTransactionAsync(Teacher teacher)
    {
        await _teacherRepository.AddTeacherAsync(teacher);
        await _teacherRepository.SaveChangesAsync();
    }

    private async Task SendConfirmationEmailAsync(AppUser user)
    {
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmationLink = GenerateConfirmationLink(user.Email!, token);

        var emailBody = BuildEmailContent(confirmationLink);

        var options = new EmailTemplateOptions
        {
            Header = "Welcome to Dirassati!",
            Footer = "Dirassati - Your Educational Partner"
        };

        await _emailService.SendEmailWithTemplateAsync(
            user.Email!,
            "Confirm Your Email",
            emailBody,
            options,
            "Dirassati Team",
            "noreply@dirassati.com"
        );
    }

    private string GenerateConfirmationLink(string email, string VerificationToken)
    {
        var httpContextTemp = _httpContext.HttpContext ?? throw new InvalidOperationException("HttpContext is null");
        var encodedToken = Uri.EscapeDataString(VerificationToken);
        // Remove URL encoding for the token
        var link = _linkGenerator.GetUriByName(
            httpContextTemp,
            "VerifyEmail",
            new { email, VerificationToken = encodedToken }
        );

        return link ?? throw new InvalidOperationException("Confirmation link generation failed. Verify route configuration.");
    }
    private static string BuildEmailContent(string confirmationLink)
    {
        return $@"
                <h1 style='color: #2E86C1;'>Welcome to Dirassati!</h1>
                <p>Please confirm your email by clicking the link below:</p>
                <p><a href='{confirmationLink}' style='background-color: #2E86C1; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>
                    Confirm Email
                </a></p>
                <p>If the button doesn't work, copy this link:<br>{confirmationLink}</p>";
    }

    public async Task<Result<GetTeacherInfosDto, string>> GetTeacherInfos(string TeacherId, string SchoolId)
    {
        var result = new Result<GetTeacherInfosDto, string>();
        if (!Guid.TryParse(TeacherId, out Guid teacherGuid) || !Guid.TryParse(SchoolId, out Guid schoolGuid))
        {
            return result.Failure("Invalid teacher ID or school ID format", 400);
        }

        var teacher = await _dbContext.Teachers.Include(t => t.ContractType).Include(t => t.User).FirstOrDefaultAsync(t => t.TeacherId == teacherGuid && t.SchoolId == schoolGuid);
        if (teacher is null)
            return result.Failure("Teacher Not Found", 404);
        var teacherDto = _mapper.Map<GetTeacherInfosDto>(teacher);
        return result.Success(teacherDto);

    }
    public async Task<Result<List<GetTeacherInfosDto>, string>> GetTeachers(string SchoolId)
    {
        var result = new Result<List<GetTeacherInfosDto>, string>();
        if (!Guid.TryParse(SchoolId, out Guid schoolIdGuid))
        {
            return result.Failure($"Invalid teacher ID or school ID format ", 400);
        }

        var queryResult = await _dbContext.Teachers.Where(t => t.SchoolId == schoolIdGuid).ProjectTo<GetTeacherInfosDto>(_mapper.ConfigurationProvider).ToListAsync();
        return result.Success(queryResult);

    }

    public async Task<Result<List<ContractTypeDto>, string>> GetContractTypes()
    {
        try
        {
            var ContractTypes = await _dbContext.ContractTypes.Select(c => new ContractTypeDto { ContractTypeId = c.ContractId, Name = c.Name }).ToListAsync();
            return new Result<List<ContractTypeDto>, string>().Success(ContractTypes);
        }
        catch (Exception)
        {

            return new Result<List<ContractTypeDto>, string>().Failure("Failed to retrieve contract types", 500);
        }
    }

    internal async Task<Result<GetStudentReportDto, string>> AddTeacherReportAsync(string? TeacherId, AddStudentReportDto reportDto)
    {
        var result = new Result<GetStudentReportDto, string>();

        try
        {
            if (string.IsNullOrEmpty(TeacherId) || !Guid.TryParse(TeacherId, out Guid teacherId))
            {
                return result.Failure("Invalid teacher ID", 400);
            }

            var report = new StudentReport
            {
                TeacherId = teacherId,
                StudentId = reportDto.StudentId,
                Title = reportDto.Title,
                Description = reportDto.Description,
                ReportDate = reportDto.ReportDate
            };

            _dbContext.StudentReports.Add(report);
            await _dbContext.SaveChangesAsync();

            // Load the student data for the response
            var reportWithStudent = await _dbContext.StudentReports
            .Include(r => r.Student)
            .FirstAsync(r => r.StudentReportId == report.StudentReportId);

            var responseDto = _mapper.Map<GetStudentReportDto>(reportWithStudent);
            return result.Success(responseDto);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error Happened ==> \n{ex}");
            return result.Failure($"Failed to add teacher report: {ex.Message}", 500);
        }
    }

    internal async Task UpdateStudentReportStatus(GetStudentReportDto report)
    {
        var existingReport = await _dbContext.StudentReports.FirstOrDefaultAsync(rep => rep.StudentReportId == report.Id);
        if (existingReport != null)
        {

            existingReport.StudentReportStatusId = (int)ReportStatusEnum.Sent;
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task TriggerSendReportNotification(GetStudentReportDto reportDto)
    {
        var student = await _dbContext.Students.FirstOrDefaultAsync(s => s.StudentId == reportDto.StudentId);
        var teacher = await _dbContext.Teachers
                        .Include(t => t.User)

                        .FirstOrDefaultAsync(t => t.TeacherId == reportDto.TeacherId);
        if (teacher is null)
            return;
        if (student == null)
            return;
        // Construct the group name for the parent
        string groupName = $"parent-{student.ParentId}";
        reportDto.Teacher = new BasePersonDto
        {
            FirstName = teacher.User.FirstName,
            LastName = teacher.User.LastName,
        };
        // Send the report to the parent(s) in the group
        await _hubContext.Clients.Group(groupName).ReceiveNewReport(reportDto);

        await UpdateStudentReportStatus(reportDto);

    }
}

