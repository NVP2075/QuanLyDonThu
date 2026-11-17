using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuanLyDonThu.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<QuanLyDonThuContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        if (builder.Environment.IsDevelopment())
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
        }
    })
    .AddEntityFrameworkStores<QuanLyDonThuContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
    options.LoginPath = "/Account/Login");

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}")
    .WithStaticAssets();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    foreach (var roleName in new[] { "Admin", "SinhVien", "CanBo", "LanhDao" })
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Không thể tạo role {roleName}: {string.Join(", ", roleResult.Errors.Select(error => error.Description))}");
            }
        }

    const string studentId = "DH52302390";
    var studentUser = await userManager.FindByNameAsync(studentId);
    if (studentUser is null)
    {
        studentUser = new ApplicationUser
        {
            UserName = studentId,
            Email = $"{studentId.ToLowerInvariant()}@student.stu.edu.vn",
            EmailConfirmed = true
        };

        var userResult = await userManager.CreateAsync(studentUser, "52302390");
        if (!userResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Không thể tạo tài khoản sinh viên: {string.Join(", ", userResult.Errors.Select(error => error.Description))}");
        }
    }

    if (!await userManager.IsInRoleAsync(studentUser, "SinhVien"))
    {
        var roleResult = await userManager.AddToRoleAsync(studentUser, "SinhVien");
        if (!roleResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Không thể gán role SinhVien: {string.Join(", ", roleResult.Errors.Select(error => error.Description))}");
        }
    }

    var context = scope.ServiceProvider.GetRequiredService<QuanLyDonThuContext>();
    var studentProfile = await context.SinhViens.FindAsync(studentId);
    if (studentProfile is null)
    {
        context.SinhViens.Add(new SinhVien
        {
            MaSinhVien = studentId,
            UserId = studentUser.Id,
            HoTen = "Sinh viên thử nghiệm",
            Lop = "Chưa cập nhật"
        });
        await context.SaveChangesAsync();
    }
    }

    var adminUser = await userManager.FindByNameAsync("admin");
    if (adminUser is null)
    {
        adminUser = new ApplicationUser
        {
            UserName = "admin",
            Email = "admin@stu.edu.vn",
            EmailConfirmed = true
        };

        var userResult = await userManager.CreateAsync(adminUser, "Admin@123");
        if (!userResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Không thể tạo tài khoản Admin: {string.Join(", ", userResult.Errors.Select(error => error.Description))}");
        }
    }

    if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
    {
        var roleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
        if (!roleResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Không thể gán role Admin: {string.Join(", ", roleResult.Errors.Select(error => error.Description))}");
        }
    }
}

app.Run();
