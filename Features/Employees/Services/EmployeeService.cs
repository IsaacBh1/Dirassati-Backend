using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Dirassati_Backend.Common.Services;
using Dirassati_Backend.Common.Services.EmailService;
using Dirassati_Backend.Data;
using Dirassati_Backend.Data.Models;
using Dirassati_Backend.Features.Employees.Dtos;
using Dirassati_Backend.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Dirassati_Backend.Features.Employees.Services
{
    public interface IEmployeeService
    {
        Task<EmployeeResponseDto> CreateEmployeeAsync(CreateEmployeeDto dto);
        Task<EmployeeResponseDto> GetEmployeeByIdAsync(Guid employeeId);
        Task<IEnumerable<EmployeeListDto>> GetAllEmployeesAsync(int page = 1, int pageSize = 10);
        Task<EmployeeResponseDto> UpdateEmployeeAsync(Guid employeeId, UpdateEmployeeDto dto);
        Task<bool> DeleteEmployeeAsync(Guid employeeId);
        Task<bool> ResetEmployeePasswordAsync(PasswordResetDto dto);
        Task<bool> ResendCredentialsAsync(Guid employeeId);
        Task<bool> ToggleEmployeeStatusAsync(Guid employeeId);
    }

    public class EmployeeService : IEmployeeService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<EmployeeService> _logger;

        public EmployeeService(
            UserManager<AppUser> userManager,
            AppDbContext context,
            IEmailService emailService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<EmployeeService> logger)
        {
            _userManager = userManager;
            _context = context;
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        private Guid GetCurrentSchoolId()
        {
            var schoolIdClaim = _httpContextAccessor.HttpContext?.User.FindFirstValue("SchoolId");
            if (schoolIdClaim == null || !Guid.TryParse(schoolIdClaim, out var schoolId))
            {
                throw new UnauthorizedAccessException("Invalid or missing school ID in token");
            }
            return schoolId;
        }

        public async Task<EmployeeResponseDto> CreateEmployeeAsync(CreateEmployeeDto dto)
        {
            var schoolId = GetCurrentSchoolId();

            // Check if user with this email already exists
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("User with this email already exists");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Generate a random password
                var temporaryPassword = GenerateSecurePassword();

                // Create the user
                var user = new AppUser
                {
                    UserName = dto.Email,
                    Email = dto.Email,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    BirthDate = dto.BirthDate,
                    PhoneNumber = dto.PhoneNumber,
                    EmailConfirmed = true // Admin creates verified accounts
                };

                var result = await _userManager.CreateAsync(user, temporaryPassword);
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }

                // Create address if provided
                Address? address = null;
                if (!string.IsNullOrEmpty(dto.Street) || !string.IsNullOrEmpty(dto.City))
                {
                    address = new Address
                    {
                        Street = dto.Street,
                        City = dto.City,
                        State = dto.State,
                        PostalCode = dto.PostalCode,
                        Country = dto.Country
                    };

                    _context.Adresses.Add(address);
                    await _context.SaveChangesAsync();

                    user.AdresseId = address.AdresseId;
                    await _userManager.UpdateAsync(user);
                }

                // Create the employee record
                var employee = new Employee
                {
                    UserId = user.Id,
                    Position = dto.Position,
                    HireDate = dto.HireDate,
                    ContractType = dto.ContractType,
                    IsActive = dto.IsActive,
                    Permissions = dto.Permissions,
                    SchoolId = schoolId
                };

                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                // Send credentials email
                await SendCredentialsEmailAsync(user.Email, dto.FirstName, temporaryPassword);

                _logger.LogInformation("Employee created successfully: {Email} with ID: {EmployeeId}", dto.Email, employee.EmployeeId);

                return await GetEmployeeByIdAsync(employee.EmployeeId);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<EmployeeResponseDto> GetEmployeeByIdAsync(Guid employeeId)
        {
            var schoolId = GetCurrentSchoolId();

            var employee = await _context.Employees
                .Include(e => e.User)
                .ThenInclude(u => u.Address)
                .Include(e => e.School)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.SchoolId == schoolId);

            if (employee == null)
            {
                throw new KeyNotFoundException("Employee not found");
            }

            return MapToEmployeeResponseDto(employee);
        }

        public async Task<IEnumerable<EmployeeListDto>> GetAllEmployeesAsync(int page = 1, int pageSize = 10)
        {
            var schoolId = GetCurrentSchoolId();

            var employees = await _context.Employees
                .Include(e => e.User)
                .Where(e => e.SchoolId == schoolId)
                .OrderBy(e => e.User.FirstName)
                .ThenBy(e => e.User.LastName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new EmployeeListDto
                {
                    EmployeeId = e.EmployeeId,
                    Email = e.User.Email ?? "",
                    FullName = $"{e.User.FirstName} {e.User.LastName}",
                    Position = e.Position,
                    HireDate = e.HireDate,
                    ContractType = e.ContractType,
                    IsActive = e.IsActive,
                    Permissions = e.Permissions
                })
                .ToListAsync();

            return employees;
        }

        public async Task<EmployeeResponseDto> UpdateEmployeeAsync(Guid employeeId, UpdateEmployeeDto dto)
        {
            var schoolId = GetCurrentSchoolId();

            var employee = await _context.Employees
                .Include(e => e.User)
                .ThenInclude(u => u.Address)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.SchoolId == schoolId);

            if (employee == null)
            {
                throw new KeyNotFoundException("Employee not found");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Update user information
                employee.User.FirstName = dto.FirstName;
                employee.User.LastName = dto.LastName;
                employee.User.BirthDate = dto.BirthDate;
                employee.User.PhoneNumber = dto.PhoneNumber;

                // Update or create address
                if (!string.IsNullOrEmpty(dto.Street) || !string.IsNullOrEmpty(dto.City))
                {
                    if (employee.User.Address != null)
                    {
                        employee.User.Address.Street = dto.Street;
                        employee.User.Address.City = dto.City;
                        employee.User.Address.State = dto.State;
                        employee.User.Address.PostalCode = dto.PostalCode;
                        employee.User.Address.Country = dto.Country;
                    }
                    else
                    {
                        var address = new Address
                        {
                            Street = dto.Street,
                            City = dto.City,
                            State = dto.State,
                            PostalCode = dto.PostalCode,
                            Country = dto.Country
                        };

                        _context.Adresses.Add(address);
                        await _context.SaveChangesAsync();

                        employee.User.AdresseId = address.AdresseId;
                    }
                }

                // Update employee information
                employee.Position = dto.Position;
                employee.HireDate = dto.HireDate;
                employee.ContractType = dto.ContractType;
                employee.IsActive = dto.IsActive;
                employee.Permissions = dto.Permissions;

                await _userManager.UpdateAsync(employee.User);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation("Employee updated successfully: {EmployeeId}", employeeId);

                return MapToEmployeeResponseDto(employee);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> DeleteEmployeeAsync(Guid employeeId)
        {
            var schoolId = GetCurrentSchoolId();

            var employee = await _context.Employees
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.SchoolId == schoolId);

            if (employee == null)
            {
                return false;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Remove employee record
                _context.Employees.Remove(employee);

                // Remove user account
                await _userManager.DeleteAsync(employee.User);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Employee deleted successfully: {EmployeeId}", employeeId);

                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> ResetEmployeePasswordAsync(PasswordResetDto dto)
        {
            var schoolId = GetCurrentSchoolId();

            var employee = await _context.Employees
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.EmployeeId == dto.EmployeeId && e.SchoolId == schoolId);

            if (employee == null)
            {
                return false;
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(employee.User);
            var result = await _userManager.ResetPasswordAsync(employee.User, token, dto.NewPassword);

            if (result.Succeeded)
            {
                if (string.IsNullOrEmpty(employee.User.Email))
                {
                    throw new InvalidOperationException("Employee email is missing.");
                }

                await SendPasswordResetEmailAsync(employee.User.Email, employee.User.FirstName, dto.NewPassword);
                _logger.LogInformation("Password reset successfully for employee: {EmployeeId}", dto.EmployeeId);
                return true;
            }

            return false;
        }

        public async Task<bool> ResendCredentialsAsync(Guid employeeId)
        {
            var schoolId = GetCurrentSchoolId();

            var employee = await _context.Employees
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.SchoolId == schoolId);

            if (employee == null)
            {
                return false;
            }

            // Generate new password
            var newPassword = GenerateSecurePassword(12);
            var token = await _userManager.GeneratePasswordResetTokenAsync(employee.User);
            var result = await _userManager.ResetPasswordAsync(employee.User, token, newPassword);

            if (result.Succeeded)
            {
                if (string.IsNullOrEmpty(employee.User.Email))
                {
                    throw new InvalidOperationException("Employee email is missing.");
                }
                await SendCredentialsEmailAsync(employee.User.Email, employee.User.FirstName, newPassword);
                _logger.LogInformation("Credentials resent for employee: {EmployeeId}", employeeId);
                return true;
            }

            return false;
        }

        public async Task<bool> ToggleEmployeeStatusAsync(Guid employeeId)
        {
            var schoolId = GetCurrentSchoolId();

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.SchoolId == schoolId);

            if (employee == null)
            {
                return false;
            }

            employee.IsActive = !employee.IsActive;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Employee status toggled: {EmployeeId} - Active: {IsActive}", employeeId, employee.IsActive);

            return true;
        }

        private string GenerateSecurePassword(int length = 12)
        {
            if (length < 6)
                throw new ArgumentException("Password length must be at least 6 to meet Identity requirements.");

            const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lower = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string special = "!@#$%^&*()-_=+[]{}|;:,.<>?";

            var rand = new Random();
            var passwordChars = new List<char>
            {
                upper[rand.Next(upper.Length)],       
                lower[rand.Next(lower.Length)],       
                digits[rand.Next(digits.Length)],     
                special[rand.Next(special.Length)]    
            };

            // Fill the rest of the password with a mix of all characters
            string allChars = upper + lower + digits + special;
            for (int i = passwordChars.Count; i < length; i++)
            {
                passwordChars.Add(allChars[rand.Next(allChars.Length)]);
            }

            // Shuffle to randomize the position of required characters
            return new string(passwordChars.OrderBy(_ => rand.Next()).ToArray());
        }


        private async Task SendCredentialsEmailAsync(string email, string firstName, string password)
        {
            var subject = "Welcome to Dirassati - Your Account Credentials";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2>Welcome to Dirassati, {firstName}!</h2>
                    
                    <p>Your employee account has been created successfully. Here are your login credentials:</p>
                    
                    <div style='background-color: #f8f9fa; padding: 20px; border-radius: 5px; margin: 20px 0;'>
                        <p><strong>Email:</strong> {email}</p>
                        <p><strong>Password:</strong> {password}</p>
                    </div>
                    
                    <p><strong>Important Security Notice:</strong></p>
                    <ul>
                        <li>Please log in and change your password immediately</li>
                        <li>Do not share these credentials with anyone</li>
                        <li>Keep your login information secure</li>
                    </ul>
                    
                    <p>If you have any questions or need assistance, please contact your administrator.</p>
                    
                    <p>Best regards,<br>Dirassati Team</p>
                </div>";

            var options = new EmailTemplateOptions
            {
                Header = "Account Created Successfully",
                Title = "Dirassati - Account Credentials",
                Footer = "This is an automated message. Please do not reply to this email."
            };

            await _emailService.SendEmailWithTemplateAsync(email, subject, body, options);
        }

        private async Task SendPasswordResetEmailAsync(string email, string firstName, string newPassword)
        {
            var subject = "Dirassati - Password Reset";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2>Password Reset - {firstName}</h2>
                    
                    <p>Your password has been reset by an administrator. Here is your new password:</p>
                    
                    <div style='background-color: #f8f9fa; padding: 20px; border-radius: 5px; margin: 20px 0;'>
                        <p><strong>New Password:</strong> {newPassword}</p>
                    </div>
                    
                    <p><strong>Important:</strong> Please log in and change this password to something secure that only you know.</p>
                    
                    <p>Best regards,<br>Dirassati Team</p>
                </div>";

            var options = new EmailTemplateOptions
            {
                Header = "Password Reset",
                Title = "Dirassati - Password Reset",
                Footer = "This is an automated message. Please do not reply to this email."
            };

            await _emailService.SendEmailWithTemplateAsync(email, subject, body, options);
        }

        private static EmployeeResponseDto MapToEmployeeResponseDto(Employee employee)
        {
            return new EmployeeResponseDto
            {
                EmployeeId = employee.EmployeeId,
                UserId = employee.UserId,
                Email = employee.User.Email ?? "",
                FirstName = employee.User.FirstName,
                LastName = employee.User.LastName,
                BirthDate = employee.User.BirthDate,
                Position = employee.Position,
                HireDate = employee.HireDate,
                ContractType = employee.ContractType,
                IsActive = employee.IsActive,
                Permissions = employee.Permissions,
                PhoneNumber = employee.User.PhoneNumber,
                Address = employee.User.Address != null ? new AddressDto
                {
                    AdresseId = employee.User.Address.AdresseId,
                    Street = employee.User.Address.Street,
                    City = employee.User.Address.City,
                    State = employee.User.Address.State,
                    PostalCode = employee.User.Address.PostalCode,
                    Country = employee.User.Address.Country
                } : null
            };
        }
    }
}