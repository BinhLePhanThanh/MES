using System.Text.Json;
using Microsoft.EntityFrameworkCore;

public class CustomerService: ICustomerService
{
    private readonly AppDbContext _context;

    public CustomerService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Customer> GetCustomerByIdAsync(int id)
    {
        return await _context.Customers.FindAsync(id);
    }

    public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
    {
        return await _context.Customers.ToListAsync();
    }

    public async Task<Customer> CreateCustomerAsync(Customer customer)
    {
        if(customer.Code== null)
        {
            throw new ArgumentException("Customer code cannot be null");
        }
        if(_context.Customers.Any(c => c.Code == customer.Code))
        {
            throw new InvalidOperationException($"Customer with code {customer.Code} already exists.");
        }
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        return customer;
    }

    public async Task<Customer> UpdateCustomerAsync(int id,Customer customer)
    {
        var existingCustomer = await GetCustomerByIdAsync(id);
        if (existingCustomer == null) return null;

        _context.Entry(existingCustomer).CurrentValues.SetValues(customer);


        _context.Customers.Update(existingCustomer);
        await _context.SaveChangesAsync();
        return existingCustomer;
    }

    public async Task<bool> DeleteCustomerAsync(int id)
    {
        var customer = await GetCustomerByIdAsync(id);
        if (customer == null) return false;

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();
        return true;
    }
    public async Task<PagedResult<Customer>> GetPagedDynamicAsync(Dictionary<string, object> filters)
    {
        int page = filters.TryGetValue("Page", out var pg) && pg is JsonElement pElem && pElem.TryGetInt32(out var pVal) ? pVal : 1;
        int pageSize = filters.TryGetValue("PageSize", out var ps) && ps is JsonElement sElem && sElem.TryGetInt32(out var sVal) ? sVal : 10;

        var query = await _context.Customers.AsQueryable().ApplyDynamicFilter(filters);
        return await query.ToPagedResultAsync(page, pageSize);
    }

  
}