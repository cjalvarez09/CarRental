# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project status

This is an early-stage skeleton for a car rental API. Most of the structure exists as empty scaffolding: `CarRental.Application` and `CarRental.Infrastructure` have no source files yet, `CarRental.API` still contains the default ASP.NET template (`WeatherForecastController`), and there are no test files or cross-project references configured in any `.csproj`. Expect to be setting up real wiring (project references, DI, persistence) as part of most tasks here, not just adding to an established pattern.

## Solution layout

- `CarRental.slnx` — the solution file (new XML-based `.slnx` format, not `.sln`). Contains:
  - `src/CarRental.Application` — CQRS commands/queries (MediatR), validators (FluentValidation), and application-layer abstractions
  - `src/CarRental.API` — ASP.NET Core Web API (`Microsoft.NET.Sdk.Web`), Swashbuckle + `Microsoft.AspNetCore.OpenApi` for Swagger/OpenAPI
  - `src/CarRental.Domain` — plain class library for domain entities
  - `src/CarRental.Infrastructure` — plain class library, currently empty
  - `tests/CarRental.UnitTests` and `tests/CarRental.IntegrationTests` — xUnit projects (xunit, xunit.runner.visualstudio, coverlet.collector), currently with no test files and no reference to the source projects
- All projects target `net10.0` with `Nullable` and `ImplicitUsings` enabled.

## Commands

Build and run from the repository root using the `.slnx` solution file:

```powershell
dotnet build CarRental.slnx
dotnet run --project src/CarRental.API/CarRental.API.csproj
```

Run tests (currently no test files exist, so these run 0 tests):

```powershell
dotnet test tests/CarRental.UnitTests/CarRental.UnitTests.csproj
dotnet test tests/CarRental.IntegrationTests/CarRental.IntegrationTests.csproj
```

Run a single test once tests exist: `dotnet test --filter "FullyQualifiedName~ClassName.MethodName"`.

## Domain model (`CarRental.Domain.Entities`)

- `Car` — `Id`, `Type`, `Model`, `Services` (`HashSet<Service>`)
- `Customer` — `Id`, `FullName`, `Address`, `Email`
- `Rental` — `Id`, `Customer`, `StartDate`, `EndDate`, `Car`
- `Service` — `Id`, `Date`

These are plain data classes (no behavior, no EF Core annotations/configuration yet). `CarRental.Application` and `CarRental.Infrastructure` don't yet reference `CarRental.Domain` or each other — when adding logic, wire up the intended layering (`API` → `CarRental.Application` → `CarRental.Domain`, `CarRental.Infrastructure` implementing `CarRental.Application` abstractions) via `ProjectReference` entries as needed rather than assuming it's already in place.
