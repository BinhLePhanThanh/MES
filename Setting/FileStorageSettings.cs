public class FileStorageSettings
{
    public Dictionary<string, ModelFolder> Models { get; set; } = new();
    public Dictionary<string, List<List<string>>> FileSignatures { get; set; } = new();
}
public class ModelFolder
    {
        public string RelativePath { get; set; } = string.Empty;  // ví dụ: "formulas", "products"
        public int MaxFileSizeMB { get; set; } = 50;
        public string[] AllowedExtensions { get; set; } = Array.Empty<string>();
    }
public class ModelFileResult
{
    public string StoredFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/octet-stream";
    public long Size { get; set; }
    public string FullPath { get; set; } = string.Empty;
}