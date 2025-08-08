using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class TypeController : ControllerBase
{
    private readonly ITypeService _service;
    private readonly IMapper _mapper;

    public TypeController(ITypeService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }
    [HttpGet] public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id) =>
        await _service.GetByIdAsync(id) is { } t ? Ok(t) : NotFound();
    [HttpPost]
    public async Task<IActionResult> Create(TypeDto t)
    {
        int processedById = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        t.CreatedById = processedById;
        t.UpdatedById = processedById;
        t.CreatedAt = DateTime.UtcNow;
        t.UpdatedAt = DateTime.UtcNow;
        t.Description ??= string.Empty; // Ensure Description is not null
        // Map DTO to Entity
        return Ok(await _service.AddAsync(_mapper.Map<Type>(t)));
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Type t)
    {
        int processedById = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        t.UpdatedById = processedById;
        t.UpdatedAt = DateTime.UtcNow;
        t.Description ??= string.Empty; // Ensure Description is not null
        // Update the entity
        return await _service.UpdateAsync(id, t) is { } u ? Ok(u) : NotFound();
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id) =>
        await _service.DeleteAsync(id) ? Ok() : NotFound();
}