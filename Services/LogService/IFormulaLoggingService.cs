public interface IFormulaLoggingService
{
    Task LogAsync(int formulaId, string action, int? performedById, string? performedByName = null, string? detail = null);
    Task<PagedResult<FormulaLog>> GetPagedDynamicAsync(Dictionary<string, object> filters);
}
