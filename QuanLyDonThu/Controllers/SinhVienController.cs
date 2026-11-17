using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyDonThu.Models;
using QuanLyDonThu.ViewModels;

namespace QuanLyDonThu.Controllers;

[Authorize(Roles = "SinhVien")]
public class SinhVienController : Controller
{
    private const long MaxEvidenceFileSize = 5 * 1024 * 1024;

    private readonly QuanLyDonThuContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<SinhVienController> _logger;

    public SinhVienController(
        QuanLyDonThuContext context,
        IWebHostEnvironment environment,
        ILogger<SinhVienController> logger)
    {
        _context = context;
        _environment = environment;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var maSinhVien = await GetCurrentStudentIdAsync();
        if (maSinhVien is null)
        {
            return Forbid();
        }

        var now = DateTime.Now;
        var dotTiepNhans = await _context.DotTiepNhans
            .AsNoTracking()
            .Where(d => d.ThoiGianMo <= now && d.ThoiGianDong >= now)
            .OrderBy(d => d.ThoiGianDong)
            .Select(d => new DotTiepNhanListItemViewModel
            {
                MaDot = d.MaDot,
                TenSuKien = d.TenSuKien,
                ThoiGianMo = d.ThoiGianMo,
                ThoiGianDong = d.ThoiGianDong
            })
            .ToListAsync();

        var donThuGanDay = await _context.DonThus
            .AsNoTracking()
            .Where(d => d.MaSinhVien == maSinhVien)
            .OrderByDescending(d => d.NgayTao)
            .ThenByDescending(d => d.MaDon)
            .Take(5)
            .Select(d => new DonThuListItemViewModel
            {
                MaDon = d.MaDon,
                MaCode = d.MaCode,
                TieuDe = d.TieuDe,
                NgayTao = d.NgayTao,
                NgayHenXuLy = d.NgayHenXuLy,
                TrangThai = d.TrangThai
            })
            .ToListAsync();

        return View(new SinhVienDashboardViewModel
        {
            DotTiepNhans = dotTiepNhans,
            DonThuGanDay = donThuGanDay
        });
    }

