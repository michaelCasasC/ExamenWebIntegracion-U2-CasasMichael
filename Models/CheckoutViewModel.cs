using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace NorthwindApp.Models;

public class CheckoutViewModel
{
    [Required(ErrorMessage = "Customer is required.")]
    public string Customerid { get; set; } = null!;

    [Required(ErrorMessage = "Employee is required.")]
    public int Employeeid { get; set; }

    [Required(ErrorMessage = "Shipping method is required.")]
    public int Shipvia { get; set; }

    [Range(0, 1000000, ErrorMessage = "Freight must be a valid amount.")]
    public decimal Freight { get; set; }

    [Required(ErrorMessage = "Ship name is required.")]
    public string Shipname { get; set; } = null!;

    [Required(ErrorMessage = "Ship address is required.")]
    public string Shipaddress { get; set; } = null!;

    [Required(ErrorMessage = "Ship city is required.")]
    public string Shipcity { get; set; } = null!;

    public string? Shipregion { get; set; }

    [Required(ErrorMessage = "Postal code is required.")]
    public string Shippostalcode { get; set; } = null!;

    [Required(ErrorMessage = "Ship country is required.")]
    public string Shipcountry { get; set; } = null!;

    public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();

    public decimal OrderTotal => Items.Sum(item => item.Subtotal) + Freight;
}
