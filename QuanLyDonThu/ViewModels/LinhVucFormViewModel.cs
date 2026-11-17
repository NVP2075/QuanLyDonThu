using System.ComponentModel.DataAnnotations;

namespace QuanLyDonThu.ViewModels;

public class LinhVucFormViewModel
{
    public int? MaLinhVuc { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên lĩnh vực.")]
    [StringLength(200, ErrorMessage = "Tên lĩnh vực không được vượt quá 200 ký tự.")]
    [Display(Name = "Tên lĩnh vực")]
    public string TenLinhVuc { get; set; } = string.Empty;

    [Range(1, 3650, ErrorMessage = "Số ngày xử lý phải lớn hơn 0.")]
    [Display(Name = "Số ngày xử lý mặc định")]
    public int SoNgayXuLyMacDinh { get; set; } = 1;

    [Required(ErrorMessage = "Vui lòng chọn đơn vị phụ trách.")]
    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn đơn vị phụ trách.")]
    [Display(Name = "Đơn vị phụ trách")]
    public int? MaDonVi { get; set; }

    [Display(Name = "Đang hoạt động")]
    public bool DangHoatDong { get; set; } = true;
}
