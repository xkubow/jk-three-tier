using JK.Playground.Models;

namespace JK.Playground.Services;

public interface IOrderService
{
    public Task<Guid> CreateAsync(Guid consumerId, IList<CreateOrderProductRequest> products);
}