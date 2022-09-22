using DMartMallSoftware.Models;
using System.Xml.Linq;

namespace DMartMallSoftware.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        Task<List<Order>> GetAllOrders(string? name);
        Task<Order> GetOrderById(int? Id,int? code,DateTime? date);

        public Task<decimal> PlaceOrder(Order order);
        public Task<int> UpdateOrder(Order order);

        public Task<int> Delete(int id);
    }
}
