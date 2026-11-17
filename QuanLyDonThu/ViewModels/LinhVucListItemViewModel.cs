namespace QuanLyDonThu.ViewModels;

public class LinhVucListItemViewModel
{
    public int MaLinhVuc { get; set; }

    public string TenLinhVuc { get; set; } = string.Empty;

    public int SoNgayXuLyMacDinh { get; set; }

    public string TenDonVi { get; set; } = string.Empty;

    public bool DangHoatDong { get; set; }
}
