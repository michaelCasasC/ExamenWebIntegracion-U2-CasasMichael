using System;
using System.Collections.Generic;

namespace NorthwindApp.Models;

public partial class Region
{
    public int Regionid { get; set; }

    public string? Regiondescription { get; set; }

    public virtual ICollection<Territory> Territories { get; set; } = new List<Territory>();
}
