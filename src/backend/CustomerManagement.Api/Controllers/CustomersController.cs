using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CustomerManagement.Api.Extensions;
using CustomerManagement.Api.Models;
using CustomerManagement.Application.Commands;
using CustomerManagement.Application.DTOs;
using CustomerManagement.Application.Queries;

namespace CustomerManagement.Api.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize(Policy = AuthPolicies.Applicant)]
public class CustomersController : ControllerBase
{
    private readonly IDispatcher _dispatcher;

    public CustomersController(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [HttpGet]
    public async Task<IActionResult> GetCustomers()
    {
        var result = await _dispatcher.QueryAsync(new GetCustomersByUserQuery(User.GetEmail()));
        return result.ToActionResult();
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActiveCustomers()
    {
        var result = await _dispatcher.QueryAsync(new GetActiveCustomersByUserQuery(User.GetEmail()));
        return result.ToActionResult();
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetCustomer(int id)
    {
        var result = await _dispatcher.QueryAsync(new GetCustomerQuery(id, User.GetEmail()));
        return result.ToActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> CreateCustomer([FromBody] CustomerCreateDto dto)
    {
        var result = await _dispatcher.SendAsync(new CreateCustomerCommand(dto, User.GetEmail()));
        return result.ToActionResult();
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCustomer(int id, [FromBody] CustomerUpdateDto dto)
    {
        var result = await _dispatcher.SendAsync(new UpdateCustomerCommand(id, dto, User.GetEmail()));
        return result.ToActionResult();
    }

    [HttpPost("{id:int}/archive")]
    public async Task<IActionResult> ArchiveCustomer(int id)
    {
        var result = await _dispatcher.SendAsync(new ArchiveCustomerCommand(id, User.GetEmail()));
        return result.ToActionResult();
    }

    [HttpPost("{id:int}/activate")]
    public async Task<IActionResult> ActivateCustomer(int id)
    {
        var result = await _dispatcher.SendAsync(new ActivateCustomerCommand(id, User.GetEmail()));
        return result.ToActionResult();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        var result = await _dispatcher.SendAsync(new DeleteCustomerCommand(id, User.GetEmail()));
        if (!result.IsSuccess)
            return result.ToActionResult();

        return Ok(new { success = true });
    }
}