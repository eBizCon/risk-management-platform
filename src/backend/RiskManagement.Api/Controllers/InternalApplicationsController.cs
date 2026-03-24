using Microsoft.AspNetCore.Mvc;
using RiskManagement.Api.Extensions;
using RiskManagement.Application.Queries;

namespace RiskManagement.Api.Controllers;

[ApiController]
[Route("api/internal/applications")]
public class InternalApplicationsController : ControllerBase
{
    private readonly IDispatcher _dispatcher;

    public InternalApplicationsController(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [HttpGet("exists")]
    public async Task<IActionResult> CheckExists([FromQuery] int customerId)
    {
        var result = await _dispatcher.QueryAsync(new CheckApplicationsExistQuery(customerId));
        return result.ToActionResult();
    }
}