using Dirassati_Backend.Common.Extensions;
using Dirassati_Backend.Common.Services;
using Dirassati_Backend.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Dirassati_Backend.Features.Auth.Register.Services;
using Dirassati_Backend.Data.Models;
using Dirassati_Backend.Common;
using Dirassati_Backend.Features.Teachers.Dtos;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Dirassati_Backend.Hubs.Interfaces;
using Dirassati_Backend.Data.Enums;
using Dirassati_Backend.Common.Dtos;
namespace Dirassati_Backend.Features.Teachers.Services;

public class TeacherServices
{
    private readonly ITeacherRepository _teacherRepository;
    private readonly AppDbContext _dbContext;
    private readonly UserManager<AppUser> _userManager;
    private readonly LinkGenerator _linkGenerator;
    private readonly IHttpContextAccessor _httpContext;
    private readonly IEmailService _emailService;
    private readonly SendCridentialsService _sendCridentialsService;
    private readonly IMapper _mapper;
    private readonly IHubContext<ParentNotificationHub, IParentClient> _hubContext;

    public TeacherServices(
        ITeacherRepository teacherRepository,
        AppDbContext dbContext,
        UserManager<AppUser> userManager,
        LinkGenerator linkGenerator,
        IHttpContextAccessor httpContext,
        IEmailService emailService,
        SendCridentialsService sendCridentialsService,
        IMapper mapper,
         IHubContext<ParentNotificationHub, IParentClient> hubContext)
    {
        _teacherRepository = teacherRepository;
        _dbContext = dbContext;
        _userManager = userManager;
        _linkGenerator = linkGenerator;
        _httpContext = httpContext;
        _emailService = emailService;
        _sendCridentialsService = sendCridentialsService;
        _mapper = mapper;
        _hubContext = hubContext;
    }

    public async Task<Guid> RegisterTeacherAsync(TeacherInfosDTO teacherDto, string schoolId, bool isSeed = false)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {

            var (user, teacher) = await CreateTeacherEntitiesAsync(teacherDto, schoolId);

            await SaveTeacherWithTransactionAsync(teacher);
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


    public async Task<(AppUser user, Teacher teacher)> CreateTeacherEntitiesAsync(TeacherInfosDTO teacherDto, string SchoolId)
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
            throw new Exception($"User creation failed: {createResult.Errors.ToCustomString()}");

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
        if (Guid.TryParse(SchoolId, out Guid schoolIdGuid))
        {
            if ((await _dbContext.Schools.FirstOrDefaultAsync(s => s.SchoolId == schoolIdGuid)) == null)
            {
                throw new Exception("School is not found");
            }
        }
        else
        {
            throw new Exception("Invalid School Id");
        }
    }

    private async Task ValidateContractTypeAsync(int contractTypeId)
    {
        if (!await _dbContext.ContractTypes.AnyAsync(ct => ct.ContractId == contractTypeId))
            throw new Exception($"Contract type {contractTypeId} not found");
    }

    private async Task ValidateSchoolExistsAsync(Guid schoolId)
    {
        if (!await _dbContext.Schools.AnyAsync(s => s.SchoolId == schoolId))
            throw new Exception($"School {schoolId} not found");
    }

    private async Task AssignSubjectsAsync(Teacher teacher, List<int>? subjectIds)
    {
        if (subjectIds?.Any() == true)
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

        await _emailService.SendEmailAsync(
            user.Email!,
            "Confirm Your Email",
            BuildEmailContent(confirmationLink),
            "Dirassati Team",
            "noreply@dirassati.com",
            true
        );
    }

    private string GenerateConfirmationLink(string email, string token)
    {
        var httpContext = _httpContext.HttpContext ?? throw new Exception("HttpContext is null");

        // Remove URL encoding for the token
        var link = _linkGenerator.GetUriByName(
            httpContext,
            "VerifyEmail",
            new { email, token } // Pass raw token
        );

        return link ?? throw new Exception("Confirmation link generation failed. Verify route configuration.");
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

    public async Task<Result<GetTeacherInfosDTO, string>> GetTeacherInfos(string TeacherId, string SchoolId)
    {
        var result = new Result<GetTeacherInfosDTO, string>();
        if (!Guid.TryParse(TeacherId, out Guid teacherGuid) || !Guid.TryParse(SchoolId, out Guid schoolGuid))
        {
            return result.Failure("Invalid teacher ID or school ID format", 400);
        }

        var teacher = await _dbContext.Teachers.Include(t => t.ContractType).Include(t => t.User).FirstOrDefaultAsync(t => t.TeacherId == teacherGuid && t.SchoolId == schoolGuid);
        if (teacher is null)
            return result.Failure("Teacher Not Found", 404);
        var teacherDto = _mapper.Map<GetTeacherInfosDTO>(teacher);
        return result.Success(teacherDto);

    }
    public async Task<Result<List<GetTeacherInfosDTO>, string>> GetTeachers(string SchoolId)
    {
        var result = new Result<List<GetTeacherInfosDTO>, string>();
        if (!Guid.TryParse(SchoolId, out Guid schoolGuid))
        {
            return result.Failure("Invalid teacher ID or school ID format", 400);
        }

        var rerult = _dbContext.Teachers.Include(t => t.ContractType).Include(t => t.User);
        foreach (var item in rerult)
        {
            Console.WriteLine(item.User.Email);
        }
        var teachers = await rerult.Select(t => _mapper.Map<GetTeacherInfosDTO>(t)).ToListAsync();
        foreach (var item in teachers)
        {
            Console.WriteLine(item.Email);
        }
        return result.Success(teachers);

    }

    public async Task<Result<List<ContractTypeDTO>, string>> GetContractTypes()
    {
        try
        {
            var ContractTypes = await _dbContext.ContractTypes.Select(c => new ContractTypeDTO { ContractTypeId = c.ContractId, Name = c.Name }).ToListAsync();
            return new Result<List<ContractTypeDTO>, string>().Success(ContractTypes);
        }
        catch (System.Exception)
        {

            return new Result<List<ContractTypeDTO>, string>().Failure("Failed to retrieve contract types", 500);
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
                        .Include(t => t.User) // Eager load the User
                        .FirstOrDefaultAsync(t => t.TeacherId == reportDto.TeacherId);
        if (teacher is null)
            return;
        if (student == null)
            return;
        // Construct the group name for the parent
        string groupName = $"parent-{student.ParentId}";
        reportDto.Teacher = new SimpleTeacherDto
        {
            FirstName = teacher.User.FirstName,
            LastName = teacher.User.FirstName,
        };
        // Send the report to the parent(s) in the group
        await _hubContext.Clients.Group(groupName).ReceiveNewReport(reportDto);

        await UpdateStudentReportStatus(reportDto);

    }
}

/*

{"type":1,"target":"ReceiveNewReport","arguments":[{"id":"95e60f54-4788-42ae-b8fc-e932b41ff6eb","teacherId":"3ae8af26-9421-48a7-ba12-f1914072f3e0","studentId":"deba38c6-8746-4709-a340-d56946b56a69","title":"He is a hola boy ","description":"NO WAAAY!!!","reportDate":"2025-03-29T15:17:05.778Z","createdAt":"2025-03-29T17:27:58.4982727Z","updatedAt":"2025-03-29T17:27:58.4982727Z","subject":null,"teacher":null,"student":{"firstName":"Mohamed","lastName":"Mohamed"}}]}

*/
