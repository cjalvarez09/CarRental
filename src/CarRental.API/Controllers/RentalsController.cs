using CarRental.API.Requests;
using CarRental.Application.Common.Models;
using CarRental.Application.Rentals.Commands.CancelRental;
using CarRental.Application.Rentals.Commands.ModifyRental;
using CarRental.Application.Rentals.Commands.RegisterRental;
using CarRental.Application.Rentals.Queries.GetRentalById;
using CarRental.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRental.API.Controllers;

[ApiController]
[Route("api/rentals")]
[Authorize]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
public class RentalsController(ISender sender) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(RentalDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RentalDto>> Register(RegisterRentalCommand command, CancellationToken cancellationToken)
    {
        var rental = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = rental.Id }, rental);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = Roles.Employee)]
    [ProducesResponseType(typeof(RentalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RentalDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var rental = await sender.Send(new GetRentalByIdQuery(id), cancellationToken);
        return Ok(rental);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Employee)]
    [ProducesResponseType(typeof(RentalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RentalDto>> Modify(int id, ModifyRentalRequest request, CancellationToken cancellationToken)
    {
        var rental = await sender.Send(new ModifyRentalCommand(id, request.StartDate, request.EndDate), cancellationToken);
        return Ok(rental);
    }

    [HttpPost("{id:int}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        await sender.Send(new CancelRentalCommand(id), cancellationToken);
        return NoContent();
    }
}
