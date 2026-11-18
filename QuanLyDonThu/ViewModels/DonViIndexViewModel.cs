namespace QuanLyDonThu.ViewModels;

public class DonViIndexViewModel
{
    public int TotalCount { get; set; }

    public int ActiveCount { get; set; }

    public int InactiveCount { get; set; }

    public string? Search { get; set; }

    public string Status { get; set; } = "all";

    public PagedResult<DonViListItemViewModel> Page { get; set; } = new();
}
