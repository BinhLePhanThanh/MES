using System.Text.Json.Serialization;

public enum FormulaStatus
{
    Draft = 0,
    Submitted = 1,
    Approved = 2,
    Rejected = 3
}
public class Formula
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public int? ProductId { get; set; }
    public Product? Product { get; set; } = null!;
    public bool? IsStandard { get; set; } = false;
    public int? CreatedById { get; set; }
    public Employee? CreatedBy { get; set; } = null!;
    public int? ProcessedById { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public FormulaStatus Status { get; set; } = FormulaStatus.Draft; // 0: Draft, 1: Submitted, 2: Approved, 3: Rejected
    public Employee? ProcessedBy { get; set; } = null!;

    public ICollection<FormulaMaterial>? FormulaMaterials { get; set; }
    public ICollection<FormulaProperty>? FormulaProperties { get; set; }
    public ICollection<FormulaLog>? FormulaLogs { get; set; }
    public ICollection<FormulaFile>? FormulaFiles { get; set; }

}