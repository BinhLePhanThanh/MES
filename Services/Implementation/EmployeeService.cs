using System.Text.Json;
using Microsoft.EntityFrameworkCore;

public class EmployeeService : IEmployeeService
{
    private readonly AppDbContext _context;
    public EmployeeService(AppDbContext context) => _context = context;

    public async Task<IEnumerable<Employee>> GetAllAsync() => await _context.Employees.ToListAsync();

    public async Task<Employee?> GetByIdAsync(int id) => await _context.Employees
        // .Include(e => e.CreatedProducts)
        // .Include(e => e.CreatedFormulas)
        // .Include(e => e.ApprovedFormulas)
        .FirstOrDefaultAsync(e => e.Id == id);

    public async Task<Employee?> AddAsync(Employee employee)
    {
        var existing = await _context.Employees
            .AnyAsync(e => e.Username.ToLower() == employee.Username.ToLower());

        if (existing)
        {
            // Có thể quăng exception hoặc return null
            throw new InvalidOperationException("Username đã tồn tại.");
            // hoặc return null;
        }

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();
        return employee;
    }

    public async Task<Employee?> UpdateAsync(int id, Employee employee)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        try
        {
            var existing = await _context.Employees
                .Include(e => e.CreatedProducts)
                .Include(e => e.CreatedFormulas)
                .Include(e => e.ApprovedFormulas)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (existing is null)
            {
                await transaction.RollbackAsync();
                return null;
            }

            _context.Entry(existing).CurrentValues.SetValues(employee);

            if (employee.CreatedFormulas != null)
            {
                //existing.CreatedFormulas.Clear();
                foreach (var formula in employee.CreatedFormulas)
                {
                    var attached = await _context.Formulas.FindAsync(formula.Id);
                    if (attached is not null)
                        existing.CreatedFormulas.Add(attached);
                }
            }
            if (employee.ApprovedFormulas != null)
            {
                //existing.ApprovedFormulas.Clear();
                foreach (var formula in employee.ApprovedFormulas)
                {
                    var attached = await _context.Formulas.FindAsync(formula.Id);
                    if (attached is not null)
                        existing.ApprovedFormulas.Add(attached);
                }
            }
            if (employee.CreatedProducts != null)
            {
                //existing.CreatedProducts.Clear();
                foreach (var product in employee.CreatedProducts)
                {
                    var attached = await _context.Products.FindAsync(product.Id);
                    if (attached is not null)
                        existing.CreatedProducts.Add(attached);
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return existing;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee is null) return false;
        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();
        return true;
    }
    public async Task<PagedResult<Employee>> GetPagedDynamicAsync(Dictionary<string, object> filters)
    {
        int page = filters.TryGetValue("Page", out var pg) && pg is JsonElement pElem && pElem.TryGetInt32(out var pVal) ? pVal : 1;
        int pageSize = filters.TryGetValue("PageSize", out var ps) && ps is JsonElement sElem && sElem.TryGetInt32(out var sVal) ? sVal : 10;

        var query = _context.Employees.AsQueryable().ApplyDynamicFilter(filters);
        return await query.ToPagedResultAsync(page, pageSize);
    }
}