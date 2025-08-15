using System.Text.Json;

public class FormulaLoggingService : IFormulaLoggingService
{
    private readonly AppDbContext _context;

    public FormulaLoggingService(AppDbContext context)
    {
        _context = context;
    }

    public async Task LogAsync(int formulaId, string action, int? performedById, string? performedByName = null, string? detail = null)
    {
        var log = new FormulaLog
        {
            FormulaId = formulaId,
            Action = action,
            PerformedById = performedById,
            PerformedByName = performedByName,
            Detail = detail
        };

        _context.FormulaLogs.Add(log);
        await _context.SaveChangesAsync();
    }
      public async Task<PagedResult<FormulaLog>> GetPagedDynamicAsync(Dictionary<string, object> filters)
    {
        int page = filters.TryGetValue("Page", out var pg) && pg is JsonElement pElem && pElem.TryGetInt32(out var pVal) ? pVal : 1;
        int pageSize = filters.TryGetValue("PageSize", out var ps) && ps is JsonElement sElem && sElem.TryGetInt32(out var sVal) ? sVal : 10;

        var query = await _context.FormulaLogs.AsQueryable().ApplyDynamicFilter(filters);
        query = query.OrderByDescending(log => log.Timestamp);
        return await query.ToPagedResultAsync(page, pageSize);
    }
}

