using CarRental.Application.Cars.Commands.CreateCar;
using CarRental.Application.Cars.Commands.DeleteCar;
using CarRental.Application.Cars.Commands.UpdateCar;
using CarRental.Application.Cars.Queries.CheckAvailability;
using CarRental.Application.Cars.Queries.GetAllCars;
using CarRental.Application.Cars.Queries.GetCarById;
using CarRental.Application.Common.Models;
using CarRental.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRental.API.Controllers;

[ApiController]
[Route("api/cars")]
[Authorize]
public class CarsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = Roles.Employee)]
    [ProducesResponseType(typeof(IReadOnlyList<CarDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CarDto>>> GetAll(CancellationToken cancellationToken)
    {
        var cars = await sender.Send(new GetAllCarsQuery(), cancellationToken);
        return Ok(cars);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = Roles.Employee)]
    [ProducesResponseType(typeof(CarDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CarDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var car = await sender.Send(new GetCarByIdQuery(id), cancellationToken);
        return Ok(car);
    }

    [HttpGet("availability")]
    [ProducesResponseType(typeof(IReadOnlyList<CarDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CarDto>>> CheckAvailability(
        [FromQuery] string type,
        [FromQuery] string? model,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken)
    {
        var cars = await sender.Send(new CheckAvailabilityQuery(type, model, startDate, endDate), cancellationToken);
        return Ok(cars);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Employee)]
    [ProducesResponseType(typeof(CarDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<CarDto>> Create(CreateCarCommand command, CancellationToken cancellationToken)
    {
        var car = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = car.Id }, car);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Employee)]
    [ProducesResponseType(typeof(CarDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CarDto>> Update(int id, UpdateCarRequest request, CancellationToken cancellationToken)
    {
        var car = await sender.Send(new UpdateCarCommand(id, request.Type, request.Model), cancellationToken);
        return Ok(car);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Employee)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteCarCommand(id), cancellationToken);
        return NoContent();
    }
}

public record UpdateCarRequest(string Type, string Model);
