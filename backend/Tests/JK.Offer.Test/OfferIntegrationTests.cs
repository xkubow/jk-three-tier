using FluentAssertions;
using JK.Offer.Database;
using JK.Offer.Database.Entities;
using JK.Offer.Database.Repositories;
using JK.Offer.MappingProfiles;
using Microsoft.EntityFrameworkCore;
using Xunit;
using AutoMapper;
using Testcontainers.PostgreSql;
using JK.Offer.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using JK.Offer.Contracts;
using JK.Platform.Persistence.EfCore.Extensions;
using Microsoft.Extensions.Configuration;
using JK.Offer.Models;
using Microsoft.Extensions.Options;
using JK.Offer.Configurations;
using WireMock.Server;
using WireMock.Settings;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using JK.Offer.Client.Grpc;
using JK.Offer.Proto;
using Grpc.Net.Client;
using Google.Protobuf;

using CreateOfferRequest = JK.Offer.Contracts.CreateOfferRequest;
using UpdateOfferRequest = JK.Offer.Contracts.UpdateOfferRequest;

namespace JK.Offer.Test;

public class OfferIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("jk_offer_test")
        .WithUsername("test_user")
        .WithPassword("test_password")
        .Build();

    private IServiceProvider _serviceProvider = null!;
    private IServiceScope _scope = null!;
    private OfferDbContext _context = null!;
    private IOfferService _offerService = null!;

    public async Task InitializeAsync()
    {
        // 1. Spustíme Docker kontejner
        await _postgreSqlContainer.StartAsync();

        // 2. Příprava DI kontejneru
        var services = new ServiceCollection();
        
        // Konfigurace DbContextu s dynamickým connection stringem
        string connectionString = _postgreSqlContainer.GetConnectionString();
        services.AddDbContext<OfferDbContext>(options => options.UseNpgsql(connectionString));

        // Základní služby platformy
        services.AddLogging(logging => logging.AddConsole());
        services.AddAutoMapper(typeof(OfferMappingProfile).Assembly);
        services.AddUnitOfWork();
        
        // Registrace OfferService a jeho závislostí
        services.AddScoped<IOfferService, OfferService>();
        services.AddScoped<IOfferRepository, OfferRepository>();
        
        // Mock konfigurace pro IOptionsSnapshot
        var configuration = new ConfigurationBuilder().Build();
        services.AddSingleton<IConfiguration>(configuration);
        services.Configure<OfferConfiguration>(options => {
            // Zde lze nastavit testovací hodnoty
        });

        _serviceProvider = services.BuildServiceProvider();
        _scope = _serviceProvider.CreateScope();
        
        _context = _scope.ServiceProvider.GetRequiredService<OfferDbContext>();
        _offerService = _scope.ServiceProvider.GetRequiredService<IOfferService>();

        // 3. Vytvoření schématu
        await _context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        _scope?.Dispose();
        if (_context != null)
        {
            await _context.DisposeAsync();
        }
        await _postgreSqlContainer.DisposeAsync();
    }

    [Fact]
    public async Task CreateAsync_Should_SaveOfferToDatabase()
    {
        // ARRANGE
        var request = new CreateOfferRequest
        {
            Number = "OFF-2024-001",
            TotalAmount = 1500.00m,
            ExpiresAt = DateTime.UtcNow.AddDays(10)
        };

        // ACT
        var result = await _offerService.CreateAsync(request);

        // ASSERT
        result.Should().NotBeNull();
        result.Number.Should().Be(request.Number);
        result.TotalAmount.Should().Be(request.TotalAmount);
        result.Id.Should().NotBeEmpty();

        // Ověření přímo v DB
        var dbOffer = await _context.Offers.FindAsync(result.Id);
        dbOffer.Should().NotBeNull();
        dbOffer!.Number.Should().Be(request.Number);
    }

    [Fact]
    public async Task GetByIdAsync_Should_ReturnOffer_WhenExists()
    {
        // ARRANGE
        var createRequest = new CreateOfferRequest
        {
            Number = "OFF-GET",
            TotalAmount = 999.99m,
            ExpiresAt = DateTime.UtcNow.AddDays(5)
        };
        var createdOffer = await _offerService.CreateAsync(createRequest);

        // ACT
        var retrievedOffer = await _offerService.GetByIdAsync(createdOffer.Id);

        // ASSERT
        retrievedOffer.Should().NotBeNull();
        retrievedOffer!.Id.Should().Be(createdOffer.Id);
        retrievedOffer.Number.Should().Be("OFF-GET");
    }

    [Fact]
    public async Task UpdateAsync_Should_ModifyExistingOffer()
    {
        // ARRANGE
        var createRequest = new CreateOfferRequest
        {
            Number = "OFF-UPDATE",
            TotalAmount = 100m,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };
        var createdOffer = await _offerService.CreateAsync(createRequest);
        
        var updateRequest = new UpdateOfferRequest
        {
            Status = "Sent",
            TotalAmount = 200m,
            ExpiresAt = DateTime.UtcNow.AddDays(2)
        };

        // ACT
        var updatedOffer = await _offerService.UpdateAsync(createdOffer.Id, updateRequest);

        // ASSERT
        updatedOffer.Should().NotBeNull();
        updatedOffer!.Status.Should().Be("Sent");
        updatedOffer.TotalAmount.Should().Be(200m);
        
        // Ověření v DB
        var dbOffer = await _context.Offers.FindAsync(createdOffer.Id);
        dbOffer!.TotalAmount.Should().Be(200m);
    }

    [Fact]
    public async Task DeleteAsync_Should_RemoveOffer()
    {
        // ARRANGE
        var createRequest = new CreateOfferRequest
        {
            Number = "OFF-DELETE",
            TotalAmount = 0m,
            ExpiresAt = DateTime.UtcNow
        };
        var createdOffer = await _offerService.CreateAsync(createRequest);

        // ACT
        var deleteResult = await _offerService.DeleteAsync(createdOffer.Id);

        // ASSERT
        deleteResult.Should().BeTrue();
        
        var dbOffer = await _context.Offers.FindAsync(createdOffer.Id);
        dbOffer.Should().BeNull();
    }

    [Fact]
    public async Task OfferGrpcClient_GetByIdAsync_Should_ReturnMockedResponse_UsingWireMock()
    {
        // ARRANGE
        var settings = new WireMockServerSettings
        {
            UseHttp2 = true
        };
        using var server = WireMockServer.Start(settings);
        var offerId = Guid.NewGuid();
        var expectedResponse = new OfferMessage
        {
            Id = offerId.ToString(),
            Number = "WIRE-001",
            TotalAmount = 2500.50,
            Status = "Draft",
            CreatedAt = DateTime.UtcNow.ToString("O"),
            UpdatedAt = DateTime.UtcNow.ToString("O"),
            ExpiresAt = DateTime.UtcNow.AddDays(30).ToString("O")
        };

        server
            .Given(Request.Create()
                .WithPath("/jk.offer.OfferGrpc/GetById")
                .UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/grpc")
                .WithTrailingHeader("grpc-status", "0")
                .WithBody(FrameGrpcMessage(expectedResponse)));

        var channel = GrpcChannel.ForAddress(server.Url!);
        var grpcClient = new OfferGrpc.OfferGrpcClient(channel);
        var sut = new OfferGrpcClient(grpcClient);

        // ACT
        var result = await sut.GetByIdAsync(offerId);

        // ASSERT
        result.Should().NotBeNull();
        result!.Id.Should().Be(offerId);
        result.Number.Should().Be("WIRE-001");
        result.TotalAmount.Should().Be(2500.50m);
        result.Status.Should().Be("Draft");
    }

    private static byte[] FrameGrpcMessage(IMessage message)
    {
        var messageBytes = message.ToByteArray();
        var framedBytes = new byte[messageBytes.Length + 5];
        framedBytes[0] = 0; // compression flag
        var lengthBytes = BitConverter.GetBytes(messageBytes.Length);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(lengthBytes);
        }
        Array.Copy(lengthBytes, 0, framedBytes, 1, 4);
        Array.Copy(messageBytes, 0, framedBytes, 5, messageBytes.Length);
        return framedBytes;
    }
}
