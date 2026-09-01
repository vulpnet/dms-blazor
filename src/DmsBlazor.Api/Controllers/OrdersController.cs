using DmsBlazor.Api.Data;
using DmsBlazor.Shared.Models;
using DmsBlazor.Shared.Services;
using Microsoft.AspNetCore.Mvc;

namespace DmsBlazor.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    // Tính giá + khuyến mãi cho giỏ hàng hiện tại — gọi mỗi khi khách đổi số lượng,
    // không lưu gì cả, chỉ trả lại kết quả tính toán để hiển thị trực tiếp.
    [HttpPost("price")]
    public ActionResult<PricedOrder> Price([FromBody] CreateOrderRequest request)
    {
        var priced = OrderPricingService.Price(request.Lines, MockData.Products, request.Channel);
        return priced;
    }

    // Xác nhận đặt hàng — demo nên chỉ sinh mã đơn giả lập, không lưu database
    [HttpPost("confirm")]
    public ActionResult<OrderConfirmation> Confirm([FromBody] CreateOrderRequest request)
    {
        var priced = OrderPricingService.Price(request.Lines, MockData.Products, request.Channel);
        var code = $"DH{Random.Shared.Next(100000, 999999)}";
        return new OrderConfirmation { OrderCode = code, Total = priced.Total };
    }
}
