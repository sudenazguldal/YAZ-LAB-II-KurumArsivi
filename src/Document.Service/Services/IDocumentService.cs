using Document.Service.DTOs;
using Document.Service.Models;

namespace Document.Service.Services;

public interface IDocumentService
{
    Task<List<DocumentFile>> GetAllAsync();
    Task<DocumentFile?> GetByIdAsync(string id);
    Task CreateAsync(CreateDocumentDto dto);
    Task UpdateAsync(string id, CreateDocumentDto dto);
    Task DeleteAsync(string id);
}
