using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyDonThu.Models;
using QuanLyDonThu.ViewModels;

namespace QuanLyDonThu.Controllers;

[Authorize(Roles = "Admin")]
public class UserManagementController : Controller
{
    private static readonly string[] AvailableRoles = ["SinhVien", "CanBo", "Admin", "LanhDao"];

    private readonly QuanLyDonThuContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserManagementController(
        QuanLyDonThuContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? search, string? role, int page = 1)
    {
        const int pageSize = 10;
        var users = await _userManager.Users
            .AsNoTracking()
            .OrderBy(user => user.UserName)
            .ToListAsync();
        var studentProfiles = await _context.SinhViens
            .AsNoTracking()
            .ToDictionaryAsync(student => student.UserId);
        var unitNames = await _context.DonVis
            .AsNoTracking()
            .ToDictionaryAsync(unit => unit.MaDonVi, unit => unit.TenDonVi);
        var model = new List<UserManagementListItemViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var userRole = roles.FirstOrDefault() ?? "Chưa gán";
            studentProfiles.TryGetValue(user.Id, out var student);
            model.Add(new UserManagementListItemViewModel
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email,
                Role = userRole,
                HoTen = student?.HoTen,
                Lop = student?.Lop,
                TenDonVi = user.MaDonVi.HasValue && unitNames.TryGetValue(user.MaDonVi.Value, out var unitName) ? unitName : null,
                IsLocked = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow
            });
        }

        var normalizedSearch = search?.Trim();
        if (!string.IsNullOrEmpty(normalizedSearch))
        {
            model = model.Where(user =>
                    user.UserName.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase)
                    || (user.Email?.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ?? false)
                    || (user.HoTen?.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            model = model.Where(user => user.Role == role).ToList();
        }

        page = Math.Max(1, page);
        var totalCount = model.Count;
        var pagedUsers = model.OrderBy(user => user.UserName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        ViewBag.Pagination = new PaginationViewModel
        {
            PageNumber = page,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Search = search,
            Role = role
        };
        ViewBag.Search = search;
        ViewBag.Role = role;
        ViewBag.Roles = new SelectList(new[]
        {
            new { Value = "SinhVien", Text = "Sinh viên" },
            new { Value = "CanBo", Text = "Cán bộ" },
            new { Value = "Admin", Text = "Quản trị viên" },
            new { Value = "LanhDao", Text = "Lãnh đạo" }
        }, "Value", "Text", role);
        return View(new UserManagementIndexViewModel
        {
            Search = search,
            Role = role,
            Page = new PagedResult<UserManagementListItemViewModel>
            {
                Items = pagedUsers,
                PageNumber = page,
                PageSize = pageSize,
                TotalCount = totalCount
            }
        });
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        PopulateRoles();
        await PopulateDonViSelectListAsync();
        return View(new UserManagementFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserManagementFormViewModel model)
    {
        await ValidateStaffUnitAsync(model);
        if (model.Role == "SinhVien")
        {
            ValidateStudentProfile(model);
        }

        if (!ModelState.IsValid)
        {
            await PopulateFormSelectListsAsync(model.Role, model.MaDonVi);
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.UserName.Trim(),
            Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim(),
            MaDonVi = model.Role == "CanBo" ? model.MaDonVi : null,
            EmailConfirmed = true
        };
        var createResult = await _userManager.CreateAsync(user, model.Password);
        if (!createResult.Succeeded)
        {
            AddIdentityErrors(createResult);
            await PopulateFormSelectListsAsync(model.Role, model.MaDonVi);
            return View(model);
        }

        var addRoleResult = await _userManager.AddToRoleAsync(user, model.Role);
        if (!addRoleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            AddIdentityErrors(addRoleResult);
            await PopulateFormSelectListsAsync(model.Role, model.MaDonVi);
            return View(model);
        }

        if (model.Role == "SinhVien")
        {
            _context.SinhViens.Add(new SinhVien
            {
                MaSinhVien = user.UserName!,
                UserId = user.Id,
                HoTen = model.HoTen!.Trim(),
                Lop = model.Lop!.Trim()
            });

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                await _userManager.DeleteAsync(user);
                ModelState.AddModelError(string.Empty, "Không thể tạo hồ sơ sinh viên. MSSV có thể đã tồn tại.");
                await PopulateFormSelectListsAsync(model.Role, model.MaDonVi);
                return View(model);
            }
        }

        TempData["Success"] = "Tạo tài khoản thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetStudentPassword(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        var studentProfile = await _context.SinhViens
            .AsNoTracking()
            .FirstOrDefaultAsync(student => student.UserId == user.Id);
        if (studentProfile is null || !await _userManager.IsInRoleAsync(user, "SinhVien"))
        {
            return BadRequest();
        }

        var studentId = studentProfile.MaSinhVien;
        var temporaryPassword = GenerateTemporaryPassword();
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetResult = await _userManager.ResetPasswordAsync(user, token, temporaryPassword);
        if (!resetResult.Succeeded)
        {
            TempData["Error"] = string.Join(" ", resetResult.Errors.Select(error => error.Description));
            return RedirectToAction(nameof(Index));
        }

        TempData["ResetPasswordResult"] = $"Mật khẩu tạm thời mới của {studentId}: {temporaryPassword}. Hãy chuyển trực tiếp cho sinh viên và yêu cầu đổi mật khẩu sau khi đăng nhập.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkAction(string action, List<string> ids)
    {
        var selectedIds = ids.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList();
        if (selectedIds.Count == 0)
        {
            TempData["Error"] = "Vui lòng chọn ít nhất một tài khoản.";
            return RedirectToAction(nameof(Index));
        }

        var updated = 0;
        var skipped = 0;
        foreach (var id in selectedIds)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
            {
                skipped++;
                continue;
            }

            if (action is "lock" or "unlock")
            {
                if (_userManager.GetUserId(User) == user.Id)
                {
                    skipped++;
                    continue;
                }

                var currentlyLocked = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow;
                if ((action == "lock" && currentlyLocked) || (action == "unlock" && !currentlyLocked))
                {
                    skipped++;
                    continue;
                }

                var result = await _userManager.SetLockoutEndDateAsync(
                    user,
                    action == "lock" ? DateTimeOffset.UtcNow.AddYears(100) : null);
                if (result.Succeeded) updated++; else skipped++;
                continue;
            }

            TempData["Error"] = "Thao tác hàng loạt không hợp lệ.";
            return RedirectToAction(nameof(Index));
        }

        TempData[updated > 0 ? "Success" : "Error"] = skipped == 0
            ? $"Đã cập nhật {updated} tài khoản."
            : $"Đã cập nhật {updated} tài khoản; bỏ qua {skipped} tài khoản không phù hợp hoặc thao tác thất bại.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        var roles = await _userManager.GetRolesAsync(user);
        var student = await _context.SinhViens.AsNoTracking().FirstOrDefaultAsync(profile => profile.UserId == user.Id);
        var model = new UserManagementFormViewModel
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email,
            Role = roles.FirstOrDefault() ?? string.Empty,
            HoTen = student?.HoTen,
            Lop = student?.Lop,
            MaDonVi = user.MaDonVi
        };
        await PopulateFormSelectListsAsync(model.Role, model.MaDonVi);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, UserManagementFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        ModelState.Remove(nameof(model.Password));
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        var currentRole = currentRoles.FirstOrDefault();
        var isCurrentUser = _userManager.GetUserId(User) == user.Id;
        if (isCurrentUser && (model.Role != "Admin" && model.Role != currentRole))
        {
            ModelState.AddModelError(nameof(model.Role), "Không thể đổi vai trò của tài khoản Admin đang đăng nhập.");
        }

        var student = await _context.SinhViens.FirstOrDefaultAsync(profile => profile.UserId == user.Id);
        if (model.Role == "SinhVien")
        {
            ValidateStudentProfile(model);
        }
        else if (student is not null && await _context.DonThus.AnyAsync(petition => petition.MaSinhVien == student.MaSinhVien))
        {
            ModelState.AddModelError(nameof(model.Role), "Không thể bỏ vai trò Sinh viên vì tài khoản đã có đơn thư.");
        }

        await ValidateStaffUnitAsync(model);
        if (!ModelState.IsValid)
        {
            await PopulateFormSelectListsAsync(model.Role, model.MaDonVi);
            return View(model);
        }

        user.Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
        user.MaDonVi = model.Role == "CanBo" ? model.MaDonVi : null;
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            AddIdentityErrors(updateResult);
            await PopulateFormSelectListsAsync(model.Role, model.MaDonVi);
            return View(model);
        }

        if (currentRole != model.Role)
        {
            if (currentRoles.Count > 0)
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                {
                    AddIdentityErrors(removeResult);
                    await PopulateFormSelectListsAsync(model.Role, model.MaDonVi);
                    return View(model);
                }
            }

            var addResult = await _userManager.AddToRoleAsync(user, model.Role);
            if (!addResult.Succeeded)
            {
                if (currentRoles.Count > 0)
                {
                    await _userManager.AddToRolesAsync(user, currentRoles);
                }

                AddIdentityErrors(addResult);
                await PopulateFormSelectListsAsync(model.Role, model.MaDonVi);
                return View(model);
            }
        }

        if (model.Role == "SinhVien")
        {
            if (student is null)
            {
                student = new SinhVien { MaSinhVien = user.UserName!, UserId = user.Id };
                _context.SinhViens.Add(student);
            }

            student.HoTen = model.HoTen!.Trim();
            student.Lop = model.Lop!.Trim();
        }
        else if (student is not null)
        {
            _context.SinhViens.Remove(student);
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = "Cập nhật tài khoản thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleLock(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        if (_userManager.GetUserId(User) == user.Id)
        {
            TempData["Error"] = "Không thể khóa tài khoản Admin đang đăng nhập.";
            return RedirectToAction(nameof(Index));
        }

        var isLocked = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow;
        var result = isLocked
            ? await _userManager.SetLockoutEndDateAsync(user, null)
            : await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));

        if (!result.Succeeded)
        {
            AddIdentityErrors(result);
            TempData["Error"] = "Không thể thay đổi trạng thái tài khoản.";
        }
        else
        {
            TempData["Success"] = isLocked ? "Đã mở khóa tài khoản." : "Đã khóa tài khoản.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> ResetPassword(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        return View(new ResetUserPasswordViewModel { UserId = user.Id, UserName = user.UserName ?? string.Empty });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetUserPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user is null)
        {
            return NotFound();
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
        if (!result.Succeeded)
        {
            AddIdentityErrors(result);
            model.UserName = user.UserName ?? string.Empty;
            return View(model);
        }

        TempData["Success"] = $"Đã đặt lại mật khẩu cho tài khoản {user.UserName}.";
        return RedirectToAction(nameof(Index));
    }

    private void PopulateRoles(string? selectedRole = null)
    {
        ViewBag.Roles = new SelectList(new[]
        {
            new { Value = "SinhVien", Text = "Sinh viên" },
            new { Value = "CanBo", Text = "Cán bộ" },
            new { Value = "Admin", Text = "Quản trị viên" },
            new { Value = "LanhDao", Text = "Lãnh đạo" }
        }, "Value", "Text", selectedRole);
    }

    private async Task PopulateFormSelectListsAsync(string? selectedRole = null, int? selectedDonVi = null)
    {
        PopulateRoles(selectedRole);
        await PopulateDonViSelectListAsync(selectedDonVi);
    }

    private async Task PopulateDonViSelectListAsync(int? selectedId = null)
    {
        var donVis = await _context.DonVis
            .AsNoTracking()
            .Where(unit => unit.DangHoatDong == true)
            .OrderBy(unit => unit.TenDonVi)
            .ToListAsync();
        ViewBag.MaDonVi = new SelectList(donVis, nameof(DonVi.MaDonVi), nameof(DonVi.TenDonVi), selectedId);
    }

    private async Task ValidateStaffUnitAsync(UserManagementFormViewModel model)
    {
        if (model.Role != "CanBo")
        {
            model.MaDonVi = null;
            return;
        }

        if (!model.MaDonVi.HasValue || !await _context.DonVis
                .AsNoTracking()
                .AnyAsync(unit => unit.MaDonVi == model.MaDonVi.Value && unit.DangHoatDong == true))
        {
            ModelState.AddModelError(nameof(model.MaDonVi), "Cán bộ phải được gán vào một đơn vị đang hoạt động.");
        }
    }

    private void ValidateStudentProfile(UserManagementFormViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.HoTen))
        {
            ModelState.AddModelError(nameof(model.HoTen), "Vui lòng nhập họ tên sinh viên.");
        }

        if (string.IsNullOrWhiteSpace(model.Lop))
        {
            ModelState.AddModelError(nameof(model.Lop), "Vui lòng nhập lớp sinh viên.");
        }
    }

    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }

    private static string GenerateTemporaryPassword()
    {
        return $"Stu@{Guid.NewGuid():N}7";
    }
}
