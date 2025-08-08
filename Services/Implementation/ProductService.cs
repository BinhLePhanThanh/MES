using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Text.Json;
using System.Transactions;


public class ProductService : IProductService
{
    private readonly AppDbContext _context;
    public ProductService(AppDbContext context) => _context = context;

    public async Task<IEnumerable<Product>> GetAllAsync() =>
        await _context.Products.ToListAsync();

    public async Task<Product?> GetByIdAsync(int id) =>
        await _context.Products.Include(p => p.Formulas)
            .Include(p => p.CreatedBy)
            .Include(p => p.Type)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<Product> CreateAsync(Product product)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

        try
        {
            if (product.CreatedBy is not null && product.CreatedBy.Id == 0)
                _context.Employees.Add(product.CreatedBy);

            if (product.Type is not null && product.Type.Id == 0)
                _context.Types.Add(product.Type);

            await _context.SaveChangesAsync();

            if (product.CreatedById == 0 && product.CreatedBy is not null)
                product.CreatedById = product.CreatedBy.Id;

            if (product.TypeId == 0 && product.Type is not null)
                product.TypeId = product.Type.Id;

            if(product.Type!=null&&!product.Type.IsActive)
            {
                throw new InvalidOperationException("Product type must be active.");
            }
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
            return product;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }


    public async Task<Product?> UpdateAsync(int id, Product product)
    {
    await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

        try
        {
            var existing = await _context.Products
                .Include(p => p.Formulas) // Bao gồm formulas để có thể cập nhật
                .FirstOrDefaultAsync(p => p.Id == id);

            if (existing is null) return null;

            // Nếu CreatedBy là object mới
            if (product.CreatedBy is not null && product.CreatedBy.Id == 0)
            {
                _context.Employees.Add(product.CreatedBy);
                await _context.SaveChangesAsync();
                product.CreatedById = product.CreatedBy.Id;
            }

            // Nếu Type là object mới
            if (product.Type is not null && product.Type.Id == 0)
            {
                _context.Types.Add(product.Type);
                await _context.SaveChangesAsync();
                product.TypeId = product.Type.Id;
            }

            // Cập nhật các thuộc tính đơn giản
            _context.Entry(existing).CurrentValues.SetValues(product);

            // Xử lý cập nhật danh sách Formulas
            if (product.Formulas != null)
            {
                //existing.Formulas.Clear();
                foreach (var formula in product.Formulas)
                {
                    Formula? attached = null;

                    // Nếu là formula mới (Id = 0) thì thêm vào context
                    if (formula.Id == 0)
                    {
                        attached = formula;
                        _context.Formulas.Add(attached);
                        await _context.SaveChangesAsync(); // để lấy Id nếu cần
                    }
                    else
                    {
                        attached = await _context.Formulas.FindAsync(formula.Id);
                    }

                    if (attached is not null)
                        existing.Formulas.Add(attached);
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
        var product = await _context.Products.FindAsync(id);
        if (product == null) return false;

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return true;
    }
    public async Task<PagedResult<Product>> GetPagedDynamicAsync(Dictionary<string, object> filters)
    {
        int page = filters.TryGetValue("Page", out var pg) && pg is JsonElement pElem && pElem.TryGetInt32(out var pVal) ? pVal : 1;
        int pageSize = filters.TryGetValue("PageSize", out var ps) && ps is JsonElement sElem && sElem.TryGetInt32(out var sVal) ? sVal : 10;

        var query = _context.Products.AsQueryable().ApplyDynamicFilter(filters);
        return await query.ToPagedResultAsync(page, pageSize);
    }

}
