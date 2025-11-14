using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhoneBookDbNormalized.Services;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace PhoneBookDbNormalized.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestForm request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var employee = await _accountService.AuthenticateAsync(request.Username, request.Password);
            if (employee == null)
                return Unauthorized(new { message = "Tên đăng nhập hoặc mật khẩu không đúng" });
            var employeeRoleIds = await _accountService.GetUserRolesAsync(employee.UserId);
            var adminRoleIds = new[] { 1, 2, 4, 8, 10, 20 };
            bool isAdmin = employeeRoleIds.Any(id => adminRoleIds.Contains(id));


            return Ok(new
            {
                message = "Đăng nhập thành công",
                user = new
                {
                    userId = employee.UserId,
                    username = employee.UserName,
                    fullName = employee.FullName,
                    employeeCode = employee.EmployeeCode,
                    positionName = employee.PositionName,
                    departmentId = employee.DepartmentId,
                    isAdmin = isAdmin,
                    roles = employeeRoleIds
                }
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { message = "Đăng xuất thành công" });
        }

    }

    public class LoginRequestForm
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        public required string Username { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        public required string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}