using Grpc.Net.Client;
using JK.Configuration.Contracts;
using JK.Configuration.Proto;

namespace JK.Configuration.Client.Grpc;

/// <summary>
/// Typed gRPC client for Configuration service. Use for cross-module calls.
/// </summary>
public class ConfigurationGrpcClient : IConfigurationGrpcClient
{
    private readonly ConfigurationGrpc.ConfigurationGrpcClient _client;

    public ConfigurationGrpcClient(GrpcChannel channel)
    {
        _client = new ConfigurationGrpc.ConfigurationGrpcClient(channel);
    }

    public ConfigurationGrpcClient(string address) : this(GrpcChannel.ForAddress(address))
    {
    }

    public async Task<ConfigurationDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await _client.GetByIdAsync(
            new GetConfigurationByIdRequest { Id = id.ToString() },
            cancellationToken: cancellationToken);
        return FromMessage(response);
    }

    public async Task<PagedResponse<ConfigurationDto>> ListAsync(Contracts.ListConfigurationRequest request, CancellationToken cancellationToken = default)
    {
        var protoRequest = new Proto.ListConfigurationRequest
        {
            Page = request.Page,
            PageSize = request.PageSize,
            SortDirection = request.SortDirection ?? "asc"
        };
        if (request.MarketCode != null) protoRequest.MarketCode = request.MarketCode;
        if (request.ServiceCode != null) protoRequest.ServiceCode = request.ServiceCode;
        if (request.SearchTerm != null) protoRequest.SearchTerm = request.SearchTerm;
        if (request.SortBy != null) protoRequest.SortBy = request.SortBy;

        var response = await _client.ListAsync(protoRequest, cancellationToken: cancellationToken);
        var items = response.Items.Select(FromMessage).Where(x => x != null).Cast<ConfigurationDto>().ToList();
        return new PagedResponse<ConfigurationDto>
        {
            Items = items,
            Page = response.Page,
            PageSize = response.PageSize,
            TotalCount = response.TotalCount
        };
    }

    public async Task<ConfigurationDto> CreateAsync(Contracts.CreateConfigurationRequest request, CancellationToken cancellationToken = default)
    {
        var protoRequest = new Proto.CreateConfigurationRequest { Key = request.Key, Value = request.Value };
        if (request.MarketCode != null) protoRequest.MarketCode = request.MarketCode;
        if (request.ServiceCode != null) protoRequest.ServiceCode = request.ServiceCode;
        if (request.CreatedBy != null) protoRequest.CreatedBy = request.CreatedBy;

        var response = await _client.CreateAsync(protoRequest, cancellationToken: cancellationToken);
        return FromMessage(response)!;
    }

    public async Task<ConfigurationDto?> UpdateAsync(Guid id, Contracts.UpdateConfigurationRequest request, CancellationToken cancellationToken = default)
    {
        var protoRequest = new Proto.UpdateConfigurationRequest { Id = id.ToString(), Value = request.Value };
        if (request.UpdatedBy != null) protoRequest.UpdatedBy = request.UpdatedBy;

        var response = await _client.UpdateAsync(protoRequest, cancellationToken: cancellationToken);
        return FromMessage(response);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await _client.DeleteAsync(
            new DeleteConfigurationRequest { Id = id.ToString() },
            cancellationToken: cancellationToken);
        return response.Success;
    }

    private static ConfigurationDto? FromMessage(ConfigurationMessage msg)
    {
        if (string.IsNullOrEmpty(msg.Id) || !Guid.TryParse(msg.Id, out var id))
            return null;
        if (!DateTime.TryParse(msg.CreatedAt, out var createdAt)) createdAt = DateTime.UtcNow;
        if (!DateTime.TryParse(msg.UpdatedAt, out var updatedAt)) updatedAt = DateTime.UtcNow;

        return new ConfigurationDto
        {
            Id = id,
            MarketCode = msg.HasMarketCode ? msg.MarketCode : null,
            ServiceCode = msg.HasServiceCode ? msg.ServiceCode : null,
            Key = msg.Key,
            Value = msg.Value,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            CreatedBy = msg.HasCreatedBy ? msg.CreatedBy : null,
            UpdatedBy = msg.HasUpdatedBy ? msg.UpdatedBy : null
        };
    }
}
