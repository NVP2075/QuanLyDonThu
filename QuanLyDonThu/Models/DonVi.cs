using System;
using System.Collections.Generic;

namespace QuanLyDonThu.Models;

public partial class DonVi
{
    public int MaDonVi { get; set; }

    public string TenDonVi { get; set; } = null!;

    public int? MaDonViCha { get; set; }

    public bool? DangHoatDong { get; set; }

    public virtual ICollection<DonThu> DonThus { get; set; } = new List<DonThu>();

    public virtual ICollection<ApplicationUser> CanBos { get; set; } = new List<ApplicationUser>();

    public virtual ICollection<DotTiepNhan> DotTiepNhans { get; set; } = new List<DotTiepNhan>();

    public virtual ICollection<DonVi> InverseMaDonViChaNavigation { get; set; } = new List<DonVi>();

    public virtual DonVi? MaDonViChaNavigation { get; set; }

    public virtual ICollection<LinhVuc> LinhVucs { get; set; } = new List<LinhVuc>();
}
