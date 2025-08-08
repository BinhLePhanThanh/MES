public interface IFormulaPropertyService
{
    Task<IEnumerable<FormulaProperty>> GetAllAsync();
    Task<FormulaProperty?> GetAsync(int formulaId, int propertyId);
    Task<FormulaProperty> AddAsync(FormulaProperty fp);
    Task<bool> DeleteAsync(int formulaId, int propertyId);
    Task<FormulaProperty?> UpdateAsync(int formulaId, int propertyId, FormulaProperty fp);
    Task<PagedResult<FormulaProperty>> GetPagedDynamicAsync(Dictionary<string, object> filters);

}