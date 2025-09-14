using Mango.Services.OrderAPI.Models.Dto;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mango.Services.OrderAPI.Models
{
    public class OrderDetails
    {
        [Key]
        public int OrderDetailsId { get; set; }
        public int CartHeaderId { get; set; }

        [ForeignKey("OrderHeaderId")]
        public OrderHeader? CartHeader { get; set; }
        public int ProductId { get; set; }
        [NotMapped]
        public ProductDto? Product { get; set; }
        public int Count { get; set; }

        //These properties are used for displaying product details in the order with the original information when the order was placed.
        //In case the product details change later, we still have the original information stored in the order details.
        public string ProductName { get; set; }
        public double Price { get; set; }
    }
}
