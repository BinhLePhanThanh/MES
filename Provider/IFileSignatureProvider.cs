
public interface IFileSignatureProvider
{
    Dictionary<string, List<byte[]>> GetFileSignatures();
}
