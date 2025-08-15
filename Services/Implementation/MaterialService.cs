using System.Text.Json;
using Microsoft.EntityFrameworkCore;

public class MaterialService : IMaterialService
{
    private readonly AppDbContext _context;
    public MaterialService(AppDbContext context) => _context = context;

    public async Task<IEnumerable<Material>> GetAllAsync() => await _context.Materials.ToListAsync();

    public async Task<Material?> GetByIdAsync(int id) => await _context.Materials.Include(m => m.FormulaMaterials)
        .FirstOrDefaultAsync(m => m.Id == id);

    public async Task<Material> AddAsync(Material material)
    {
        _context.Materials.Add(material);
        await _context.SaveChangesAsync();
        return material;
    }

    public async Task<Material?> UpdateAsync(int id, Material material)
    {
    await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

        try
        {
            var existing = await _context.Materials
                .Include(m => m.FormulaMaterials)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (existing is null) return null;

            _context.Entry(existing).CurrentValues.SetValues(material);

            if (material.FormulaMaterials != null)
            {
                //existing.FormulaMaterials.Clear();
                foreach (var fm in material.FormulaMaterials)
                {
                    FormulaMaterial? attached = null;

                    if (fm.FormulaId != 0 && fm.MaterialId != 0)
                    {
                        attached = await _context.FormulaMaterials
                            .FindAsync(fm.FormulaId, fm.MaterialId);
                    }

                    if (attached is null)
                    {
                        attached = fm;
                        _context.FormulaMaterials.Add(attached);
                        await _context.SaveChangesAsync();
                    }

                    existing.FormulaMaterials.Add(attached);
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
        var material = await _context.Materials.FindAsync(id);
        if (material is null) return false;
        _context.Materials.Remove(material);
        await _context.SaveChangesAsync();
        return true;
    }
    public async Task<PagedResult<Material>> GetPagedDynamicAsync(Dictionary<string, object> filters)
    {
        int page = filters.TryGetValue("Page", out var pg) && pg is JsonElement pElem && pElem.TryGetInt32(out var pVal) ? pVal : 1;
        int pageSize = filters.TryGetValue("PageSize", out var ps) && ps is JsonElement sElem && sElem.TryGetInt32(out var sVal) ? sVal : 10;

        var query = await _context.Materials.AsQueryable().ApplyDynamicFilter(filters);
        return await query.ToPagedResultAsync(page, pageSize);
    }
}