using System.ComponentModel.DataAnnotations;

namespace QuanLyDonThu.ViewModels;

public class CanBoChuyenDonViewModel
{
    public int MaDon { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn đơn vị tiếp nhận.")]
    [Display(Name = "Đơn vị tiếp nhận")]
    public int? MaDonViMoi { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập lý do chuyển đơn.")]
    [StringLength(500, ErrorMessage = "Lý do không được vượt quá 500 ký tự.")]
    [Display(Name = "Lý do chuyển đơn")]
    public string LyDo { get; set; } = string.Empty;
}
