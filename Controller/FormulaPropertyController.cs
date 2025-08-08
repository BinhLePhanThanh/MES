using AutoMapper;
using Microsoft.AspNetCore.Mvc;
[ApiController]
[Route("api/[controller]")]
public class FormulaPropertyController : ControllerBase
{
    private readonly IFormulaPropertyService _service;
    private readonly IMapper _mapper;

    public FormulaPropertyController(IFormulaPropertyService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }
    [HttpGet] public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());
    [HttpGet("{formulaId}/{propertyId}")]
    public async Task<IActionResult> Get(int formulaId, int propertyId) =>
        await _service.GetAsync(formulaId, propertyId) is { } fp ? Ok(fp) : NotFound();

    [HttpPost] public async Task<IActionResult> Create(FormulaPropertyDto fp) => Ok(await _service.AddAsync(_mapper.Map<FormulaProperty>(fp)));
    [HttpPut("{formulaId}/{propertyId}")]
    public async Task<IActionResult> Update(int formulaId, int propertyId, FormulaPropertyDto fp)
    {
        if (fp == null || fp.FormulaId != formulaId || fp.PropertyId != propertyId)
            return BadRequest("Invalid data provided.");

        var updatedFp = await _service.UpdateAsync(formulaId,propertyId,_mapper.Map<FormulaProperty>(fp));
        return updatedFp is not null ? Ok(updatedFp) : NotFound();
    }
    [HttpDelete("{formulaId}/{propertyId}")]
    public async Task<IActionResult> Delete(int formulaId, int propertyId) =>
        await _service.DeleteAsync(formulaId, propertyId) ? Ok() : NotFound();
    [HttpPost("paged")]
    public async Task<IActionResult> GetPaged([FromBody] Dictionary<string, object> filters) =>
        Ok(await _service.GetPagedDynamicAsync(filters));
}