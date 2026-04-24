using Google.Protobuf.WellKnownTypes;
using JK.Messaging.Contracts;
using JK.Messaging.Contracts.Enums;
using JK.Messaging.Proto;
using JK.Platform.Core.DependencyInjection.Attributes;
using JK.Platform.Grpc.Client.Decorators;
using JK.Platform.Grpc.Client.Factory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CreateApiMessageTaskRequest = JK.Messaging.Contracts.CreateApiMessageTaskRequest;

namespace JK.Messaging.Client.Grpc;

[Injectable(lifetime: ServiceLifetime.Scoped)]
public class ApiMessageTaskGrpcClient : ClientBaseDecorator<GrpcApiMessageTask.GrpcApiMessageTaskClient, ApiMessageTaskGrpcClient>, IApiMessageTaskGrpcClient
{
    protected override string ServerUrlConfigurationKey { get; } = "JK.Messaging.ApiMessageTaskGrpc.Server.Url";

    public ApiMessageTaskGrpcClient(IGrpcClientFactory<GrpcApiMessageTask.GrpcApiMessageTaskClient> channel, IConfiguration configuration, ILogger<ApiMessageTaskGrpcClient> logger) : base(channel, configuration, logger)
    {
    }

    public async Task<ApiMessageTaskDto> CreateAsync(CreateApiMessageTaskRequest request, CancellationToken cancellationToken = default)
    {
        var protoRequest = new Proto.CreateApiMessageTaskRequest
        {
            TaskId = request.TaskId,
            TaskName = request.TaskName,
            TargetUrl = request.TargetUrl,
            MaxAttempts = request.MaxAttempts,
            Delay = request.Delay.HasValue ? Duration.FromTimeSpan(request.Delay.Value) : null,
            RetryDelay = request.RetryDelay.HasValue ? Duration.FromTimeSpan(request.RetryDelay.Value) : null
        };

        var response = await Client.CreateAsync(protoRequest, cancellationToken: cancellationToken);
        return MapFromResponse(response);
    }

    public async Task<ApiMessageTaskDto?> GetStateAsync(string taskId, CancellationToken cancellationToken = default)
    {
        var response = await Client.GetStateAsync(
            new GetApiMessageTaskStateRequest { TaskId = taskId },
            cancellationToken: cancellationToken);
        return MapFromResponse(response);
    }

    public async Task CancelAsync(string taskId, CancellationToken cancellationToken = default)
    {
        await Client.CancelAsync(
            new CancelApiMessageTaskRequest { TaskId = taskId },
            cancellationToken: cancellationToken);
    }

    private static ApiMessageTaskDto MapFromResponse(ApiMessageTaskStateResponse response)
    {
        return new ApiMessageTaskDto
        {
            TaskId = response.TaskId,
            TaskName = response.TaskName,
            TargetUrl = response.TargetUrl,
            TaskState = (ApiMessageStateEnum)response.TaskState,
            Attempts = response.Attempts,
            MaxAttempts = response.MaxAttempts,
            LastError = response.LastError,
            CreatedOn = response.CreatedOn?.ToDateTime() ?? DateTime.MinValue,
            StartTime = response.StartTime?.ToDateTime(),
            FinishTime = response.FinishTime?.ToDateTime(),
            NextRetryOn = response.NextRetryOn?.ToDateTime()
        };
    }
}
