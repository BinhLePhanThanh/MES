using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;
    
    public string Password { get; set; } = string.Empty;

    public int Role { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    [ForeignKey("CreatedBy")]
    public int? CreatedById { get; set; }
    public Employee? CreatedBy { get; set; }
    public ICollection<Product>? CreatedProducts { get; set; } = new List<Product>();


    public ICollection<Formula>? CreatedFormulas { get; set; } = new List<Formula>();

    
    public ICollection<Formula>? ApprovedFormulas { get; set; } = new List<Formula>();
}
