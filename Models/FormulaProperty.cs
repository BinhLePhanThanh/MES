using System.Text.Json.Serialization;

public class FormulaProperty
{
    public int FormulaId { get; set; }
    public Formula? Formula { get; set; } = null!;

    public int PropertyId { get; set; }
    public Property? Property { get; set; } = null!;

    public double Quantity { get; set; }

    public int? UnitId { get; set; }
    public Unit? Unit { get; set; } = null!;
}
