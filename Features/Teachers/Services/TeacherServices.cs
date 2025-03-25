using Dirassati_Backend.Common.Extensions;
using Dirassati_Backend.Common.Services;
using Dirassati_Backend.Domain.Models;
using Dirassati_Backend.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Dirassati_Backend.Features.Auth.Register.Services;
using Dirassati_Backend.Data.Models;

namespace Dirassati_Backend.Features.Teachers.Services
{
    public class TeacherServices
    {
        private readonly ITeacherRepository _teacherRepository;
        private readonly AppDbContext _dbContext;
        private readonly UserManager<AppUser> _userManager;
        private readonly LinkGenerator _linkGenerator;
        private readonly IHttpContextAccessor _httpContext;
        private readonly IEmailService _emailService;
        private readonly SendCridentialsService _sendCridentialsService;

        public TeacherServices(
            ITeacherRepository teacherRepository,
            AppDbContext dbContext,
            UserManager<AppUser> userManager,
            LinkGenerator linkGenerator,
            IHttpContextAccessor httpContext,
            IEmailService emailService,
            SendCridentialsService sendCridentialsService)
        {
            _teacherRepository = teacherRepository;
            _dbContext = dbContext;
            _userManager = userManager;
            _linkGenerator = linkGenerator;
            _httpContext = httpContext;
            _emailService = emailService;
            _sendCridentialsService = sendCridentialsService;
        }

        public async Task<Guid> RegisterTeacherAsync(TeacherInfosDTO teacherDto)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {

                var (user, teacher) = await CreateTeacherEntitiesAsync(teacherDto);

                await SaveTeacherWithTransactionAsync(teacher);

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

        private async Task<(AppUser user, Teacher teacher)> CreateTeacherEntitiesAsync(TeacherInfosDTO teacherDto)
        {
            ValidateSchoolClaims();
            var schoolId = Guid.Parse(_httpContext.HttpContext!.User.FindFirst("SchoolId")!.Value);
            await ValidateContractTypeAsync(teacherDto.ContractTypeId);
            await ValidateSchoolExistsAsync(schoolId);

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
                SchoolId = schoolId,
                User = user,
                Photo = teacherDto.Photo 
            };

            await AssignSubjectsAsync(teacher, teacherDto.SubjectIds);
            return (user, teacher);
        }
        private void ValidateSchoolClaims()
        {
            if (_httpContext.HttpContext?.User.FindFirst("SchoolId") == null ||
                _httpContext.HttpContext.User.FindFirst("SchoolTypeId") == null)
            {
                throw new Exception("School claims missing in token");
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
    }
}