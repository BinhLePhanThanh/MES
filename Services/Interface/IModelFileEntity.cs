public interface IModelFileEntity
{
    int Id { get; set; }

    string FileName { get; set; }
    string StoredFileName { get; set; }
    string ContentType { get; set; }
    void SetEntityId(int id);
    void SetFileInfo(string originalName, string storedName, string contentType, long size, int uploadedBy);
}