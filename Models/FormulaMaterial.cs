using System.Text.Json.Serialization;

public class FormulaMaterial
{
    public int FormulaId { get; set; }
    public Formula? Formula { get; set; } = null!;

    public int MaterialId { get; set; }
    public Material? Material { get; set; } = null!;

    public double Quantity { get; set; }

    public int? UnitId { get; set; }
    public Unit? Unit { get; set; } = null!;
}