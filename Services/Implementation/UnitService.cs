using Microsoft.EntityFrameworkCore;

public class UnitService : IUnitService
{
    private readonly AppDbContext _context;
    public UnitService(AppDbContext context) => _context = context;

    public async Task<IEnumerable<Unit>> GetAllAsync() => await _context.Units.ToListAsync();

    public async Task<Unit?> GetByIdAsync(int id) => await _context.Units.Include(u => u.FormulaMaterials)
        .FirstOrDefaultAsync(u => u.Id == id);

    public async Task<Unit> AddAsync(Unit unit)
    {
        _context.Units.Add(unit);
        await _context.SaveChangesAsync();
        return unit;
    }

    public async Task<Unit?> UpdateAsync(int id, Unit unit)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

        try
        {
            var existing = await _context.Units
                .Include(u => u.FormulaMaterials)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (existing is null)
                return null;

            _context.Entry(existing).CurrentValues.SetValues(unit);

            if (unit.FormulaMaterials != null)
            {
                //_context.FormulaMaterials.RemoveRange(existing.FormulaMaterials);

                // Thêm lại từ unit mới (nếu có)
                foreach (var fm in unit.FormulaMaterials)
                {
                    fm.UnitId = existing.Id;
                    _context.FormulaMaterials.Add(fm);
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
        var unit = await _context.Units.FindAsync(id);
        if (unit is null) return false;
        _context.Units.Remove(unit);
        await _context.SaveChangesAsync();
        return true;
    }
}
