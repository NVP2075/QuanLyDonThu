using System.ComponentModel.DataAnnotations;

namespace QuanLyDonThu.ViewModels;

public class DonViFormViewModel
{
    public int? MaDonVi { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên đơn vị.")]
    [StringLength(200, ErrorMessage = "Tên đơn vị không được vượt quá 200 ký tự.")]
    [Display(Name = "Tên đơn vị")]
    public string TenDonVi { get; set; } = string.Empty;

    [Display(Name = "Đang hoạt động")]
    public bool DangHoatDong { get; set; } = true;
}
