namespace Document.Service.DTOs;

public class CreateDocumentDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string UploadedBy { get; set; } = string.Empty;
}