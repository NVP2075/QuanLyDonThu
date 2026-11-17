using System;
using System.Collections.Generic;

namespace QuanLyDonThu.Models;

public partial class PhanHoi
{
    public int MaPhanHoi { get; set; }

    public int MaDon { get; set; }

    public string NguoiGuiId { get; set; } = null!;

    public string NoiDung { get; set; } = null!;

    public DateTime? NgayGui { get; set; }

    public virtual DonThu MaDonNavigation { get; set; } = null!;

    public virtual ApplicationUser NguoiGui { get; set; } = null!;
}
