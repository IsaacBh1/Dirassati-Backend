using System.Security.Claims;
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

    public class EmployeeService(
        UserManager<AppUser> userManager,
        AppDbContext context,
        IEmailService emailService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<EmployeeService> logger) : IEmployeeService
    {
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly AppDbContext _context = context;
        private readonly IEmailService _emailService = emailService;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly ILogger<EmployeeService> _logger = logger;

        private Guid GetCurrentSchoolId()
        {
            using var scope = _logger.BeginScope("GetCurrentSchoolId");

            var schoolIdClaim = _httpContextAccessor.HttpContext?.User.FindFirstValue("SchoolId");
            if (schoolIdClaim == null || !Guid.TryParse(schoolIdClaim, out var schoolId))
            {
                _logger.LogWarning("Invalid or missing school ID in token. SchoolIdClaim: {SchoolIdClaim}", schoolIdClaim);
                throw new UnauthorizedAccessException("Invalid or missing school ID in token");
            }

            _logger.LogDebug("Retrieved school ID: {SchoolId}", schoolId);
            return schoolId;
        }

        public async Task<EmployeeResponseDto> CreateEmployeeAsync(CreateEmployeeDto dto)
        {
            using var scope = _logger.BeginScope("CreateEmployee for {Email}", dto.Email);
            _logger.LogInformation("Starting employee creation process for {Email}", dto.Email);

            var schoolId = GetCurrentSchoolId();
            _logger.LogDebug("Creating employee for school: {SchoolId}", schoolId);

            // Check if user with this email already exists
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("Attempted to create employee with existing email: {Email}", dto.Email);
                throw new InvalidOperationException("User with this email already exists");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            _logger.LogDebug("Database transaction started for employee creation");

            try
            {
                // Generate a random password
                var temporaryPassword = GenerateSecurePassword();
                _logger.LogDebug("Generated temporary password for new employee");

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

                _logger.LogDebug("Creating user account for {Email}", dto.Email);
                var result = await _userManager.CreateAsync(user, temporaryPassword);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to create user account for {Email}. Errors: {Errors}", dto.Email, errors);
                    throw new InvalidOperationException($"Failed to create user: {errors}");
                }

                _logger.LogDebug("User account created successfully with ID: {UserId}", user.Id);

                // Create address if provided
                Address? address = null;
                if (!string.IsNullOrEmpty(dto.Street) || !string.IsNullOrEmpty(dto.City))
                {
                    _logger.LogDebug("Creating address for employee");
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
                    _logger.LogDebug("Address created with ID: {AddressId}", address.AdresseId);

                    user.AdresseId = address.AdresseId;
                    await _userManager.UpdateAsync(user);
                    _logger.LogDebug("User updated with address reference");
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
                _logger.LogDebug("Employee record created with ID: {EmployeeId}", employee.EmployeeId);

                await transaction.CommitAsync();
                _logger.LogDebug("Database transaction committed successfully");

                // Send credentials email
                _logger.LogDebug("Sending credentials email to {Email}", user.Email);
                await SendCredentialsEmailAsync(user.Email, dto.FirstName, temporaryPassword);

                _logger.LogInformation("Employee created successfully: {Email} with ID: {EmployeeId} for school: {SchoolId}",
                    dto.Email, employee.EmployeeId, schoolId);

                return await GetEmployeeByIdAsync(employee.EmployeeId);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating employee for {Email}. Transaction rolled back", dto.Email);
                throw;
            }
        }

        public async Task<EmployeeResponseDto> GetEmployeeByIdAsync(Guid employeeId)
        {
            using var scope = _logger.BeginScope("GetEmployeeById: {EmployeeId}", employeeId);
            _logger.LogDebug("Retrieving employee details for ID: {EmployeeId}", employeeId);

            var schoolId = GetCurrentSchoolId();

            var employee = await _context.Employees
                .Include(e => e.User)
                .ThenInclude(u => u.Address)
                .Include(e => e.School)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.SchoolId == schoolId);

            if (employee == null)
            {
                _logger.LogWarning("Employee not found with ID: {EmployeeId} for school: {SchoolId}", employeeId, schoolId);
                throw new KeyNotFoundException("Employee not found");
            }

            _logger.LogDebug("Employee retrieved successfully: {EmployeeId}", employeeId);
            return MapToEmployeeResponseDto(employee);
        }

        public async Task<IEnumerable<EmployeeListDto>> GetAllEmployeesAsync(int page = 1, int pageSize = 10)
        {
            using var scope = _logger.BeginScope("GetAllEmployees - Page: {Page}, PageSize: {PageSize}", page, pageSize);
            _logger.LogDebug("Retrieving employees list - Page: {Page}, PageSize: {PageSize}", page, pageSize);

            // Add validation logging
            if (page < 1 || pageSize < 1)
            {
                _logger.LogWarning("Invalid pagination parameters: Page={Page}, PageSize={PageSize}", page, pageSize);
                throw new ArgumentException("Page and PageSize must be greater than 0");
            }

            var schoolId = GetCurrentSchoolId();

            // Add performance logging
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

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

            stopwatch.Stop();
            _logger.LogDebug("Database query completed in {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);

            _logger.LogInformation("Retrieved {Count} employees for school: {SchoolId} (Page: {Page}, PageSize: {PageSize})",
                employees.Count, schoolId, page, pageSize);

            return employees;
        }

        public async Task<EmployeeResponseDto> UpdateEmployeeAsync(Guid employeeId, UpdateEmployeeDto dto)
        {
            using var scope = _logger.BeginScope("UpdateEmployee: {EmployeeId}", employeeId);
            _logger.LogInformation("Starting employee update process for ID: {EmployeeId}", employeeId);

            var schoolId = GetCurrentSchoolId();

            var employee = await _context.Employees
                .Include(e => e.User)
                .ThenInclude(u => u.Address)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.SchoolId == schoolId);

            if (employee == null)
            {
                _logger.LogWarning("Employee not found for update with ID: {EmployeeId} for school: {SchoolId}", employeeId, schoolId);
                throw new KeyNotFoundException("Employee not found");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            _logger.LogDebug("Database transaction started for employee update");

            try
            {
                _logger.LogDebug("Updating user information for employee: {EmployeeId}", employeeId);
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
                        _logger.LogDebug("Updating existing address for employee: {EmployeeId}", employeeId);
                        employee.User.Address.Street = dto.Street;
                        employee.User.Address.City = dto.City;
                        employee.User.Address.State = dto.State;
                        employee.User.Address.PostalCode = dto.PostalCode;
                        employee.User.Address.Country = dto.Country;
                    }
                    else
                    {
                        _logger.LogDebug("Creating new address for employee: {EmployeeId}", employeeId);
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
                        _logger.LogDebug("New address created with ID: {AddressId}", address.AdresseId);
                    }
                }

                _logger.LogDebug("Updating employee information for ID: {EmployeeId}", employeeId);
                // Update employee information
                employee.Position = dto.Position;
                employee.HireDate = dto.HireDate;
                employee.ContractType = dto.ContractType;
                employee.IsActive = dto.IsActive;
                employee.Permissions = dto.Permissions;

                await _userManager.UpdateAsync(employee.User);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                _logger.LogDebug("Database transaction committed successfully");

                _logger.LogInformation("Employee updated successfully: {EmployeeId}", employeeId);

                return MapToEmployeeResponseDto(employee);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error updating employee: {EmployeeId}. Transaction rolled back", employeeId);
                throw;
            }
        }

        public async Task<bool> DeleteEmployeeAsync(Guid employeeId)
        {
            using var scope = _logger.BeginScope("DeleteEmployee: {EmployeeId}", employeeId);
            _logger.LogInformation("Starting employee deletion process for ID: {EmployeeId}", employeeId);

            var schoolId = GetCurrentSchoolId();

            var employee = await _context.Employees
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.SchoolId == schoolId);

            if (employee == null)
            {
                _logger.LogWarning("Employee not found for deletion with ID: {EmployeeId} for school: {SchoolId}", employeeId, schoolId);
                return false;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            _logger.LogDebug("Database transaction started for employee deletion");

            try
            {
                _logger.LogDebug("Removing employee record: {EmployeeId}", employeeId);
                // Remove employee record
                _context.Employees.Remove(employee);

                _logger.LogDebug("Removing user account for employee: {EmployeeId}", employeeId);
                // Remove user account
                await _userManager.DeleteAsync(employee.User);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                _logger.LogDebug("Database transaction committed successfully");

                _logger.LogInformation("Employee deleted successfully: {EmployeeId}", employeeId);

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error deleting employee: {EmployeeId}. Transaction rolled back", employeeId);
                throw;
            }
        }

        public async Task<bool> ResetEmployeePasswordAsync(PasswordResetDto dto)
        {
            using var scope = _logger.BeginScope("ResetEmployeePassword: {EmployeeId}", dto.EmployeeId);
            _logger.LogInformation("Starting password reset for employee: {EmployeeId}", dto.EmployeeId);

            var schoolId = GetCurrentSchoolId();

            var employee = await _context.Employees
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.EmployeeId == dto.EmployeeId && e.SchoolId == schoolId);

            if (employee == null)
            {
                _logger.LogWarning("Employee not found for password reset with ID: {EmployeeId} for school: {SchoolId}", dto.EmployeeId, schoolId);
                return false;
            }

            try
            {
                _logger.LogDebug("Generating password reset token for employee: {EmployeeId}", dto.EmployeeId);
                var token = await _userManager.GeneratePasswordResetTokenAsync(employee.User);
                var result = await _userManager.ResetPasswordAsync(employee.User, token, dto.NewPassword);

                if (result.Succeeded)
                {
                    if (string.IsNullOrEmpty(employee.User.Email))
                    {
                        _logger.LogError("Employee email is missing for password reset notification. EmployeeId: {EmployeeId}", dto.EmployeeId);
                        throw new InvalidOperationException("Employee email is missing.");
                    }

                    _logger.LogDebug("Sending password reset notification email to employee: {EmployeeId}", dto.EmployeeId);
                    await SendPasswordResetEmailAsync(employee.User.Email, employee.User.FirstName, dto.NewPassword);
                    _logger.LogInformation("Password reset successfully for employee: {EmployeeId}", dto.EmployeeId);
                    return true;
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogWarning("Password reset failed for employee: {EmployeeId}. Errors: {Errors}", dto.EmployeeId, errors);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for employee: {EmployeeId}", dto.EmployeeId);
                throw;
            }
        }

        public async Task<bool> ResendCredentialsAsync(Guid employeeId)
        {
            using var scope = _logger.BeginScope("ResendCredentials: {EmployeeId}", employeeId);
            _logger.LogInformation("Starting credentials resend for employee: {EmployeeId}", employeeId);

            var schoolId = GetCurrentSchoolId();

            var employee = await _context.Employees
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.SchoolId == schoolId);

            if (employee == null)
            {
                _logger.LogWarning("Employee not found for credentials resend with ID: {EmployeeId} for school: {SchoolId}", employeeId, schoolId);
                return false;
            }

            try
            {
                // Generate new password
                _logger.LogDebug("Generating new password for employee: {EmployeeId}", employeeId);
                var newPassword = GenerateSecurePassword(12);
                var token = await _userManager.GeneratePasswordResetTokenAsync(employee.User);
                var result = await _userManager.ResetPasswordAsync(employee.User, token, newPassword);

                if (result.Succeeded)
                {
                    if (string.IsNullOrEmpty(employee.User.Email))
                    {
                        _logger.LogError("Employee email is missing for credentials resend. EmployeeId: {EmployeeId}", employeeId);
                        throw new InvalidOperationException("Employee email is missing.");
                    }

                    _logger.LogDebug("Sending new credentials email to employee: {EmployeeId}", employeeId);
                    await SendCredentialsEmailAsync(employee.User.Email, employee.User.FirstName, newPassword);
                    _logger.LogInformation("Credentials resent for employee: {EmployeeId}", employeeId);
                    return true;
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogWarning("Failed to reset password for credentials resend. EmployeeId: {EmployeeId}. Errors: {Errors}", employeeId, errors);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending credentials for employee: {EmployeeId}", employeeId);
                throw;
            }
        }

        public async Task<bool> ToggleEmployeeStatusAsync(Guid employeeId)
        {
            using var scope = _logger.BeginScope("ToggleEmployeeStatus: {EmployeeId}", employeeId);
            _logger.LogInformation("Toggling status for employee: {EmployeeId}", employeeId);

            var schoolId = GetCurrentSchoolId();

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.SchoolId == schoolId);

            if (employee == null)
            {
                _logger.LogWarning("Employee not found for status toggle with ID: {EmployeeId} for school: {SchoolId}", employeeId, schoolId);
                return false;
            }

            try
            {
                var previousStatus = employee.IsActive;
                employee.IsActive = !employee.IsActive;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Employee status toggled: {EmployeeId} - Previous: {PreviousStatus}, Current: {CurrentStatus}",
                    employeeId, previousStatus, employee.IsActive);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling status for employee: {EmployeeId}", employeeId);
                throw;
            }
        }

        private static string GenerateSecurePassword(int length = 12)
        {
            if (length < 6)
            {
                throw new ArgumentException("Password length must be at least 6 to meet Identity requirements.");
            }

            // Add logging for password generation (without exposing the actual password)
            var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<EmployeeService>();
            logger.LogDebug("Generating secure password with length: {Length}", length);

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
            _logger.LogDebug("Sending credentials email to {Email}", email);

            try
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
                _logger.LogInformation("Credentials email sent successfully to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send credentials email to {Email}", email);
                throw;
            }
        }

        private async Task SendPasswordResetEmailAsync(string email, string firstName, string newPassword)
        {
            _logger.LogDebug("Sending password reset email to {Email}", email);

            try
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
                _logger.LogInformation("Password reset email sent successfully to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to {Email}", email);
                throw;
            }
        }

        private static EmployeeResponseDto MapToEmployeeResponseDto(Employee employee)
        {
            // Add null check logging
            if (employee?.User == null)
            {
                var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<EmployeeService>();
                logger.LogError("Cannot map employee to response DTO: Employee or User is null");
                throw new ArgumentNullException(nameof(employee), "Employee and User cannot be null");
            }

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