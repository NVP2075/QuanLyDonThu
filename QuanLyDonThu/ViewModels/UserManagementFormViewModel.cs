using System.ComponentModel.DataAnnotations;

namespace QuanLyDonThu.ViewModels;

public class UserManagementFormViewModel
{
    public string? Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập (MSSV/mã cán bộ).")]
    [StringLength(256)]
    [Display(Name = "Tên đăng nhập (MSSV/mã cán bộ)")]
    public string UserName { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    [StringLength(256)]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn vai trò.")]
    [RegularExpression("^(SinhVien|CanBo|Admin|LanhDao)$", ErrorMessage = "Vai trò không hợp lệ.")]
    [Display(Name = "Vai trò")]
    public string Role { get; set; } = "SinhVien";

    [Display(Name = "Đơn vị công tác")]
    public int? MaDonVi { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu khởi tạo")]
    public string Password { get; set; } = string.Empty;

    [StringLength(150, ErrorMessage = "Họ tên không được vượt quá 150 ký tự.")]
    [Display(Name = "Họ tên sinh viên")]
    public string? HoTen { get; set; }

    [StringLength(50, ErrorMessage = "Lớp không được vượt quá 50 ký tự.")]
    [Display(Name = "Lớp")]
    public string? Lop { get; set; }
}
