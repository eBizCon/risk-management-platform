using Microsoft.AspNetCore.Mvc;
using CustomerManagement.Api.Extensions;
using CustomerManagement.Application.Queries;

namespace CustomerManagement.Api.Controllers;

[ApiController]
[Route("api/internal/customers")]
public class InternalCustomersController : ControllerBase
{
    private readonly IDispatcher _dispatcher;

    public InternalCustomersController(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetCustomer(int id)
    {
        var result = await _dispatcher.QueryAsync(new GetCustomerInternalQuery(id));
        return result.ToActionResult();
    }
}
