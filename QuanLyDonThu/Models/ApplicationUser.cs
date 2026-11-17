using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace QuanLyDonThu.Models;

public class ApplicationUser : IdentityUser
{
    public virtual ICollection<DonThu> DonThus { get; set; } = new List<DonThu>();

    public virtual ICollection<LichSuXuLy> LichSuXuLies { get; set; } = new List<LichSuXuLy>();

    public virtual ICollection<PhanHoi> PhanHois { get; set; } = new List<PhanHoi>();

    public virtual SinhVien? SinhVien { get; set; }
}