using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

public class Unit
{
  public int Id { get; set; }
  public string Name { get; set; } = string.Empty;
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
  public ICollection<FormulaMaterial>? FormulaMaterials { get; set; }
}