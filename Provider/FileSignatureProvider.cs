using Microsoft.Extensions.Options;

public class FileSignatureProvider : IFileSignatureProvider
{
    private readonly IOptions<FileStorageSettings> _settings;

    public FileSignatureProvider(IOptions<FileStorageSettings> settings)
    {
        _settings = settings;
    }

    public Dictionary<string, List<byte[]>> GetFileSignatures()
    {
        return _settings.Value.FileSignatures.ToDictionary(
            kv => kv.Key.ToLowerInvariant(),
            kv => kv.Value.Select(signatureList => 
                signatureList.Select(h => Convert.ToByte(h, 16)).ToArray()).ToList()
        );
    }
}