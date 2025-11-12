using PhoneBookDbNormalized.Models;
namespace PhoneBookDbNormalized.Repositories
{
    public interface IAuthRepository
    {
        Task<Employee?> GetEmployeeByUsernameAsync(string username);
        Task<List<int>> GetUserRolesAsync(int userId);
    }
}
