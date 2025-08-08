using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

public class GenericModelFileService : IGenericModelFileService
{
    private readonly IFileStorageService _fileStorage;
    private readonly AppDbContext _db;
    private Dictionary<string, List<byte[]>> _fileSignatures;


    public GenericModelFileService(
        IFileSignatureProvider fileSignatureProvider,
        IFileStorageService fileStorage,
        AppDbContext db)
    {
        _fileSignatures = fileSignatureProvider.GetFileSignatures();
        _fileStorage = fileStorage;
        _db = db;
    }

    public async Task<TFile> SaveFileAsync<TEntity, TFile>(
    int entityId,
    IFormFile file,
    int uploadedById)
    where TFile : class, IModelFileEntity, new()
    {
        var ext = Path.GetExtension(file.FileName).TrimStart('.').ToLowerInvariant();

        if (_fileSignatures.TryGetValue(ext, out var expectedSignatures))
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var fileBytes = ms.ToArray();

            var match = expectedSignatures.Any(sig =>
                fileBytes.Length >= sig.Length &&
                fileBytes.Take(sig.Length).SequenceEqual(sig)
            );

            if (!match)
            {
                throw new InvalidDataException($"File {file.FileName} không đúng định dạng {ext} theo signature.");
            }

            ms.Position = 0;
            file = new FormFile(ms, 0, ms.Length, file.Name, file.FileName)
            {
                Headers = file.Headers,
                ContentType = file.ContentType
            };
        }

        var modelKey = typeof(TEntity).Name;
        var result = await _fileStorage.SaveFileAsync(modelKey, file);

        var fileEntity = new TFile();
        fileEntity.SetEntityId(entityId);
        fileEntity.SetFileInfo(file.FileName, result.StoredFileName, result.ContentType, result.Size, uploadedById);

        _db.Add(fileEntity);
        await _db.SaveChangesAsync();

        return fileEntity;
    }

    public async Task<(Stream FileStream, string ContentType, string FileName)> GetFileAsync<TEntity, TFile>(
        int entityId,
        int fileId)
        where TFile : class, IModelFileEntity
    {
        var modelKey = typeof(TEntity).Name;

        var fileEntity = await _db.Set<TFile>()
            .FirstOrDefaultAsync(f =>
                EF.Property<int?>(f, modelKey + "Id") == entityId &&
                EF.Property<int>(f, "Id") == fileId);

        if (fileEntity == null)
            throw new FileNotFoundException("File not found in database.");

        var filePath = _fileStorage.GetFilePath(modelKey, fileEntity.StoredFileName);
        if (!System.IO.File.Exists(filePath))
            throw new FileNotFoundException("File not found on disk.");

        var stream = System.IO.File.OpenRead(filePath);
        return (stream, fileEntity.ContentType, fileEntity.FileName);
    }

    public async Task DeleteFileAsync<TEntity, TFile>(
        int entityId,
        int fileId)
        where TFile : class, IModelFileEntity
    {
        var modelKey = typeof(TEntity).Name;

        var fileEntity = await _db.Set<TFile>()
            .FirstOrDefaultAsync(f =>
                EF.Property<int?>(f, modelKey + "Id") == entityId &&
                EF.Property<int>(f, "Id") == fileId);

        if (fileEntity == null)
            throw new FileNotFoundException("File not found.");

        var filePath = _fileStorage.GetFilePath(modelKey, fileEntity.StoredFileName);
        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
        }

        _db.Remove(fileEntity);
        await _db.SaveChangesAsync();
    }
    public async Task<bool> FileExistsAsync<TEntity, TFile>(
        int entityId,
        int fileId)
        where TFile : class, IModelFileEntity
    {
        var modelKey = typeof(TEntity).Name;

        return await _db.Set<TFile>().AnyAsync(f =>
            EF.Property<int?>(f, modelKey + "Id") == entityId &&
            EF.Property<int>(f, "Id") == fileId);
    }


}
