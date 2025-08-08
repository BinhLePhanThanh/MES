using System.Text.Json;
using Microsoft.EntityFrameworkCore;

public class FormulaService : IFormulaService
{
    private readonly AppDbContext _context;
    private readonly IFormulaLoggingService _loggingService;
    public FormulaService(AppDbContext context, IFormulaLoggingService loggingService)
    {
        _context = context;
        _loggingService = loggingService;
    }
    public async Task<IEnumerable<Formula>> GetAllAsync() =>
        await _context.Formulas
            // .Include(f => f.CreatedBy)
            // .Include(f => f.ProcessedBy)
            // .Include(f => f.Product)
            // .Include(f => f.FormulaMaterials)
            //     .ThenInclude(fm => fm.Material)
            // .Include(f => f.FormulaMaterials)
            //     .ThenInclude(fm => fm.Unit)
            // .Include(f => f.FormulaProperties)
            //     .ThenInclude(fp => fp.Property)
            // .Include(f => f.FormulaProperties)
            //     .ThenInclude(fp => fp.Unit)
            .ToListAsync();

    public async Task<Formula?> GetByIdAsync(int id) =>
        await _context.Formulas
            .Include(f => f.CreatedBy)
            .Include(f => f.ProcessedBy)
            .Include(f => f.Product)
            .Include(f => f.FormulaLogs)
            .Include(f=>f.FormulaFiles)
            .Include(f => f.FormulaMaterials)
                .ThenInclude(fm => fm.Material)
            .Include(f => f.FormulaMaterials)
                .ThenInclude(fm => fm.Unit)
            .Include(f => f.FormulaProperties)
                .ThenInclude(fp => fp.Property)
            .Include(f => f.FormulaProperties)
                .ThenInclude(fp => fp.Unit)
            .FirstOrDefaultAsync(f => f.Id == id);

    public async Task<Formula> AddAsync(Formula formula, string message = "Created by user")
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Nếu Product chưa tồn tại
            if (formula.Product is not null && formula.Product.Id == 0)
                _context.Products.Add(formula.Product);

            // Nếu CreatedBy chưa tồn tại
            if (formula.CreatedBy is not null && formula.CreatedBy.Id == 0)
                _context.Employees.Add(formula.CreatedBy);

            // Nếu ApprovedBy chưa tồn tại
            if (formula.ProcessedBy is not null && formula.ProcessedBy.Id == 0)
                _context.Employees.Add(formula.ProcessedBy);

            // FormulaMaterials: nếu Material/Unit chưa tồn tại
            foreach (var fm in formula.FormulaMaterials ?? [])
            {
                if (fm.Material is not null && fm.Material.Id == 0)
                    _context.Materials.Add(fm.Material);

                if (fm.Unit is not null && fm.Unit.Id == 0)
                    _context.Units.Add(fm.Unit);
            }

            // FormulaProperties: nếu Property/Unit chưa tồn tại
            foreach (var fp in formula.FormulaProperties ?? [])
            {
                if (fp.Property is not null && fp.Property.Id == 0)
                    _context.Properties.Add(fp.Property);

                if (fp.Unit is not null && fp.Unit.Id == 0)
                    _context.Units.Add(fp.Unit);
            }
            if (formula.Product != null && formula.Product.IsActive == false)
            {
                throw new InvalidOperationException("Product must be active.");
            }
            if (formula.CreatedBy != null && formula.CreatedBy.IsActive == false)
            {
                throw new InvalidOperationException("Formula's creator must be active.");
            }
            _context.Formulas.Add(formula);
            await _context.SaveChangesAsync();
            await _loggingService.LogAsync(
                formula.Id,
                "Created",
                formula.CreatedById,
                "Check Id",
                 message
            );
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();


            return formula;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Formula> ApproveAsync(int id, int processedById, string message = "Approved by user")
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

