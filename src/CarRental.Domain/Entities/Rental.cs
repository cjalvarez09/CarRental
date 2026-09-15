using System;
using System.Collections.Generic;
using System.Text;

namespace CarRental.Domain.Entities;

public class Rental
{
    public int Id { get; set; }
    public required Customer Customer { get; set; }
    public required DateTime StartDate { get; set; }
    public required DateTime EndDate { get; set; }
    public required Car Car { get; set; }
}
