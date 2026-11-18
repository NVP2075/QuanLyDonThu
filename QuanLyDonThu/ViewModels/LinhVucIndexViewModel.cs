namespace QuanLyDonThu.ViewModels;

public class LinhVucIndexViewModel
{
    public int TotalCount { get; set; }

    public int ActiveCount { get; set; }

    public int InactiveCount { get; set; }

    public double AverageDays { get; set; }

    public string? Search { get; set; }

    public string Status { get; set; } = "all";

    public PagedResult<LinhVucListItemViewModel> Page { get; set; } = new();
}
