# CLAUDE.md

Guidance for Claude Code when working in this repository.

## What this is

Backend for an airport car rental system (technical challenge). A .NET 10 Web API to be consumed by an Angular app. Customers check availability and book cars; employees manage cars, customers and rentals. The core rule: a car can only be rented by one customer at a time.

What the challenge asks for, and it's all in the code: global exception handler, DDD + CQRS, a design pattern (Repository), DI, in-memory cache with invalidation, Docker, authentication, authorization and soft delete. It also asks for descriptive conventional commits (at least 10, done by the developer) and a README section describing the AI-assisted workflow. API docs live in `docs/API.md` (plus `docs/openapi.json`).

## Solution layout

`CarRental.slnx` (new XML solution format, not `.sln`):

- `src/CarRental.Domain` - plain entities, enums, domain exceptions, repository interfaces. No dependencies.
- `src/CarRental.Application` - CQRS with MediatR 12.4.1, FluentValidation validators, pipeline behaviors, DTOs, and the abstractions the outer layers implement (`IUnitOfWork`, `ICacheService`, `ICurrentUserService`, `IJwtTokenGenerator`, `IPasswordHasher`).
- `src/CarRental.Infrastructure` - EF Core + SQLite (`CarRentalDbContext`), repositories, unit of work, memory cache service, JWT generator, password hasher.
- `src/CarRental.API` - controllers, `GlobalExceptionHandler`, `CurrentUserService`, `Program.cs` (composition root).
- `tests/CarRental.UnitTests` - xUnit + NSubstitute.
- `tests/CarRental.IntegrationTests` - xUnit + `WebApplicationFactory<Program>` against a real temporary SQLite file.

References go inward: API -> Application/Infrastructure, Infrastructure -> Application -> Domain.

## Commands

```bash
dotnet build CarRental.slnx
dotnet run --project src/CarRental.API          # http://localhost:5255, Swagger at /swagger (Development only)
dotnet test CarRental.slnx                       # unit + integration
dotnet format CarRental.slnx --verify-no-changes # must pass; drop the flag to auto-fix
```

The SQLite file `carrental.db` is created next to the API (working directory) by `EnsureCreated()` on startup. There are no migrations, so delete the file after changing the model. It's gitignored.

## Conventions

- **Entities are plain data.** No validation, no behavior, no factory methods. Input validation goes in FluentValidation validators (one per command, in the same folder as the command). Rules that need persisted state (overlap, "already cancelled", "in use") go in the handlers and throw domain exceptions.
- **Vertical slices in Application**: `Feature/Commands|Queries/Name/{Command, Validator, Handler}`. Handlers use repositories + `IUnitOfWork.SaveChangesAsync`.
- **Error mapping** (`GlobalExceptionHandler`): `ValidationException` 400, `NotFoundException` 404, `InvalidCredentialsException` 401 (must stay before the `DomainException` case), any other `DomainException` 409, everything else 500 with a generic message. Bodies are ProblemDetails.
- **Cancelling rentals**: employees can cancel any rental; a customer can only cancel their own (someone else's answers 404) and only if it starts after today (`RentalInProgressException`, 409). The check lives in `CancelRentalCommandHandler`, which is why `POST /rentals/{id}/cancel` has no role restriction.
- **Authorization**: two roles, `Customer` and `Employee`. Always use the `Roles` constants, never string literals. Class-level and method-level `[Authorize]` attributes combine with AND, so controllers use a plain class-level `[Authorize]` and add `[Authorize(Roles = Roles.Employee)]` only on the Employee-only actions.
- **Tests** are named `Given_..._When_..._Then_...` and have `// Given`, `// When`, `// Then` blocks. Plain xUnit `Assert` and NSubstitute on purpose (FluentAssertions went paid, Moq had the SponsorLink issue).
- Code style is in `.editorconfig` (LF, file-scoped namespaces, primary constructors, Allman braces).
- Conventional commits. Don't commit unless asked.

## Things that already bit us

- **MediatR 12 void commands**: `IRequest` no longer inherits `IRequest<Unit>`, so a behavior constrained with `where TRequest : IRequest<TResponse>` is silently skipped for void commands (no validation, no cache invalidation). Behaviors use `where TRequest : notnull`. `MediatRPipelineTests` covers this.
- **Cache**: pipeline order is Validation, Caching, CacheInvalidation. Queries opt in by implementing `ICacheableQuery` (key, tags), commands invalidate by implementing `ICacheInvalidatingCommand`. Tags are `cars`, `customers`, `rentals`. Rental commands also invalidate `cars` because availability depends on rentals, and `RegisterCommand` invalidates `customers`. A new command or query that doesn't declare its tags means stale reads. `MemoryCacheService` must stay a singleton (it keeps the per-tag `CancellationTokenSource`s).
- **ValidationBehaviour** creates one `ValidationContext` per validator. A shared one duplicates errors when a request has several validators.
- **Error responses**: `GlobalExceptionHandler` writes the JSON itself with `WriteAsJsonAsync(..., contentType: "application/problem+json")`. `IProblemDetailsService.TryWriteAsync` negotiates against `Accept` and failed for Swagger UI's `Accept: text/plain`, and `WriteAsJsonAsync` overwrites `ContentType` unless you pass it. `UseStatusCodePages()` gives 401/403/route-404 responses a body.
- **Swashbuckle 10 uses Microsoft.OpenApi v2**: types are in the `Microsoft.OpenApi` namespace (not `.Models`) and the security requirement is built with `AddSecurityRequirement(document => ...)` and `OpenApiSecuritySchemeReference`.
- **Soft delete**: `Car` and `Customer` implement `ISoftDelete`; the global query filter is applied by reflection in `CarRentalDbContext.ApplySoftDeleteQueryFilters`. Repository `Remove` methods just set the flags (deliberately not shared through a helper). Deleting a car/customer with any rental throws `CarInUseException`/`CustomerInUseException`, which is also why the EF Core warning about `Rental`'s required navigations is suppressed in `AddInfrastructure`.
- **Users and customers**: a `Customer` user has `User.CustomerId` and the JWT carries a `customerId` claim. `RegisterRentalCommandHandler` uses the caller's own customer when the caller is a Customer, whatever `customerId` came in the body. Employees have no linked profile. `POST /api/customers` creates a profile without a user on purpose (see README).
- **CORS** is a single policy fed by `Cors:AllowedOrigins` (default `http://localhost:4200`), applied with `UseCors` after `UseHttpsRedirection` and before authentication. `CorsTests` checks that error responses built by `GlobalExceptionHandler` and by the auth middleware keep the CORS headers, since otherwise the browser reports a CORS failure instead of the real error.
- `WebApplicationFactory` needs `public partial class Program;` at the end of `Program.cs`. Keep it.

## Working notes

- When starting the API to test something by hand, stop it by PID (find it with `Get-NetTCPConnection -LocalPort <port>`), never with `taskkill /IM dotnet.exe`, and delete `carrental.db*` afterwards.
- `Jwt:Secret` in `appsettings.json` is a development secret checked in on purpose so the project runs without setup. Known simplifications are listed in the README; check there before "fixing" one of them.
