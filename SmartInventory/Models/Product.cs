using System.ComponentModel.DataAnnotations;

namespace SmartInventory.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public int StockQuantity { get; set; }

        public int ReorderLevel { get; set; }

        public double Price { get; set; }

        public double DailySales { get; set; }
    }
}