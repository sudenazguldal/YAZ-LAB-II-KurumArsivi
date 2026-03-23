using Search.Service.DTOs;

namespace Search.Service.Services;

public interface ISearchService
{
    Task IndexDocumentAsync(IndexDocumentRequest request);
    Task<List<SearchDocumentDto>> SearchAsync(string query);
    Task<List<SearchDocumentDto>> GetAllAsync();
}