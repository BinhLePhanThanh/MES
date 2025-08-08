using System.Text.Json;
using Microsoft.EntityFrameworkCore;

public class FormulaMaterialService : IFormulaMaterialService
{
    private readonly AppDbContext _context;
    public FormulaMaterialService(AppDbContext context) => _context = context;

    public async Task<IEnumerable<FormulaMaterial>> GetAllAsync() =>
        await _context.FormulaMaterials
            // .Include(fm => fm.Material)
            // .Include(fm => fm.Unit)
            // .Include(fm => fm.Formula)
            //     .ThenInclude(f => f.Product)
            .ToListAsync();

    public async Task<FormulaMaterial?> GetAsync(int formulaId, int materialId) =>
        await _context.FormulaMaterials
            .Include(fm => fm.Material)
            .Include(fm => fm.Unit)
            .Include(fm => fm.Formula)
                .ThenInclude(f => f.Product)
            .FirstOrDefaultAsync(fm => fm.FormulaId == formulaId && fm.MaterialId == materialId);

    public async Task<FormulaMaterial> AddAsync(FormulaMaterial fm)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            if (fm.Formula is not null && fm.Formula.Id == 0)
                _context.Formulas.Add(fm.Formula);

            if (fm.Material is not null && fm.Material.Id == 0)
                _context.Materials.Add(fm.Material);

            if (fm.Unit is not null && fm.Unit.Id == 0)
                _context.Units.Add(fm.Unit);

            await _context.SaveChangesAsync();

            if (fm.FormulaId == 0) fm.FormulaId = fm.Formula.Id;
            if (fm.MaterialId == 0)
            {
                fm.MaterialId = fm.Material.Id;
                if(fm.Material!=null&&fm.Material.IsActive == false)
                {
                    throw new InvalidOperationException("Material must be active.");
                }
            }
            if (fm.UnitId == 0) fm.UnitId = fm.Unit.Id;

            _context.FormulaMaterials.Add(fm);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
            return fm;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<FormulaMaterial?> UpdateAsync(int formulaId, int materialId, FormulaMaterial updated)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

        try
        {
            var existing = await _context.FormulaMaterials
                .FirstOrDefaultAsync(fm => fm.FormulaId == formulaId && fm.MaterialId == materialId);

            if (existing is null) return null;

            // Update liên kết Unit nếu có thay đổi
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

            if(updated.Material is not null && updated.Material.Id == 0)
            {
                _context.Materials.Add(updated.Material);
                await _context.SaveChangesAsync();
                existing.MaterialId = updated.Material.Id;
            }
            else if (updated.MaterialId != 0)
            {
                existing.MaterialId = updated.MaterialId;
            }
            if(updated.Material is not null&&!updated.Material.IsActive)
            {
                throw new InvalidOperationException("Material must be active.");
            }
            // Update định lượng
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


    public async Task<bool> DeleteAsync(int formulaId, int materialId)
    {
        var fm = await _context.FormulaMaterials.FindAsync(formulaId, materialId);
        if (fm is null) return false;
        _context.FormulaMaterials.Remove(fm);
        await _context.SaveChangesAsync();
        return true;
    }
     public async Task<PagedResult<FormulaMaterial>> GetPagedDynamicAsync(Dictionary<string, object> filters)
    {
        int page = filters.TryGetValue("Page", out var pg) && pg is JsonElement pElem && pElem.TryGetInt32(out var pVal) ? pVal : 1;
        int pageSize = filters.TryGetValue("PageSize", out var ps) && ps is JsonElement sElem && sElem.TryGetInt32(out var sVal) ? sVal : 10;

        var query = _context.FormulaMaterials.AsQueryable().ApplyDynamicFilter(filters);
        return await query.ToPagedResultAsync(page, pageSize);
    }
}
