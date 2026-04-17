using JK.Playground.Database;
using JK.Playground.Database.Repositories;
using JK.Playground.Models;

namespace JK.Playground.Services;

public class OrderService
{
    private readonly OrderRepository _orderRepository;
    private readonly ProductRepository _productRepository;
    private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
    public OrderService(OrderRepository orderRepository, ProductRepository productRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
    }
    public async Task<Guid> CreateAsync(Guid consumerId, IList<CreateOrderProductRequest> products)
    {
        //Check if the amount of that product exists
        try
        {
            await _semaphoreSlim.WaitAsync();

            var isProductAvailable = await _productRepository.ExistsAmountAsync(products.ToDictionary(p => p.ProductId, p => p.Quantity));
            if (!isProductAvailable)
                throw new InvalidOperationException("One or more products are not available in the required quantity");

            //Create Order with those products
            OrderModel order = new() { ConsumerId = consumerId, Products = products.Select(p => new OrderProductModel() { ProductId = p.ProductId, Quantity = p.Quantity }) };
            var createdOrder = await _orderRepository.CreateAsync(order);
            if (createdOrder == null)
                throw new InvalidOperationException("Order could not be created");

            //Return OrderId
            return createdOrder.Id;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }
}