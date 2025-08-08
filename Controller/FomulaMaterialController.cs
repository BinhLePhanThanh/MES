using AutoMapper;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class FormulaMaterialController : ControllerBase
{
    private readonly IFormulaMaterialService _service;
    private readonly IMapper _mapper;

  public FormulaMaterialController(IFormulaMaterialService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    [HttpGet] public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

    [HttpGet("{formulaId}/{materialId}")]
    public async Task<IActionResult> Get(int formulaId, int materialId) =>
        await _service.GetAsync(formulaId, materialId) is { } fm ? Ok(fm) : NotFound();

    [HttpPost] public async Task<IActionResult> Create(FormulaMaterialDto fm) => Ok(await _service.AddAsync(_mapper.Map<FormulaMaterial>(fm)));
    [HttpPut("{formulaId}/{materialId}")]
    public async Task<IActionResult> Update(int formulaId, int materialId, FormulaMaterialDto fm)
    {
        if (fm == null || fm.FormulaId != formulaId || fm.MaterialId != materialId)
            return BadRequest("Invalid data provided.");

        var updatedFm = await _service.UpdateAsync(formulaId, materialId, _mapper.Map<FormulaMaterial>(fm));
        return updatedFm is not null ? Ok(updatedFm) : NotFound();
    }
    [HttpDelete("{formulaId}/{materialId}")]
    public async Task<IActionResult> Delete(int formulaId, int materialId) =>
        await _service.DeleteAsync(formulaId, materialId) ? Ok() : NotFound();

    [HttpPost("paged")]
    public async Task<IActionResult> GetPaged([FromBody] Dictionary<string, object> filters) =>
        Ok(await _service.GetPagedDynamicAsync(filters));
}