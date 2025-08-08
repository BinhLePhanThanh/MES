public interface IFileStorageService
{
    Task<ModelFileResult> SaveFileAsync(string modelKey, IFormFile file);
    bool IsExtensionAllowed(string modelKey, string fileName);
    bool IsFileSizeAcceptable(string modelKey, long fileSize);
    string GetFilePath(string modelKey, string storedFileName);
}
