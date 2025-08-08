using System.Text.Json;
using Microsoft.EntityFrameworkCore;

public class RoleService : IRoleService
{
    private readonly AppDbContext _context;

    public RoleService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Role>> GetAllAsync()
    {
        return await _context.Roles.ToListAsync();
    }

    public async Task<Role?> GetByIdAsync(int id)
    {
        return await _context.Roles.FindAsync(id);
    }

    public async Task<Role> AddAsync(Role role)
    {
        _context.Roles.Add(role);
        await _context.SaveChangesAsync();
        return role;
    }

    public async Task<Role?> UpdateAsync(int id, Role role)
    {
        var existingRole = await GetByIdAsync(id);
        if (existingRole == null) return null;

        _context.Entry(existingRole).CurrentValues.SetValues(role);
        await _context.SaveChangesAsync();
        return existingRole;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var role = await GetByIdAsync(id);
        if (role == null) return false;

        _context.Roles.Remove(role);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<PagedResult<Role>> GetPagedDynamicAsync(Dictionary<string, object> filters)
    {
        int page = filters.TryGetValue("Page", out var pg) && pg is JsonElement pElem && pElem.TryGetInt32(out var pVal) ? pVal : 1;
        int pageSize = filters.TryGetValue("PageSize", out var ps) && ps is JsonElement sElem && sElem.TryGetInt32(out var sVal) ? sVal : 10;

        var query = _context.Roles.AsQueryable().ApplyDynamicFilter(filters);
        return await query.ToPagedResultAsync(page, pageSize);
    }

}