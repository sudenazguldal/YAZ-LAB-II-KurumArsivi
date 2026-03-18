using Document.Service.Models;

namespace Document.Service.Repositories;

public interface IDocumentRepository
{
    Task<List<DocumentFile>> GetAllAsync();
    Task<DocumentFile?> GetByIdAsync(string id);
    Task CreateAsync(DocumentFile document);
    Task UpdateAsync(string id, DocumentFile document);
    Task DeleteAsync(string id);
}