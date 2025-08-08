
public interface IRoleService
{
    Task<IEnumerable<Role>> GetAllAsync();
    Task<Role?> GetByIdAsync(int id);
    Task<Role> AddAsync(Role role);
    Task<Role?> UpdateAsync(int id, Role role);
    Task<bool> DeleteAsync(int id);
    Task<PagedResult<Role>> GetPagedDynamicAsync(Dictionary<string, object> filters);
}