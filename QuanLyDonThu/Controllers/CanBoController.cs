using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDonThu.Models;
using QuanLyDonThu.ViewModels;

namespace QuanLyDonThu.Controllers;

[Authorize(Roles = "CanBo")]
public class CanBoController : Controller
{
    private readonly QuanLyDonThuContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _environment;

    public CanBoController(
        QuanLyDonThuContext context,
        UserManager<ApplicationUser> userManager,
        IWebHostEnvironment environment)
    {
        _context = context;
        _userManager = userManager;
        _environment = environment;
    }

    public async Task<IActionResult> Index(int? trangThai, string? search)
    {
        var user = await GetCurrentStaffAsync();
        if (user?.MaDonVi is not int maDonVi)
        {
            return Forbid();
        }

        var unitPetitions = _context.DonThus
            .AsNoTracking()
            .Where(petition => petition.MaDonViHienTai == maDonVi);
        if (trangThai is >= 0 and <= 2)
        {
            unitPetitions = unitPetitions.Where(petition => petition.TrangThai == trangThai.Value);
        }

        var normalizedSearch = search?.Trim();
        if (!string.IsNullOrEmpty(normalizedSearch))
        {
            unitPetitions = unitPetitions.Where(petition => petition.MaCode!.Contains(normalizedSearch)
                || petition.TieuDe.Contains(normalizedSearch)
                || petition.MaSinhVien.Contains(normalizedSearch)
                || petition.MaSinhVienNavigation.HoTen.Contains(normalizedSearch));
        }

        var now = DateTime.Now;
        var model = new CanBoIndexViewModel
        {
            TenDonVi = await _context.DonVis.Where(unit => unit.MaDonVi == maDonVi).Select(unit => unit.TenDonVi).FirstOrDefaultAsync() ?? string.Empty,
            SoDonMoi = await _context.DonThus.CountAsync(petition => petition.MaDonViHienTai == maDonVi && petition.TrangThai == 0),
            SoDonDangXuLy = await _context.DonThus.CountAsync(petition => petition.MaDonViHienTai == maDonVi && petition.TrangThai == 1),
            SoDonDaDong = await _context.DonThus.CountAsync(petition => petition.MaDonViHienTai == maDonVi && petition.TrangThai == 2),
            SoDonQuaHan = await _context.DonThus.CountAsync(petition => petition.MaDonViHienTai == maDonVi && petition.TrangThai != 2 && petition.NgayHenXuLy < now),
            TrangThai = trangThai,
            Search = search,
            DonThus = await unitPetitions
            .OrderBy(petition => petition.TrangThai == 0 ? 0 : 1)
            .ThenByDescending(petition => petition.NgayTao)
            .Select(petition => new CanBoDonThuListItemViewModel
            {
                MaDon = petition.MaDon,
                MaCode = petition.MaCode,
                TieuDe = petition.TieuDe,
                MaSinhVien = petition.MaSinhVien,
                HoTenSinhVien = petition.MaSinhVienNavigation.HoTen,
                TrangThai = petition.TrangThai,
                NgayTao = petition.NgayTao,
                NgayHenXuLy = petition.NgayHenXuLy,
                MaCanBoXuLy = petition.MaCanBoXuLy,
                QuaHan = petition.TrangThai != 2 && petition.NgayHenXuLy < now,
                SoNgayConLai = EF.Functions.DateDiffDay(now, petition.NgayHenXuLy)
            })
            .ToListAsync()
        };

        return View(model);
    }

