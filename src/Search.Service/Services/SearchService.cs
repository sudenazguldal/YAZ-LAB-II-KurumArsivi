using MongoDB.Driver;
using Search.Service.DTOs;
using Search.Service.Models;
using Search.Service.Settings;
using Microsoft.Extensions.Options;

namespace Search.Service.Services;

internal sealed class SearchService : ISearchService
{
    private readonly IMongoCollection<SearchDocument> _documents;

    public SearchService(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _documents = database.GetCollection<SearchDocument>(settings.Value.CollectionName);
    }

    public async Task IndexDocumentAsync(IndexDocumentRequest request)
    {
        // Aynı documentId varsa güncelle, yoksa ekle
        var existing = await _documents
            .Find(d => d.DocumentId == request.DocumentId)
            .FirstOrDefaultAsync();

        if (existing != null)
        {
            var update = Builders<SearchDocument>.Update
                .Set(d => d.Title, request.Title)
                .Set(d => d.Content, request.Content)
                .Set(d => d.Tags, request.Tags);

            await _documents.UpdateOneAsync(d => d.DocumentId == request.DocumentId, update);
        }
        else
        {
            var doc = new SearchDocument
            {
                DocumentId = request.DocumentId,
                Title = request.Title,
                Content = request.Content,
                Tags = request.Tags,
                CreatedAt = DateTime.UtcNow
            };
            await _documents.InsertOneAsync(doc);
        }
    }

    public async Task<List<SearchDocumentDto>> SearchAsync(string query)
    {
        var filter = Builders<SearchDocument>.Filter.Or(
            Builders<SearchDocument>.Filter.Regex(d => d.Title,
                new MongoDB.Bson.BsonRegularExpression(query, "i")),
            Builders<SearchDocument>.Filter.Regex(d => d.Content,
                new MongoDB.Bson.BsonRegularExpression(query, "i")),
            Builders<SearchDocument>.Filter.AnyEq(d => d.Tags, query)
        );

        var results = await _documents.Find(filter).ToListAsync();
        return results.Select(d => new SearchDocumentDto
        {
            DocumentId = d.DocumentId,
            Title = d.Title,
            Content = d.Content,
            Tags = d.Tags,
            CreatedAt = d.CreatedAt
        }).ToList();
    }

    public async Task<List<SearchDocumentDto>> GetAllAsync()
    {
        var results = await _documents.Find(_ => true).ToListAsync();
        return results.Select(d => new SearchDocumentDto
        {
            DocumentId = d.DocumentId,
            Title = d.Title,
            Content = d.Content,
            Tags = d.Tags,
            CreatedAt = d.CreatedAt
        }).ToList();
    }
}