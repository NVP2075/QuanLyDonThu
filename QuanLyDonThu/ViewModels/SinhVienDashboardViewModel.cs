namespace QuanLyDonThu.ViewModels;

public class SinhVienDashboardViewModel
{
    public IReadOnlyList<DotTiepNhanListItemViewModel> DotTiepNhans { get; set; } = Array.Empty<DotTiepNhanListItemViewModel>();

    public IReadOnlyList<DonThuListItemViewModel> DonThuGanDay { get; set; } = Array.Empty<DonThuListItemViewModel>();
}
