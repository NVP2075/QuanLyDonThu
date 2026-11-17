using System;
using System.Collections.Generic;

namespace QuanLyDonThu.Models;

public partial class DotTiepNhan
{
    public int MaDot { get; set; }

    public string TenSuKien { get; set; } = null!;

    public DateTime NgaySuKien { get; set; }

    public int MaDonVi { get; set; }

    public DateTime ThoiGianMo { get; set; }

    public DateTime ThoiGianDong { get; set; }

    public virtual ICollection<DonThu> DonThus { get; set; } = new List<DonThu>();

    public virtual DonVi MaDonViNavigation { get; set; } = null!;
}
