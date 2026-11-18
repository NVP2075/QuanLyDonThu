using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace QuanLyDonThu.Models;

public partial class QuanLyDonThuContext : IdentityDbContext<ApplicationUser>
{
    public QuanLyDonThuContext()
    {
    }

    public QuanLyDonThuContext(DbContextOptions<QuanLyDonThuContext> options)
        : base(options)
    {
    }

    public virtual DbSet<DonThu> DonThus { get; set; }

    public virtual DbSet<DonVi> DonVis { get; set; }

    public virtual DbSet<DotTiepNhan> DotTiepNhans { get; set; }

    public virtual DbSet<LichSuXuLy> LichSuXuLies { get; set; }

    public virtual DbSet<LinhVuc> LinhVucs { get; set; }

    public virtual DbSet<PhanHoi> PhanHois { get; set; }

    public virtual DbSet<SinhVien> SinhViens { get; set; }

    public virtual DbSet<TepDinhKem> TepDinhKems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.HasIndex(e => e.NormalizedEmail, "EmailIndex");

            entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedUserName] IS NOT NULL)");

            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.UserName).HasMaxLength(256);

            entity.HasOne(e => e.DonVi)
                .WithMany(d => d.CanBos)
                .HasForeignKey(e => e.MaDonVi)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_AspNetUsers_DonVi_MaDonVi");
        });

        modelBuilder.Entity<DonThu>(entity =>
        {
            entity.HasKey(e => e.MaDon).HasName("PK__DonThu__3D89F568FE00B3ED");

            entity.ToTable("DonThu");

            entity.HasIndex(e => e.MaCode, "UQ__DonThu__152C7C5CB61F3AFE").IsUnique();

            entity.Property(e => e.MaCanBoXuLy).HasMaxLength(450);
            entity.Property(e => e.MaCode)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.MaSinhVien)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.NgayDong).HasColumnType("datetime");
            entity.Property(e => e.NgayHenXuLy).HasColumnType("datetime");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.TieuDe).HasMaxLength(300);

            entity.HasOne(d => d.MaCanBoXuLyNavigation).WithMany(p => p.DonThus)
                .HasForeignKey(d => d.MaCanBoXuLy)
                .HasConstraintName("FK__DonThu__MaCanBoX__07C12930");

            entity.HasOne(d => d.MaDonViHienTaiNavigation).WithMany(p => p.DonThus)
                .HasForeignKey(d => d.MaDonViHienTai)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DonThu__MaDonViH__06CD04F7");

            entity.HasOne(d => d.MaDotNavigation).WithMany(p => p.DonThus)
                .HasForeignKey(d => d.MaDot)
                .HasConstraintName("FK__DonThu__MaDot__05D8E0BE");

            entity.HasOne(d => d.MaLinhVucNavigation).WithMany(p => p.DonThus)
                .HasForeignKey(d => d.MaLinhVuc)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DonThu__MaLinhVu__04E4BC85");

            entity.HasOne(d => d.MaSinhVienNavigation).WithMany(p => p.DonThus)
                .HasForeignKey(d => d.MaSinhVien)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DonThu__MaSinhVi__03F0984C");
        });

        modelBuilder.Entity<DonVi>(entity =>
        {
            entity.HasKey(e => e.MaDonVi).HasName("PK__DonVi__DDA5A6CF532DFF22");

            entity.ToTable("DonVi");

            entity.Property(e => e.DangHoatDong).HasDefaultValue(true);
            entity.Property(e => e.TenDonVi).HasMaxLength(200);

            entity.HasOne(d => d.MaDonViChaNavigation).WithMany(p => p.InverseMaDonViChaNavigation)
                .HasForeignKey(d => d.MaDonViCha)
                .HasConstraintName("FK_DonVi_Cha");

        });

        modelBuilder.Entity<DotTiepNhan>(entity =>
        {
            entity.HasKey(e => e.MaDot).HasName("PK__DotTiepN__3D89F56EA807508B");

            entity.ToTable("DotTiepNhan");

            entity.Property(e => e.NgaySuKien).HasColumnType("datetime");
            entity.Property(e => e.TenSuKien).HasMaxLength(250);
            entity.Property(e => e.ThoiGianDong).HasColumnType("datetime");
            entity.Property(e => e.ThoiGianMo).HasColumnType("datetime");

            entity.HasOne(d => d.MaDonViNavigation).WithMany(p => p.DotTiepNhans)
                .HasForeignKey(d => d.MaDonVi)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DotTiepNh__MaDon__7A672E12");
        });

        modelBuilder.Entity<LichSuXuLy>(entity =>
        {
            entity.HasKey(e => e.MaLichSu).HasName("PK__LichSuXu__C443222A14361D62");

            entity.ToTable("LichSuXuLy");

            entity.Property(e => e.GhiChu).HasMaxLength(500);
            entity.Property(e => e.NguoiThucHienId).HasMaxLength(450);
            entity.Property(e => e.ThoiGian)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MaDonNavigation).WithMany(p => p.LichSuXuLies)
                .HasForeignKey(d => d.MaDon)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LichSuXuL__MaDon__14270015");

            entity.HasOne(d => d.NguoiThucHien).WithMany(p => p.LichSuXuLies)
                .HasForeignKey(d => d.NguoiThucHienId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LichSuXuL__Nguoi__151B244E");
        });

        modelBuilder.Entity<LinhVuc>(entity =>
        {
            entity.HasKey(e => e.MaLinhVuc).HasName("PK__LinhVuc__318894A04871715B");

            entity.ToTable("LinhVuc");

            entity.HasIndex(e => e.MaDonVi, "IX_LinhVuc_MaDonVi");

            entity.Property(e => e.DangHoatDong).HasDefaultValue(true);
            entity.Property(e => e.TenLinhVuc).HasMaxLength(200);

            entity.HasOne(d => d.MaDonViNavigation).WithMany(p => p.LinhVucs)
                .HasForeignKey(d => d.MaDonVi)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LinhVuc_DonVi_MaDonVi");
        });

        modelBuilder.Entity<PhanHoi>(entity =>
        {
            entity.HasKey(e => e.MaPhanHoi).HasName("PK__PhanHoi__3458D20FB422F6E2");

            entity.ToTable("PhanHoi");

            entity.Property(e => e.NgayGui)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NguoiGuiId).HasMaxLength(450);

            entity.HasOne(d => d.MaDonNavigation).WithMany(p => p.PhanHois)
                .HasForeignKey(d => d.MaDon)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PhanHoi__MaDon__0F624AF8");

            entity.HasOne(d => d.NguoiGui).WithMany(p => p.PhanHois)
                .HasForeignKey(d => d.NguoiGuiId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PhanHoi__NguoiGu__10566F31");
        });

        modelBuilder.Entity<SinhVien>(entity =>
        {
            entity.HasKey(e => e.MaSinhVien).HasName("PK__SinhVien__939AE775C38DF4DE");

            entity.ToTable("SinhVien");

            entity.HasIndex(e => e.UserId, "UQ__SinhVien__1788CC4D0A687A92").IsUnique();

            entity.Property(e => e.MaSinhVien)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.HoTen).HasMaxLength(150);
            entity.Property(e => e.Lop).HasMaxLength(50);

            entity.HasOne(d => d.User).WithOne(p => p.SinhVien)
                .HasForeignKey<SinhVien>(d => d.UserId)
                .HasConstraintName("FK__SinhVien__UserId__7E37BEF6");
        });

        modelBuilder.Entity<TepDinhKem>(entity =>
        {
            entity.HasKey(e => e.MaTep).HasName("PK__TepDinhK__314EA1A813EE3609");

            entity.ToTable("TepDinhKem");

            entity.Property(e => e.LoaiTep).HasMaxLength(50);
            entity.Property(e => e.NgayTaiLen)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TenTepGoc).HasMaxLength(255);
            entity.Property(e => e.TenTepLuuTru).HasMaxLength(255);

            entity.HasOne(d => d.MaDonNavigation).WithMany(p => p.TepDinhKems)
                .HasForeignKey(d => d.MaDon)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TepDinhKe__MaDon__0B91BA14");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
