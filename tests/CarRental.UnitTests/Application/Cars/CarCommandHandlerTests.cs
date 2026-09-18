using CarRental.Application.Cars.Commands.CreateCar;
using CarRental.Application.Cars.Commands.DeleteCar;
using CarRental.Application.Cars.Commands.UpdateCar;
using CarRental.Application.Cars.Queries.CheckAvailability;
using CarRental.Application.Cars.Queries.GetAllCars;
using CarRental.Application.Cars.Queries.GetCarById;
using CarRental.Application.Common.Exceptions;
using CarRental.Application.Common.Interfaces;
using CarRental.Domain.Entities;
using CarRental.Domain.Exceptions;
using CarRental.Domain.Repositories;

namespace CarRental.UnitTests.Application.Cars;

public class CarCommandHandlerTests
{
    private readonly ICarRepository _carRepository = Substitute.For<ICarRepository>();
    private readonly IRentalRepository _rentalRepository = Substitute.For<IRentalRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    public class Create : CarCommandHandlerTests
    {
        [Fact]
        public async Task Given_ANewCar_When_Creating_Then_AddsItAndSaves()
        {
            // Given
            var handler = new CreateCarCommandHandler(_carRepository, _unitOfWork);

            // When
            var result = await handler.Handle(new CreateCarCommand("SUV", "RAV4"), CancellationToken.None);

            // Then
            await _carRepository.Received(1).AddAsync(
                Arg.Is<Car>(car => car.Type == "SUV" && car.Model == "RAV4"),
                Arg.Any<CancellationToken>());
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
            Assert.Equal("SUV", result.Type);
            Assert.Equal("RAV4", result.Model);
        }
    }

    public class Update : CarCommandHandlerTests
    {
        [Fact]
        public async Task Given_AnUnknownCar_When_Updating_Then_ThrowsNotFoundException()
        {
            // Given
            _carRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((Car?)null);
            var handler = new UpdateCarCommandHandler(_carRepository, _unitOfWork);

            // When
            Func<Task> act = () => handler.Handle(new UpdateCarCommand(1, "SUV", "RAV4"), CancellationToken.None);

            // Then
            await Assert.ThrowsAsync<NotFoundException>(act);
        }

        [Fact]
        public async Task Given_AnExistingCar_When_Updating_Then_UpdatesItAndSaves()
        {
            // Given
            var car = new Car { Id = 1, Type = "Sedan", Model = "Corolla" };
            _carRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(car);
            var handler = new UpdateCarCommandHandler(_carRepository, _unitOfWork);

            // When
            var result = await handler.Handle(new UpdateCarCommand(1, "SUV", "RAV4"), CancellationToken.None);

            // Then
            Assert.Equal("SUV", car.Type);
            Assert.Equal("RAV4", car.Model);
            Assert.Equal("RAV4", result.Model);
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }

    public class Delete : CarCommandHandlerTests
    {
        private DeleteCarCommandHandler CreateHandler() => new(_carRepository, _rentalRepository, _unitOfWork);

        [Fact]
        public async Task Given_AnUnknownCar_When_Deleting_Then_ThrowsNotFoundException()
        {
            // Given
            _carRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((Car?)null);

            // When
            Func<Task> act = () => CreateHandler().Handle(new DeleteCarCommand(1), CancellationToken.None);

            // Then
            await Assert.ThrowsAsync<NotFoundException>(act);
        }

        [Fact]
        public async Task Given_ACarWithRentals_When_Deleting_Then_ThrowsCarInUseExceptionAndDoesNotDelete()
        {
            // Given
            var car = new Car { Id = 1, Type = "Sedan", Model = "Corolla" };
            _carRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(car);
            _rentalRepository.ExistsForCarAsync(1, Arg.Any<CancellationToken>()).Returns(true);

            // When
            Func<Task> act = () => CreateHandler().Handle(new DeleteCarCommand(1), CancellationToken.None);

            // Then
            await Assert.ThrowsAsync<CarInUseException>(act);
            _carRepository.DidNotReceive().Remove(Arg.Any<Car>());
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Given_ACarWithoutRentals_When_Deleting_Then_RemovesItAndSaves()
        {
            // Given
            var car = new Car { Id = 1, Type = "Sedan", Model = "Corolla" };
            _carRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(car);
            _rentalRepository.ExistsForCarAsync(1, Arg.Any<CancellationToken>()).Returns(false);

            // When
            await CreateHandler().Handle(new DeleteCarCommand(1), CancellationToken.None);

            // Then
            _carRepository.Received(1).Remove(car);
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }

    public class Queries : CarCommandHandlerTests
    {
        [Fact]
        public async Task Given_AnUnknownCar_When_GettingById_Then_ThrowsNotFoundException()
        {
            // Given
            _carRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((Car?)null);
            var handler = new GetCarByIdQueryHandler(_carRepository);

            // When
            Func<Task> act = () => handler.Handle(new GetCarByIdQuery(1), CancellationToken.None);

            // Then
            await Assert.ThrowsAsync<NotFoundException>(act);
        }

        [Fact]
        public async Task Given_AnExistingCar_When_GettingById_Then_ReturnsItsDto()
        {
            // Given
            _carRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
                .Returns(new Car { Id = 1, Type = "Sedan", Model = "Corolla" });
            var handler = new GetCarByIdQueryHandler(_carRepository);

            // When
            var result = await handler.Handle(new GetCarByIdQuery(1), CancellationToken.None);

            // Then
            Assert.Equal(1, result.Id);
            Assert.Equal("Sedan", result.Type);
        }

        [Fact]
        public async Task Given_SeveralCars_When_GettingAll_Then_ReturnsEachOneAsDto()
        {
            // Given
            _carRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(
            [
                new Car { Id = 1, Type = "Sedan", Model = "Corolla" },
                new Car { Id = 2, Type = "SUV", Model = "RAV4" }
            ]);
            var handler = new GetAllCarsQueryHandler(_carRepository);

            // When
            var result = await handler.Handle(new GetAllCarsQuery(), CancellationToken.None);

            // Then
            Assert.Equal([1, 2], result.Select(car => car.Id));
        }

        [Fact]
        public async Task Given_AvailableCars_When_CheckingAvailability_Then_ReturnsThemForTheGivenFilter()
        {
            // Given
            var start = new DateTime(2030, 10, 1);
            var end = new DateTime(2030, 10, 5);
            _carRepository.GetAvailableAsync("Sedan", "Corolla", start, end, Arg.Any<CancellationToken>())
                .Returns([new Car { Id = 3, Type = "Sedan", Model = "Corolla" }]);
            var handler = new CheckAvailabilityQueryHandler(_carRepository);

            // When
            var result = await handler.Handle(
                new CheckAvailabilityQuery("Sedan", "Corolla", start, end), CancellationToken.None);

            // Then
            var car = Assert.Single(result);
            Assert.Equal(3, car.Id);
        }
    }
}
