namespace Search.Service.DTOs;

public sealed class IndexDocumentRequest
{
    public string DocumentId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public List<string> Tags { get; init; } = new();
}