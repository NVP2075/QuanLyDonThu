using System;
using System.Collections.Generic;

namespace QuanLyDonThu.Models;

public partial class LichSuXuLy
{
    public int MaLichSu { get; set; }

    public int MaDon { get; set; }

    public int? TrangThaiCu { get; set; }

    public int? TrangThaiMoi { get; set; }

    public string NguoiThucHienId { get; set; } = null!;

    public string? GhiChu { get; set; }

    public DateTime? ThoiGian { get; set; }

    public virtual DonThu MaDonNavigation { get; set; } = null!;

    public virtual ApplicationUser NguoiThucHien { get; set; } = null!;
}
