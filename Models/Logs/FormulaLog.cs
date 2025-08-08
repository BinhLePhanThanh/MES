using System.ComponentModel.DataAnnotations.Schema;

public class FormulaLog
{
    public int Id { get; set; }
    [ForeignKey("Formula")]
    public int FormulaId { get; set; }
    public Formula Formula { get; set; } = null!;

    public string Action { get; set; } = string.Empty; // "Create", "Update", etc.
    [ForeignKey("PerformedBy")]
    public int? PerformedById { get; set; }               // <-- Thêm dòng này
    public Employee? PerformedBy { get; set; }           // Người thực hiện hành động
    public string? PerformedByName { get; set; }          // Tùy chọn, lưu tên cho dễ đọc UI

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public string? Detail { get; set; }                   // JSON hoặc mô tả
}
