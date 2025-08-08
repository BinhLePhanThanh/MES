using System.ComponentModel.DataAnnotations.Schema;

public class FormulaFile : IModelFileEntity
{
    public int Id { get; set; }
    [ForeignKey(nameof(Formula))]
    public int? FormulaId { get; set; }
    public Formula? Formula { get; set; } = null!;
    public string FileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime UploadedAt { get; set; }
    
    [ForeignKey(nameof(UploadedBy))]
    public int? UploadedById { get; set; }
    public Employee? UploadedBy { get; set; } = null!;

    public void SetEntityId(int id) => FormulaId = id;

    public void SetFileInfo(string original, string stored, string content, long size, int by)
    {
        FileName = original;
        StoredFileName = stored;
        ContentType = content;
        Size = size;
        UploadedById = by;
        UploadedAt = DateTime.UtcNow;
    }
}
