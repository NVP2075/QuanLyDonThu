namespace QuanLyDonThu.ViewModels;

public class SinhVienDashboardViewModel
{
    public string HoTenSinhVien { get; set; } = string.Empty;

    public int TongDon { get; set; }

    public int DangXuLy { get; set; }

    public int DaDong { get; set; }

    public IReadOnlyList<DotTiepNhanListItemViewModel> DotTiepNhans { get; set; } = Array.Empty<DotTiepNhanListItemViewModel>();

    public IReadOnlyList<DonThuListItemViewModel> DonThuGanDay { get; set; } = Array.Empty<DonThuListItemViewModel>();
}
