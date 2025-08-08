public interface IPropertyService
{
    Task<IEnumerable<Property>> GetAllAsync();
    Task<Property?> GetByIdAsync(int id);
    Task<Property> AddAsync(Property property);
    Task<Property?> UpdateAsync(int id, Property property);
    Task<bool> DeleteAsync(int id);
}