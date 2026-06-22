using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NorthwindApp.Data;
using NorthwindApp.Models;

namespace NorthwindApp.Controllers
{
    public class ProductsController : Controller
    {
        private readonly NorthwindContext _context;

        public ProductsController(NorthwindContext context)
        {
            _context = context;
        }

       public async Task<IActionResult> Index()
        {

            var productos = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .OrderBy(p => p.Productname)
                .ToListAsync();

            //Prodcutos ordenados por precio
            var productosCaros = await _context.Products
                .OrderByDescending(p => p.Unitprice)
                .Take(10)
                .ToListAsync();

            //Productos con A en el nombre
            var productosConA = await _context.Products
                .Where(p => p.Productname.Contains("a"))
                .ToListAsync();



            //Productos con su categoría
            var productosCategoria = await _context.Products
                .Include(p => p.Category)
                .Take(10)
                .ToListAsync();


            //Productos con su proveedor
            var productosProveedor = await _context.Products
                .Include(p => p.Supplier)
                .Take(10)
                .ToListAsync();
            // Productos de categoría "Beverages" usando Join
            var bebidas = await (
                from p in _context.Products
                join c in _context.Categories
                    on p.Categoryid equals c.Categoryid
                where c.Categoryname == "Beverages"
                select p
            ).Include(p => p.Category)
             .Include(p => p.Supplier)
             .ToListAsync();

            return View(bebidas);


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
                .FirstOrDefaultAsync(m => m.Productid == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            ViewData["Categoryid"] = new SelectList(_context.Categories, "Categoryid", "Categoryid");
            ViewData["Supplierid"] = new SelectList(_context.Suppliers, "Supplierid", "Supplierid");
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Productid,Productname,Supplierid,Categoryid,Quantityperunit,Unitprice,Unitsinstock,Unitsonorder,Reorderlevel,Discontinued")] Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Categoryid"] = new SelectList(_context.Categories, "Categoryid", "Categoryid", product.Categoryid);
            ViewData["Supplierid"] = new SelectList(_context.Suppliers, "Supplierid", "Supplierid", product.Supplierid);
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["Categoryid"] = new SelectList(_context.Categories, "Categoryid", "Categoryid", product.Categoryid);
            ViewData["Supplierid"] = new SelectList(_context.Suppliers, "Supplierid", "Supplierid", product.Supplierid);
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
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
            ViewData["Categoryid"] = new SelectList(_context.Categories, "Categoryid", "Categoryid", product.Categoryid);
            ViewData["Supplierid"] = new SelectList(_context.Suppliers, "Supplierid", "Supplierid", product.Supplierid);
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(m => m.Productid == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Productid == id);
        }
    }
}