        var processedBy = await _context.Employees.FindAsync(processedById);
        if (processedBy is null) throw new KeyNotFoundException("ApprovedBy employee not found");
        else if (!processedBy.IsActive)
            throw new InvalidOperationException("ApprovedBy employee must be active.");
        var formula = await _context.Formulas.Include(e => e.Product).FirstOrDefaultAsync(e => e.Id == id);
        if (formula is null) throw new KeyNotFoundException("Formula not found");
        if (formula.Status != FormulaStatus.Submitted)
            throw new InvalidOperationException("Formula has not been submitted.");
        if (formula.ProcessedById != null && formula.ProcessedById != processedById)
            throw new InvalidOperationException("Formula must be processed by the pre-specified user.");
        formula.Status = FormulaStatus.Approved;
        formula.Product.FormulaApprobationCount++;
        formula.ProcessedBy = processedBy;
        formula.ProcessedById = processedBy.Id;
        formula.ProcessedAt = DateTime.UtcNow;
        await _loggingService.LogAsync(
        formula.Id,
        "Approved",
        processedBy.Id,
        processedBy.Name,
         message
    );

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return formula;
    }
    public async Task<Formula> DisapproveAsync(int id, int processedById, string message = "Disapproved by user")
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

        var formula = await _context.Formulas.Include(e => e.Product).FirstOrDefaultAsync(e => e.Id == id);
        if (formula is null) throw new KeyNotFoundException("Formula not found");
        if (!(formula.Status != FormulaStatus.Approved || formula.Status != FormulaStatus.Rejected))
            throw new InvalidOperationException("Formula has not been approved.");
        // if (formula.ProcessedById != null && formula.ProcessedById != processedById)
        //     throw new InvalidOperationException("Formula must be processed by the pre-specified user.");

        formula.Status = FormulaStatus.Draft;
        formula.ProcessedAt = null;
        formula.Product.FormulaApprobationCount--;

        await _loggingService.LogAsync(
            formula.Id,
            "Disapproved",
             processedById,
            "Check Id",
             message
        );

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return formula;
    }
    public async Task<Formula> RejectAsync(int id, int processedById, string message = "Rejected by user")
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

        var processedBy = await _context.Employees.FindAsync(processedById);
        if (processedBy is null) throw new KeyNotFoundException("RejectedBy employee not found");
        else if (!processedBy.IsActive)
            throw new InvalidOperationException("RejectedBy employee must be active.");
        var formula = await _context.Formulas.Include(e => e.Product).FirstOrDefaultAsync(e => e.Id == id);
        if (formula is null) throw new KeyNotFoundException("Formula not found");
        if (formula.Status != FormulaStatus.Submitted)
            throw new InvalidOperationException("Formula has not been submitted.");
        if (formula.ProcessedById != null && formula.ProcessedById != processedById)
            throw new InvalidOperationException("Formula must be processed by the pre-specified user.");

        if (formula.Status == FormulaStatus.Approved)
            formula.Product.FormulaApprobationCount--;

        formula.Status = FormulaStatus.Rejected;
        formula.ProcessedBy = processedBy;
        formula.ProcessedById = processedBy.Id;
        formula.ProcessedAt = DateTime.UtcNow;
        await _loggingService.LogAsync(
            formula.Id,
            "Rejected",
            processedBy.Id,
            processedBy.Name,
            message
        );

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return formula;
    }

    public async Task<Formula> SubmitAsync(int id, int processedById, string message = "Submitted by user")
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

        var formula = await _context.Formulas.FindAsync(id);
        if (formula is null) throw new KeyNotFoundException("Formula not found");

        if (formula.Status != FormulaStatus.Draft)
            throw new InvalidOperationException("Formula has already been submitted.");

        formula.Status = FormulaStatus.Submitted;
        await _loggingService.LogAsync(
            formula.Id,
            "Submitted",
            processedById,
            "Check Id",
             message
        );

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return formula;
    }

    public async Task<Formula> UnsubmitAsync(int id, int processedById, string message = "Unsubmitted by user")
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

        var formula = await _context.Formulas.FindAsync(id);
        if (formula is null) throw new KeyNotFoundException("Formula not found");

        if (formula.Status != FormulaStatus.Submitted)
            throw new InvalidOperationException("Only submitted formula can be unsubmitted.");

        formula.Status = FormulaStatus.Draft;
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        await _loggingService.LogAsync(
        formula.Id,
        "Unsubmitted",
        processedById,
        "Check Id",
        message
    );

        return formula;
    }

    public async Task<Formula?> UpdateAsync(int id, Formula formula, int processedById, string message = "Updated by user")
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

        try
        {
            var processedBy = await _context.Employees.FindAsync(processedById);
            if (processedBy is null) throw new KeyNotFoundException("ProcessedBy employee not found");
            else if (!processedBy.IsActive)
                throw new InvalidOperationException("ProcessedBy employee must be active.");
            var existing = await _context.Formulas
                .Include(f => f.FormulaMaterials)
                .Include(f => f.FormulaProperties)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (existing is null) return null;
            if (existing.Status != FormulaStatus.Draft && existing.IsStandard == false)
                throw new InvalidOperationException("Only draft or standard formulas can be updated.");
            // Cập nhật thuộc tính cơ bản
            _context.Entry(existing).CurrentValues.SetValues(formula);


            // CreatedBy
            if (formula.CreatedBy is not null && formula.CreatedBy.Id == 0)
            {
                _context.Employees.Add(formula.CreatedBy);
                await _context.SaveChangesAsync();
                existing.CreatedById = formula.CreatedBy.Id;
            }
            else
            {
                existing.CreatedById = formula.CreatedById;
            }

            if (formula.ProcessedBy is not null && formula.ProcessedBy.Id == 0)
            {
                _context.Employees.Add(formula.ProcessedBy);
                await _context.SaveChangesAsync();
                existing.ProcessedById = formula.ProcessedBy.Id;
            }
            else
            {
                existing.ProcessedById = formula.ProcessedById;
            }

            // Product
            if (formula.Product is not null && formula.Product.Id == 0)
            {
                _context.Products.Add(formula.Product);
                await _context.SaveChangesAsync();
                existing.ProductId = formula.Product.Id;
            }
            else
            {
                existing.ProductId = formula.ProductId;
            }

            // FormulaMaterials: xóa cũ, thêm mới
            if (formula.FormulaMaterials != null)
            {
                //_context.FormulaMaterials.RemoveRange(existing.FormulaMaterials);

                foreach (var fm in formula.FormulaMaterials)
                {
                    if (fm.Material is not null && fm.Material.Id == 0)
                        _context.Materials.Add(fm.Material);
                    if (fm.Unit is not null && fm.Unit.Id == 0)
                        _context.Units.Add(fm.Unit);
                }

                await _context.SaveChangesAsync(); // ensure Material & Unit có Id

                foreach (var fm in formula.FormulaMaterials ?? [])
                {
                    existing.FormulaMaterials.Add(new FormulaMaterial
                    {
                        FormulaId = id,
                        MaterialId = fm.MaterialId != 0 ? fm.MaterialId : fm.Material.Id,
                        UnitId = fm.UnitId != 0 ? fm.UnitId : fm.Unit.Id,
                        Quantity = fm.Quantity
                    });
                }
            }

            // FormulaProperties: tương tự
            if (formula.FormulaProperties != null)
            {
                //_context.FormulaProperties.RemoveRange(existing.FormulaProperties);

                foreach (var fp in formula.FormulaProperties)
                {
                    if (fp.Property is not null && fp.Property.Id == 0)
                        _context.Properties.Add(fp.Property);
                    if (fp.Unit is not null && fp.Unit.Id == 0)
                        _context.Units.Add(fp.Unit);
                }

                await _context.SaveChangesAsync();

                foreach (var fp in formula.FormulaProperties ?? [])
                {
                    existing.FormulaProperties.Add(new FormulaProperty
                    {
                        FormulaId = id,
                        PropertyId = fp.PropertyId != 0 ? fp.PropertyId : fp.Property.Id,
                        UnitId = fp.UnitId != 0 ? fp.UnitId : fp.Unit.Id,
                        Quantity = fp.Quantity
                    });
                }
            }
            // await _loggingService.LogAsync(
            //     existing.Id,
            //     "Updated",
            //     processedBy.Id,
            //     processedBy.Name,
            //     $"Formula '{existing.Name}' updated. " + message
            // );

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


    public async Task<bool> DeleteAsync(int id, int processedById, string message = "Deleted by user")
    {
        var formula = await _context.Formulas.FindAsync(id);
        if (formula is null) return false;
        if (formula.Status != FormulaStatus.Draft)
            throw new InvalidOperationException("Only draft formulas can be deleted.");
        _context.Formulas.Remove(formula);
        await _loggingService.LogAsync(
            formula.Id,
            "Deleted",
            processedById,
            "Check Id",
            $"Formula '{formula.Name}' deleted. " + message
        );

        await _context.SaveChangesAsync();
        return true;
    }


    public async Task<PagedResult<Formula>> GetPagedDynamicAsync(Dictionary<string, object> filters)
    {
        int page = filters.TryGetValue("Page", out var pg) && pg is JsonElement pElem && pElem.TryGetInt32(out var pVal) ? pVal : 1;
        int pageSize = filters.TryGetValue("PageSize", out var ps) && ps is JsonElement sElem && sElem.TryGetInt32(out var sVal) ? sVal : 10;

        var query = _context.Formulas
        //.Include(f => f.CreatedBy)
        // .Include(f => f.ProcessedBy)
        // .Include(f => f.Product)
        // .Include(f => f.FormulaMaterials)
        // .Include(f => f.FormulaProperties)
        .AsQueryable().ApplyDynamicFilter(filters);
        return await query.ToPagedResultAsync(page, pageSize);
    }
}
