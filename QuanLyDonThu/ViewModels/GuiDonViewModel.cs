using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace QuanLyDonThu.ViewModels;

public class GuiDonViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn lĩnh vực.")]
    [Display(Name = "Lĩnh vực đơn thư")]
    public int MaLinhVuc { get; set; }

    public int? MaDot { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tiêu đề đơn.")]
    [StringLength(300, ErrorMessage = "Tiêu đề không được vượt quá 300 ký tự.")]
    [Display(Name = "Tiêu đề đơn")]
    public string TieuDe { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập nội dung đơn.")]
    [Display(Name = "Nội dung chi tiết")]
    public string NoiDung { get; set; } = string.Empty;

    [Display(Name = "Tệp minh chứng")]
    public IFormFile? FileMinhChung { get; set; }
}
