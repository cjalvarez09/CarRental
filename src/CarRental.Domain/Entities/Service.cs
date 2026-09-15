using System;
using System.Collections.Generic;
using System.Text;

namespace CarRental.Domain.Entities;

public class Service
{
    public int Id { get; set; }
    public required DateTime Date { get; set; }
}