    public async Task<IActionResult> ChiTiet(int id)
    {
        var user = await GetCurrentStaffAsync();
        if (user?.MaDonVi is not int maDonVi)
        {
            return Forbid();
        }

        var petition = await GetUnitPetitionQuery(maDonVi, id)
            .AsNoTracking()
            .Include(item => item.MaSinhVienNavigation)
            .Include(item => item.MaLinhVucNavigation)
            .Include(item => item.MaDonViHienTaiNavigation)
            .Include(item => item.MaDotNavigation)
            .Include(item => item.PhanHois).ThenInclude(reply => reply.NguoiGui)
            .Include(item => item.TepDinhKems)
            .FirstOrDefaultAsync();
        if (petition is null)
        {
            return NotFound();
        }

        return View(new CanBoChiTietDonThuViewModel
        {
            MaDon = petition.MaDon,
            MaCode = petition.MaCode,
            TieuDe = petition.TieuDe,
            NoiDung = petition.NoiDung,
            MaSinhVien = petition.MaSinhVien,
            HoTenSinhVien = petition.MaSinhVienNavigation.HoTen,
            TenLop = petition.MaSinhVienNavigation.Lop,
            TenLinhVuc = petition.MaLinhVucNavigation.TenLinhVuc,
            TenDonVi = petition.MaDonViHienTaiNavigation.TenDonVi,
            TenDot = petition.MaDotNavigation?.TenSuKien,
            TrangThai = petition.TrangThai,
            NgayTao = petition.NgayTao,
            NgayHenXuLy = petition.NgayHenXuLy,
            MaCanBoXuLy = petition.MaCanBoXuLy,
            QuaHan = petition.TrangThai != 2 && petition.NgayHenXuLy < DateTime.Now,
            DonViChuyenTiep = await _context.DonVis.AsNoTracking()
                .Where(unit => unit.MaDonVi != maDonVi && unit.DangHoatDong == true)
                .OrderBy(unit => unit.TenDonVi)
                .Select(unit => new DonViSelectItemViewModel { MaDonVi = unit.MaDonVi, TenDonVi = unit.TenDonVi })
                .ToListAsync(),
            PhanHois = petition.PhanHois.OrderBy(reply => reply.NgayGui).Select(reply => new PhanHoiItemViewModel
            {
                NguoiGui = reply.NguoiGui.UserName ?? string.Empty,
                NoiDung = reply.NoiDung,
                NgayGui = reply.NgayGui
            }).ToList(),
            TepMinhChungs = petition.TepDinhKems.Select(file => new TepMinhChungItemViewModel
            {
                MaTep = file.MaTep,
                TenTepGoc = file.TenTepGoc,
                DuongDan = Url.Action(nameof(TaiTepMinhChung), new { id = file.MaTep }) ?? string.Empty
            }).ToList()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Nhan(int id)
    {
        var user = await GetCurrentStaffAsync();
        if (user?.MaDonVi is not int maDonVi)
        {
            return Forbid();
        }

        var petition = await GetUnitPetitionQuery(maDonVi, id).FirstOrDefaultAsync();
        if (petition is null)
        {
            return NotFound();
        }

        if (petition.TrangThai != 0)
        {
            TempData["Error"] = "Đơn thư không còn ở trạng thái mới để tiếp nhận.";
            return RedirectToAction(nameof(ChiTiet), new { id });
        }

        petition.MaCanBoXuLy = user.Id;
        _context.Entry(petition).Property(item => item.RowVersion).OriginalValue = petition.RowVersion;
        petition.TrangThai = 1;
        _context.LichSuXuLies.Add(new LichSuXuLy
        {
            MaDon = petition.MaDon,
            TrangThaiCu = 0,
            TrangThaiMoi = 1,
            NguoiThucHienId = user.Id,
            GhiChu = "Cán bộ tiếp nhận xử lý đơn.",
            ThoiGian = DateTime.Now
        });

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            TempData["Error"] = "Đơn thư vừa được cập nhật bởi người khác. Vui lòng tải lại trang.";
            return RedirectToAction(nameof(ChiTiet), new { id });
        }

        TempData["Success"] = "Đã nhận xử lý đơn thư.";
        return RedirectToAction(nameof(ChiTiet), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PhanHoi(CanBoPhanHoiViewModel model)
    {
        var user = await GetCurrentStaffAsync();
        if (user?.MaDonVi is not int maDonVi)
        {
            return Forbid();
        }

        var petition = await GetUnitPetitionQuery(maDonVi, model.MaDon).FirstOrDefaultAsync();
        if (petition is null)
        {
            return NotFound();
        }

        if (petition.MaCanBoXuLy != user.Id || petition.TrangThai != 1)
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Nội dung phản hồi không hợp lệ.";
            return RedirectToAction(nameof(ChiTiet), new { id = petition.MaDon });
        }

        var oldStatus = petition.TrangThai;
        petition.TrangThai = 2;
        petition.NgayDong = DateTime.Now;
        _context.PhanHois.Add(new PhanHoi
        {
            MaDon = petition.MaDon,
            NguoiGuiId = user.Id,
            NoiDung = model.NoiDung.Trim(),
            NgayGui = DateTime.Now
        });
        _context.LichSuXuLies.Add(new LichSuXuLy
        {
            MaDon = petition.MaDon,
            TrangThaiCu = oldStatus,
            TrangThaiMoi = 2,
            NguoiThucHienId = user.Id,
            GhiChu = "Cán bộ đã phản hồi và đóng đơn.",
            ThoiGian = DateTime.Now
        });

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            TempData["Error"] = "Đơn thư vừa được cập nhật bởi người khác. Vui lòng tải lại trang.";
            return RedirectToAction(nameof(ChiTiet), new { id = petition.MaDon });
        }

        TempData["Success"] = "Đã gửi phản hồi và đóng đơn thư.";
        return RedirectToAction(nameof(ChiTiet), new { id = petition.MaDon });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Chuyen(CanBoChuyenDonViewModel model)
    {
        var user = await GetCurrentStaffAsync();
        if (user?.MaDonVi is not int maDonVi)
        {
            return Forbid();
        }

        var petition = await GetUnitPetitionQuery(maDonVi, model.MaDon).FirstOrDefaultAsync();
        if (petition is null)
        {
            return NotFound();
        }

        if (petition.MaCanBoXuLy != user.Id || petition.TrangThai != 1)
        {
            return Forbid();
        }

        var destination = model.MaDonViMoi.HasValue
            ? await _context.DonVis.FirstOrDefaultAsync(unit => unit.MaDonVi == model.MaDonViMoi.Value && unit.DangHoatDong == true)
            : null;
        if (destination is null || destination.MaDonVi == maDonVi || !ModelState.IsValid)
        {
            TempData["Error"] = "Đơn vị nhận hoặc lý do chuyển tiếp không hợp lệ.";
            return RedirectToAction(nameof(ChiTiet), new { id = petition.MaDon });
        }

        petition.MaDonViHienTai = destination.MaDonVi;
        petition.MaCanBoXuLy = null;
        petition.TrangThai = 0;
        _context.Entry(petition).Property(item => item.RowVersion).OriginalValue = petition.RowVersion;
        _context.LichSuXuLies.Add(new LichSuXuLy
        {
            MaDon = petition.MaDon,
            TrangThaiCu = 1,
            TrangThaiMoi = 0,
            NguoiThucHienId = user.Id,
            GhiChu = $"Chuyển đến {destination.TenDonVi}. Lý do: {model.LyDo.Trim()}",
            ThoiGian = DateTime.Now
        });

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            TempData["Error"] = "Đơn thư vừa được cập nhật bởi người khác. Vui lòng tải lại trang.";
            return RedirectToAction(nameof(ChiTiet), new { id = petition.MaDon });
        }

        TempData["Success"] = "Đã chuyển đơn thư đến đơn vị " + destination.TenDonVi + ".";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> TaiTepMinhChung(int id)
    {
        var user = await GetCurrentStaffAsync();
        if (user?.MaDonVi is not int maDonVi)
        {
            return Forbid();
        }

        var file = await _context.TepDinhKems
            .AsNoTracking()
            .Where(item => item.MaTep == id && item.MaDonNavigation.MaDonViHienTai == maDonVi)
            .Select(item => new { item.TenTepLuuTru, item.TenTepGoc })
            .FirstOrDefaultAsync();
        if (file is null)
        {
            return NotFound();
        }

        var fileName = Path.GetFileName(file.TenTepLuuTru);
        var uploadsDirectory = Path.GetFullPath(Path.Combine(_environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot"), "uploads"));
        var path = Path.GetFullPath(Path.Combine(uploadsDirectory, fileName));
        if (!path.StartsWith(uploadsDirectory + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest();
        }

        if (!System.IO.File.Exists(path))
        {
            return NotFound();
        }

        var contentType = Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".pdf" => "application/pdf",
            _ => null
        };

        return contentType is null
            ? BadRequest()
            : PhysicalFile(path, contentType, file.TenTepGoc);
    }

    private Task<ApplicationUser?> GetCurrentStaffAsync()
    {
        return _userManager.GetUserAsync(User);
    }

    private IQueryable<DonThu> GetUnitPetitionQuery(int maDonVi, int id)
    {
        return _context.DonThus.Where(petition => petition.MaDon == id && petition.MaDonViHienTai == maDonVi);
    }
}
