using System.Reflection;
using CarRental.Domain.Common;
using CarRental.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Infrastructure.Persistence;

public class CarRentalDbContext(DbContextOptions<CarRentalDbContext> options) : DbContext(options)
{
    public DbSet<Car> Cars => Set<Car>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Rental> Rentals => Set<Rental>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Car>()
            .HasMany(car => car.Services)
            .WithOne()
            .HasForeignKey("CarId")
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasIndex(user => user.Username)
            .IsUnique();

        modelBuilder.Entity<Rental>(builder =>
        {
            builder.Property(rental => rental.Status)
                .HasConversion<string>();

            builder.HasOne(rental => rental.Customer)
                .WithMany()
                .HasForeignKey(rental => rental.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(rental => rental.Car)
                .WithMany()
                .HasForeignKey(rental => rental.CarId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        ApplySoftDeleteQueryFilters(modelBuilder);
    }

    private static void ApplySoftDeleteQueryFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
                continue;

            typeof(CarRentalDbContext)
                .GetMethod(nameof(SetSoftDeleteQueryFilter), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(entityType.ClrType)
                .Invoke(null, [modelBuilder]);
        }
    }

    private static void SetSoftDeleteQueryFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ISoftDelete
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(entity => !entity.IsDeleted);
    }
}
