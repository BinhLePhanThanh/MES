using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class UnitController : ControllerBase
{
    private readonly IUnitService _service;
    private readonly IMapper _mapper;

    public UnitController(IUnitService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }
    [HttpGet] public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id) =>
        await _service.GetByIdAsync(id) is { } u ? Ok(u) : NotFound();
    [HttpPost]
    public async Task<IActionResult> Create(UnitDto u)
    {
        int processedById = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        u.CreatedById = processedById;
        u.UpdatedById = processedById;
        u.CreatedAt = DateTime.UtcNow;
        u.UpdatedAt = DateTime.UtcNow;
        u.Description ??= string.Empty; // Ensure Description is not null
        return Ok(await _service.AddAsync(_mapper.Map<Unit>(u)));
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Unit u)
    {
        int processedById = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        u.UpdatedById = processedById;
        u.UpdatedAt = DateTime.UtcNow;
        u.Description ??= string.Empty;
        return await _service.UpdateAsync(id, u) is { } up ? Ok(up) : NotFound();
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id) =>
        await _service.DeleteAsync(id) ? Ok() : NotFound();
}