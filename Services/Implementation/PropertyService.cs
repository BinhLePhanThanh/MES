using Microsoft.EntityFrameworkCore;

public class PropertyService : IPropertyService
{
    private readonly AppDbContext _context;
    public PropertyService(AppDbContext context) => _context = context;

    public async Task<IEnumerable<Property>> GetAllAsync() => await _context.Properties.ToListAsync();

    public async Task<Property?> GetByIdAsync(int id) => await _context.Properties
        .Include(p => p.FormulaProperties)
        .ThenInclude(fp => fp.Formula)
        .ThenInclude(f => f.Product)
        .FirstOrDefaultAsync(p => p.Id == id);


    public async Task<Property> AddAsync(Property property)
    {
        _context.Properties.Add(property);
        await _context.SaveChangesAsync();
        return property;
    }

    public async Task<Property?> UpdateAsync(int id, Property property)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

        try
        {
            var existing = await _context.Properties
                .Include(p => p.FormulaProperties)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (existing is null) return null;

            // Cập nhật các trường đơn giản
            _context.Entry(existing).CurrentValues.SetValues(property);

            // Cập nhật FormulaProperties nếu có gửi lên
            if (property.FormulaProperties != null)
            {
                //existing.FormulaProperties.Clear();
                foreach (var fp in property.FormulaProperties)
                {
                    FormulaProperty? attached = null;

                    if (fp.FormulaId != 0 && fp.PropertyId != 0)
                    {
                        attached = await _context.FormulaProperties
                            .FindAsync(fp.FormulaId, fp.PropertyId);
                    }

                    // Nếu không tìm thấy thì thêm mới
                    if (attached is null)
                    {
                        attached = fp;
                        _context.FormulaProperties.Add(attached);
                        await _context.SaveChangesAsync();
                    }

                    existing.FormulaProperties.Add(attached);
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return existing;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }


    public async Task<bool> DeleteAsync(int id)
    {
        var property = await _context.Properties.FindAsync(id);
        if (property is null) return false;
        _context.Properties.Remove(property);
        await _context.SaveChangesAsync();
        return true;
    }
}