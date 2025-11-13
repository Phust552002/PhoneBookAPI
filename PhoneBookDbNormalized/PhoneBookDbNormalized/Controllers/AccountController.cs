using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhoneBookDbNormalized.Services;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace PhoneBookDbNormalized.Controllers
{
    [Authorize(Roles = "Manager,Admin")]
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
            if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { message = "Thiếu thông tin đăng nhập" });

            var employee = await _accountService.AuthenticateAsync(request.Username, request.Password);
            if (employee == null)
                return Unauthorized(new { message = "Tên đăng nhập hoặc mật khẩu không đúng" });

            var employeeRoleIds = await _accountService.GetUserRolesAsync(employee.UserId);
            var adminRoleIds = new[] { 1, 2, 4, 8, 10, 20 };
            bool isAdmin = employeeRoleIds.Any(id => adminRoleIds.Contains(id));

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, employee.UserId.ToString()),
                new Claim(ClaimTypes.Name, employee.UserName ?? ""),
                new Claim("FullName", employee.FullName ?? employee.UserName ?? ""),
                new Claim("EmployeeCode", employee.EmployeeCode ?? ""),
                new Claim("PositionName", employee.PositionName ?? "Nhân viên"),
                new Claim("DepartmentId", employee.DepartmentId?.ToString() ?? "0"),
                new Claim("IsAdmin", isAdmin.ToString())
            };

            foreach (var roleId in employeeRoleIds)
            {
                claims.Add(new Claim(ClaimTypes.Role, roleId.ToString()));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = request.RememberMe,
                ExpiresUtc = request.RememberMe
                    ? DateTimeOffset.UtcNow.AddDays(1)
                    : DateTimeOffset.UtcNow.AddHours(1)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

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
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { message = "Đăng xuất thành công" });
        }

    }

    public class LoginRequestForm
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        public string? Password { get; set; }

        public bool RememberMe { get; set; }
    }
}