using System;
using System.Collections.Generic;
using System.Text;

namespace CarRental.Domain.Entities;

public class Car
{
    public Car()
    {
        Services = [];
    }

    public int Id { get; set; }
    public required string Type { get; set; }
    public required string Model { get; set; }
    public HashSet<Service> Services { get; set; }
}
