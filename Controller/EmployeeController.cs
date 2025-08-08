using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeService _service;
    private readonly IMapper _mapper;
    private readonly IAuthService _authService;
    public EmployeeController(IEmployeeService service, IMapper mapper, IAuthService authService)
    {
        _service = service;
        _mapper = mapper;
        _authService = authService;
    }
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request);
        if (result == null)
            return Unauthorized(new { message = "Invalid username or password" });

        return Ok(result);
    }

    [HttpGet] public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());
    [Authorize(Roles = "1")]

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id) =>
    await _service.GetByIdAsync(id) is { } e ? Ok(e) : NotFound();
    [AllowAnonymous]
    [HttpPost] public async Task<IActionResult> Create(EmployeeDto e) 
    {
        try
        {
            var addedEmployee = await _service.AddAsync(_mapper.Map<Employee>(e));
            return CreatedAtAction(nameof(Get), new { id = addedEmployee.Id }, addedEmployee);
        }
        catch (InvalidOperationException ex)
        {
            // Trả lỗi 409 Conflict nếu Username đã tồn tại
            return Conflict(new { message = ex.Message });
        }
    }
    [HttpPut("{id}")]
    [Authorize(Roles = "1")]
    public async Task<IActionResult> Update(int id, Employee e)
    {
        if (id != e.Id) return BadRequest("ID mismatch");
        var result = await _service.UpdateAsync(id, e);
        return result is not null ? Ok(result) : NotFound();
    }
    [Authorize(Roles = "1")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id) =>
            await _service.DeleteAsync(id) ? Ok() : NotFound();
    [Authorize(Roles = "1")]

    [HttpPost("paged")]
    public async Task<IActionResult> GetPaged([FromBody] Dictionary<string, object> filters) =>
Ok(await _service.GetPagedDynamicAsync(filters));
}