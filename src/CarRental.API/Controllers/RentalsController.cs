using CarRental.Application.Common.Models;
using CarRental.Application.Rentals.Commands.CancelRental;
using CarRental.Application.Rentals.Commands.ModifyRental;
using CarRental.Application.Rentals.Commands.RegisterRental;
using CarRental.Application.Rentals.Queries.GetRentalById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRental.API.Controllers;

[ApiController]
[Route("api/rentals")]
[Authorize]
public class RentalsController(ISender sender) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(RentalDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<RentalDto>> Register(RegisterRentalCommand command, CancellationToken cancellationToken)
    {
        var rental = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = rental.Id }, rental);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(RentalDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<RentalDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var rental = await sender.Send(new GetRentalByIdQuery(id), cancellationToken);
        return Ok(rental);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(RentalDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<RentalDto>> Modify(int id, ModifyRentalRequest request, CancellationToken cancellationToken)
    {
        var rental = await sender.Send(new ModifyRentalCommand(id, request.StartDate, request.EndDate), cancellationToken);
        return Ok(rental);
    }

    [HttpPost("{id:int}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        await sender.Send(new CancelRentalCommand(id), cancellationToken);
        return NoContent();
    }
}

public record ModifyRentalRequest(DateTime StartDate, DateTime EndDate);
