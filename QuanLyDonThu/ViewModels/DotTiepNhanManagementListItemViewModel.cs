namespace QuanLyDonThu.ViewModels;

public class DotTiepNhanManagementListItemViewModel
{
    public int MaDot { get; set; }

    public string TenSuKien { get; set; } = string.Empty;

    public string TenDonVi { get; set; } = string.Empty;

    public DateTime NgaySuKien { get; set; }

    public DateTime ThoiGianMo { get; set; }

    public DateTime ThoiGianDong { get; set; }

    public int SoDon { get; set; }

    public bool DangMo { get; set; }
}
