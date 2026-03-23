using Document.Service.DTOs;
using Document.Service.Models;
using Document.Service.Repositories;

namespace Document.Service.Services;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _repository;
    private readonly HttpClient _httpClient;

    public DocumentService(IDocumentRepository repository, IHttpClientFactory httpClientFactory)
    {
        _repository = repository;
        _httpClient = httpClientFactory.CreateClient("SearchService");
    }

    public async Task<List<DocumentFile>> GetAllAsync()
        => await _repository.GetAllAsync();

    public async Task<DocumentFile?> GetByIdAsync(string id)
        => await _repository.GetByIdAsync(id);

    public async Task CreateAsync(CreateDocumentDto dto)
    {
        var document = new DocumentFile
        {
            Title = dto.Title,
            Content = dto.Content,
            Category = dto.Category,
            UploadedBy = dto.UploadedBy,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.CreateAsync(document);
        await NotifySearchService(document);
    }

    public async Task UpdateAsync(string id, CreateDocumentDto dto)
    {
        var document = new DocumentFile
        {
            Id = id,
            Title = dto.Title,
            Content = dto.Content,
            Category = dto.Category,
            UploadedBy = dto.UploadedBy,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.UpdateAsync(id, document);
        await NotifySearchService(document);
    }

    public async Task DeleteAsync(string id)
        => await _repository.DeleteAsync(id);

    private async Task NotifySearchService(DocumentFile document)
    {
        try
        {
            var indexRequest = new
            {
                documentId = document.Id,
                title = document.Title,
                content = document.Content,
                tags = new[] { document.Category }
            };

            await _httpClient.PostAsJsonAsync("/api/search/index", indexRequest);
        }
        catch
        {
            // Search Service çevrimdışı olsa bile belge kaydedilsin
        }
    }
}
