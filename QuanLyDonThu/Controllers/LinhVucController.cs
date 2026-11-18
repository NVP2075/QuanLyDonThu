using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyDonThu.Models;
using QuanLyDonThu.ViewModels;

namespace QuanLyDonThu.Controllers;

[Authorize(Roles = "Admin")]
public class LinhVucController : Controller
{
    private readonly QuanLyDonThuContext _context;

    public LinhVucController(QuanLyDonThuContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? search, string? status, int page = 1)
    {
        const int pageSize = 10;
        var fields = _context.LinhVucs.AsNoTracking();
        var normalizedSearch = search?.Trim();
        if (!string.IsNullOrEmpty(normalizedSearch))
        {
            fields = fields.Where(field => field.TenLinhVuc.Contains(normalizedSearch)
                || field.MaDonViNavigation.TenDonVi.Contains(normalizedSearch)
                || field.MaLinhVuc.ToString().Contains(normalizedSearch));
        }

        status = status is "active" or "inactive" ? status : "all";
        if (status == "active") fields = fields.Where(field => field.DangHoatDong == true);
        if (status == "inactive") fields = fields.Where(field => field.DangHoatDong != true);

        var totalCount = await fields.CountAsync();
        page = Math.Max(1, page);
        var linhVucs = await fields
            .AsNoTracking()
            .Include(l => l.MaDonViNavigation)
            .OrderBy(l => l.TenLinhVuc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = linhVucs.Select(l => new LinhVucListItemViewModel
        {
            MaLinhVuc = l.MaLinhVuc,
            TenLinhVuc = l.TenLinhVuc,
            SoNgayXuLyMacDinh = l.SoNgayXuLyMacDinh,
            TenDonVi = l.MaDonViNavigation.TenDonVi,
            DangHoatDong = l.DangHoatDong == true
        }).ToList();

        var model = new LinhVucIndexViewModel
        {
            TotalCount = await _context.LinhVucs.CountAsync(),
            ActiveCount = await _context.LinhVucs.CountAsync(field => field.DangHoatDong == true),
            InactiveCount = await _context.LinhVucs.CountAsync(field => field.DangHoatDong != true),
            AverageDays = await _context.LinhVucs.Select(field => (double?)field.SoNgayXuLyMacDinh).AverageAsync() ?? 0,
            Search = search,
            Status = status,
            Page = new PagedResult<LinhVucListItemViewModel>
            {
                Items = items,
                PageNumber = page,
                PageSize = pageSize,
                TotalCount = totalCount
            }
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await PopulateDonViSelectListAsync();
        return View(new LinhVucFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LinhVucFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDonViSelectListAsync(model.MaDonVi);
            return View(model);
        }

        var donVi = await FindActiveDonViAsync(model.MaDonVi!.Value);
        if (donVi is null)
        {
            ModelState.AddModelError(nameof(model.MaDonVi), "Đơn vị không tồn tại hoặc đã ngừng hoạt động.");
            await PopulateDonViSelectListAsync(model.MaDonVi);
            return View(model);
        }

        var linhVuc = new LinhVuc
        {
            TenLinhVuc = model.TenLinhVuc.Trim(),
            MaDonVi = donVi.MaDonVi,
            SoNgayXuLyMacDinh = model.SoNgayXuLyMacDinh,
            DangHoatDong = model.DangHoatDong
        };

        _context.LinhVucs.Add(linhVuc);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Thêm lĩnh vực thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var linhVuc = await _context.LinhVucs
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.MaLinhVuc == id.Value);

        if (linhVuc is null)
        {
            return NotFound();
        }

        var selectedDonViId = (int?)linhVuc.MaDonVi;

        await PopulateDonViSelectListAsync(selectedDonViId);
        return View(new LinhVucFormViewModel
        {
            MaLinhVuc = linhVuc.MaLinhVuc,
            TenLinhVuc = linhVuc.TenLinhVuc,
            SoNgayXuLyMacDinh = linhVuc.SoNgayXuLyMacDinh,
            MaDonVi = selectedDonViId,
            DangHoatDong = linhVuc.DangHoatDong ?? false
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, LinhVucFormViewModel model)
    {
        if (model.MaLinhVuc != id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await PopulateDonViSelectListAsync(model.MaDonVi);
            return View(model);
        }

        var donVi = await FindActiveDonViAsync(model.MaDonVi!.Value);
        if (donVi is null)
        {
            ModelState.AddModelError(nameof(model.MaDonVi), "Đơn vị không tồn tại hoặc đã ngừng hoạt động.");
            await PopulateDonViSelectListAsync(model.MaDonVi);
            return View(model);
        }

        var linhVuc = await _context.LinhVucs
            .FirstOrDefaultAsync(l => l.MaLinhVuc == id);

        if (linhVuc is null)
        {
            return NotFound();
        }

        if (linhVuc.DangHoatDong == true && !model.DangHoatDong
            && await _context.DonThus.AnyAsync(petition => petition.MaLinhVuc == id && petition.TrangThai != 2))
        {
            ModelState.AddModelError(nameof(model.DangHoatDong), "Không thể ngừng hoạt động lĩnh vực vì vẫn còn đơn thư chưa đóng.");
            await PopulateDonViSelectListAsync(model.MaDonVi);
            return View(model);
        }

        linhVuc.TenLinhVuc = model.TenLinhVuc.Trim();
        linhVuc.MaDonVi = donVi.MaDonVi;
        linhVuc.SoNgayXuLyMacDinh = model.SoNgayXuLyMacDinh;
        linhVuc.DangHoatDong = model.DangHoatDong;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Cập nhật lĩnh vực thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var linhVuc = await _context.LinhVucs.FindAsync(id);
        if (linhVuc is null)
        {
            return NotFound();
        }

        if (linhVuc.DangHoatDong == true && await _context.DonThus.AnyAsync(petition => petition.MaLinhVuc == id && petition.TrangThai != 2))
        {
            TempData["Error"] = "Không thể ngừng hoạt động lĩnh vực vì vẫn còn đơn thư chưa đóng.";
            return RedirectToAction(nameof(Index));
        }

        linhVuc.DangHoatDong = !(linhVuc.DangHoatDong ?? false);
        await _context.SaveChangesAsync();

        TempData["Success"] = linhVuc.DangHoatDong == true
            ? "Đã mở hoạt động cho lĩnh vực."
            : "Đã ngừng hoạt động lĩnh vực.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkToggleStatus(List<int> ids)
    {
        var selectedIds = ids.Where(id => id > 0).Distinct().ToList();
        var linhVucs = await _context.LinhVucs.Where(linhVuc => selectedIds.Contains(linhVuc.MaLinhVuc)).ToListAsync();
        if (linhVucs.Count == 0)
        {
            TempData["Error"] = "Vui lòng chọn ít nhất một lĩnh vực hợp lệ.";
            return RedirectToAction(nameof(Index));
        }

        var blockedCount = 0;
        foreach (var linhVuc in linhVucs)
        {
            if (linhVuc.DangHoatDong == true && await _context.DonThus.AnyAsync(petition => petition.MaLinhVuc == linhVuc.MaLinhVuc && petition.TrangThai != 2))
            {
                blockedCount++;
                continue;
            }
            linhVuc.DangHoatDong = !(linhVuc.DangHoatDong ?? false);
        }

        await _context.SaveChangesAsync();
        var changedCount = linhVucs.Count - blockedCount;
        TempData[changedCount > 0 ? "Success" : "Error"] = blockedCount > 0
            ? $"Đã cập nhật {changedCount} lĩnh vực; không thể ngừng hoạt động {blockedCount} lĩnh vực còn đơn chưa đóng."
            : $"Đã đổi trạng thái {changedCount} lĩnh vực.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<DonVi?> FindActiveDonViAsync(int maDonVi)
    {
        return await _context.DonVis
            .FirstOrDefaultAsync(d => d.MaDonVi == maDonVi && d.DangHoatDong == true);
    }

    private async Task PopulateDonViSelectListAsync(int? selectedId = null)
    {
        var donVis = await _context.DonVis
            .AsNoTracking()
            .Where(d => d.DangHoatDong == true)
            .OrderBy(d => d.TenDonVi)
            .ToListAsync();

        ViewBag.MaDonVi = new SelectList(donVis, nameof(DonVi.MaDonVi), nameof(DonVi.TenDonVi), selectedId);
    }
}
