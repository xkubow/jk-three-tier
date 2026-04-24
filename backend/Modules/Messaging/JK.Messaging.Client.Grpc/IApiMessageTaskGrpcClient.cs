using JK.Messaging.Contracts;

namespace JK.Messaging.Client.Grpc;

public interface IApiMessageTaskGrpcClient
{
    Task<ApiMessageTaskDto> CreateAsync(CreateApiMessageTaskRequest request, CancellationToken cancellationToken = default);
    Task<ApiMessageTaskDto?> GetStateAsync(string taskId, CancellationToken cancellationToken = default);
    Task CancelAsync(string taskId, CancellationToken cancellationToken = default);
}