    [HttpGet]
    public async Task<IActionResult> GuiDon(int? maDot)
    {
        if (await GetCurrentStudentIdAsync() is null)
        {
            return Forbid();
        }

        if (maDot.HasValue && !await IsOpenIntakeAsync(maDot.Value))
        {
            return NotFound();
        }

        var model = new GuiDonViewModel { MaDot = maDot };
        await PopulateLinhVucSelectListAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GuiDon(GuiDonViewModel model)
    {
        var maSinhVien = await GetCurrentStudentIdAsync();
        if (maSinhVien is null)
        {
            return Forbid();
        }

        var linhVuc = await _context.LinhVucs
            .AsNoTracking()
            .Where(l => l.MaLinhVuc == model.MaLinhVuc
                && l.DangHoatDong == true
                && l.MaDonViNavigation.DangHoatDong == true)
            .Select(l => new { l.MaDonVi, l.SoNgayXuLyMacDinh })
            .FirstOrDefaultAsync();

        if (linhVuc is null)
        {
            ModelState.AddModelError(nameof(model.MaLinhVuc), "Lĩnh vực không tồn tại hoặc hiện không khả dụng.");
        }

        if (model.MaDot.HasValue && !await IsOpenIntakeAsync(model.MaDot.Value))
        {
            ModelState.AddModelError(nameof(model.MaDot), "Đợt tiếp nhận không tồn tại hoặc đã đóng.");
        }

        if (!await ValidateEvidenceAsync(model.FileMinhChung))
        {
            ModelState.AddModelError(nameof(model.FileMinhChung), "Tệp phải là JPG, PNG hoặc PDF và không vượt quá 5 MB.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateLinhVucSelectListAsync(model.MaLinhVuc);
            return View(model);
        }

        string? savedFilePath = null;
        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            string? relativeFilePath = null;

            if (model.FileMinhChung is not null)
            {
                (savedFilePath, relativeFilePath) = await SaveEvidenceAsync(model.FileMinhChung);
            }

            var createdAt = DateTime.Now;
            var donThu = new DonThu
            {
                MaCode = Guid.NewGuid().ToString("N")[..20],
                MaSinhVien = maSinhVien,
                MaLinhVuc = model.MaLinhVuc,
                MaDot = model.MaDot,
                MaDonViHienTai = linhVuc!.MaDonVi,
                TieuDe = model.TieuDe.Trim(),
                NoiDung = model.NoiDung.Trim(),
                TrangThai = 0,
                NgayTao = createdAt,
                NgayHenXuLy = createdAt.AddDays(linhVuc.SoNgayXuLyMacDinh)
            };

            _context.DonThus.Add(donThu);
            await _context.SaveChangesAsync();

            donThu.MaCode = $"DT-{createdAt:yyyy}-{donThu.MaDon:D4}";

            if (model.FileMinhChung is not null && relativeFilePath is not null)
            {
                var originalName = Path.GetFileName(model.FileMinhChung.FileName);
                if (originalName.Length > 255)
                {
                    originalName = originalName[..255];
                }

                _context.TepDinhKems.Add(new TepDinhKem
                {
                    MaDon = donThu.MaDon,
                    TenTepGoc = originalName,
                    TenTepLuuTru = relativeFilePath,
                    LoaiTep = Path.GetExtension(model.FileMinhChung.FileName).ToLowerInvariant(),
                    KichThuoc = checked((int)model.FileMinhChung.Length),
                    NgayTaiLen = createdAt
                });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["Success"] = "Gửi đơn thư thành công.";
            return RedirectToAction(nameof(ChiTiet), new { id = donThu.MaDon });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to create a petition for student {MaSinhVien}.", maSinhVien);
            DeleteSavedEvidence(savedFilePath);
            ModelState.AddModelError(string.Empty, "Không thể gửi đơn thư lúc này. Vui lòng thử lại.");
            await PopulateLinhVucSelectListAsync(model.MaLinhVuc);
            return View(model);
        }
    }

    public async Task<IActionResult> LichSu()
    {
        var maSinhVien = await GetCurrentStudentIdAsync();
        if (maSinhVien is null)
        {
            return Forbid();
        }

        var donThus = await _context.DonThus
            .AsNoTracking()
            .Where(d => d.MaSinhVien == maSinhVien)
            .OrderByDescending(d => d.NgayTao)
            .ThenByDescending(d => d.MaDon)
            .Select(d => new DonThuListItemViewModel
            {
                MaDon = d.MaDon,
                MaCode = d.MaCode,
                TieuDe = d.TieuDe,
                NgayTao = d.NgayTao,
                NgayHenXuLy = d.NgayHenXuLy,
                TrangThai = d.TrangThai
            })
            .ToListAsync();

        return View(donThus);
    }

    public async Task<IActionResult> ChiTiet(int id)
    {
        var maSinhVien = await GetCurrentStudentIdAsync();
        if (maSinhVien is null)
        {
            return Forbid();
        }

        var donThu = await _context.DonThus
            .AsNoTracking()
            .Where(d => d.MaDon == id && d.MaSinhVien == maSinhVien)
            .Include(d => d.MaLinhVucNavigation)
            .Include(d => d.MaDonViHienTaiNavigation)
            .Include(d => d.MaDotNavigation)
            .Include(d => d.PhanHois)
                .ThenInclude(p => p.NguoiGui)
            .Include(d => d.TepDinhKems)
            .FirstOrDefaultAsync();

        if (donThu is null)
        {
            return NotFound();
        }

        var model = new ChiTietDonThuViewModel
        {
            MaDon = donThu.MaDon,
            MaCode = donThu.MaCode,
            TieuDe = donThu.TieuDe,
            NoiDung = donThu.NoiDung,
            TenLinhVuc = donThu.MaLinhVucNavigation.TenLinhVuc,
            TenDonVi = donThu.MaDonViHienTaiNavigation.TenDonVi,
            TenDot = donThu.MaDotNavigation?.TenSuKien,
            TrangThai = donThu.TrangThai,
            NgayTao = donThu.NgayTao,
            NgayHenXuLy = donThu.NgayHenXuLy,
            PhanHois = donThu.PhanHois
                .OrderBy(p => p.NgayGui)
                .Select(p => new PhanHoiItemViewModel
                {
                    NguoiGui = p.NguoiGui.UserName ?? string.Empty,
                    NoiDung = p.NoiDung,
                    NgayGui = p.NgayGui
                })
                .ToList(),
            TepMinhChungs = donThu.TepDinhKems
                .Select(t => new TepMinhChungItemViewModel
                {
                    MaTep = t.MaTep,
                    TenTepGoc = t.TenTepGoc,
                    DuongDan = Url.Action(nameof(TaiTepMinhChung), new { id = t.MaTep }) ?? string.Empty
                })
                .ToList()
        };

        return View(model);
    }

    public async Task<IActionResult> TaiTepMinhChung(int id)
    {
        var maSinhVien = await GetCurrentStudentIdAsync();
        if (maSinhVien is null)
        {
            return Forbid();
        }

        var tep = await _context.TepDinhKems
            .AsNoTracking()
            .Where(t => t.MaTep == id && t.MaDonNavigation.MaSinhVien == maSinhVien)
            .Select(t => new { t.TenTepLuuTru, t.TenTepGoc })
            .FirstOrDefaultAsync();

        if (tep is null)
        {
            return NotFound();
        }

        var fileName = Path.GetFileName(tep.TenTepLuuTru);
        var webRootPath = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        var uploadsDirectory = Path.GetFullPath(Path.Combine(webRootPath, "uploads"));
        var filePath = Path.GetFullPath(Path.Combine(uploadsDirectory, fileName));
        if (!filePath.StartsWith(uploadsDirectory + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest();
        }

        if (!System.IO.File.Exists(filePath))
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
            : PhysicalFile(filePath, contentType, tep.TenTepGoc);
    }

    private async Task<bool> IsOpenIntakeAsync(int maDot)
    {
        var now = DateTime.Now;
        return await _context.DotTiepNhans
            .AsNoTracking()
            .AnyAsync(d => d.MaDot == maDot && d.ThoiGianMo <= now && d.ThoiGianDong >= now);
    }

    private async Task PopulateLinhVucSelectListAsync(int? selectedId = null)
    {
        var linhVucs = await _context.LinhVucs
            .AsNoTracking()
            .Where(l => l.DangHoatDong == true && l.MaDonViNavigation.DangHoatDong == true)
            .OrderBy(l => l.TenLinhVuc)
            .Select(l => new { l.MaLinhVuc, l.TenLinhVuc })
            .ToListAsync();

        ViewBag.MaLinhVuc = new SelectList(linhVucs, nameof(LinhVuc.MaLinhVuc), nameof(LinhVuc.TenLinhVuc), selectedId);
    }

    private async Task<bool> ValidateEvidenceAsync(IFormFile? file)
    {
        if (file is null)
        {
            return true;
        }

        if (file.Length is <= 0 or > MaxEvidenceFileSize)
        {
            return false;
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension is not ".jpg" and not ".jpeg" and not ".png" and not ".pdf")
        {
            return false;
        }

        var header = new byte[8];
        await using var stream = file.OpenReadStream();
        var bytesRead = await stream.ReadAsync(header.AsMemory());

        return extension switch
        {
            ".jpg" or ".jpeg" => bytesRead >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
            ".png" => bytesRead >= 8 && header.SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
            ".pdf" => bytesRead >= 5 && header[0] == 0x25 && header[1] == 0x50 && header[2] == 0x44 && header[3] == 0x46 && header[4] == 0x2D,
            _ => false
        };
    }

    private async Task<(string PhysicalPath, string RelativePath)> SaveEvidenceAsync(IFormFile file)
    {
        var webRootPath = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        var uploadsDirectory = Path.Combine(webRootPath, "uploads");
        Directory.CreateDirectory(uploadsDirectory);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var physicalPath = Path.Combine(uploadsDirectory, storedFileName);

        try
        {
            await using var target = new FileStream(physicalPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            await file.CopyToAsync(target);
        }
        catch
        {
            DeleteSavedEvidence(physicalPath);
            throw;
        }

        return (physicalPath, $"uploads/{storedFileName}");
    }

    private void DeleteSavedEvidence(string? filePath)
    {
        if (filePath is null || !System.IO.File.Exists(filePath))
        {
            return;
        }

        try
        {
            System.IO.File.Delete(filePath);
        }
        catch (IOException exception)
        {
            _logger.LogWarning(exception, "Could not remove an evidence file after petition creation failed.");
        }
    }

    private async Task<string?> GetCurrentStudentIdAsync()
    {
        var userName = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(userName))
        {
            return null;
        }

        var studentExists = await _context.SinhViens
            .AsNoTracking()
            .AnyAsync(s => s.MaSinhVien == userName);

        return studentExists ? userName : null;
    }
}
