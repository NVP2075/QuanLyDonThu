using System.ComponentModel.DataAnnotations;

namespace QuanLyDonThu.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập Mã Sinh Viên / Mã Cán bộ")]
    [Display(Name = "Tài khoản (MSSV/Mã CB)")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}
