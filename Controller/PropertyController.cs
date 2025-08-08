using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class PropertyController : ControllerBase
{
    private readonly IPropertyService _service;
    private readonly IMapper _mapper;

    public PropertyController(IPropertyService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }
    [HttpGet] public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id) =>
        await _service.GetByIdAsync(id) is { } p ? Ok(p) : NotFound();
    [HttpPost]
    public async Task<IActionResult> Create(PropertyDto p)
    {
        int processedById = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        p.CreatedById = processedById;
        p.UpdatedById = processedById;
        p.CreatedAt = DateTime.UtcNow;
        p.UpdatedAt = DateTime.UtcNow;
        p.Description ??= string.Empty; // Ensure Description is not null
        return Ok(await _service.AddAsync(_mapper.Map<Property>(p)));
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Property p)
    {
        if (id != p.Id) return BadRequest("ID mismatch");
        int processedById = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        p.UpdatedById = processedById;
        p.UpdatedAt = DateTime.UtcNow;
        p.Description ??= string.Empty; // Ensure Description is not null

        // Update the entity
        var result = await _service.UpdateAsync(id, p);
        return result is not null ? Ok(result) : NotFound();
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id) =>
        await _service.DeleteAsync(id) ? Ok() : NotFound();
}