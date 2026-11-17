using System;
using System.Collections.Generic;

namespace QuanLyDonThu.Models;

public partial class LinhVuc
{
    public int MaLinhVuc { get; set; }

    public string TenLinhVuc { get; set; } = null!;

    public int MaDonVi { get; set; }

    public int SoNgayXuLyMacDinh { get; set; }

    public bool? DangHoatDong { get; set; }

    public virtual ICollection<DonThu> DonThus { get; set; } = new List<DonThu>();

    public virtual DonVi MaDonViNavigation { get; set; } = null!;
}
