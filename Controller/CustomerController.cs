using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _service;

    public CustomerController(ICustomerService service)
    {
        _service = service;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Customer>> GetCustomerById(int id)
    {
        var customer = await _service.GetCustomerByIdAsync(id);
        if (customer == null) return NotFound();
        return Ok(customer);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Customer>>> GetAllCustomers()
    {
        var customers = await _service.GetAllCustomersAsync();
        return Ok(customers);
    }

    [HttpPost]
    public async Task<ActionResult<Customer>> CreateCustomer([FromBody] Customer customer)
    {
        var created = await _service.CreateCustomerAsync(customer);
        return CreatedAtAction(nameof(GetCustomerById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Customer>> UpdateCustomer(int id, [FromBody] Customer customer)
    {
        var updated = await _service.UpdateCustomerAsync(id, customer);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        var success = await _service.DeleteCustomerAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }

    [HttpPost("paged")]
    public async Task<ActionResult<PagedResult<Customer>>> GetPaged([FromBody] Dictionary<string, object> filters)
    {
        var result = await _service.GetPagedDynamicAsync(filters);
        return Ok(result);
    }
}
