using JK.Playground.Models;

namespace JK.Playground.Database.Repositories;

public interface IOrderRepository
{
    Task<OrderModel?> CreateAsync(OrderModel order);
}