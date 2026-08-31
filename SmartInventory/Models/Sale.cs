using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartInventory.Models
{
    public class Sale
    {
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public DateTime SaleDate { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; } = null!;
    }
}