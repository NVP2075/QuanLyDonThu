namespace QuanLyDonThu.ViewModels;

public class UserManagementListItemViewModel
{
    public string Id { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string Role { get; set; } = string.Empty;

    public string RoleDisplayName => Role switch
    {
        "SinhVien" => "Sinh viên",
        "CanBo" => "Cán bộ",
        "Admin" => "Quản trị viên",
        "LanhDao" => "Lãnh đạo",
        _ => "Chưa gán"
    };

    public string? HoTen { get; set; }

    public string? Lop { get; set; }

    public string? TenDonVi { get; set; }

    public bool IsLocked { get; set; }
}
