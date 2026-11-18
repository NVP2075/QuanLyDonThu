namespace QuanLyDonThu.ViewModels;

public class UserManagementIndexViewModel
{
    public PagedResult<UserManagementListItemViewModel> Page { get; set; } = new();

    public string? Search { get; set; }

    public string? Role { get; set; }
}
