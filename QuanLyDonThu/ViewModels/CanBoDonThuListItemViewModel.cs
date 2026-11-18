namespace QuanLyDonThu.ViewModels;

public class CanBoDonThuListItemViewModel
{
    public int MaDon { get; set; }

    public string? MaCode { get; set; }

    public string TieuDe { get; set; } = string.Empty;

    public string MaSinhVien { get; set; } = string.Empty;

    public string? HoTenSinhVien { get; set; }

    public int TrangThai { get; set; }

    public DateTime? NgayTao { get; set; }

    public DateTime NgayHenXuLy { get; set; }

    public string? MaCanBoXuLy { get; set; }

    public bool QuaHan { get; set; }

    public int SoNgayConLai { get; set; }
}
