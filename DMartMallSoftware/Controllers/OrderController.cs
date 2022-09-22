using DMartMallSoftware.Models;
using DMartMallSoftware.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DMartMallSoftware.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderRepository _orderRepo;

        public OrderController(IOrderRepository orderRepo)
        {
            _orderRepo = orderRepo;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllOrders(string? name)
        {
            try
            {
                var order = await _orderRepo.GetAllOrders(name);
                return Ok(order);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("id/code/date")]
        public async Task<IActionResult> GetOrderById(int? Id,int? code,DateTime? date)
        {
            try
            {
                var order = await _orderRepo.GetOrderById(Id,code,date);
                return Ok(order);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder(Order order)
        {
            try
            {
                var result = await _orderRepo.PlaceOrder(order);

                if (result == 0)
                {
                    return StatusCode(409, "The request could not be processed because of conflict in the request");
                }
                else
                {
                    return StatusCode(200, string.Format("Record Inserted Successfuly with grand total is {0}", result));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateOrder(Order order)
        {
            try
            {
                var result = await _orderRepo.UpdateOrder(order);

                if (result == 0)
                {
                    return StatusCode(409, "The request could not be processed because of conflict in the request");
                }
                else
                {
                    return StatusCode(200, string.Format("Record Updated Successfuly with order Id {0}", result));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpDelete("id")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            try
            {
                var order = await _orderRepo.Delete(id);
                return Ok(order);
            }
            catch (Exception ex)
            {
                //log error
                return StatusCode(500, ex.Message);
            }
        }
    }
}
