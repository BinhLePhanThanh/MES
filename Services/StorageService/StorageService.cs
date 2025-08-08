using Microsoft.Extensions.Options;

public class FileStorageService : IFileStorageService
{
    private readonly FileStorageSettings _settings;
    private readonly string _rootPath;

    public FileStorageService(IOptions<FileStorageSettings> options, IWebHostEnvironment env)
    {
        _settings = options.Value;
        _rootPath = env.ContentRootPath;
    }

    public async Task<ModelFileResult> SaveFileAsync(string modelKey, IFormFile file)
    {
        if (!_settings.Models.TryGetValue(modelKey, out var config))
            throw new InvalidOperationException($"No config for model: {modelKey}");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!config.AllowedExtensions.Contains(ext) && !config.AllowedExtensions.Contains("*"))
            throw new InvalidOperationException($"Extension {ext} is not allowed.");

        if (file.Length > config.MaxFileSizeMB * 1024 * 1024)
            throw new InvalidOperationException("File size exceeds limit.");

        var storedFileName = $"{Guid.NewGuid()}{ext}";

        // ✅ Thêm modelKey vào thư mục lưu file
        var dir = Path.Combine(_rootPath, config.RelativePath, modelKey);
        Directory.CreateDirectory(dir);

        var fullPath = Path.Combine(dir, storedFileName);

        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return new ModelFileResult
        {
            StoredFileName = storedFileName,
            ContentType = file.ContentType ?? "application/octet-stream",
            Size = file.Length,
            FullPath = fullPath
        };
    }

    public bool IsExtensionAllowed(string modelKey, string fileName)
    {
        if (!_settings.Models.TryGetValue(modelKey, out var config)) return false;

        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return config.AllowedExtensions.Contains(ext) || config.AllowedExtensions.Contains("*");
    }

    public bool IsFileSizeAcceptable(string modelKey, long fileSize)
    {
        if (!_settings.Models.TryGetValue(modelKey, out var config)) return false;

        return fileSize <= config.MaxFileSizeMB * 1024 * 1024;
    }

    public string GetFilePath(string modelKey, string storedFileName)
    {
        if (!_settings.Models.TryGetValue(modelKey, out var config))
            throw new InvalidOperationException($"No config for model: {modelKey}");

        // ✅ Trùng path với lúc lưu
        return Path.Combine(_rootPath, config.RelativePath, modelKey, storedFileName);
    }
}
