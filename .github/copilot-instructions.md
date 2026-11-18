# Copilot Instructions

## Project Guidelines
- Project requirements for QuanLyDonThu: Graduation project managing student petitions at STU, built with ASP.NET Core MVC, SQL Server, EF Core Database-First, and ASP.NET Core Identity.
- Roles: SinhVien, CanBo, Admin, LanhDao.
- No public registration; Admin creates accounts; Identity UserName stores student ID (MSSV) or staff code.
- Use a custom login flow with role-based redirects: redirect SinhVien to SinhVien/Index, CanBo to CanBo/Index, Admin to Admin/Index.
- Petition rules: students submit freely or via intake batches; route automatically to the unit responsible for the selected field; statuses: 0 New, 1 Processing after staff accepts and is assigned, 2 Responded, 3 Closed; only assigned staff can respond or close; overdue when NgayHenXuLy has passed and petition is not closed.
- For QuanLyDonThu, prefer keeping ViewModel classes in a dedicated ViewModels folder rather than alongside EF entity models.
- For Razor views, keep presentation stylesheets and JavaScript behavior in separate static files under wwwroot rather than embedding them inline, except for small view-specific data when necessary.
- Only CanBo manages intake campaigns (DotTiepNhan); Admin must not have menu access or controller authorization for intake management.
- For Admin management pages, ensure complete applicable UI/backend polish: include pagination for tables, tooltips on icon buttons, confirmation dialogs for risky actions, and accurate workflow/schema handling. Disable units/fields rather than hard-delete when referenced by petitions. Staff remains assigned to exactly one unit unless user changes that requirement.

## Coding Rules
- Use ViewModels rather than passing entities to views.
- Apply [Authorize(Roles = "...")] on controllers and [ValidateAntiForgeryToken] on all POST actions.
- Use PRG after successful POST.
- Secure uploads to JPG/PNG/PDF and max 5 MB with randomized filenames saved under wwwroot/uploads and database path.
- Use EF Core LINQ including Include/Where/Select as appropriate.