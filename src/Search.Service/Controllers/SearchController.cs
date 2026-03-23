using Microsoft.AspNetCore.Mvc;
using Search.Service.DTOs;
using Search.Service.Services;

namespace Search.Service.Controllers;

[ApiController]
[Route("api/search")]
public sealed class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;

    public SearchController(ISearchService searchService)
    {
        _searchService = searchService;
    }

    // Document.Service iç ağdan bu endpoint'e POST atar
    [HttpPost("index")]
    public async Task<IActionResult> IndexDocument([FromBody] IndexDocumentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.DocumentId) ||
            string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(new { message = "DocumentId and Title are required" });

        await _searchService.IndexDocumentAsync(request);
        return Ok(new { message = "Document indexed successfully" });
    }

    // Arama
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string? q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new { message = "Query parameter 'q' is required" });

        var results = await _searchService.SearchAsync(q);
        return Ok(results);
    }

    // Tüm dokümanları listele
    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        var results = await _searchService.GetAllAsync();
        return Ok(results);
    }
}