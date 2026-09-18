using CarRental.Application;
using CarRental.Application.Cars.Commands.CreateCar;
using CarRental.Application.Cars.Commands.DeleteCar;
using CarRental.Application.Common.Interfaces;
using CarRental.Domain.Entities;
using CarRental.Domain.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CarRental.UnitTests.Application.Behaviours;

// Exercises the behaviors through the real MediatR pipeline (not by calling them directly), which is
// what caught void commands (IRequest) silently skipping every behavior.
public class MediatRPipelineTests
{
    private readonly ICacheService _cacheService = Substitute.For<ICacheService>();
    private readonly ICarRepository _carRepository = Substitute.For<ICarRepository>();
    private readonly ISender _sender;

    public MediatRPipelineTests()
    {
        _carRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(new Car { Id = 1, Type = "Sedan", Model = "Corolla" });

        var services = new ServiceCollection();
        services.AddApplication();
        services.AddSingleton(_cacheService);
        services.AddSingleton(_carRepository);
        services.AddSingleton(Substitute.For<IRentalRepository>());
        services.AddSingleton(Substitute.For<IUnitOfWork>());
        _sender = services.BuildServiceProvider().GetRequiredService<ISender>();
    }

    [Fact]
    public async Task Given_ACommandReturningAResponse_When_Sent_Then_ItsCacheTagsAreInvalidated()
    {
        // Given
        var command = new CreateCarCommand("SUV", "RAV4");

        // When
        await _sender.Send(command);

        // Then
        _cacheService.Received(1).Invalidate("cars");
    }

    [Fact]
    public async Task Given_AVoidCommandDeclaringCacheTags_When_Sent_Then_ItsCacheTagsAreInvalidated()
    {
        // Given
        var command = new DeleteCarCommand(1);

        // When
        await _sender.Send(command);

        // Then
        _cacheService.Received(1).Invalidate("cars");
    }

    [Fact]
    public async Task Given_AVoidCommandWithInvalidData_When_Sent_Then_ItIsRejectedByValidation()
    {
        // Given
        var command = new DeleteCarCommand(0);

        // When
        Func<Task> act = () => _sender.Send(command);

        // Then
        await Assert.ThrowsAsync<ValidationException>(act);
        _cacheService.DidNotReceive().Invalidate(Arg.Any<string>());
    }
}
