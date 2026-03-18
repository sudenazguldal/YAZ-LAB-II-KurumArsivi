using Document.Service.DTOs;
using Document.Service.Services;
using Microsoft.AspNetCore.Mvc;

namespace Document.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentController : ControllerBase
{
    private readonly IDocumentService _service;

    public DocumentController(IDocumentService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var documents = await _service.GetAllAsync();
        return Ok(documents);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var document = await _service.GetByIdAsync(id);
        if (document is null) return NotFound();
        return Ok(document);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateDocumentDto dto)
    {
        await _service.CreateAsync(dto);
        return Created();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, CreateDocumentDto dto)
    {
        await _service.UpdateAsync(id, dto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
