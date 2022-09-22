using Microsoft.AspNetCore.DataProtection.KeyManagement;

namespace DMartMallSoftware.Models
{
    public class OrderDetails
    {
        public int detailsId { get; set; }
        public int orderId { get; set; }
        public int productId { get; set; }
        public string?  productName { get; set; }
        public decimal  productPrice { get; set; }
        public decimal quentity { get; set; }
        public int discountId { get; set; }
        public decimal discountInPerc { get; set; }
        public decimal discount { get; set; }
        public decimal totalAmount { get; set; }
        public decimal totalDiscount { get; set; }
        public decimal netAmount { get; set; }
    }
}