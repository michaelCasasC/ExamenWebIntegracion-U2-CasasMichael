namespace NorthwindApp.Models;

public class CartItemViewModel
{
    public int Productid { get; set; }
    public string Productname { get; set; } = null!;
    public decimal Unitprice { get; set; }
    public int Quantity { get; set; }
    public short UnitsInStock { get; set; }

    public decimal Subtotal => Unitprice * Quantity;
}
