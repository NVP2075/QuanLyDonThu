using System;
using System.Collections.Generic;

namespace QuanLyDonThu.Models;

public partial class TepDinhKem
{
    public int MaTep { get; set; }

    public int MaDon { get; set; }

    public string TenTepGoc { get; set; } = null!;

    public string TenTepLuuTru { get; set; } = null!;

    public string? LoaiTep { get; set; }

    public int? KichThuoc { get; set; }

    public DateTime? NgayTaiLen { get; set; }

    public virtual DonThu MaDonNavigation { get; set; } = null!;
}
