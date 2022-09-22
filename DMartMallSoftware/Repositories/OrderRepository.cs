using Dapper;
using DMartMallSoftware.Context;
using DMartMallSoftware.Models;
using DMartMallSoftware.Repositories.Interfaces;
using System.Data.Common;
using System.Xml.Linq;

namespace DMartMallSoftware.Repositories
{
    public class OrderRepository:IOrderRepository
    {
        private readonly DapperContext _context;

        public OrderRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<List<Order>> GetAllOrders(string? name)
        {
            List<Order> orders = null;
            var query = @"Select * from DMartBill where custName like '%'+@custName+'%'";
            string ID;
            using (var connection = _context.CreateConnection())
            {
                if (name == null)
                {
                    name = "";
                }
                var ordersraw = await connection.QueryAsync<Order>(query, new
                {
                    custName = name
                });
                
                    orders = ordersraw.ToList();
                    foreach (var order in orders)
                    {
                        var orderdetailsrow = await connection.QueryAsync<OrderDetails>(@"select o.orderID,o.detailsId,p.productId,p.productName,p.productPrice,
                    d.discountId,d.discountInPerc,o.quentity,o.totalAmount,o.totaldiscount,o.netAmount,o.discount  from Product p inner join Odetails o on 
                    p.productId=o.productId inner join Discount d on o.discountId=d.discountId where o.detailsId =any (select detailsId from Odetails 
                    where orderId=@orderId)", new { orderId = order.orderId });
                        order.OrderDetails = orderdetailsrow.ToList();
                    }
                    return orders;
                
            }
        }

        public async Task<Order> GetOrderById(int? Id, int? code, DateTime? date)
        {
            Order order = null;
            var query = "Select * from DMartBill where orderId=@orderId or orderCode=@orderCode or orderDate=@orderDate";
            using (var connection = _context.CreateConnection())
            {
                var ordersraw = await connection.QueryAsync<Order>(query, new { orderId = Id,orderCode=code,orderDate=date });
                order = ordersraw.FirstOrDefault();
                if (order != null)
                {
                    var orderdetailsrow = await connection.QueryAsync<OrderDetails>(@"select o.orderID,o.detailsId,p.productId,p.productName,p.productPrice,
                    d.discountId,d.discountInPerc,o.quentity,o.totalAmount,o.totaldiscount,o.netAmount,o.discount from Product p inner join Odetails o on 
                    p.productId=o.productId inner join Discount d on o.discountId=d.discountId where o.detailsId =any (select detailsId from Odetails 
                    where orderId=@orderId)", new { orderId =order.orderId});
                    order.OrderDetails = orderdetailsrow.ToList();
                }
                return order;
            }
        }
        public async Task<decimal> PlaceOrder(Order order)
        {
            price p = new price();
            double result1 = 0;
            int result = 0;
            var query = @"insert into DMartBill(orderCode,custName,mobileNumber,orderDate,remark,shippingAddress,billingAddress)
                          VALUES (@orderCode,@custName,@mobileNumber,@orderDate,@remark,@shippingAddress,@billingAddress);
                          SELECT CAST(SCOPE_IDENTITY() as int)";
            order.orderDate = DateTime.Now;
            List<OrderDetails> odlist = new List<OrderDetails>();
            odlist = order.OrderDetails.ToList();

            using (var connection = _context.CreateConnection())
            {
                
                result = await connection.ExecuteScalarAsync<int>(query, order);
                // if (result != 0)
                // {
                p = await AddProduct(odlist, result);
                order.subTotal = p.totalamount;
                order.totalDiscount = p.totaldiscount;
                order.grandTotal = p.netamount;
                var qry1 = "update DMartBill set subTotal=@subTotal,totalDiscount=@totalDiscount,grandTotal=@grandTotal where orderId=@orderId";

                var result3 = await connection.ExecuteAsync(qry1, new { subTotal = order.subTotal , totalDiscount = order.totalDiscount, grandTotal = order.grandTotal, orderId = result });


                return p.netamount;
            }

        }
           
        private async Task<price> AddProduct(List<OrderDetails> orders, int result2)
        {
            decimal subtotal=0;
            decimal grandtotal=0;
            decimal totaldiscount = 0;
            price p=new price();
            using (var connection = _context.CreateConnection())
            {
                if (orders.Count > 0)
                {
                    foreach (OrderDetails order in orders)
                    {
                        order.orderId = result2;

                        var query = @"insert into Odetails(productId,quentity,discountId,orderId)
                                      VALUES(@productId,@quentity,@discountId,@orderId);
                                      SELECT CAST(SCOPE_IDENTITY() as int)";

                        var result1 = await connection.ExecuteScalarAsync<int>(query, order);
                        //result = result + result1;
                        var pquery = "select productName,productPrice from Product where productId=@productId";
                        orders = (List<OrderDetails>)await connection.QueryAsync<OrderDetails>(pquery, new { productId = order.productId });
                        order.productPrice = orders[0].productPrice;
                        order.productName = orders[0].productName;
                        var pquery1 = "select discountInPerc from Discount where discountId=@discountId";
                        order.discountInPerc =await connection.QuerySingleAsync<decimal>(pquery1, new { discountId = order.discountId });
                        order.discount=(order.productPrice/100)*order.discountInPerc;
                        order.totalDiscount =order.discount* order.quentity;

                        order.totalAmount = order.productPrice * order.quentity;
                        order.netAmount=order.totalAmount-order.totalDiscount;
                        var qry1 = @"update Odetails set totalAmount=@totalAmount,totalDiscount=@totalDiscount,
                                    netAmount=@netAmount ,discount=@discount where detailsId=@detailsId";

                        var result3 = await connection.ExecuteAsync(qry1, new 
                        { 
                            totalAmount = order.totalAmount,
                            totalDiscount=order.totalDiscount,
                            netAmount=order.netAmount,
                            discount=order.discount,
                            detailsId = result1 });
                        subtotal = subtotal + order.totalAmount;
                        grandtotal = grandtotal + order.netAmount;
                        totaldiscount=totaldiscount+order.totalDiscount;

                    }
                    p.totalamount = subtotal;
                    p.netamount = grandtotal;
                    p.totaldiscount = totaldiscount;

                }
                return p;
            }
        }

        public async Task<int> UpdateOrder(Order order)
        {
            price p = new price();
            int result = 0;
            var query = @"update DMartBill set orderCode = @orderCode, custName = @custName, mobileNumber = @mobileNumber,
                          orderDate=@orderDate,remark=@remark,shippingAddress = @shippingAddress, billingAddress = @billingAddress where orderId = @orderId";

            using (var connection = _context.CreateConnection())
            {
                result = await connection.ExecuteAsync(query, order);
                if (result != 0)
                {
                    result = await connection.ExecuteAsync(@"delete from Odetails where orderId=@orderId"
                                                           , new { orderId = order.orderId });
                    p= await AddProduct(order.OrderDetails, order.orderId);
                    order.subTotal = p.totalamount;
                    order.totalDiscount = p.totaldiscount;
                    order.grandTotal = p.netamount;
                    var qry1 = "update DMartBill set subTotal=@subTotal,totalDiscount=@totalDiscount,grandTotal=@grandTotal where orderId=@orderId";

                    var result3 = await connection.ExecuteAsync(qry1, new { subTotal = order.subTotal, totalDiscount = order.totalDiscount, grandTotal = order.grandTotal, orderId = result });


                   

                }
                return result;
            }
        }

        public async Task<int> Delete(int id)
        {

            var query = @"Delete from Odetails where orderId=@orderId
                          Delete from DMartBill where orderId=@orderId";
            using (var connection = _context.CreateConnection())
            {
                var result = await connection.ExecuteAsync(query, new { orderId = id });
                return result;
            }
        }
    }
}
