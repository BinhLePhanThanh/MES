using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class RoleController : ControllerBase
{
    private readonly IRoleService _service;
    private readonly IMapper _mapper;

    public RoleController(IRoleService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id) =>
        await _service.GetByIdAsync(id) is { } role ? Ok(role) : NotFound();

    [HttpPost]
    public async Task<IActionResult> Create(RoleDto roleDto)
    {
        int processedById = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        roleDto.CreatedById = processedById;
        roleDto.UpdatedById = processedById;
        roleDto.CreatedAt = DateTime.UtcNow;
        roleDto.UpdatedAt = DateTime.UtcNow;
        roleDto.Description ??= string.Empty;
        return Ok(await _service.AddAsync(_mapper.Map<Role>(roleDto)));

    }
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Role role)
    {
        if (id != role.Id) return BadRequest("ID mismatch");
        int processedById = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        role.UpdatedById = processedById;
        role.UpdatedAt = DateTime.UtcNow;
        role.Description ??= string.Empty; // Ensure Description is not null
        var result = await _service.UpdateAsync(id, role);
        return result is not null ? Ok(result) : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id) =>
        await _service.DeleteAsync(id) ? Ok() : NotFound();
}