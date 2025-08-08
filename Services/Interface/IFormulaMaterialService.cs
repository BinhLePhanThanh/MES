public interface IFormulaMaterialService
{
    Task<IEnumerable<FormulaMaterial>> GetAllAsync();
    Task<FormulaMaterial?> GetAsync(int formulaId, int materialId);
    Task<FormulaMaterial> AddAsync(FormulaMaterial fm);
    Task<bool> DeleteAsync(int formulaId, int materialId);
    Task<FormulaMaterial?> UpdateAsync(int formulaId, int materialId, FormulaMaterial fm);
    Task<PagedResult<FormulaMaterial>> GetPagedDynamicAsync(Dictionary<string, object> filters);
}