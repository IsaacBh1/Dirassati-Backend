using System.Security.Claims;
using FluentEmail.Core;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Dirassati_Backend.Common.Services;

public class EmailServices(IFluentEmail fluentEmail, IHttpContextAccessor httpContextAccessor, AppDbContext dbContext) : IEmailService
{
    private readonly IFluentEmail _fluentEmail = fluentEmail;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly AppDbContext _dbContext = dbContext;
    private string? _schoolEmail;
    private string? _schoolName;
    private async Task<bool> InitializeSchoolInfoAsync()
    {
        if (_schoolEmail != null) return false;

        var schoolId = _httpContextAccessor.HttpContext?.User.FindFirstValue("SchoolId");
        if (schoolId == null) return true;

        var school = await _dbContext.Schools
            .FirstOrDefaultAsync(s => s.SchoolId.ToString() == schoolId)
            ?? throw new Exception("School not found");

        _schoolEmail = school.Email;
        _schoolName = school.Name;
        return false;
    }

    public async Task SendEmailAsync(string to, string subject, string body, string? fromName, string? fromEmail, bool isHTML = false)
    {
        try
        {
            bool isEmailFromApp = false;
            if (fromName == null && fromEmail == null)
            {
                isEmailFromApp = await InitializeSchoolInfoAsync();
                if (!isEmailFromApp)
                    _fluentEmail.SetFrom(_schoolEmail, _schoolName);
            }
            else
            {
                _fluentEmail.SetFrom(fromEmail, fromName);
            }

            var response = await _fluentEmail
                .To(to)
                .Subject(subject)
                .Body(body, isHTML)
                .SendAsync();
            if (!response.Successful)
                throw new Exception($"Failed to send email: {string.Join(", ", response.ErrorMessages)}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error Happended\n{e.Message}");
            throw;
        }
    }

}
