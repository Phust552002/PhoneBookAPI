using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhoneBookDbNormalized.Models;
using PhoneBookDbNormalized.Services;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace PhoneBookDbNormalized.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Thiếu thông tin đăng nhập.");

            var employee = await _authService.AuthenticateAsync(request.Username, request.Password);
            if (employee == null)
                return Unauthorized("Tên đăng nhập hoặc mật khẩu không đúng.");

            var employeeRoleIds = await _authService.GetUserRolesAsync(employee.UserId);
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
                Message = "Đăng nhập thành công",
                User = new
                {
                    employee.UserId,
                    employee.UserName,
                    employee.FullName,
                    IsAdmin = isAdmin,
                    Roles = employeeRoleIds
                }
            });
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { Message = "Đăng xuất thành công" });
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            var username = User.Identity?.Name;
            var fullName = User.FindFirst("FullName")?.Value;
            var isAdmin = User.FindFirst("IsAdmin")?.Value;
            var roles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            return Ok(new
            {
                username,
                fullName,
                isAdmin,
                roles
            });
        }
    }

    public class LoginRequest
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [Display(Name = "Tên đăng nhập")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string? Password { get; set; }

        [Display(Name = "Ghi nhớ đăng nhập")]
        public bool RememberMe { get; set; }
    }
}
