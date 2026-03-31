using Document.Service.Models;
using Document.Service.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Document.Service.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly IMongoCollection<DocumentFile> _documents;

    public DocumentRepository(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _documents = database.GetCollection<DocumentFile>(settings.Value.DocumentsCollection);
    }

    public async Task<List<DocumentFile>> GetAllAsync()
        => await _documents.Find(_ => true).ToListAsync();

    public async Task<DocumentFile?> GetByIdAsync(string id)
        => await _documents.Find(d => d.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(DocumentFile document)
        => await _documents.InsertOneAsync(document);

    public async Task UpdateAsync(string id, DocumentFile document)
        => await _documents.ReplaceOneAsync(d => d.Id == id, document);

    public async Task DeleteAsync(string id)
        => await _documents.DeleteOneAsync(d => d.Id == id);
}
