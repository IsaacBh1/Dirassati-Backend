using System.Net;
using Dirassati_Backend.Features.Employees.Dtos;
using Dirassati_Backend.Features.Employees.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dirassati_Backend.Features.Employees.Controllers
{
    [ApiController]
    [Route("api/employees")]
    [Tags("Employee Management")]
    [Authorize(Roles = "Employee")]
    [Produces("application/json")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly ILogger<EmployeeController> _logger;

        public EmployeeController(IEmployeeService employeeService, ILogger<EmployeeController> logger)
        {
            _employeeService = employeeService;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new employee account and sends credentials via email
        /// </summary>
        /// <param name="dto">Employee creation details</param>
        /// <returns>Created employee information</returns>
        [HttpPost]
        [ProducesResponseType(typeof(EmployeeResponseDto), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Conflict)]
        public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeDto dto)
        {
            try
            {
                var employee = await _employeeService.CreateEmployeeAsync(dto);
                return CreatedAtAction(nameof(GetEmployeeById), new { id = employee.EmployeeId }, employee);
            }
            catch (InvalidOperationException ex)
            {
                    _logger.LogWarning(ex, "Failed to create employee: {Message}", ex.Message);
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating employee");
                return BadRequest(new { message = "An error occurred while creating the employee" });
            }
        }

        /// <summary>
        /// Gets an employee by ID
        /// </summary>
        /// <param name="id">Employee ID</param>
        /// <returns>Employee details</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(EmployeeResponseDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetEmployeeById(Guid id)
        {
            try
            {
                var employee = await _employeeService.GetEmployeeByIdAsync(id);
                return Ok(employee);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Employee not found" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Gets all employees with pagination
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10, max: 50)</param>
        /// <returns>List of employees</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<EmployeeListDto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAllEmployees([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                // Limit page size to prevent excessive data loading
                pageSize = Math.Min(pageSize, 50);
                page = Math.Max(page, 1);

                var employees = await _employeeService.GetAllEmployeesAsync(page, pageSize);
                
                return Ok(new
                {
                    data = employees,
                    pagination = new
                    {
                        page,
                        pageSize,
                        hasNext = employees.Count() == pageSize
                    }
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Updates an existing employee
        /// </summary>
        /// <param name="id">Employee ID</param>
        /// <param name="dto">Updated employee details</param>
        /// <returns>Updated employee information</returns>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(EmployeeResponseDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> UpdateEmployee(Guid id, [FromBody] UpdateEmployeeDto dto)
        {
            try
            {
                var employee = await _employeeService.UpdateEmployeeAsync(id, dto);
                return Ok(employee);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Employee not found" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating employee {EmployeeId}", id);
                return BadRequest(new { message = "An error occurred while updating the employee" });
            }
        }

        /// <summary>
        /// Deletes an employee account
        /// </summary>
        /// <param name="id">Employee ID</param>
        /// <returns>Success confirmation</returns>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> DeleteEmployee(Guid id)
        {
            try
            {
                var success = await _employeeService.DeleteEmployeeAsync(id);
                if (!success)
                {
                    return NotFound(new { message = "Employee not found" });
                }

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting employee {EmployeeId}", id);
                return BadRequest(new { message = "An error occurred while deleting the employee" });
            }
        }

        /// <summary>
        /// Resets an employee's password and sends new credentials via email
        /// </summary>
        /// <param name="dto">Password reset details</param>
        /// <returns>Success confirmation</returns>
        [HttpPost("reset-password")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ResetEmployeePassword([FromBody] PasswordResetDto dto)
        {
            try
            {
                var success = await _employeeService.ResetEmployeePasswordAsync(dto);
                if (!success)
                {
                    return NotFound(new { message = "Employee not found" });
                }

                return Ok(new { message = "Password reset successfully and new credentials sent via email" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for employee {EmployeeId}", dto.EmployeeId);
                return BadRequest(new { message = "An error occurred while resetting the password" });
            }
        }

        /// <summary>
        /// Resends login credentials to an employee's email
        /// </summary>
        /// <param name="dto">Resend credentials details</param>
        /// <returns>Success confirmation</returns>
        [HttpPost("resend-credentials")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ResendCredentials([FromBody] ResendCredentialsDto dto)
        {
            try
            {
                var success = await _employeeService.ResendCredentialsAsync(dto.EmployeeId);
                if (!success)
                {
                    return NotFound(new { message = "Employee not found" });
                }

                return Ok(new { message = "New credentials generated and sent via email" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending credentials for employee {EmployeeId}", dto.EmployeeId);
                return BadRequest(new { message = "An error occurred while resending credentials" });
            }
        }

        /// <summary>
        /// Toggles an employee's active status
        /// </summary>
        /// <param name="id">Employee ID</param>
        /// <returns>Success confirmation</returns>
        [HttpPatch("{id:guid}/toggle-status")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ToggleEmployeeStatus(Guid id)
        {
            try
            {
                var success = await _employeeService.ToggleEmployeeStatusAsync(id);
                if (!success)
                {
                    return NotFound(new { message = "Employee not found" });
                }

                return Ok(new { message = "Employee status updated successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling status for employee {EmployeeId}", id);
                return BadRequest(new { message = "An error occurred while updating employee status" });
            }
        }
    }
}