using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NorthwindApp.Data;
using NorthwindApp.Models;

namespace NorthwindApp.Controllers;

[Authorize]
public class CartController : Controller
{
    private readonly NorthwindContext _context;
    private const string CartSessionKey = "CartItems";

    public CartController(NorthwindContext context)
    {
        _context = context;
    }

    private List<CartItemViewModel> GetCart()
    {
        return HttpContext.Session.GetObject<List<CartItemViewModel>>(CartSessionKey) ?? new List<CartItemViewModel>();
    }

    private void SaveCart(List<CartItemViewModel> cart)
    {
        HttpContext.Session.SetObject(CartSessionKey, cart);
    }

    public IActionResult Index()
    {
        var cart = GetCart();
        return View(cart);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToCart(int productId, int quantity, string returnUrl)
    {
        if (quantity < 1)
        {
            TempData["CartMessage"] = "Quantity must be at least 1.";
            return Redirect(returnUrl);
        }

        var product = await _context.Products.FindAsync(productId);
        if (product == null || product.Discontinued == true)
        {
            TempData["CartMessage"] = "Product not available.";
            return Redirect(returnUrl);
        }

        if (product.Unitsinstock == null || quantity > product.Unitsinstock)
        {
            TempData["CartMessage"] = "Selected quantity exceeds available stock.";
            return Redirect(returnUrl);
        }

        var cart = GetCart();
        var item = cart.FirstOrDefault(x => x.Productid == productId);
        if (item == null)
        {
            cart.Add(new CartItemViewModel
            {
                Productid = product.Productid,
                Productname = product.Productname,
                Quantity = quantity,
                Unitprice = product.Unitprice ?? 0m,
                UnitsInStock = product.Unitsinstock ?? 0
            });
        }
        else
        {
            item.Quantity += quantity;
            if (item.Quantity > item.UnitsInStock)
            {
                item.Quantity = item.UnitsInStock;
            }
        }

        SaveCart(cart);
        TempData["CartMessage"] = $"Added {quantity} unit(s) of {product.Productname} to your cart.";
        return Redirect(returnUrl);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Remove(int productId)
    {
        var cart = GetCart();
        cart.RemoveAll(x => x.Productid == productId);
        SaveCart(cart);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Clear()
    {
        SaveCart(new List<CartItemViewModel>());
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Checkout()
    {
        var cart = GetCart();
        if (!cart.Any())
        {
            TempData["CartMessage"] = "Your cart is empty.";
            return RedirectToAction(nameof(Index));
        }

        var customers = await _context.Customers.OrderBy(c => c.Customerid).ToListAsync();
        ViewData["Customerid"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(customers, "Customerid", "Companyname");
        var employees = await _context.Employees.OrderBy(e => e.Employeeid).ToListAsync();
        ViewData["Employeeid"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(employees, "Employeeid", "Employeeid");
        var shippers = await _context.Shippers.OrderBy(s => s.Shipperid).ToListAsync();
        ViewData["Shipvia"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(shippers, "Shipperid", "Companyname");

        var model = new CheckoutViewModel
        {
            Items = cart,
            Freight = 0m
        };

        if (User.Identity?.Name != null)
        {
            var candidate = User.Identity.Name.Split('@')[0].ToUpperInvariant();
            var customer = await _context.Customers.FindAsync(candidate);
            if (customer != null)
            {
                model.Customerid = customer.Customerid;
            }
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutViewModel model)
    {
        var cart = GetCart();
        if (!cart.Any())
        {
            TempData["CartMessage"] = "Your cart is empty.";
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            var customers = await _context.Customers.OrderBy(c => c.Customerid).ToListAsync();
            ViewData["Customerid"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(customers, "Customerid", "Companyname", model.Customerid);
            var employees = await _context.Employees.OrderBy(e => e.Employeeid).ToListAsync();
            ViewData["Employeeid"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(employees, "Employeeid", "Employeeid", model.Employeeid);
            var shippers = await _context.Shippers.OrderBy(s => s.Shipperid).ToListAsync();
            ViewData["Shipvia"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(shippers, "Shipperid", "Companyname", model.Shipvia);
            return View(model);
        }

        var order = new Order
        {
            Customerid = model.Customerid,
            Employeeid = model.Employeeid,
            Orderdate = DateOnly.FromDateTime(DateTime.Today),
            Requireddate = DateOnly.FromDateTime(DateTime.Today.AddDays(7)),
            Shipvia = model.Shipvia,
            Freight = model.Freight,
            Shipname = model.Shipname,
            Shipaddress = model.Shipaddress,
            Shipcity = model.Shipcity,
            Shipregion = model.Shipregion,
            Shippostalcode = model.Shippostalcode,
            Shipcountry = model.Shipcountry
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        foreach (var cartItem in cart)
        {
            var product = await _context.Products.FindAsync(cartItem.Productid);
            if (product == null)
            {
                continue;
            }

            if (product.Unitsinstock == null || cartItem.Quantity > product.Unitsinstock)
            {
                ModelState.AddModelError(string.Empty, $"There is not enough stock for {product.Productname}.");
                var customers = await _context.Customers.OrderBy(c => c.Customerid).ToListAsync();
                ViewData["Customerid"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(customers, "Customerid", "Companyname", model.Customerid);
                ViewData["Employeeid"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Employees.OrderBy(e => e.Employeeid), "Employeeid", "Employeeid", model.Employeeid);
                ViewData["Shipvia"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Shippers.OrderBy(s => s.Shipperid), "Shipperid", "Companyname", model.Shipvia);
                return View(model);
            }

            var orderDetail = new Orderdetail
            {
                Orderid = order.Orderid,
                Productid = product.Productid,
                Unitprice = product.Unitprice,
                Quantity = (short)cartItem.Quantity,
                Discount = 0m
            };
            _context.Orderdetails.Add(orderDetail);

            product.Unitsinstock = (short)(product.Unitsinstock - cartItem.Quantity);
        }

        await _context.SaveChangesAsync();
        SaveCart(new List<CartItemViewModel>());

        return RedirectToAction(nameof(Confirmation), new { orderId = order.Orderid });
    }

    public async Task<IActionResult> Confirmation(int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.Orderdetails)
            .ThenInclude(od => od.Product)
            .Include(o => o.Customer)
            .Include(o => o.Employee)
            .FirstOrDefaultAsync(o => o.Orderid == orderId);

        if (order == null)
        {
            return NotFound();
        }

        return View(order);
    }
}
