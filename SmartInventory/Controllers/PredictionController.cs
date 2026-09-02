using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Data;

namespace SmartInventory.Controllers
{
    public class PredictionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PredictionController(ApplicationDbContext context)
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
            }

            return View(products);
        }
    }
}