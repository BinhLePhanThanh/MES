using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

public class Type
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }
    [ForeignKey("CreatedBy")]
    public int? CreatedById { get; set; }
    public Employee? CreatedBy { get; set; }
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    [ForeignKey("UpdatedBy")]
    public int? UpdatedById { get; set; }
    public Employee? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    [JsonIgnore]
    public ICollection<Product>? Products { get; set; }
}