using Microsoft.EntityFrameworkCore;

public class TypeService : ITypeService
{
    private readonly AppDbContext _context;
    public TypeService(AppDbContext context) => _context = context;

    public async Task<IEnumerable<Type>> GetAllAsync() => await _context.Types.ToListAsync();

    public async Task<Type?> GetByIdAsync(int id) => await _context.Types
    
    .FindAsync(id);

    public async Task<Type> AddAsync(Type type)
    {
        _context.Types.Add(type);
        await _context.SaveChangesAsync();
        return type;
    }

    public async Task<Type?> UpdateAsync(int id, Type type)
    {
    await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

        try
        {
            var existing = await _context.Types
                .Include(t => t.Products)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (existing is null) return null;

            _context.Entry(existing).CurrentValues.SetValues(type);

            if (type.Products != null)
            {
                //existing.Products.Clear();
                foreach (var product in type.Products)
                {
                    Product? attached = null;

                    if (product.Id != 0)
                    {
                        attached = await _context.Products.FindAsync(product.Id);
                    }

                    if (attached is null)
                    {
                        attached = product;
                        _context.Products.Add(attached);
                        await _context.SaveChangesAsync();
                    }

                    existing.Products.Add(attached);
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
        var type = await _context.Types.FindAsync(id);
        if (type is null) return false;
        _context.Types.Remove(type);
        await _context.SaveChangesAsync();
        return true;
    }
}