using System.ComponentModel.DataAnnotations;

namespace QuanLyDonThu.ViewModels;

public class CanBoPhanHoiViewModel
{
    public int MaDon { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập nội dung phản hồi.")]
    [StringLength(4000, ErrorMessage = "Nội dung phản hồi không được vượt quá 4000 ký tự.")]
    [Display(Name = "Nội dung phản hồi")]
    public string NoiDung { get; set; } = string.Empty;
}
