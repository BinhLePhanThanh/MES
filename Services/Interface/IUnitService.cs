public interface IUnitService
{
    Task<IEnumerable<Unit>> GetAllAsync();
    Task<Unit?> GetByIdAsync(int id);
    Task<Unit> AddAsync(Unit unit);
    Task<Unit?> UpdateAsync(int id, Unit unit);
    Task<bool> DeleteAsync(int id);
}