using System.ComponentModel.DataAnnotations;

namespace QuanLyDonThu.ViewModels;

public class DotTiepNhanFormViewModel : IValidatableObject
{
    public int? MaDot { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên đợt tiếp nhận.")]
    [StringLength(250, ErrorMessage = "Tên đợt không được vượt quá 250 ký tự.")]
    [Display(Name = "Tên đợt tiếp nhận")]
    public string TenSuKien { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn ngày sự kiện.")]
    [Display(Name = "Ngày sự kiện")]
    public DateTime NgaySuKien { get; set; } = DateTime.Today;

    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn đơn vị phụ trách.")]
    [Display(Name = "Đơn vị phụ trách")]
    public int MaDonVi { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn thời gian mở.")]
    [Display(Name = "Thời gian mở")]
    public DateTime ThoiGianMo { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "Vui lòng chọn thời gian đóng.")]
    [Display(Name = "Thời gian đóng")]
    public DateTime ThoiGianDong { get; set; } = DateTime.Now.AddDays(7);

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ThoiGianDong <= ThoiGianMo)
        {
            yield return new ValidationResult("Thời gian đóng phải sau thời gian mở.", [nameof(ThoiGianDong)]);
        }
    }
}
