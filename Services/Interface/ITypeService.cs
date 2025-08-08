public interface ITypeService
{
    Task<IEnumerable<Type>> GetAllAsync();
    Task<Type?> GetByIdAsync(int id);
    Task<Type> AddAsync(Type type);
    Task<Type?> UpdateAsync(int id, Type type);
    Task<bool> DeleteAsync(int id);
}