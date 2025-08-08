using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class MaterialController : ControllerBase
{
    private readonly IMaterialService _service;
    private readonly IMapper _mapper;

    public MaterialController(IMaterialService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }
    [HttpGet] public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id) =>
        await _service.GetByIdAsync(id) is { } m ? Ok(m) : NotFound();
    [HttpPost]
    public async Task<IActionResult> Create(MaterialDto m)
    {
        int processedById = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        m.CreatedById = processedById;
        m.UpdatedById = processedById;
        m.CreatedAt = DateTime.UtcNow;
        m.UpdatedAt = DateTime.UtcNow;
        m.Description ??= string.Empty;
        return Ok(await _service.AddAsync(_mapper.Map<Material>(m)));
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Material m)
    {
        if (id != m.Id) return BadRequest("ID mismatch");
        int processedById = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        m.UpdatedById = processedById;
        m.UpdatedAt = DateTime.UtcNow;
        m.Description ??= string.Empty; // Ensure Description is not null
        // Update the entity
        var result = await _service.UpdateAsync(id, m);
        return result is not null ? Ok(result) : NotFound();
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id) =>
        await _service.DeleteAsync(id) ? Ok() : NotFound();
    [HttpPost("paged")]
    public async Task<IActionResult> GetPaged([FromBody] Dictionary<string, object> filters) =>
        Ok(await _service.GetPagedDynamicAsync(filters));
}