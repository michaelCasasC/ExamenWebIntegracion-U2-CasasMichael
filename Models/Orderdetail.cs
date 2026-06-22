using System;
using System.Collections.Generic;

namespace NorthwindApp.Models;

public partial class Orderdetail
{
    public int Orderid { get; set; }

    public int Productid { get; set; }

    public decimal? Unitprice { get; set; }

    public short? Quantity { get; set; }

    public decimal? Discount { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
