using Microsoft.IdentityModel.Tokens;
using PhoneBookDbNormalized.Data;
using PhoneBookDbNormalized.Models;
using PhoneBookDbNormalized.Models.DTOs;
using PhoneBookDbNormalized.Settings;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public interface IAuthService
{
    LoginResponse Login(LoginRequest request);
    User GetUserByUsername(string username);
    List<User> GetAllUsers();
}

