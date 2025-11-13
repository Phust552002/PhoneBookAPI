using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using PhoneBookDbNormalized.Models;
using PhoneBookDbNormalized.Data;
using PhoneBookDbNormalized.Settings;
using Microsoft.EntityFrameworkCore;
using PhoneBookDbNormalized.Models.DTOs;

namespace PhoneBookDbNormalized.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;

        // HARDCODED USERS - Không cần database
        private readonly List<User> _users = new List<User>
    {
        new User
        {
            Id = 1,
            Username = "manager",
            Password = "456",
            Role = "Manager"
        },
        new User
        {
            Id = 2,
            Username = "admin",
            Password = "123",
            Role = "Admin"
        },
        new User
        {
            Id = 3,
            Username = "manager2",
            Password = "456",
            Role = "Manager"
        }
    };

        public AuthService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public LoginResponse Login(LoginRequest request)
        {
            var user = _users.FirstOrDefault(u =>
                u.Username == request.Username &&
                u.Password == request.Password);

            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid username or password");
            }
            var token = GenerateJwtToken(user);

            return new LoginResponse
            {
                Token = token,
                Username = user.Username,
                Role = user.Role
            };
        }

        public User GetUserByUsername(string username)
        {
            return _users.FirstOrDefault(u => u.Username == username);
        }

        public List<User> GetAllUsers()
        {
            return _users.Select(u => new User
            {
                Id = u.Id,
                Username = u.Username,
                Role = u.Role
            }).ToList();
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["Secret"];

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secretKey);

            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpirationMinutes"])),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}