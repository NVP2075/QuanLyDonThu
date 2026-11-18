namespace QuanLyDonThu.ViewModels;

public class CanBoChiTietDonThuViewModel
{
    public int MaDon { get; set; }

    public string? MaCode { get; set; }

    public string TieuDe { get; set; } = string.Empty;

    public string NoiDung { get; set; } = string.Empty;

    public string MaSinhVien { get; set; } = string.Empty;

    public string HoTenSinhVien { get; set; } = string.Empty;

    public string TenLop { get; set; } = string.Empty;

    public string TenLinhVuc { get; set; } = string.Empty;

    public string TenDonVi { get; set; } = string.Empty;

    public string? TenDot { get; set; }

    public int TrangThai { get; set; }

    public DateTime? NgayTao { get; set; }

    public DateTime NgayHenXuLy { get; set; }

    public string? MaCanBoXuLy { get; set; }

    public bool QuaHan { get; set; }

    public IReadOnlyList<DonViSelectItemViewModel> DonViChuyenTiep { get; set; } = Array.Empty<DonViSelectItemViewModel>();

    public IReadOnlyList<PhanHoiItemViewModel> PhanHois { get; set; } = Array.Empty<PhanHoiItemViewModel>();

    public IReadOnlyList<TepMinhChungItemViewModel> TepMinhChungs { get; set; } = Array.Empty<TepMinhChungItemViewModel>();
}
