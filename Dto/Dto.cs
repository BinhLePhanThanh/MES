public class ProductDto
{
    public string Name { get; set; } = string.Empty;
    public int? TypeId { get; set; }
    public ProductStatus? Status { get; set; }

    public DateTime CreatedAt { get; set; }
    public int? CreatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedById { get; set; }
    public string? Description { get; set; } = string.Empty;
}

public class FormulaDto
{
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int? ProductId { get; set; }
    public int? CreatedById { get; set; }
    public bool? IsStandard { get; set; } = false;
    public FormulaStatus? Status { get; set; } = FormulaStatus.Draft;
    public int ProcessedById { get; set; }
    public string message { get; set; } = string.Empty;
}

public class MaterialDto
{
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }
    
    public int? CreatedById { get; set; }
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    public int? UnitId { get; set; }
    public int? UpdatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class EmployeeDto
{
    public string Name { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
    public int? CreatedById { get; set; }

    public int Role { get; set; } = 1;
    public bool IsActive { get; set; } = true;
}
public class TypeDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? CreatedById { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    public int? UpdatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class UnitDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? CreatedById { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    public int? UpdatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class PropertyDto
{
    public string Name { get; set; } = string.Empty;

    public int? UnitId { get; set; }

    public string? Description { get; set; }

    public int? CreatedById { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    public int? UpdatedById { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

public class FormulaMaterialDto
{
    public int FormulaId { get; set; }
    public int MaterialId { get; set; }
    public int? UnitId { get; set; }
    public double Quantity { get; set; }
}

public class FormulaPropertyDto
{
    public int FormulaId { get; set; }
    public int PropertyId { get; set; }
    public int? UnitId { get; set; }
    public double Quantity { get; set; }
}
public class RoleDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? CreatedById { get; set; }
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    public int? UpdatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
public class CustomerDto
{

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;  // Có thể bao gồm cả máy bàn và di động

    public string Website { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

}
public class LoginRequestDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
public class LoginResponseDto
{
    public int EmployeeId { get; set; }
    public string Token { get; set; } = string.Empty;
    public int Role { get; set; } = -1; // -1 indicates no role assigned
}