using CarRental.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Infrastructure.Persistence;

public class CarRentalDbContext(DbContextOptions<CarRentalDbContext> options) : DbContext(options)
{
    public DbSet<Car> Cars => Set<Car>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Rental> Rentals => Set<Rental>();
    public DbSet<Service> Services => Set<Service>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Car>()
            .HasMany(car => car.Services)
            .WithOne()
            .HasForeignKey("CarId")
            .OnDelete(DeleteBehavior.Cascade);

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
    }
}
