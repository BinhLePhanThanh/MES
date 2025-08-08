using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class FormulaController : ControllerBase
{
    private readonly IFormulaService _service;
    private readonly IMapper _mapper;
    private readonly IFormulaLoggingService _loggingService;
    private readonly IGenericModelFileService _fileService;


    public FormulaController(IFormulaService service, IMapper mapper, IFormulaLoggingService loggingService,
        IGenericModelFileService fileService)
    {
        _service = service;
        _mapper = mapper;
        _loggingService = loggingService;
        _fileService = fileService;
    }

    [HttpPost("logs/{formulaId}")]
    public async Task<IActionResult> GetLogs(int formulaId, [FromBody] Dictionary<string, object> filters)
    {
        if (formulaId <= 0) return BadRequest("Invalid formula ID");
        filters["FormulaId"] = formulaId; // Ensure the filter includes the formula ID
        var logs = await _loggingService.GetPagedDynamicAsync(filters);
        return Ok(logs);
    }
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id) =>
        await _service.GetByIdAsync(id) is { } f ? Ok(f) : NotFound();

    [HttpPost]
    public async Task<IActionResult> Create(FormulaDto f) =>
        Ok(await _service.AddAsync(_mapper.Map<Formula>(f)));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Formula f)
    {
        int processedById = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        if (id != f.Id) return BadRequest("ID mismatch");
        var result = await _service.UpdateAsync(id, f, processedById);
        return result is not null ? Ok(result) : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        int processedById = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        return await _service.DeleteAsync(id, processedById) ? Ok() : NotFound();
    }
    [HttpPost("paged")]
    public async Task<IActionResult> GetPaged([FromBody] Dictionary<string, object> filters) =>
        Ok(await _service.GetPagedDynamicAsync(filters));

    // ======= NEW ACTIONS FOR STATUS =======

    /// <summary>
    /// Gửi công thức để chờ duyệt
    /// </summary>
    [HttpPost("{id}/submit")]
    public async Task<IActionResult> Submit(int id, [FromBody] string message = "Submitted by user")
    {
        int processedById = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        if (id <= 0) return BadRequest("Invalid ID");
        var formula = await _service.SubmitAsync(id, processedById, message);
        return Ok(formula);
    }

    /// <summary>
    /// Hủy gửi công thức (trở về trạng thái nháp)
    /// </summary>
    [HttpPost("{id}/unsubmit")]
    public async Task<IActionResult> Unsubmit(int id, [FromBody] string message = "Unsubmitted by user")
    {
        int processedById = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        if (id <= 0) return BadRequest("Invalid ID");
        var formula = await _service.UnsubmitAsync(id, processedById, message);
        return Ok(formula);
    }

    /// <summary>
    /// Duyệt công thức
    /// </summary>
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve([FromRoute] int id, [FromQuery] int processedById, [FromBody] string message = "Approved by user")
    {
        var formula = await _service.ApproveAsync(id, processedById, message);
        return Ok(formula);
    }

    /// <summary>
    /// Từ chối công thức
    /// </summary>
    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject([FromRoute] int id, [FromQuery] int processedById, [FromBody] string message = "Rejected by user")
    {
        var formula = await _service.RejectAsync(id, processedById, message);
        return Ok(formula);
    }
    //[Authorize(Roles = "1")]
    [HttpPost("{id}/disapprove")]
    public async Task<IActionResult> Disapprove([FromRoute] int id, [FromQuery] int processedById, [FromBody] string message = "Disapproved by user")
    {
        var formula = await _service.DisapproveAsync(id, processedById, message);
        return Ok(formula);
    }

    [HttpPost("{formulaId}/uploadFiles")]
    public async Task<IActionResult> UploadFormulaFile(
    int formulaId,
    IFormFile file)
    {
        var uploadedById = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        if (uploadedById <= 0) return BadRequest("Invalid ID");

        var saved = await _fileService.SaveFileAsync<Formula, FormulaFile>(
            entityId: formulaId,
            file: file,
            uploadedById: uploadedById);

        return Ok(new
        {
            saved.Id,
            saved.FileName,
            saved.StoredFileName,
            saved.ContentType,
            saved.Size,
            saved.UploadedAt,
            saved.UploadedById
        });
    }
    [HttpGet("download/{fileId}")]
    public async Task<IActionResult> Download(int formulaId, int fileId)
    {
        var (stream, contentType, fileName) =
            await _fileService.GetFileAsync<Formula, FormulaFile>(formulaId, fileId);

        return File(stream, contentType, fileName);
    }

    // DELETE: api/formulas/5/files/10
    [HttpDelete("delete-file/{fileId}")]
    public async Task<IActionResult> Delete(int formulaId, int fileId)
    {
        await _fileService.DeleteFileAsync<Formula, FormulaFile>(formulaId, fileId);
        return NoContent();
    }
    [HttpGet("preview/{fileId}")]
    public async Task<IActionResult> Preview(int formulaId, int fileId)
    {
        var (stream, contentType, fileName) =
            await _fileService.GetFileAsync<Formula, FormulaFile>(formulaId, fileId);

        var safeFileName = Uri.EscapeDataString(fileName);
        Response.Headers["Content-Disposition"] = $"attachment; filename*=UTF-8''{safeFileName}";

        return File(stream, contentType);
    }



}
