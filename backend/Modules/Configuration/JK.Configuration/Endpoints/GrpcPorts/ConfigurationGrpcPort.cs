using Grpc.Core;
using JK.Configuration.Contracts;
using JK.Configuration.Proto;
using JK.Configuration.Services;

namespace JK.Configuration.Endpoints.GrpcPorts;

public class ConfigurationGrpcPort : ConfigurationGrpc.ConfigurationGrpcBase
{
    private readonly IConfigurationService _service;

    public ConfigurationGrpcPort(IConfigurationService service)
    {
        _service = service;
    }

    public override async Task<ConfigurationMessage> GetById(GetConfigurationByIdRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var id))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Id"));
        var item = await _service.GetByIdAsync(id, context.CancellationToken);
        if (item == null)
            throw new RpcException(new Status(StatusCode.NotFound, "Configuration not found"));
        return ToMessage(item);
    }

    public override async Task<ListConfigurationResponse> List(Proto.ListConfigurationRequest request, ServerCallContext context)
    {
        var listRequest = new Contracts.ListConfigurationRequest
        {
            MarketCode = request.HasMarketCode ? request.MarketCode : null,
            ServiceCode = request.HasServiceCode ? request.ServiceCode : null,
            SearchTerm = request.HasSearchTerm ? request.SearchTerm : null,
            Page = request.Page > 0 ? request.Page : 1,
            PageSize = request.PageSize > 0 ? request.PageSize : 20,
            SortBy = request.HasSortBy ? request.SortBy : null,
            SortDirection = string.IsNullOrEmpty(request.SortDirection) ? "asc" : request.SortDirection
        };
        var result = await _service.ListAsync(listRequest, context.CancellationToken);
        var response = new ListConfigurationResponse
        {
            Page = result.Page,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        };
        response.Items.AddRange(result.Items.Select(ToMessage));
        return response;
    }

    public override async Task<ConfigurationMessage> Create(Proto.CreateConfigurationRequest request, ServerCallContext context)
    {
        var createRequest = new Contracts.CreateConfigurationRequest
        {
            MarketCode = request.HasMarketCode ? request.MarketCode : null,
            ServiceCode = request.HasServiceCode ? request.ServiceCode : null,
            Key = request.Key,
            Value = request.Value,
            CreatedBy = request.HasCreatedBy ? request.CreatedBy : null
        };
        var item = await _service.CreateAsync(createRequest, context.CancellationToken);
        return ToMessage(item);
    }

    public override async Task<ConfigurationMessage> Update(Proto.UpdateConfigurationRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var id))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Id"));
        var updateRequest = new Contracts.UpdateConfigurationRequest
        {
            Value = request.Value,
            UpdatedBy = request.HasUpdatedBy ? request.UpdatedBy : null
        };
        var item = await _service.UpdateAsync(id, updateRequest, context.CancellationToken);
        if (item == null)
            throw new RpcException(new Status(StatusCode.NotFound, "Configuration not found"));
        return ToMessage(item);
    }

    public override async Task<DeleteConfigurationResponse> Delete(DeleteConfigurationRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var id))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Id"));
        var deleted = await _service.DeleteAsync(id, context.CancellationToken);
        return new DeleteConfigurationResponse { Success = deleted };
    }

    private static ConfigurationMessage ToMessage(ConfigurationDto dto)
    {
        var msg = new ConfigurationMessage
        {
            Id = dto.Id.ToString(),
            Key = dto.Key,
            Value = dto.Value,
            CreatedAt = dto.CreatedAt.ToString("O"),
            UpdatedAt = dto.UpdatedAt.ToString("O")
        };
        if (dto.MarketCode != null) msg.MarketCode = dto.MarketCode;
        if (dto.ServiceCode != null) msg.ServiceCode = dto.ServiceCode;
        if (dto.CreatedBy != null) msg.CreatedBy = dto.CreatedBy;
        if (dto.UpdatedBy != null) msg.UpdatedBy = dto.UpdatedBy;
        return msg;
    }
}
