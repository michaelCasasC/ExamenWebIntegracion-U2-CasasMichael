using System;
using System.Collections.Generic;

namespace NorthwindApp.Models;

public partial class Shipper
{
    public int Shipperid { get; set; }

    public string? Companyname { get; set; }

    public string? Phone { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
