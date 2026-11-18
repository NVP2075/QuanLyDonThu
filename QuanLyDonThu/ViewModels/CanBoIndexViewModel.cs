namespace QuanLyDonThu.ViewModels;

public class CanBoIndexViewModel
{
    public string TenDonVi { get; set; } = string.Empty;

    public int SoDonMoi { get; set; }

    public int SoDonDangXuLy { get; set; }

    public int SoDonDaDong { get; set; }

    public int SoDonQuaHan { get; set; }

    public int? TrangThai { get; set; }

    public string? Search { get; set; }

    public IReadOnlyList<CanBoDonThuListItemViewModel> DonThus { get; set; } = Array.Empty<CanBoDonThuListItemViewModel>();
}
