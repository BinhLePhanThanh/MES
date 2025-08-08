using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
public enum ProductStatus
{
    NotApplied = 0,
    Applied = 1
}
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ProductStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    [ForeignKey("CreatedBy")]
    public int? CreatedById { get; set; }
    public Employee? CreatedBy { get; set; } = null!;
    public int FormulaApprobationCount { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public DateTime? UpdatedAt { get; set; }
    [ForeignKey("UpdatedBy")]
    public int? UpdatedById { get; set; }
    public Employee? UpdatedBy { get; set; } = null!;
    public string? Description { get; set; } = string.Empty;
    
    [ForeignKey("Type")]
    public int? TypeId { get; set; }
    public Type? Type { get; set; } = null!;

    public ICollection<Formula>? Formulas { get; set; }
}