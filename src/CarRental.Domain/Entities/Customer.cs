using System;
using System.Collections.Generic;
using System.Runtime;
using System.Text;

namespace CarRental.Domain.Entities;

public class Customer
{
    public int Id { get; set; }
    public required string FullName { get; set; }
    public required string Address { get; set; }
    public required string Email { get; set; }
}
