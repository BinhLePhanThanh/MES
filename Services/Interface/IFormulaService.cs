public interface IFormulaService
{
    Task<IEnumerable<Formula>> GetAllAsync();
    Task<Formula?> GetByIdAsync(int id);
    Task<Formula> AddAsync(Formula formula, string message = "Created by user");
    Task<Formula?> UpdateAsync(int id, Formula formula, int processedById,string message = "Updated by user");
    Task<bool> DeleteAsync(int id, int processedByI, string message = "Deleted by user");
    Task<Formula> ApproveAsync(int id, int processedById,string message = "Approved by user");
    Task<Formula> RejectAsync(int id, int processedById,string message = "Rejected by user");
    Task<Formula> SubmitAsync(int id, int processedById,string message = "Submitted by user");
    Task<Formula> UnsubmitAsync(int id, int processedById, string message = "Unsubmitted by user");
    Task<Formula>DisapproveAsync(int id, int processedById, string message = "Disapproved by user");
    Task<PagedResult<Formula>> GetPagedDynamicAsync(Dictionary<string, object> filters);

}