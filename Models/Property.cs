using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

public class Property
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    [ForeignKey("Unit")]
    public int? UnitId { get; set; }
    public Unit? Unit { get; set; } = null!;
    public string? Description { get; set; }
    [ForeignKey("CreatedBy")]
    public int? CreatedById { get; set; }
    public Employee? CreatedBy { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    [ForeignKey("UpdatedBy")]
    public int? UpdatedById { get; set; }
    public Employee? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public ICollection<FormulaProperty>? FormulaProperties { get; set; }
}
