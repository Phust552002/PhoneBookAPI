using System.ComponentModel.DataAnnotations;

namespace PhoneBookDbNormalized.Models.DTOs
{
    public class LoginRequest    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [Display(Name = "Tên đăng nhập")]
        public required string Username { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public required string Password { get; set; }
    }
}
