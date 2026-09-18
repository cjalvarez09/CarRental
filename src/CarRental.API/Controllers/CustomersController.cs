using CarRental.Application.Common.Models;
using CarRental.Application.Customers.Commands.DeleteCustomer;
using CarRental.Application.Customers.Commands.RegisterCustomer;
using CarRental.Application.Customers.Commands.UpdateCustomer;
using CarRental.Application.Customers.Queries.GetAllCustomers;
using CarRental.Application.Customers.Queries.GetCustomerById;
using CarRental.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRental.API.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
public class CustomersController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = Roles.Employee)]
    [ProducesResponseType(typeof(IReadOnlyList<CustomerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<CustomerDto>>> GetAll(CancellationToken cancellationToken)
    {
        var customers = await sender.Send(new GetAllCustomersQuery(), cancellationToken);
        return Ok(customers);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = Roles.Employee)]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var customer = await sender.Send(new GetCustomerByIdQuery(id), cancellationToken);
        return Ok(customer);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Employee)]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CustomerDto>> Register(RegisterCustomerCommand command, CancellationToken cancellationToken)
    {
        var customer = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Employee)]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerDto>> Update(int id, UpdateCustomerRequest request, CancellationToken cancellationToken)
    {
        var customer = await sender.Send(
            new UpdateCustomerCommand(id, request.FullName, request.Address, request.Email), cancellationToken);
        return Ok(customer);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Employee)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteCustomerCommand(id), cancellationToken);
        return NoContent();
    }
}

public record UpdateCustomerRequest(string FullName, string Address, string Email);
