using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDonThu.Models;
using QuanLyDonThu.ViewModels;

namespace QuanLyDonThu.Controllers;

[Authorize(Roles = "Admin")]
public class DonViController : Controller
{
    private readonly QuanLyDonThuContext _context;

    public DonViController(QuanLyDonThuContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? search, string? status, int page = 1)
    {
        const int pageSize = 10;
        var units = _context.DonVis.AsNoTracking();
        var normalizedSearch = search?.Trim();
        if (!string.IsNullOrEmpty(normalizedSearch))
        {
            units = units.Where(unit => unit.TenDonVi.Contains(normalizedSearch) || unit.MaDonVi.ToString().Contains(normalizedSearch));
        }

        status = status is "active" or "inactive" ? status : "all";
        if (status == "active") units = units.Where(unit => unit.DangHoatDong == true);
        if (status == "inactive") units = units.Where(unit => unit.DangHoatDong != true);

        var totalCount = await units.CountAsync();
        page = Math.Max(1, page);
        var donVis = await units
            .AsNoTracking()
            .OrderBy(d => d.TenDonVi)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new DonViListItemViewModel
            {
                MaDonVi = d.MaDonVi,
                TenDonVi = d.TenDonVi,
                DangHoatDong = d.DangHoatDong == true,
                SoDonChuaDong = d.DonThus.Count(petition => petition.TrangThai != 2)
            })
            .ToListAsync();

        var model = new DonViIndexViewModel
        {
            TotalCount = await _context.DonVis.CountAsync(),
            ActiveCount = await _context.DonVis.CountAsync(unit => unit.DangHoatDong == true),
            InactiveCount = await _context.DonVis.CountAsync(unit => unit.DangHoatDong != true),
            Search = search,
            Status = status,
            Page = new PagedResult<DonViListItemViewModel>
            {
                Items = donVis,
                PageNumber = page,
                PageSize = pageSize,
                TotalCount = totalCount
            }
        };

        return View(model);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new DonViFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DonViFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var donVi = new DonVi
        {
            TenDonVi = model.TenDonVi.Trim(),
            DangHoatDong = model.DangHoatDong
        };

        _context.DonVis.Add(donVi);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Thêm đơn vị thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var donVi = await _context.DonVis
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.MaDonVi == id.Value);

        if (donVi is null)
        {
            return NotFound();
        }

        return View(new DonViFormViewModel
        {
            MaDonVi = donVi.MaDonVi,
            TenDonVi = donVi.TenDonVi,
            DangHoatDong = donVi.DangHoatDong ?? false
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, DonViFormViewModel model)
    {
        if (model.MaDonVi != id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var donVi = await _context.DonVis.FindAsync(id);
        if (donVi is null)
        {
            return NotFound();
        }

        if (donVi.DangHoatDong == true && !model.DangHoatDong
            && await _context.DonThus.AnyAsync(petition => petition.MaDonViHienTai == id && petition.TrangThai != 2))
        {
            ModelState.AddModelError(nameof(model.DangHoatDong), "Không thể ngừng hoạt động đơn vị vì vẫn còn đơn thư chưa đóng.");
            return View(model);
        }

        donVi.TenDonVi = model.TenDonVi.Trim();
        donVi.DangHoatDong = model.DangHoatDong;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Cập nhật đơn vị thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var donVi = await _context.DonVis.FindAsync(id);
        if (donVi is null)
        {
            return NotFound();
        }

        if (donVi.DangHoatDong == true && await _context.DonThus.AnyAsync(petition => petition.MaDonViHienTai == id && petition.TrangThai != 2))
        {
            TempData["Error"] = "Không thể ngừng hoạt động đơn vị vì vẫn còn đơn thư chưa đóng.";
            return RedirectToAction(nameof(Index));
        }

        donVi.DangHoatDong = !(donVi.DangHoatDong ?? false);
        await _context.SaveChangesAsync();

        TempData["Success"] = donVi.DangHoatDong == true
            ? "Đã mở hoạt động cho đơn vị."
            : "Đã ngừng hoạt động đơn vị.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkToggleStatus(List<int> ids)
    {
        var selectedIds = ids.Where(id => id > 0).Distinct().ToList();
        var donVis = await _context.DonVis.Where(donVi => selectedIds.Contains(donVi.MaDonVi)).ToListAsync();
        if (donVis.Count == 0)
        {
            TempData["Error"] = "Vui lòng chọn ít nhất một đơn vị hợp lệ.";
            return RedirectToAction(nameof(Index));
        }

        var blockedCount = 0;
        foreach (var donVi in donVis)
        {
            if (donVi.DangHoatDong == true && await _context.DonThus.AnyAsync(petition => petition.MaDonViHienTai == donVi.MaDonVi && petition.TrangThai != 2))
            {
                blockedCount++;
                continue;
            }
            donVi.DangHoatDong = !(donVi.DangHoatDong ?? false);
        }

        await _context.SaveChangesAsync();
        var changedCount = donVis.Count - blockedCount;
        TempData[changedCount > 0 ? "Success" : "Error"] = blockedCount > 0
            ? $"Đã cập nhật {changedCount} đơn vị; không thể ngừng hoạt động {blockedCount} đơn vị còn đơn chưa đóng."
            : $"Đã đổi trạng thái {changedCount} đơn vị.";
        return RedirectToAction(nameof(Index));
    }
}
