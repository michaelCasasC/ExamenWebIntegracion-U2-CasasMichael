using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using NorthwindApp.Data;
using NorthwindApp.Models;

namespace NorthwindApp.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly NorthwindContext _context;

        public ProductsController(NorthwindContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index(string? searchString, int? pageNumber, int? pageSize)
        {
            int page = pageNumber ?? 1;
            int size = pageSize ?? 10;

            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Where(p => p.Discontinued != true); // Logical deletion filter

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p => p.Productname.ToLower().Contains(searchString.ToLower()) || 
                                         (p.Category != null && p.Category.Categoryname.ToLower().Contains(searchString.ToLower())));
            }

            var sortedQuery = query.OrderBy(p => p.Productname);

            int totalItems = await sortedQuery.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)size);

            if (page < 1) page = 1;
            if (totalPages > 0 && page > totalPages) page = totalPages;

            var products = await sortedQuery
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchString = searchString;
            ViewBag.PageSize = size;
            ViewBag.HasPreviousPage = page > 1;
            ViewBag.HasNextPage = page < totalPages;

            return View(products);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> LowStock(int threshold = 10)
        {
            var items = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Where(p => p.Discontinued != true && p.Unitsinstock != null && p.Unitsinstock <= threshold)
                .OrderBy(p => p.Unitsinstock)
                .ToListAsync();

            ViewBag.Threshold = threshold;
            return View(items);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(m => m.Productid == id && m.Discontinued != true);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["Categoryid"] = new SelectList(_context.Categories, "Categoryid", "Categoryname");
            ViewData["Supplierid"] = new SelectList(_context.Suppliers, "Supplierid", "Companyname");
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Productid,Productname,Supplierid,Categoryid,Quantityperunit,Unitprice,Unitsinstock,Unitsonorder,Reorderlevel,Discontinued")] Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Categoryid"] = new SelectList(_context.Categories, "Categoryid", "Categoryname", product.Categoryid);
            ViewData["Supplierid"] = new SelectList(_context.Suppliers, "Supplierid", "Companyname", product.Supplierid);
            return View(product);
        }

        // GET: Products/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FirstOrDefaultAsync(m => m.Productid == id && m.Discontinued != true);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["Categoryid"] = new SelectList(_context.Categories, "Categoryid", "Categoryname", product.Categoryid);
            ViewData["Supplierid"] = new SelectList(_context.Suppliers, "Supplierid", "Companyname", product.Supplierid);
            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Productid,Productname,Supplierid,Categoryid,Quantityperunit,Unitprice,Unitsinstock,Unitsonorder,Reorderlevel,Discontinued")] Product product)
        {
            if (id != product.Productid)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Productid))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["Categoryid"] = new SelectList(_context.Categories, "Categoryid", "Categoryname", product.Categoryid);
            ViewData["Supplierid"] = new SelectList(_context.Suppliers, "Supplierid", "Companyname", product.Supplierid);
            return View(product);
        }

        // GET: Products/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(m => m.Productid == id && m.Discontinued != true);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                product.Discontinued = true; // Logical deletion / Soft delete
                _context.Update(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Productid == id && e.Discontinued != true);
        }
    }
}
