using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Data;
using SmartInventory.Models;

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
            var sales = await _context.Sales.ToListAsync();

            foreach (var product in products)
            {
                var productSales = sales
                    .Where(s => s.ProductId == product.Id)
                    .ToList();

                if (productSales.Any())
                {
                    var totalSales = productSales.Sum(s => s.Quantity);

                    var firstSaleDate = productSales
                        .Min(s => s.SaleDate)
                        .Date;

                    var days = (DateTime.Today - firstSaleDate).Days + 1;

                    if (days > 0)
                    {
                        product.DailySales = (double)totalSales / days;
                    }
                }
                else
                {
                    product.DailySales = 0;
                }
            }

            ViewBag.TotalProducts = products.Count;

            ViewBag.LowStockProducts = products.Count(p =>
                p.StockQuantity > 0 &&
                p.StockQuantity <= p.ReorderLevel);

            ViewBag.OutOfStockProducts = products.Count(p =>
                p.StockQuantity == 0);

            ViewBag.RestockSoon = products.Count(p =>
                p.StockQuantity > 0 &&
                (
                    p.StockQuantity <= p.ReorderLevel ||
                    (p.DailySales > 0 &&
                    (p.StockQuantity / p.DailySales) <= 7)
                 ));

            var restockProducts = products
                .Where(p =>
                    p.StockQuantity == 0 ||
                    (
                        p.StockQuantity > 0 &&
                        (
                            p.StockQuantity <= p.ReorderLevel ||
                            (p.DailySales > 0 &&
                            (p.StockQuantity / p.DailySales) <= 7)
                        )
                    )
                )
                .OrderBy(p =>
                    p.StockQuantity == 0
                        ? 0
                        : p.DailySales > 0
                            ? p.StockQuantity / p.DailySales
                            : double.MaxValue)
                .ToList();

            ViewBag.RestockProducts = restockProducts;

            return View();
        }
       
    }
}