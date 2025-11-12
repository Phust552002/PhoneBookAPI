using Dapper;
using PhoneBookDbNormalized.Models;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace PhoneBookDbNormalized.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly string _connectionString;
        public AuthRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("HRMFb")!;
        }
        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<Employee> GetEmployeeByUsernameAsync(string username)
        {
            using var conn = CreateConnection();
            var employee = await conn.QueryFirstOrDefaultAsync<Employee>(
                "Sel_GetEmployeeByUsername",
                new { Username = username },
                commandType: CommandType.StoredProcedure
            );
            return employee;
        }


        //SP: Sel_GetUserRoles
        public async Task<List<int>> GetUserRolesAsync(int userId)
        {
            using var connection = CreateConnection();

            var roles = (await connection.QueryAsync<int>(
                "Sel_GetUserRoles",
                new { UserId = userId },
                commandType: CommandType.StoredProcedure
            )).ToList();

            return roles;
        }
    }
}
