namespace QuanLyDonThu.ViewModels;

public class PaginationViewModel
{
    public int PageNumber { get; set; }

    public int TotalPages { get; set; }

    public string? Search { get; set; }

    public string? Status { get; set; }

    public string? Role { get; set; }

    public string? StatusParameter { get; set; }
}
