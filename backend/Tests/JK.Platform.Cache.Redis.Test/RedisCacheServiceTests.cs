using FluentAssertions;
using JK.Platform.Cache.Redis.Configurations;
using JK.Platform.Core.Cache;
using JK.Platform.Core.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace JK.Platform.Cache.Redis.Test;

public class RedisCacheServiceTests
{
    private readonly IServiceProvider _serviceProvider;

    public RedisCacheServiceTests()
    {
        // 1. SIMULACE STARTU APLIKACE (Příprava in-memory konfigurace)
        var inMemorySettings = new Dictionary<string, string>
        {
            {"CacheConfiguration:Provider", "Redis"},
            {"CacheConfiguration:SectionName", "MyRedisSettings"},
            {"MyRedisSettings:AllowAdmin", "true"},
            {"MyRedisSettings:ConnectTimeout", "5000"},
            {"MyRedisSettings:Database", "0"},
            {"MyRedisSettings:Hosts:0:Host", "localhost"}, 
            {"MyRedisSettings:Hosts:0:Port", "6379"},
            {"MyRedisSettings:PoolSize", "2"}
        };

        var builder = Host.CreateApplicationBuilder();
        builder.Configuration.AddInMemoryCollection(inMemorySettings!);

        // 2. INICIALIZACE VAŠEHO PLATFORMOVÉHO CONFIGURATORU
        var configurator = new BuilderConfigurator();
        configurator.ConfigureServices(builder);

        builder.Services.RegisterInjectableServices(typeof(RedisCacheService).Assembly);

        _serviceProvider = builder.Services.BuildServiceProvider();
    }

    [Fact]
    public async Task CacheService_Should_StoreAndRetrieveComplexObjectSuccessfully()
    {
        // ARRANGE
        // Vytáhneme si z DI kontejneru naši obecnou službu
        var cacheService = _serviceProvider.GetRequiredService<ICacheService>();
        
        string cacheKey = $"test:order:{Guid.NewGuid()}";
        var originalOrder = new TestOrderDto(Guid.NewGuid().ToString(), "Jan Novák", 1500.50m);

        // ACT
        // 1. Uložíme objekt do Redisu (včetně nastavení expirace na 1 minutu)
        await cacheService.SetAsync("test", cacheKey, originalOrder, TimeSpan.FromMinutes(1));

        // 2. Pokusíme se objekt ihned načíst zpět
        var retrievedOrder = await cacheService.GetAsync<TestOrderDto>("test", cacheKey);

        // ASSERT
        retrievedOrder.Should().NotBeNull();
        retrievedOrder!.OrderId.Should().Be(originalOrder.OrderId);
        retrievedOrder.CustomerName.Should().Be(originalOrder.CustomerName);
        retrievedOrder.Amount.Should().Be(originalOrder.Amount);

        // ÚKLID: Po testu klíč z Redisu smažeme, aby po sobě test nezanechal nepořádek
        await cacheService.RemoveAsync("test", cacheKey);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnNull_WhenKeyDoesNotExist()
    {
        // ARRANGE
        var cacheService = _serviceProvider.GetRequiredService<ICacheService>();
        string cacheKey = $"test:nonexistent:{Guid.NewGuid()}";

        // ACT
        var result = await cacheService.GetAsync<TestOrderDto>("test", cacheKey);

        // ASSERT
        result.Should().BeNull();
    }
}

// Pomocné DTO pro účely testu
public record TestOrderDto(string OrderId, string CustomerName, decimal Amount);
