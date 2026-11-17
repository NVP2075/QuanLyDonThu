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

    public async Task<IActionResult> Index()
    {
        var donVis = await _context.DonVis
            .AsNoTracking()
            .OrderBy(d => d.TenDonVi)
            .Select(d => new DonViListItemViewModel
            {
                MaDonVi = d.MaDonVi,
                TenDonVi = d.TenDonVi,
                DangHoatDong = d.DangHoatDong == true
            })
            .ToListAsync();

        return View(donVis);
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

        donVi.DangHoatDong = !(donVi.DangHoatDong ?? false);
        await _context.SaveChangesAsync();

        TempData["Success"] = donVi.DangHoatDong == true
            ? "Đã mở hoạt động cho đơn vị."
            : "Đã ngừng hoạt động đơn vị.";

        return RedirectToAction(nameof(Index));
    }
}
