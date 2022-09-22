using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http;

namespace DMartMallSoftware.Models
{
    public class Order
    {
        public int orderId { get; set; }
        public int orderCode { get; set; }
        public string? custName { get; set; }
        public string? mobileNumber { get; set; }
        public DateTime orderDate { get; set; }

        public List<OrderDetails> OrderDetails { get; set; }
        public decimal subTotal { get; set; }
        public decimal totalDiscount { get; set; }
        public decimal grandTotal { get; set; }
        public string? remark { get; set; }
        public string? shippingAddress { get; set; }
        public string? billingAddress { get; set; }
       
    }
}
