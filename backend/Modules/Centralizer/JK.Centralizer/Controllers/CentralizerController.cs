using JK.Centralizer.Contracts;
using JK.Order.Client.Grpc;
using JK.Order.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace JK.Centralizer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CentralizerController : ControllerBase
{
    private readonly IOrderGrpcClient _orderGrpcClient;

    public CentralizerController(IOrderGrpcClient orderGrpcClient)
    {
        _orderGrpcClient = orderGrpcClient;
    }

    [HttpGet("dashboard-summary")]
    public async Task<ActionResult<DashboardSummaryDto>> GetDashboardSummary(CancellationToken cancellationToken)
    {
        // Example: aggregating data from other services
        var orders = await _orderGrpcClient.ListAsync(new ListOrdersRequest { Page = 1, PageSize = 1 }, cancellationToken);
        
        return Ok(new DashboardSummaryDto
        {
            OrderCount = orders.TotalCount
        });
    }

    [HttpGet("orders")]
    public async Task<ActionResult<PagedResponse<OrderDto>>> GetOrders(
        [FromQuery] ListOrdersRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _orderGrpcClient.ListAsync(request, cancellationToken);
        return Ok(result);
    }
}
