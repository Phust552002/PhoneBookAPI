using Microsoft.AspNetCore.Mvc;
using PhoneBookDbNormalized.Models;

namespace PhoneBookDbNormalized.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class DepartmentsController : ControllerBase
    {
        private readonly IPhoneBookRepository _repository;
        private readonly ILogger<DepartmentsController> _logger;

        public DepartmentsController(IPhoneBookRepository repository, ILogger<DepartmentsController> logger)
        {
            _repository = repository;
            _logger = logger;
        }
        [HttpGet]
        [ProducesResponseType(typeof(List<Department>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetDepartments()
        {
                var departments = await _repository.GetDepartmentsAsync();
                return Ok(departments);
        }

        [HttpGet("{departmentId}/employees")]
        [ProducesResponseType(typeof(List<Employee>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetEmployeesByDepartment(int departmentId)
        {
                var employees = await _repository.GetEmployeesByDepartmentAsync(departmentId);
                return Ok(employees);
        }

        [HttpGet("{departmentId}/inactive-employees")]
        [ProducesResponseType(typeof(List<Employee>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetInactiveEmployeesByDepartment(int departmentId)
        {
                var employees = await _repository.GetInactiveEmployeesByDepartmentAsync(departmentId);
                return Ok(employees);
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class EmployeesController : ControllerBase
    {
        private readonly IPhoneBookRepository _repository;
        private readonly ILogger<EmployeesController> _logger;

        public EmployeesController(IPhoneBookRepository repository, ILogger<EmployeesController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<Employee>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAllEmployees()
        {
                var employees = await _repository.GetAllEmployeesAsync();
                return Ok(employees);
        }

        [HttpGet("inactive")]
        [ProducesResponseType(typeof(List<Employee>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAllInactiveEmployees()
        {
                var employees = await _repository.GetAllInactiveEmployeesAsync();
                return Ok(employees);
        }


        [HttpGet("username/{username}")]
        [ProducesResponseType(typeof(Employee), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetEmployeeByUsername(string username)
        {
                if (string.IsNullOrWhiteSpace(username))
                {
                    return BadRequest(new { message = "Username không được để trống" });
                }

                var employee = await _repository.GetEmployeeByUsernameAsync(username);

                if (employee == null)
                {
                    return NotFound(new { message = $"Không tìm thấy nhân viên với username: {username}" });
                }

                return Ok(employee);
        }
        [HttpGet("{userId}/roles")]
        [ProducesResponseType(typeof(List<int>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetUserRoles(int userId)
        {
            var roles = await _repository.GetUserRolesAsync(userId);
            return Ok(roles);

        }

        [HttpPut("{userId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UpdateEmployee(int userId, [FromBody] UpdateEmployeeRequest request)
        {
                var employee = new Employee
                {
                    UserId = userId,
                    FullName = request.FullName,
                    WorkingPhone = request.WorkingPhone,
                    HandPhone = request.HandPhone,
                    BusinessEmail = request.BusinessEmail
                };

                var result = await _repository.UpdateEmployeeAsync(employee);

                if (!result)
                {
                    return NotFound(new { message = "Không tìm thấy nhân viên hoặc cập nhật thất bại" });
                }

                return Ok(new { message = "Cập nhật thông tin nhân viên thành công", userId });
           
        }
    }


    public class UpdateEmployeeRequest
    {
        public string? FullName { get; set; }
        public string? WorkingPhone { get; set; }
        public string? HandPhone { get; set; }
        public string? BusinessEmail { get; set; }
    }
}