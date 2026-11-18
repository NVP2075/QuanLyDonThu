using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyDonThu.Models;
using QuanLyDonThu.ViewModels;

namespace QuanLyDonThu.Controllers;

[Authorize(Roles = "CanBo")]
public class DotTiepNhanController : Controller
{
    private readonly QuanLyDonThuContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DotTiepNhanController(QuanLyDonThuContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var query = _context.DotTiepNhans.AsNoTracking();
        if (User.IsInRole("CanBo"))
        {
            var unitId = await GetCurrentUnitIdAsync();
            if (unitId is null) return Forbid();
            query = query.Where(dot => dot.MaDonVi == unitId.Value);
        }

        var now = DateTime.Now;
        var model = await query.OrderByDescending(dot => dot.ThoiGianMo)
            .Select(dot => new DotTiepNhanManagementListItemViewModel
            {
                MaDot = dot.MaDot, TenSuKien = dot.TenSuKien, TenDonVi = dot.MaDonViNavigation.TenDonVi,
                NgaySuKien = dot.NgaySuKien, ThoiGianMo = dot.ThoiGianMo, ThoiGianDong = dot.ThoiGianDong,
                SoDon = dot.DonThus.Count, DangMo = dot.ThoiGianMo <= now && dot.ThoiGianDong >= now
            }).ToListAsync();
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        if (User.IsInRole("CanBo"))
        {
            var unitId = await GetCurrentUnitIdAsync();
            if (unitId is null) return Forbid();
        }
        await PopulateDonViSelectListAsync();
        return View(new DotTiepNhanFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DotTiepNhanFormViewModel model)
    {
        if (User.IsInRole("CanBo"))
        {
            var unitId = await GetCurrentUnitIdAsync();
            if (unitId is null) return Forbid();
            model.MaDonVi = unitId.Value;
        }
        await ValidateUnitAsync(model);
        if (!ModelState.IsValid)
        {
            await PopulateDonViSelectListAsync(model.MaDonVi);
            return View(model);
        }

        _context.DotTiepNhans.Add(new DotTiepNhan
        {
            TenSuKien = model.TenSuKien.Trim(), NgaySuKien = model.NgaySuKien, MaDonVi = model.MaDonVi,
            ThoiGianMo = model.ThoiGianMo, ThoiGianDong = model.ThoiGianDong
        });
        await _context.SaveChangesAsync();
        TempData["Success"] = "Tạo đợt tiếp nhận thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var query = _context.DotTiepNhans.AsNoTracking().Where(dot => dot.MaDot == id);
        if (User.IsInRole("CanBo"))
        {
            var unitId = await GetCurrentUnitIdAsync();
            if (unitId is null) return Forbid();
            query = query.Where(dot => dot.MaDonVi == unitId.Value);
        }
        var dot = await query.FirstOrDefaultAsync();
        if (dot is null) return NotFound();

        var model = new DotTiepNhanFormViewModel
        {
            MaDot = dot.MaDot, TenSuKien = dot.TenSuKien, NgaySuKien = dot.NgaySuKien,
            MaDonVi = dot.MaDonVi, ThoiGianMo = dot.ThoiGianMo, ThoiGianDong = dot.ThoiGianDong
        };
        await PopulateDonViSelectListAsync(model.MaDonVi);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, DotTiepNhanFormViewModel model)
    {
        if (model.MaDot != id) return BadRequest();
        var query = _context.DotTiepNhans.Where(dot => dot.MaDot == id);
        if (User.IsInRole("CanBo"))
        {
            var unitId = await GetCurrentUnitIdAsync();
            if (unitId is null) return Forbid();
            query = query.Where(dot => dot.MaDonVi == unitId.Value);
            model.MaDonVi = unitId.Value;
        }
        var dot = await query.FirstOrDefaultAsync();
        if (dot is null) return NotFound();
        await ValidateUnitAsync(model);
        if (!ModelState.IsValid)
        {
            await PopulateDonViSelectListAsync(model.MaDonVi);
            return View(model);
        }
        dot.TenSuKien = model.TenSuKien.Trim(); dot.NgaySuKien = model.NgaySuKien;
        dot.MaDonVi = model.MaDonVi; dot.ThoiGianMo = model.ThoiGianMo; dot.ThoiGianDong = model.ThoiGianDong;
        await _context.SaveChangesAsync();
        TempData["Success"] = "Cập nhật đợt tiếp nhận thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleOpen(int id)
    {
        var query = _context.DotTiepNhans.Where(dot => dot.MaDot == id);
        if (User.IsInRole("CanBo"))
        {
            var unitId = await GetCurrentUnitIdAsync();
            if (unitId is null) return Forbid();
            query = query.Where(dot => dot.MaDonVi == unitId.Value);
        }
        var dot = await query.FirstOrDefaultAsync();
        if (dot is null) return NotFound();

        var now = DateTime.Now;
        var isOpen = dot.ThoiGianMo <= now && dot.ThoiGianDong >= now;
        if (isOpen)
        {
            dot.ThoiGianDong = now.AddSeconds(-1);
            TempData["Success"] = "Đã đóng đợt tiếp nhận.";
        }
        else if (dot.ThoiGianDong <= now)
        {
            TempData["Error"] = "Đợt đã hết hạn. Hãy chỉnh thời gian đóng trong phần Sửa trước khi mở lại.";
            return RedirectToAction(nameof(Index));
        }
        else
        {
            dot.ThoiGianMo = now;
            TempData["Success"] = "Đã mở đợt tiếp nhận.";
        }
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private async Task<int?> GetCurrentUnitIdAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        return user?.MaDonVi;
    }

    private async Task ValidateUnitAsync(DotTiepNhanFormViewModel model)
    {
        if (!await _context.DonVis.AsNoTracking().AnyAsync(unit => unit.MaDonVi == model.MaDonVi && unit.DangHoatDong == true))
            ModelState.AddModelError(nameof(model.MaDonVi), "Vui lòng chọn một đơn vị đang hoạt động.");
    }

    private async Task PopulateDonViSelectListAsync(int? selectedId = null)
    {
        var units = _context.DonVis.AsNoTracking().Where(unit => unit.DangHoatDong == true);
        if (User.IsInRole("CanBo"))
        {
            var unitId = await GetCurrentUnitIdAsync();
            units = units.Where(unit => unit.MaDonVi == unitId);
        }
        var donVis = await units.OrderBy(unit => unit.TenDonVi).ToListAsync();
        ViewBag.MaDonVi = new SelectList(donVis, nameof(DonVi.MaDonVi), nameof(DonVi.TenDonVi), selectedId);
    }
}
