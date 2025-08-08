public interface ICustomerService
{
    Task<Customer> GetCustomerByIdAsync(int id);
    Task<IEnumerable<Customer>> GetAllCustomersAsync();
    Task<Customer> CreateCustomerAsync(Customer customer);
    Task<Customer> UpdateCustomerAsync(int id,Customer customer);
    Task<bool> DeleteCustomerAsync(int id);
    Task<PagedResult<Customer>> GetPagedDynamicAsync(Dictionary<string, object> filters);
}