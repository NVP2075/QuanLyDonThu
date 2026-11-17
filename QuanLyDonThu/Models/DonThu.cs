using System;
using System.Collections.Generic;

namespace QuanLyDonThu.Models;

public partial class DonThu
{
    public int MaDon { get; set; }

    public string? MaCode { get; set; }

    public string MaSinhVien { get; set; } = null!;

    public int MaLinhVuc { get; set; }

    public int? MaDot { get; set; }

    public int MaDonViHienTai { get; set; }

    public string? MaCanBoXuLy { get; set; }

    public string TieuDe { get; set; } = null!;

    public string NoiDung { get; set; } = null!;

    public int TrangThai { get; set; }

    public DateTime NgayHenXuLy { get; set; }

    public DateTime? NgayTao { get; set; }

    public DateTime? NgayDong { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual ICollection<LichSuXuLy> LichSuXuLies { get; set; } = new List<LichSuXuLy>();

    public virtual ApplicationUser? MaCanBoXuLyNavigation { get; set; }

    public virtual DonVi MaDonViHienTaiNavigation { get; set; } = null!;

    public virtual DotTiepNhan? MaDotNavigation { get; set; }

    public virtual LinhVuc MaLinhVucNavigation { get; set; } = null!;

    public virtual SinhVien MaSinhVienNavigation { get; set; } = null!;

    public virtual ICollection<PhanHoi> PhanHois { get; set; } = new List<PhanHoi>();

    public virtual ICollection<TepDinhKem> TepDinhKems { get; set; } = new List<TepDinhKem>();
}
