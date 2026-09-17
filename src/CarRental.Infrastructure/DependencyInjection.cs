using CarRental.Application.Common.Interfaces;
using CarRental.Domain.Repositories;
using CarRental.Infrastructure.Caching;
using CarRental.Infrastructure.Persistence;
using CarRental.Infrastructure.Persistence.Repositories;
using CarRental.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CarRental.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("CarRentalDb")
            ?? "Data Source=carrental.db";

        services.AddDbContext<CarRentalDbContext>(options => options.UseSqlite(connectionString));

        services.AddScoped<ICarRepository, CarRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IRentalRepository, RentalRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddMemoryCache();
        services.AddSingleton<ICacheService, MemoryCacheService>();

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }
}
