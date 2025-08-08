public interface IGenericModelFileService
{
    public Task<TFile> SaveFileAsync<TEntity, TFile>(
    int entityId,
    IFormFile file,
    int uploadedById
)
    where TFile : class, IModelFileEntity, new();
    public Task<(Stream FileStream, string ContentType, string FileName)> GetFileAsync<TEntity, TFile>(
        int entityId,
        int fileId)
        where TFile : class, IModelFileEntity;
    public Task DeleteFileAsync<TEntity, TFile>(
        int entityId,
        int fileId)
        where TFile : class, IModelFileEntity;
    Task<bool> FileExistsAsync<TEntity, TFile>(int entityId, int fileId)
    where TFile : class, IModelFileEntity;
}
