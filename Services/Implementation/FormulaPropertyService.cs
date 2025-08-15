using System.Text.Json;
using Microsoft.EntityFrameworkCore;

public class FormulaPropertyService : IFormulaPropertyService
{
    private readonly AppDbContext _context;
    public FormulaPropertyService(AppDbContext context) => _context = context;

    public async Task<IEnumerable<FormulaProperty>> GetAllAsync() =>
        await _context.FormulaProperties
            // .Include(fp => fp.Property)
            // .Include(fp => fp.Unit)
            // .Include(fp => fp.Formula)
            //     .ThenInclude(f => f.Product)
            .ToListAsync();

    public async Task<FormulaProperty?> GetAsync(int formulaId, int propertyId) =>
        await _context.FormulaProperties
            .Include(fp => fp.Property)
            .Include(fp => fp.Unit)
            .Include(fp => fp.Formula)
                .ThenInclude(f => f.Product)
            .FirstOrDefaultAsync(fp => fp.FormulaId == formulaId && fp.PropertyId == propertyId);

    public async Task<FormulaProperty> AddAsync(FormulaProperty fp)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Thêm các entity liên quan nếu cần
            if (fp.Formula is not null && fp.Formula.Id == 0)
                _context.Formulas.Add(fp.Formula);

            if (fp.Property is not null && fp.Property.Id == 0)
                _context.Properties.Add(fp.Property);

            if (fp.Unit is not null && fp.Unit.Id == 0)
                _context.Units.Add(fp.Unit);

            await _context.SaveChangesAsync();

            // Gán Id nếu cần
            if (fp.FormulaId == 0) fp.FormulaId = fp.Formula.Id;
            if (fp.PropertyId == 0) fp.PropertyId = fp.Property.Id;
            if (fp.UnitId == 0) fp.UnitId = fp.Unit.Id;

            _context.FormulaProperties.Add(fp);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
            return fp;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    public async Task<FormulaProperty?> UpdateAsync(int formulaId, int propertyId, FormulaProperty updated)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

        try
        {
            var existing = await _context.FormulaProperties
                .FirstOrDefaultAsync(fp => fp.FormulaId == formulaId && fp.PropertyId == propertyId);

            if (existing is null) return null;

            // Thêm Unit nếu cần
            if (updated.Unit is not null && updated.Unit.Id == 0)
            {
                _context.Units.Add(updated.Unit);
                await _context.SaveChangesAsync();
                existing.UnitId = updated.Unit.Id;
            }
            else if (updated.UnitId != 0)
            {
                existing.UnitId = updated.UnitId;
            }

            // Cập nhật định lượng
            _context.Entry(existing).CurrentValues.SetValues(updated);

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

    public async Task<bool> DeleteAsync(int formulaId, int propertyId)

    {
        var fp = await _context.FormulaProperties.FindAsync(formulaId, propertyId);
        if (fp is null) return false;
        _context.FormulaProperties.Remove(fp);
        await _context.SaveChangesAsync();
        return true;
    }
    public async Task<PagedResult<FormulaProperty>> GetPagedDynamicAsync(Dictionary<string, object> filters)
    {
        int page = filters.TryGetValue("Page", out var pg) && pg is JsonElement pElem && pElem.TryGetInt32(out var pVal) ? pVal : 1;
        int pageSize = filters.TryGetValue("PageSize", out var ps) && ps is JsonElement sElem && sElem.TryGetInt32(out var sVal) ? sVal : 10;

        var query = await _context.FormulaProperties.AsQueryable().ApplyDynamicFilter(filters);
        return await query.ToPagedResultAsync(page, pageSize);
    }
}
