
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Models;
using SmartInventory.Data;

public class SalesController : Controller
{
    private readonly ApplicationDbContext _context;

    public SalesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: SALES
    public async Task<IActionResult> Index()
    {
        var sales = await _context.Sales
            .Include(s => s.Product)
            .ToListAsync();

        return View(sales);
    }

    // GET: SALES/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var sale = await _context.Sales
            .FirstOrDefaultAsync(m => m.Id == id);
        if (sale == null)
        {
            return NotFound();
        }

        return View(sale);
    }

    // GET: SALES/Create
    public IActionResult Create()
    {
        ViewBag.Products = _context.Products.ToList();
        return View();
    }

    // POST: SALES/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Sale sale)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == sale.ProductId);

        if (product == null)
        {
            ModelState.AddModelError("", "Product not found.");
            ViewBag.Products = _context.Products.ToList();
            return View(sale);
        }

        if (sale.Quantity <= 0)
        {
            ModelState.AddModelError("Quantity", "Quantity must be greater than 0.");
            ViewBag.Products = _context.Products.ToList();
            return View(sale);
        }

        if (sale.Quantity > product.StockQuantity)
        {
            ModelState.AddModelError("Quantity", "Not enough stock available.");
            ViewBag.Products = _context.Products.ToList();
            return View(sale);
        }

        product.StockQuantity -= sale.Quantity;

        _context.Sales.Add(sale);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Sale recorded successfully. Stock has been updated.";

        return RedirectToAction(nameof(Index));
    }

    // GET: SALES/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var sale = await _context.Sales.FindAsync(id);
        if (sale == null)
        {
            return NotFound();
        }
        return View(sale);
    }

    // POST: SALES/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
    int? id,
    [Bind("Id,ProductId,Quantity,SaleDate")] Sale sale)
    {
        if (id != sale.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(sale);
        }

        var existingSale = await _context.Sales
            .FirstOrDefaultAsync(s => s.Id == sale.Id);

        if (existingSale == null)
        {
            return NotFound();
        }

        // Get the product from the existing sale
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == existingSale.ProductId);

        if (product == null)
        {
            return NotFound();
        }

        // Return the old sale quantity to stock
        product.StockQuantity += existingSale.Quantity;

        // Check whether the new quantity can be sold
        if (sale.Quantity <= 0)
        {
            ModelState.AddModelError("Quantity", "Quantity must be greater than 0.");
            product.StockQuantity -= existingSale.Quantity;
            return View(sale);
        }

        if (sale.Quantity > product.StockQuantity)
        {
            ModelState.AddModelError(
                "Quantity",
                "Not enough stock available for this sale quantity.");

            product.StockQuantity -= existingSale.Quantity;
            return View(sale);
        }

        // Reduce stock using the new sale quantity
        product.StockQuantity -= sale.Quantity;

        // Update the existing sale
        existingSale.Quantity = sale.Quantity;
        existingSale.SaleDate = sale.SaleDate;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Sale updated successfully. Stock has been adjusted.";

        return RedirectToAction(nameof(Index));
    }

    // GET: SALES/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var sale = await _context.Sales
            .FirstOrDefaultAsync(m => m.Id == id);
        if (sale == null)
        {
            return NotFound();
        }

        return View(sale);
    }

    // POST: SALES/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var sale = await _context.Sales.FindAsync(id);

        if (sale == null)
        {
            return NotFound();
        }

        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == sale.ProductId);

        if (product != null)
        {
            // Return the sold quantity to stock
            product.StockQuantity += sale.Quantity;
        }

        _context.Sales.Remove(sale);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Sale deleted successfully. Stock has been restored.";

        return RedirectToAction(nameof(Index));
    }

    private bool SaleExists(int? id)
    {
        return _context.Sales.Any(e => e.Id == id);
    }
}
