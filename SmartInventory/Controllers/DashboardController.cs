using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Data;

namespace SmartInventory.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _context.Products.ToListAsync();

            ViewBag.TotalProducts = products.Count;

            ViewBag.LowStockProducts = products.Count(p =>
                p.StockQuantity > 0 &&
                p.StockQuantity <= p.ReorderLevel);

            ViewBag.OutOfStockProducts = products.Count(p =>
                p.StockQuantity == 0);

            ViewBag.RestockSoon = products.Count(p =>
                p.DailySales > 0 &&
                (p.StockQuantity / p.DailySales) <= 7);

            return View();
        }
    }
}