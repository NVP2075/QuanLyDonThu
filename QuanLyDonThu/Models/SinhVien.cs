using System;
using System.Collections.Generic;

namespace QuanLyDonThu.Models;

public partial class SinhVien
{
    public string MaSinhVien { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public string HoTen { get; set; } = null!;

    public string Lop { get; set; } = null!;

    public virtual ICollection<DonThu> DonThus { get; set; } = new List<DonThu>();

    public virtual ApplicationUser User { get; set; } = null!;
}
