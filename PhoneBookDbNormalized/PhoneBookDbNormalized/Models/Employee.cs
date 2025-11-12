#nullable enable
using System.ComponentModel.DataAnnotations;

namespace PhoneBookDbNormalized.Models
{
    public class Employee
    {
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? EmployeeCode { get; set; }
        public string? FullName { get; set; }
        public string? WorkingPhone { get; set; }
        public string? HandPhone { get; set; }
        public string? BusinessEmail { get; set; }
        public int Status { get; set; }

        // Thêm các property cho authentication
        public string? Password { get; set; } // Chỉ dùng khi login, không hiển thị trong danh sách
        public string? PositionName { get; set; } // Chức danh
        public int? DepartmentId { get; set; } // ID phòng ban
    }
}