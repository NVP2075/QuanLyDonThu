namespace QuanLyDonThu.ViewModels;

public class DonThuListItemViewModel
{
    public int MaDon { get; set; }

    public string? MaCode { get; set; }

    public string TieuDe { get; set; } = string.Empty;

    public DateTime? NgayTao { get; set; }

    public DateTime NgayHenXuLy { get; set; }

    public int TrangThai { get; set; }
}
