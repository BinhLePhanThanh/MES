using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    [ForeignKey("CreatedBy")]
    public int? CreatedById { get; set; }
    public Employee? CreatedBy { get; set; }
    DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    [ForeignKey("UpdatedBy")]
    public int? UpdatedById { get; set; }
    public Employee? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    [JsonIgnore]
    public ICollection<Employee>? Employees { get; set; }
}
