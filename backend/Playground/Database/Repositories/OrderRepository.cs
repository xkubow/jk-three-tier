using AutoMapper;
using Backend.Database;
using JK.Playground.Database.Entities;
using JK.Playground.Models;
using Microsoft.EntityFrameworkCore;

namespace JK.Playground.Database.Repositories;

public class OrderRepository: IOrderRepository
{
    private readonly ConfigDbContext _context;
    private readonly IMapper _mapper;
    private readonly DbSet<OrderEntity> _orders;

    public OrderRepository(ConfigDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
        _orders = context.Set<OrderEntity>();
    }

    public async Task<OrderModel?> CreateAsync(OrderModel order)
    {
        var entity = _mapper.Map<OrderEntity>(order);
        _orders.Add(entity);
        await _context.SaveChangesAsync();
        return _mapper.Map<OrderModel>(entity);
    }
}